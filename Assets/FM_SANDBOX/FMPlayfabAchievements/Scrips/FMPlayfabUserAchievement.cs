using System;
using System.Collections.Generic;
using PlayFab;
using PlayFab.ClientModels;
using SimpleJSON;
using UnityEngine;

public class FMUserAchievement
{
    public string Key;

    public FMAchievementState[] States;

    public int CurrenIndex
    {
        get
        {
            int index = 0;
            if (this.States[this.States.Length - 1] == FMAchievementState.Claimed)
            {
                return this.States.Length - 1;
            }

            for (int i = 0; i < this.States.Length; i++)
            {
                if (this.States[i] != FMAchievementState.Claimed)
                {
                    return index;
                }
                index++;
            }
            return 0;
        }
    }

    /// <summary>
    /// will return the latest achieved status depending on the progress
    /// </summary>
    public FMAchievementState CurrentAchievedState
    {
        get
        {
            FMAchievementState maxState = FMAchievementState.Locked;
            FMAchievementItem achItem = FMPlayfabAchievements.Items.Find(x => x.Key.Equals(this.Key));
            int requiredAmount = 0;

            //getting current rtequired amount, if all of them are already claimed
            if (this.States[this.States.Length - 1] == FMAchievementState.Claimed)
            {
                requiredAmount = achItem.AmountRequired[this.States.Length - 1];
            }
            else
            {
                for (int i = 0; i < this.States.Length; i++)
                {
                    if (this.States[i] != FMAchievementState.Locked)
                    {
                        requiredAmount = achItem.AmountRequired[i];
                    }
                }
            }

            for (int i = 0; i < this.States.Length; i++)
            {
                if (FMPlayfabUserStatistics.GetStatisticProgress(achItem.Statistic) >=
                requiredAmount && this.States[i] != FMAchievementState.Locked)
                {
                    maxState = this.States[i];
                }
            }
            return maxState;
        }
    }

    public FMUserAchievement(string name, FMAchievementState[] state)
    {
        Key = name;
        States = state;
    }
}

public class FMPlayfabUserAchievement : MonoBehaviour
{
    public static List<FMUserAchievement> Items = new List<FMUserAchievement>();
    public Action<FMPlayfabRewardsResult> OnClaimReward;
    public static JSONNode UAjson;
    static FMPlayfabUserAchievement instance;
    bool valuesChange = false;

    public static FMPlayfabUserAchievement Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject obj = new GameObject("FMPlayfabUserAchievement");
                instance = obj.AddComponent<FMPlayfabUserAchievement>();
            }
            return instance;
        }
        set
        {
            instance = value;
        }
    }

    void Awake()
    {
        Instance = this;
    }

    public void StoreItemsFromJson(GetUserDataResult res)
    {
        Items.Clear();
        if (res ==null ||res.Data["fm_user_achievements"] == null)
        {
            Debug.Log("no user achievements yet");
            return;
        }

        UAjson = JSON.Parse(res.Data["fm_user_achievements"].Value); //storing raw values to update later and send
        var userAchievements = JSON.Parse(res.Data["fm_user_achievements"].Value);

        for (int i = 0; i < userAchievements.Count; i++)
        {
            string name = GetUserAchievementName(userAchievements[i]);
            FMAchievementState[] values = new FMAchievementState[userAchievements[i][name].AsArray.Count];

            for (int j = 0; j < userAchievements[i][name].AsArray.Count; j++)
            {
                values[j] = (FMAchievementState)userAchievements[i][name][j].AsInt;
            }

            FMUserAchievement item = new FMUserAchievement(name, values);
            Items.Add(item);
        }
    }

    public void ClaimRewardsFromPlayfab()
    {
        //check if there are new rewards to claim
        if (valuesChange)
        {
            Debug.Log("we got a new achievement update! sending to playfab");
            PlayfabUtils.Instance.ClaimAchievementReward(UAjson, OnRewardsClaimed, OnError);
            return;
        }
        Debug.Log("no achievement updates");
    }

    static void OnError(PlayFabError error)
    {
        Debug.Log("cannot get User Achievements, " + error.ErrorMessage);
    }

    public void OnRewardsClaimed(ExecuteCloudScriptResult result)
    {
        //update client user achievements
        if (OnClaimReward != null)
        {
            FMPlayfabRewardsResult res = new FMPlayfabRewardsResult(result);
            OnClaimReward(res);
            UpdateAchievementsAfterPlayfabCall();
        }
    }

    /// <summary>
    /// updates the client array of user achievements
    /// </summary>
    public void UpdateUserAchievements(ExecuteCloudScriptResult result)
    {
        var js = JSON.Parse(result.FunctionResult.ToString());
        JSONArray jsonachvs = js["userAchievements"].AsArray;
        if (jsonachvs == null || jsonachvs.Count == 0)
        {
            return;
        }

        for (int i = 0; i < jsonachvs.Count; i++)
        {
            string achName = GetUserAchievementName(jsonachvs[i]);
            FMUserAchievement userAchv = Items.Find(x => x.Key.Equals(achName));
            for (int j = 0; j < jsonachvs[i][achName].AsArray.Count; j++)
            {
                if (userAchv.States[j] != (FMAchievementState)jsonachvs[i][achName][j].AsInt)
                {
                    userAchv.States[j] = (FMAchievementState)jsonachvs[i][achName][j].AsInt;
                    //update the json as well
                    UAjson[i][userAchv.Key][j].Value = ((int)userAchv.States[j]).ToString();
                }
            }
        }
    }

    /// <summary>
    /// updates the LOCAL(client) value for the achievement, does not have any impact on playfab call
    /// </summary>
    public void SetUserAchievementAsClaimedLocal(string key, int achvIndex)
    {
        FMUserAchievement uach = GetUserAchievementFromKey(key);
        uach.States[achvIndex] = FMAchievementState.Claimed;

        //update json as well
        for (int i = 0; i < UAjson.AsArray.Count; i++)
        {
            if (GetUserAchievementName(UAjson.AsArray[i]).Equals(key))
            {
                UAjson[i][uach.Key][achvIndex] = "2";
                valuesChange = true;
                break;
            }
        }
    }

    public void UpdateAchievementsAfterPlayfabCall()
    {
        PlayfabUtils.Instance.CheckUserAchievements((res) =>
        {
            UpdateUserAchievements(res);
            valuesChange = false;
        }, (error) => Debug.Log("error on achievements update, " + error.ErrorMessage));

    }

    /// <summary>
    /// the iv_user_achievement json has the name of the IVachievement as the name of the key, and the states:
    /// 0 for obtained achievements, 1 for unclaimed achievements(shown on pop up once) and 2 for claimed achievements
    /// eg: {"login_7_times": ["1","0"]}
    /// 
    /// </summary>
    /// <param name="userAchievement"></param>
    /// <returns></returns>
    public string GetUserAchievementName(JSONNode userAchievement)
    {
        foreach (KeyValuePair<string, JSONNode> kvp in userAchievement.AsObject)
        {
            return kvp.Key;
        }
        return "";
    }

    /// <summary>
    /// return the achievement based on the key
    /// </summary>
    /// <param name="userAchievement"></param>
    /// <returns></returns>
    public FMUserAchievement GetUserAchievementFromKey(string userAchievement)
    {
        FMUserAchievement uAchievement = Items.Find(x => x.Key.Equals(userAchievement));
        return uAchievement != null ? uAchievement : null;
    }
}