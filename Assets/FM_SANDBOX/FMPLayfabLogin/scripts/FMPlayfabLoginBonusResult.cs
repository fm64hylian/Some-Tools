using PlayFab.ClientModels;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;

public class FMLoginBonusItem{
    public int Index;
    public FMRewardItem Reward;
    public FMLoginBonusState State;

    public FMLoginBonusItem(int index, FMRewardItem item, FMLoginBonusState state) {
        Index = index;
        Reward = item;
        State = state;
    }
}

public class FMPlayfabLoginBonusResult
{
    public List<FMLoginBonusItem> Bonuses = new List<FMLoginBonusItem>();
    public int RemainingHours;
    public FMRewardItem TodayReward;
    int logCountStatistic;

    public FMPlayfabLoginBonusResult(ExecuteCloudScriptResult res)
    {
           FilterResult(res);
    }

    public void FilterResult(ExecuteCloudScriptResult result)
    {
        Bonuses.Clear();
        //Debug.Log(PlayFabSimpleJson.SerializeObject(result.FunctionResult));
        JSONNode jsonResult = JSON.Parse(result.FunctionResult.ToString());
        //Debug.Log(jsonResult);
        if ((jsonResult.Value.Equals("null") || result.Error != null))
        {
            //status = FMRewardStatus.error;
            return;
        }

        //get logi_count statistic
        logCountStatistic = FMPlayfabUserStatistics.GetStatisticProgress("login_count");

        RemainingHours = jsonResult["remainingHour"] == null ? 0 : jsonResult["remainingHour"].AsInt;
        int logCount = jsonResult["login_count"] == null ? logCountStatistic : jsonResult["login_count"].AsInt; ;

        //if there's daily bonus
        string dailyRewardKey = jsonResult["daily_reward"];
        if (!string.IsNullOrEmpty(dailyRewardKey)) {
            TodayReward = FMPlayfabReward.GetRewardFromKey(dailyRewardKey);
        }


        for (int i = 0; i < jsonResult["BonusResult"].AsArray.Count; i++)
        {
            string rewardKey = jsonResult["BonusResult"].AsArray[i]["reward_key"];
            FMRewardItem reward = FMPlayfabReward.GetRewardFromKey(rewardKey);
            
            FMLoginBonusState state;
            if (logCount > i)
            {
                state = FMLoginBonusState.Claimed;
            }
            else if (logCount == i)
            {
                state = FMLoginBonusState.Today;
            }
            else
            {
                state = FMLoginBonusState.Unclaimed;
            }
            Bonuses.Add(new FMLoginBonusItem(i, reward, state));
            //var ri = new FMRewardItem(rewardKey, rewardType, rewardValue, rewardValue);
            //ClaimedRewards.Add(ri);
        }
    }

    public string GetTodayBonusMessage() {
        return  TodayReward != null ? "you got " + TodayReward.GetValue().ToString() + " "
            + TodayReward.RewardTypeValue + " as a Daily Bonus!" : "Please Log in in " + RemainingHours +
            " hour(s) to get a new Bonus";
    }
}
