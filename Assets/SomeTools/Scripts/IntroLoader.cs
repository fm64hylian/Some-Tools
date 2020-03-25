using PlayFab;
using PlayFab.ClientModels;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using SimpleJSON;

public class IntroLoader : MonoBehaviour
{
    [SerializeField]
    UISlider progressBar;
    [SerializeField]
    UILabel labLoading;
    float progress =0f;
    //float total = 0f;
    bool loadComplete = false;    

    void Start()
    {
        progressBar.value = progress;

        labLoading.text = "... Logging in ...";
        FMPlayfabLogin.LoginCustomID("64646464", OnLoginSuccess);
    }

    /// <summary>
    /// nesting so many callbacks is not that good practice, but for the sake of
    /// keeping it sinchronous, we'll do it just this one time
    /// </summary>
    /// <param name="logResult"></param>
    void OnLoginSuccess(LoginResult logResult)
    {
        List<ItemInstance> inventoryItems = new List<ItemInstance>();
        //get display name
        PlayFabClientAPI.GetAccountInfo(new GetAccountInfoRequest()
        {
            PlayFabId = logResult.PlayFabId
        }, result =>
        {
            ClientSessionData.Instance.PlayfabID = logResult.PlayFabId;
            ClientSessionData.Instance.UserName = result.AccountInfo.TitleInfo.DisplayName;
            labLoading.text = "... Loading user info ...";
            progress += 0.2f;
            //total += progress;

            //get currency
            PlayFabClientAPI.GetUserInventory(new GetUserInventoryRequest(), resInventory =>
            {
                //currency
                int CO = 0;
                int PC = 0;
                resInventory.VirtualCurrency.TryGetValue("CO", out CO);
                resInventory.VirtualCurrency.TryGetValue("PC", out PC);
                ClientSessionData.Instance.currencyCO = CO;
                ClientSessionData.Instance.currencyPC = PC;

                //inventory
                inventoryItems = resInventory.Inventory;
                labLoading.text = "... Loading Inventory ...";
                progress += 0.2f;

                //statistics
                PlayfabUtils.Instance.GetPlayerStatistics(null, statRes =>
                {
                    FMPlayfabUserStatistics.StoreItemsFromJson(statRes);
                    ClientSessionData.Instance.Statistics = FMPlayfabUserStatistics.Items;
                    labLoading.text = "... Loading User Statistics ...";
                    progress += 0.2f;

                    //get title Data
                    PlayfabUtils.Instance.GetTitleData(new List<string> { "fm_achievements", "fm_rewards" }, titleRes =>
                    {
                        FMPlayfabAchievements.Instance.StoreItemsFromJson(titleRes);
                        FMPlayfabReward.StoreItemsFromJson(titleRes);

                        ClientSessionData.Instance.Achievements = FMPlayfabAchievements.Items;
                        ClientSessionData.Instance.Rewads = FMPlayfabReward.Items;

                        labLoading.text = "... Loading Title Data ...";
                        progress += 0.2f;

                        //get catalogItems
                        PlayFabClientAPI.GetCatalogItems(new GetCatalogItemsRequest(), catalogRes =>
                        {
                            ClientSessionData.Instance.CatalogItems = catalogRes.Catalog;

                            //rossing CatalogItem and ItemInstance items
                            StoreInventoryItemsFromPlayfab(catalogRes.Catalog, inventoryItems);

                            //TODO cross catalog and instance items;
                            labLoading.text = "... Loading Catalog Items ...";
                            progress += 0.2f;
                        }
                        , error => { Debug.Log("error on get catalog info"); });
                        //end catalog

                    }, error => { Debug.Log("error on get title info"); });
                    //end get title

                }, error => { Debug.Log("error on getting statistics"); });
                //end get Statistics
            }
            , error => { Debug.Log("error on get currency info"); });
            //end get currency

        }, error => { Debug.Log("error on get account info"); });
            //end get account info
    }

    void Update()
    {
        if (loadComplete) {
            return;
        }

        if (progressBar.value >= 1f && !loadComplete) {
            loadComplete = true;
            GoToHome();            
            return;
        }
        progressBar.value = Mathf.Lerp(progress, 1f, Time.deltaTime);
    }

    /// <summary>
    /// TODO check where to put
    /// </summary>
    void StoreInventoryItemsFromPlayfab(List<CatalogItem> catalogItems, List<ItemInstance> inventoryItems) {
        List<FMInventoryItem> FMInventoryItems = new List<FMInventoryItem>();
        for (int i =0; i < inventoryItems.Count; i++) {
            ItemInstance iitem = inventoryItems[i];
            CatalogItem cItem = catalogItems.Find(x => x.ItemId.Equals(iitem.ItemId));
            if (cItem != null) {
                FMInventoryItem invItem = new FMInventoryItem();
                invItem.DisplayName = cItem.DisplayName;
                invItem.CatalogID = cItem.ItemId;
                invItem.InstanceID = iitem.ItemInstanceId;
                invItem.Description = cItem.Description;
                invItem.SpriteName = cItem.ItemImageUrl;
                invItem.Amount = cItem.IsStackable ? (int)iitem.RemainingUses : -1;
                invItem.Tags = cItem.Tags;
                invItem.Prices = cItem.VirtualCurrencyPrices;
                if (!string.IsNullOrEmpty(cItem.CustomData)) {
                    Debug.Log("item custom json "+cItem.CustomData);
                    JSONNode jsonEffects = JSON.Parse(cItem.CustomData);
                    Dictionary<string, string> dicEffect = new Dictionary<string, string>();
                    dicEffect.Add(jsonEffects["effect"].Value, jsonEffects["value"]);
                    invItem.Effects = dicEffect;
                }

                //custom item instance data
                if (iitem.CustomData != null && iitem.CustomData.ContainsKey("is_favorite")) {
                    invItem.IsFavorite = bool.Parse(iitem.CustomData["is_favorite"]);
                }
                if (iitem.CustomData != null && iitem.CustomData.ContainsKey("is_equipped")) {
                    invItem.IsEquipped = bool.Parse(iitem.CustomData["is_equipped"]);
                }

                FMInventoryItems.Add(invItem);
            }
        }
        ClientSessionData.Instance.InventoryItems = FMInventoryItems;
    }

    void GoToHome() {
        SceneManager.LoadScene("Home");
    }
}
