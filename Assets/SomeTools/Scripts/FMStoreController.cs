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

    UILabel labCO;
    UILabel labPC;

    //item detail UI
    UILabel labItenName;
    UISprite itemSprite;
    UILabel labDescription;
    UILabel labOwned;
    UIButton buttonCO;
    UIButton buttonPC;

    FMStoreItemUI selectedItem;

    void Start() {
        labCO = header.GetComponentsInChildren<UILabel>()[2];
        labPC = header.GetComponentsInChildren<UILabel>()[3];

        labCO.text = ClientSessionData.Instance.currencyCO.ToString();
        labPC.text = ClientSessionData.Instance.currencyPC.ToString();

        //getting selectedIdem UI
        labItenName = itemDetail.GetComponentsInChildren<UILabel>()[0];
        itemSprite = itemDetail.GetComponentsInChildren<UISprite>()[1];
        labDescription = itemDetail.GetComponentsInChildren<UILabel>()[1];
        labOwned = itemDetail.GetComponentsInChildren<UILabel>()[3];
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
        FilterByItem();
    }

    void DisplaySelectedItemInfo(FMStoreItemUI item) {
        buttonCO.isEnabled = true;
        buttonPC.isEnabled = true;

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

        FMInventoryItem invItem = ClientSessionData.Instance.InventoryItems.Find(x => x.CatalogID.Equals(item.Item.ItemId));
        labOwned.text = invItem != null ? invItem.Amount.ToString() : "0";

        selectedItem = item;
    }

    public void BuyWithCO() {
        uint COprice;
        bool hasCO = selectedItem.Item.VirtualCurrencyPrices.TryGetValue("CO", out COprice);
        if (hasCO && ClientSessionData.Instance.currencyCO >= COprice) {
            PurchaseSelectedItem("CO", COprice);
        }
    }

    public void BuyWithPC() {
        uint PCprice;
        bool hasPC = selectedItem.Item.VirtualCurrencyPrices.TryGetValue("PC", out PCprice);
        if (hasPC && ClientSessionData.Instance.currencyCO >= PCprice) {
            PurchaseSelectedItem("PC", PCprice);
        }
    }

    void PurchaseSelectedItem(string vc, uint price) {
        if (vc.Equals("CO")) {
            ClientSessionData.Instance.currencyCO -= (int)price;
        } else if (vc.Equals("PC")) {
            ClientSessionData.Instance.currencyPC -= (int)price;
        }

        PlayfabUtils.Instance.PurhaseItem(selectedItem.Item, vc,OnPurchased, error => { Debug.Log("error on purchase"); });
    }

    void OnPurchased(PurchaseItemResult res) {
        Debug.Log("ITEM BOUGHT (" + res.Items.Count + ") " + res.Items[0].DisplayName);

        Debug.Log("inventory count " + ClientSessionData.Instance.InventoryItems.Count);
        //updating UI
        labCO.text = ClientSessionData.Instance.currencyCO.ToString();
        labPC.text = ClientSessionData.Instance.currencyPC.ToString();
        
        //add to inventory
        FMInventoryItem inviItem = ClientSessionData.Instance.InventoryItems.Find(x => x.CatalogID.Equals(res.Items[0].ItemId));

        //if there is already an instance id, we just add 1
        if (inviItem != null && inviItem.IsStackable) {
            inviItem.Amount += 1;
            labOwned.text = inviItem.Amount.ToString();
            Debug.Log("inventory new count (stack)" + ClientSessionData.Instance.InventoryItems.Count);
            return;
        }

        //if the item is new or not stackable, we add it to the inventory
        if (inviItem == null) {
            CatalogItem cItem = ClientSessionData.Instance.CatalogItems.Find(x => x.ItemId.Equals(res.Items[0].ItemId));
            FMPlayFabInventory.AddInventoryItem(res.Items[0], ClientSessionData.Instance.InventoryItems, cItem);
            Debug.Log("inventory new count "+ ClientSessionData.Instance.InventoryItems.Count);
        }        
    }


    public void FilterByItem() {
        Filter("Item");
    }

    public void FilterByWeapon() {
        Filter("Weapon");
    }

    public void FilterByArmor() {
        Filter("Armor");
    }

    void Filter(string itemClass) {
        for (int i =0; i < itemGrid.GetComponentsInChildren<Transform>(true).Length;i++) {
            FMStoreItemUI item = itemGrid.GetComponentsInChildren<Transform>(true)[i].GetComponent<FMStoreItemUI>();
            if (item != null) {
                item.gameObject.SetActive(item.Item.ItemClass.Equals(itemClass));
            }            
        }
        itemGrid.Reposition();
    }

    public void BackToHome()
    {
        SceneManager.LoadScene("Home");
    }
}
