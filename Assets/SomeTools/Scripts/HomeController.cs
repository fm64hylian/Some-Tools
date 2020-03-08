using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PlayFab;
using PlayFab.ClientModels;

public class HomeController : MonoBehaviour
{
    [SerializeField]
    UIGrid itemGrid;
    [SerializeField]
    UILabel labUser;
    [SerializeField]
    UILabel labCurrency;

    void Start(){
        var request = new LoginWithCustomIDRequest { CustomId = "64646464", CreateAccount = false };
        PlayFabClientAPI.LoginWithCustomID(request, OnLoginSuccess, OnLoginFailure);
    }
    

    void OnLoginSuccess(LoginResult res){
        Debug.Log("login success");

        //get display name
        PlayFabClientAPI.GetAccountInfo(new GetAccountInfoRequest(){
            PlayFabId = res.PlayFabId
        }, result =>{
            labUser.text = result.AccountInfo.TitleInfo.DisplayName;            
        }, error => { Debug.Log("error on get account info"); });

        //get currency
        PlayFabClientAPI.GetUserInventory(new GetUserInventoryRequest(), resInventory => {
            int currencyValue = 0;
            resInventory.VirtualCurrency.TryGetValue("CO", out currencyValue);
            labCurrency.text = currencyValue.ToString();
        }
        , error => { Debug.Log("error on get account info"); });

        //get catalogItems
        PlayFabClientAPI.GetCatalogItems(new GetCatalogItemsRequest(), catalogRes => {
            DisplayUserItems(catalogRes.Catalog);
        }
        , error => { Debug.Log("error on get account info"); });
    }

    void OnLoginFailure(PlayFabError error){
        Debug.Log("login error");
    }

    void DisplayUserItems(List<CatalogItem> items) {
        for (int i = 0; i < items.Count; i++) {
            CatalogItem item = items[i];
            GameObject itemPrefab = Instantiate(Resources.Load("Prefabs/InventoryItemUI")) as GameObject;
            InventoryItemUI itemUI = itemPrefab.GetComponent<InventoryItemUI>();

            itemUI.SetData(item.DisplayName, item.ItemImageUrl);

            itemUI.gameObject.transform.parent = itemGrid.transform;
            itemUI.gameObject.transform.localScale = Vector3.one;            
        }

        itemGrid.Reposition();
    }

    void Update(){

    }
}
