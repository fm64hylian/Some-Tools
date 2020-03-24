using PlayFab.ClientModels;
using SimpleJSON;
using System.Collections.Generic;
using UnityEngine;
using PlayFab.Json;

public enum FMAchievementStatus
{
    success,
    error,
}

public class FMPlayfabUserAchievementResult
{
    public FMAchievementStatus status = FMAchievementStatus.success;

    public string ErrorMessage = string.Empty;

    public List<FMUserAchievement> UserItems = new List<FMUserAchievement>();

    JSONArray userAchievements;

    public FMPlayfabUserAchievementResult(ExecuteCloudScriptResult res)
    {
        //Debug.Log("user achv res: "+JSON.Parse(res.ToString()));
        Debug.Log(PlayFabSimpleJson.SerializeObject(res.FunctionResult));
        if (res.FunctionResult == null) {
            return;
        }
        var json = JSON.Parse(res.FunctionResult.ToString());
        if ((json.Value.Equals("null") || res.Error != null))
        {
            status = FMAchievementStatus.error;
            ErrorMessage = res.Error.Message;
            return;
        }

        userAchievements = json["userAchievements"].AsArray;
        ////////////////////////////////////////////////////

        //we will check the state of the achievement (locked, unlocked, claimed, etc) depeding on the json
        for (int i = 0; i < FMPlayfabAchievements.Items.Count; i++)
        {
            var item = FMPlayfabAchievements.Items[i];

            JSONNode nodeGetReward = ContainsAchievement(item.Key);
            //if (nodeGetReward != null)
            //{
            //    switch (nodeGetReward[item.Key].AsInt)
            //    {
            //        case 0:
            //            item.State = IVAchievementState.Locked;
            //            break;
            //        case 1:
            //            item.State = IVAchievementState.Unclaimed;
            //            break;
            //        case 2:
            //            item.State = IVAchievementState.Claimed;
            //            break;
            //        default:
            //            break;
            //    }
            //    item.State = nodeGetReward[item.Key].AsInt != 1 ? IVAchievementState.Unclaimed : IVAchievementState.Claimed;
            }        

        //User IV achievements
        if (userAchievements == null)
        {
            return;
        }
        foreach (JSONNode kvp in userAchievements)
        {
            string name = FMPlayfabUserAchievement.Instance.GetUserAchievementName(kvp);
            FMAchievementState[] values = new FMAchievementState[kvp.Value.Length];
            for (int j =0; j < kvp.Value.Length; j++) {
                values[j] = (FMAchievementState)(kvp[name].AsInt);
            }
            var userItem = new FMUserAchievement(name, values);
            UserItems.Add(userItem);
        }
    }

    JSONNode ContainsAchievement(string achievement)
    {
        if (userAchievements == null)
        {
            return null;
        }
        foreach (JSONNode kvp in userAchievements)
        {
            if (!kvp[achievement].Value.Equals(""))
            {
                return kvp;
            }
        }
        return null;
    }
}
