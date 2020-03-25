using PlayFab;
using PlayFab.ClientModels;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FMStoreController : MonoBehaviour
{
    [SerializeField]
    UIGrid itemGrid;
    [SerializeField]
    GameObject header;
    [SerializeField]
    GameObject itemDetail;

    UILabel labItenName;
    UISprite itemSprite;
    UILabel labDescription;
    UIButton buttonCO;
    UIButton buttonPC;

    FMStoreItemUI selectedItem;

    void Start() {
        UILabel labCO = header.GetComponentsInChildren<UILabel>()[2];
        UILabel labPC = header.GetComponentsInChildren<UILabel>()[3];

        labCO.text = ClientSessionData.Instance.currencyCO.ToString();
        labPC.text = ClientSessionData.Instance.currencyPC.ToString();

        //getting selectedIdem UI
        labItenName = itemDetail.GetComponentsInChildren<UILabel>()[0];
        itemSprite = itemDetail.GetComponentsInChildren<UISprite>()[0];
        labDescription = itemDetail.GetComponentsInChildren<UILabel>()[1];
        buttonCO = itemDetail.GetComponentsInChildren<UIButton>()[0];
        buttonPC = itemDetail.GetComponentsInChildren<UIButton>()[1];


        if (ClientSessionData.Instance.CatalogItems.Count == 0){
            PlayFabClientAPI.GetCatalogItems(new GetCatalogItemsRequest(), catalogRes =>{
                ClientSessionData.Instance.CatalogItems = catalogRes.Catalog;
                DisplayItems();
                return;
            }, error => { Debug.Log("error on get catalog info"); });
        }
        DisplayItems();
    }

    void DisplayItems(){
        for (int i = 0; i < ClientSessionData.Instance.CatalogItems.Count; i++){

            CatalogItem item = ClientSessionData.Instance.CatalogItems[i];
            GameObject itemPrefab = Instantiate(Resources.Load("Prefabs/FMStoreItemListUI")) as GameObject;
            FMStoreItemUI itemUI = itemPrefab.GetComponent<FMStoreItemUI>();

            itemUI.SetData(item);
            itemUI.OnSelected = DisplaySelectedItemInfo;

            itemUI.gameObject.transform.parent = itemGrid.transform;
            itemUI.gameObject.transform.localScale = Vector3.one;
        }

        itemGrid.Reposition();
    }

    void DisplaySelectedItemInfo(FMStoreItemUI item) {

        if (selectedItem != null) {
            selectedItem.Unselect();
        }

        labItenName.text = item.Item.DisplayName;
        itemSprite.spriteName = item.Item.ItemImageUrl;
        labDescription.text = item.Item.Description;

        uint CO;
        uint PC;         
        bool hasCO = item.Item.VirtualCurrencyPrices.TryGetValue("CO", out CO);
        bool hasPC = item.Item.VirtualCurrencyPrices.TryGetValue("PC", out PC);

        buttonCO.GetComponentInChildren<UILabel>().text = hasCO ? CO.ToString() : "--";
        buttonPC.GetComponentInChildren<UILabel>().text = hasPC ? PC.ToString() : "--";

        //disable buttons if there is no currency
        if (!hasCO)
        {
            buttonCO.isEnabled = false;
        }
        if (!hasPC)
        {
            buttonPC.isEnabled = false;
        }

        selectedItem = item;
    }

    public void BackToHome()
    {
        SceneManager.LoadScene("Home");
    }
}
