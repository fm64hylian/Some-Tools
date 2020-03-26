using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using System;
using SimpleJSON;

public class PlayfabUtils : MonoBehaviour
{
    static PlayfabUtils instance;

    public static PlayfabUtils Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject obj = new GameObject("FMPlayfabUtils");
                instance = obj.AddComponent<PlayfabUtils>();
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

    public void  GetUserReadOnlyData(List<string> jsonNames, Action<GetUserDataResult> onCallBack, Action<PlayFabError> onError) {
        var request = new GetUserDataRequest()
        {
            Keys = jsonNames
        };
        PlayFabClientAPI.GetUserReadOnlyData(request, onCallBack, onError);
    }

    public void GetPlayerStatistics(List<string> statisticNames, Action<GetPlayerStatisticsResult> onCallBack, Action<PlayFabError> onError) {
        var request = new GetPlayerStatisticsRequest()
        {
            StatisticNames = statisticNames
        };
        PlayFabClientAPI.GetPlayerStatistics(request, onCallBack, onError);
    }

    public void GetTitleData(List<string> jsonNames, Action<GetTitleDataResult> onCallBack, Action<PlayFabError> onError) {
        var request = new GetTitleDataRequest()
        {
            Keys = jsonNames
        };
        PlayFabClientAPI.GetTitleData(request, onCallBack, onError);
    }

    public void PurhaseItem(CatalogItem item, string vc, Action<PurchaseItemResult> onPurchase, Action<PlayFabError> onError) {
        uint price;
        item.VirtualCurrencyPrices.TryGetValue(vc, out price);
        var request = new PurchaseItemRequest()
        {
            CatalogVersion = "Main Catalog",
            ItemId = item.ItemId,
            Price =(int)price,
            VirtualCurrency = vc
        };

        PlayFabClientAPI.PurchaseItem(request, onPurchase, onError);
    }

    //////////////////////
    // CLOUDSCRIPS      //
    //////////////////////

    public void ExecuteCloudscript(string name, object args,Action<ExecuteCloudScriptResult> result, Action<PlayFabError> onError) {

        var request = new ExecuteCloudScriptRequest()
        {
            FunctionName = name,
            FunctionParameter = args,
            RevisionSelection = CloudScriptRevisionOption.Latest,
            GeneratePlayStreamEvent = true
        };
        PlayFabClientAPI.ExecuteCloudScript(request, result, onError);
    }

    public void CheckLoginBonus(string mode, Action<ExecuteCloudScriptResult> onCallback, Action<PlayFabError> OnError) {
        mode = string.IsNullOrEmpty(mode) ? "regular" : mode;
        var args = new { mode = mode };//JsonUtility.ToJson(new { mode = "regular" });

        ExecuteCloudscript("CheckLoginBonus", args, onCallback, OnError);
    }

    public void CheckUserAchievements(Action<ExecuteCloudScriptResult> onCallback, Action<PlayFabError> OnError) {
        ExecuteCloudscript("CheckUserAchievements", null, onCallback, OnError);
    }

    //public void CheckOldUserStatistics(Action<ExecuteCloudScriptResult> onCallBack, Action<PlayFabError> OnError) {
    //    ExecuteCloudscript("CheckOldUserStatistics", null, onCallBack, OnError);
    //}

    public void ClaimAchievementReward(JSONNode UAjson, Action<ExecuteCloudScriptResult> OnRewardsClaimed, Action<PlayFabError> OnError) {
        Dictionary<string, string> args = new Dictionary<string, string>();
        args.Add("unclaimed_achievements", UAjson.ToString());

        ExecuteCloudscript("ClaimAchievementReward",args, OnRewardsClaimed, OnError);
    }

    public void SellItem(string instanceID, string VC, Action<ExecuteCloudScriptResult> OnRewardsClaimed, Action<PlayFabError> OnError) {
        object args = new { soldItemInstanceId = instanceID , requestedVcType = VC };
        ExecuteCloudscript("SellItem", args, OnRewardsClaimed, OnError);
    }
}
