using PlayFab;
using PlayFab.ClientModels;
using SimpleJSON;
using System;
using System.Collections.Generic;
using UnityEngine;

public class FMAchievementItem
{
    public string Key;

    public string[] RewardKeys;

    public string Title;

    public int[] AmountRequired;

    public string Statistic;

    public string iconName;
}

public enum FMAchievementState
{
    Locked = 0,
    Unclaimed = 1,
    Claimed = 2
}

public class FMPlayfabAchievements : MonoBehaviour
{
    public Action<FMPlayfabUserAchievementResult> OnGetAchievement;
    public static List<FMAchievementItem> Items = new List<FMAchievementItem>();
    static FMPlayfabAchievements instance;

    public static FMPlayfabAchievements Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject obj = new GameObject("FMPlayfabAchievements");
                instance = obj.AddComponent<FMPlayfabAchievements>();
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

    public void GetUserAchivements()
    {
        PlayfabUtils.Instance.CheckUserAchievements(OnUserAchievement, OnError);
    }

    /// <summary>
    /// used to check if users haven't migrated to the new IV achievement system yet
    /// </summary>
    //public void CheckOldAchievementsMigration()
    //{
    //    PlayfabUtils.Instance.CheckOldUserStatistics((result) =>
    //    {
    //        var json = JSON.Parse(result.FunctionResult.ToString());
    //        if (json["status"].Value.Equals("success"))
    //        {
    //            Debug.Log("adding new user statistics, " + json["comment"].Value);
    //        }
    //    }, OnError);
    //}

    static void OnError(PlayFabError error)
    {
        Debug.Log("cannot get Achievements, " + error.ErrorMessage);
    }

    /// <summary>
    /// make sure to set the action BEFORE calling Login()
    /// </summary>
    /// <param name="result"></param>
    public void OnUserAchievement(ExecuteCloudScriptResult result)
    {
        if (OnGetAchievement != null)
        {
            FMPlayfabUserAchievementResult fmAchievementResult = new FMPlayfabUserAchievementResult(result);
            OnGetAchievement(fmAchievementResult);
        }
    }

    /// <summary>
    /// get title achievements
    /// </summary>
    /// <param name="res"></param>
    public void StoreItemsFromJson(GetTitleDataResult res)
    {
        Items.Clear();
        var titleAchievements = JSON.Parse(res.Data["fm_achievements"]);

        for (int i = 0; i < titleAchievements.Count; i++)
        {
            var item = new FMAchievementItem();
            item.Key = titleAchievements[i]["key"].Value;
            item.Title = titleAchievements[i]["title"].Value;

            item.AmountRequired = new int[titleAchievements[i]["count"].AsArray.Count];
            item.RewardKeys = new string[titleAchievements[i]["reward_keys"].Count];
            for (int j = 0; j < titleAchievements[i]["reward_keys"].Count; j++)
            {
                item.RewardKeys[j] = titleAchievements[i]["reward_keys"][j].Value;
                item.AmountRequired[j] = titleAchievements[i]["count"][j].AsInt;
            }
            item.Statistic = titleAchievements[i]["statistic_name"].Value;
            item.iconName = titleAchievements[i]["icon"].Value;

            Items.Add(item);
        }
    }
}
