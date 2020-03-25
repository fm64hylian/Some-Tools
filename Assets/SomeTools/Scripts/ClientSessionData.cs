using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;

public class ClientSessionData : MonoBehaviour
{
    public string PlayfabID;
    public string UserName;
    public List<FMUserStatistic> Statistics = new List<FMUserStatistic>();
    public List<FMAchievementItem> Achievements = new List<FMAchievementItem>();
    public List<FMUserAchievement> UserAchievements = new List<FMUserAchievement>();
    public List<FMRewardItem> Rewads = new List<FMRewardItem>();
    public List<CatalogItem> CatalogItems = new List<CatalogItem>();
    public List<FMInventoryItem> InventoryItems = new List<FMInventoryItem>();

    public int currencyCO;
    public int currencyPC;
    static ClientSessionData instance;    

    public static ClientSessionData Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject obj = new GameObject("ClientData");
                instance = obj.AddComponent<ClientSessionData>();
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
        DontDestroyOnLoad(gameObject);
    }
}
