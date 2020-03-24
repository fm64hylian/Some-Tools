using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using SimpleJSON;

public class FMUserStatistic
{
    public string StatisticName;

    public int Value;

    public FMUserStatistic(string key, int value)
    {
        StatisticName = key;
        Value = value;
    }
}

public class FMPlayfabUserStatistics : MonoBehaviour
{
    public static List<FMUserStatistic> Items = new List<FMUserStatistic>();

    public static void StoreItemsFromJson(GetPlayerStatisticsResult res)
    {
        Items.Clear();
        List<StatisticValue> allStatistics = res.Statistics;

        for (int i = 0; i < allStatistics.Count; i++)
        {
            var statistic = new FMUserStatistic(allStatistics[i].StatisticName, allStatistics[i].Value);
            Items.Add(statistic);
        }
    }

    /// <summary>
    /// updates the client's statistic ith the newest playfab value
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    public static void UpdateStatisticValue(string name, int value)
    {
        FMUserStatistic stat = Items.Find(x => x.StatisticName.Equals(name));
        if (stat != null)
        {
            stat.Value = value;
        }
    }

    public static int GetStatisticProgress(string statisticName)
    {
        FMUserStatistic stat = Items.Find(x => x.StatisticName.Equals(statisticName));
        if (stat != null)
        {
            return stat.Value;
        }
        return 0;
    }
}
