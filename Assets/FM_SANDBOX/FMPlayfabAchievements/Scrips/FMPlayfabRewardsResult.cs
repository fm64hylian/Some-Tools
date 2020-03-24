using PlayFab.ClientModels;
using SimpleJSON;
using System.Collections.Generic;

public enum FMRewardStatus
{
    success,
    error,
}

public class FMPlayfabRewardsResult
{
    public FMRewardStatus status = FMRewardStatus.success;
    public List<FMRewardItem> ClaimedRewards = new List<FMRewardItem>();
    ExecuteCloudScriptResult result;
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
        result = res;
        FilterResult();
    }

    public void FilterResult()
    {
        ClaimedRewards.Clear();
        jsonResult = JSON.Parse(result.FunctionResult.ToString());
        if ((jsonResult.Value.Equals("null") || result.Error != null))
        {
            status = FMRewardStatus.error;
            return;
        }

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
