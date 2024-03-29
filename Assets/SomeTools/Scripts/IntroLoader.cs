﻿using PlayFab;
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
    float total = 0.0f;
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
            progress += 0.20f;
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
                progress += 0.20f;
                //total += progress;

                //statistics
                PlayfabUtils.Instance.GetPlayerStatistics(null, statRes =>
                {
                    FMPlayfabUserStatistics.StoreItemsFromJson(statRes);
                    ClientSessionData.Instance.Statistics = FMPlayfabUserStatistics.Items;
                    labLoading.text = "... Loading User Statistics ...";
                    progress += 0.20f;
                    total += progress;

                    //get title Data
                    PlayfabUtils.Instance.GetTitleData(new List<string> { "fm_achievements", "fm_rewards" }, titleRes =>
                    {
                        FMPlayfabAchievements.Instance.StoreItemsFromJson(titleRes);
                        FMPlayfabReward.StoreItemsFromJson(titleRes);

                        ClientSessionData.Instance.Achievements = FMPlayfabAchievements.Items;
                        ClientSessionData.Instance.Rewards = FMPlayfabReward.Items;

                        labLoading.text = "... Loading Title Data ...";
                        progress += 0.20f;
                        total += progress;

                        //get catalogItems
                        PlayFabClientAPI.GetCatalogItems(new GetCatalogItemsRequest(), catalogRes =>
                        {
                            ClientSessionData.Instance.CatalogItems = catalogRes.Catalog;

                            //crossing CatalogItem and ItemInstance items
                            FMPlayFabInventory.StoreItemsFromPlayfab(catalogRes.Catalog, inventoryItems);
                            ClientSessionData.Instance.InventoryItems = FMPlayFabInventory.Items;

                            //get user equipped items (from ReadOnlyData)
                            PlayfabUtils.Instance.GetUserReadOnlyData(new List<string>() { "fm_user_equipment" },
                                useDataRes => {
                            //if created ,assign to client
                            if (useDataRes.Data.ContainsKey("fm_user_equipment"))
                                    {
                                        Debug.Log("read only data");
                                        FMPlayFabInventory.StoreSlotsFromJson(useDataRes);
                                        SceneManager.LoadScene("Store", LoadSceneMode.Single);
                                    }
                            //if not created, assign for the first time
                            else
                                    {
                                        Debug.Log("no equippment found, creating");
                                        FMPlayFabInventory.CreateUserEquipment(slotRes => {

                                            FMPlayFabInventory.StoreSlotsFromJson(slotRes);

                                            Debug.Log("getting Equip slots for first time" + ClientSessionData.Instance.Slots.Count);
                                            SceneManager.LoadScene("Store", LoadSceneMode.Single);

                                        }, error => {
                                            Debug.Log("error on get userEquipment");
                                    //end get userEquipment
                                });
                                    }

                                }, useDataError => { Debug.Log("error on get userEquipment using getUserdata"); });
                            //end get userEquipment with GetUserData

                            labLoading.text = "... Loading Catalog Items ...";
                            progress += 0.20f;
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
        progressBar.value =  progress;//Mathf.Lerp(progress, total, Time.deltaTime);
    }   

    void GoToHome() {
        SceneManager.LoadScene("Home");
    }
}
