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
    float total = 0.2f;
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
            total += progress;

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
                //total += progress;

                //statistics
                PlayfabUtils.Instance.GetPlayerStatistics(null, statRes =>
                {
                    FMPlayfabUserStatistics.StoreItemsFromJson(statRes);
                    ClientSessionData.Instance.Statistics = FMPlayfabUserStatistics.Items;
                    labLoading.text = "... Loading User Statistics ...";
                    progress += 0.2f;
                    total += progress;

                    //get title Data
                    PlayfabUtils.Instance.GetTitleData(new List<string> { "fm_achievements", "fm_rewards" }, titleRes =>
                    {
                        FMPlayfabAchievements.Instance.StoreItemsFromJson(titleRes);
                        FMPlayfabReward.StoreItemsFromJson(titleRes);

                        ClientSessionData.Instance.Achievements = FMPlayfabAchievements.Items;
                        ClientSessionData.Instance.Rewads = FMPlayfabReward.Items;

                        labLoading.text = "... Loading Title Data ...";
                        progress += 0.1f;
                        total += progress;

                        //get catalogItems
                        PlayFabClientAPI.GetCatalogItems(new GetCatalogItemsRequest(), catalogRes =>
                        {
                            ClientSessionData.Instance.CatalogItems = catalogRes.Catalog;

                            //crossing CatalogItem and ItemInstance items
                            FMPlayFabInventory.StoreItemsFromPlayfab(catalogRes.Catalog, inventoryItems);
                            ClientSessionData.Instance.InventoryItems = FMPlayFabInventory.Items;

                            //TODO cross catalog and instance items;
                            labLoading.text = "... Loading Catalog Items ...";
                            progress += 0.1f;
                            total += progress;
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
        progressBar.value = Mathf.Lerp(progress, total, Time.deltaTime);
    }   

    void GoToHome() {
        SceneManager.LoadScene("Home");
    }
}
