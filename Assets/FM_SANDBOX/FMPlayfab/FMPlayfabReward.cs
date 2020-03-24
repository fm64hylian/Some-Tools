using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using PlayFab.ClientModels;

public enum FMRewardType
{
    Currency,
    Item
}

public class FMRewardItem
{
    public string Key { get; }

    /// <summary>
    /// the type of currency or item (CR, CO, etc)
    /// </summary>
    public string RewardTypeValue { get; }

    /// <summary>
    /// 0 if the reward is an item
    /// </summary>
    public int Amount { get; }

    /// <summary>
    /// null if the reward is currency
    /// </summary>
    public string ItemKey { get; }

    public FMRewardType Type { get; }

    public FMRewardItem(string rewardKey, string rewardType, int amount, string itemKey, FMRewardType type)
    {
        this.Key = rewardKey;
        this.RewardTypeValue = rewardType;
        this.Amount = amount;
        this.ItemKey = itemKey;
        this.Type = type;
    }

    /// <summary>
    /// convenience function to get either the item or currency amount
    /// </summary>
    /// <returns></returns>
    public string GetValue()
    {
        return Type == FMRewardType.Currency ? Amount.ToString() : ItemKey;
    }
}

public class FMPlayfabReward : MonoBehaviour
{
    public static List<FMRewardItem> Items = new List<FMRewardItem>();

    public static void StoreItemsFromJson(GetTitleDataResult res)
    {
        Items.Clear();
        var allRewardsJson = JSON.Parse(res.Data["fm_rewards"]);

        for (int i = 0; i < allRewardsJson.Count; i++)
        {
            var reward = allRewardsJson[i];
            FMRewardType type = reward["item_key"] == null ? FMRewardType.Currency : FMRewardType.Item;
            int amount = type == FMRewardType.Currency ? reward["amount"].AsInt : 0;
            string itemKey = type == FMRewardType.Item ? reward["item_key"].Value : "";
            var ri = new FMRewardItem(reward["key"], reward["reward_type"], amount, itemKey, type);
            Items.Add(ri);
        }
    }

    public static FMRewardItem GetRewardFromKey(string key)
    {
        return Items.Find(x => x.Key == key);
    }
}
