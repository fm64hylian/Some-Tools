using PlayFab.ClientModels;
using PlayFab.Json;
using SimpleJSON;
using System.Collections.Generic;
using UnityEngine;

public enum FMRewardStatus
{
    success,
    error,
}

public class FMPlayfabRewardsResult : MonoBehaviour // TODO remove mono
{
    public FMRewardStatus status = FMRewardStatus.success;
    public List<FMRewardItem> ClaimedRewards = new List<FMRewardItem>();
    JSONNode jsonResult;


    public JSONNode JsonResult
    {
        get
        {
            return jsonResult;
        }
    }

    public FMPlayfabRewardsResult(ExecuteCloudScriptResult res)
    {
        FilterResult(res);
    }

    public void FilterResult(ExecuteCloudScriptResult result)
    {
        ClaimedRewards.Clear();            
        JSONNode jsonResultPF = PlayFabSimpleJson.SerializeObject(result.FunctionResult);        
        jsonResult = JSON.Parse(result.FunctionResult.ToString());

        Debug.Log("json result playfab " + jsonResultPF.ToString());
        Debug.Log("json result " + jsonResult.ToString());
        if ((jsonResult.Value.Equals("null") || result.Error != null))
        {
            status = FMRewardStatus.error;
            return;
        }

        /*{ "status":"success",
         * "rewards":
         * [{"achievement_key":"stages_created","reward_Key":"default_co_200","reward_type":"coins","amount":200}]
         * }
         */
        Debug.Log("function result  size" + jsonResult["rewards"].AsArray.Count);
        for (int i = 0; i < jsonResult["rewards"].AsArray.Count; i++)
        {
            string rewardKey = jsonResult["rewards"].AsArray[i]["reward_Key"];
            string rewardType = jsonResult["rewards"].AsArray[i]["reward_type"];

            FMRewardType rewadType = rewardType.Equals("item") ? FMRewardType.Item : FMRewardType.Currency;
            int rewardValue = rewadType == FMRewardType.Currency ? jsonResult["rewards"].AsArray[i]["amount"].AsInt : 0;
            string itemKey = rewadType == FMRewardType.Item ? jsonResult["rewards"].AsArray[i]["item_key"].Value : "";

            //var ri = new FMRewardItem(rewardKey, rewardType, rewardValue, rewardValue);
            //ClaimedRewards.Add(ri);
        }
    }
}
