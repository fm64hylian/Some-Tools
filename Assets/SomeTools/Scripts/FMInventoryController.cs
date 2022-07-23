using PlayFab;
using PlayFab.ClientModels;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using SimpleJSON;
using PlayFab.Json;

public class FMInventoryController : MonoBehaviour
{
    [SerializeField]
    GameObject header;
    [SerializeField]
    UIGrid grid;
    [SerializeField]
    UIPopupList sortList;
    [SerializeField]
    GameObject detailPanel;

    UILabel labCO;
    UILabel labPC;

    //detail panel UI
    UILabel labDetailName;
    UISprite detailItemSPrite;
    UILabel labDetailDescription;
    UILabel labDetailAmount;
    UILabel labDetailEffect;
    UILabel labDetailSellPrice;    
    UIToggle checkFavorite;
    UIToggle checkEquipped;

    FMInventoryItemUI selectedItem;

    void Start()
    {
        grid.sorting = UIGrid.Sorting.Custom;
        //header
        labCO = header.GetComponentsInChildren<UILabel>()[2];
        labPC = header.GetComponentsInChildren<UILabel>()[3];
        labCO.text = ClientSessionData.Instance.currencyCO.ToString();
        labPC.text = ClientSessionData.Instance.currencyPC.ToString();

        //detail Panel
        labDetailName = detailPanel.GetComponentsInChildren<UILabel>()[1];
        detailItemSPrite = detailPanel.GetComponentsInChildren<UISprite>()[8];
        labDetailDescription = detailPanel.GetComponentsInChildren<UILabel>()[2];
        labDetailAmount = detailPanel.GetComponentsInChildren<UILabel>()[3];
        labDetailEffect = detailPanel.GetComponentsInChildren<UILabel>()[5]; 
        labDetailSellPrice = detailPanel.GetComponentsInChildren<UILabel>()[7];
        checkFavorite = detailPanel.GetComponentsInChildren<UIToggle>()[0];
        checkEquipped = detailPanel.GetComponentsInChildren<UIToggle>()[1];
        HideDetailScreen();

        if (ClientSessionData.Instance.CatalogItems.Count == 0)
        {
            PlayFabClientAPI.GetCatalogItems(new GetCatalogItemsRequest(), catalogRes => {
                ClientSessionData.Instance.CatalogItems = catalogRes.Catalog;
                DisplayItems();
                return;
            }, error => { Debug.Log("error on get catalog info"); });
        }
        DisplayItems();
    }

    void DisplayItems() {
        List<FMInventoryItem> invItems = ClientSessionData.Instance.InventoryItems;
        for (int i = 0; i < invItems.Count; i++)
        {
            CatalogItem item = ClientSessionData.Instance.CatalogItems[i];
            GameObject itemPrefab = Instantiate(Resources.Load("Prefabs/FMInventoryItemUI")) as GameObject;
            FMInventoryItemUI itemUI = itemPrefab.GetComponent<FMInventoryItemUI>();

            itemUI.SetData(invItems[i]);
            itemUI.OnSelected = DisplaySelectedItemInfo;

            itemUI.gameObject.transform.parent = grid.transform;
            itemUI.gameObject.transform.localScale = Vector3.one;
        }

        grid.Reposition();
    }

    void DisplaySelectedItemInfo(FMInventoryItemUI item) {

        checkEquipped.gameObject.SetActive(item.Item.IsEquipment());
        labDetailName.text = item.Item.DisplayName;
        detailItemSPrite.spriteName = item.Item.SpriteName;
        labDetailDescription.text = item.Item.Description;
        labDetailAmount.text = "x "+ item.Item.Amount;

        if (item.Item.Effects.Count > 0) {
            foreach (KeyValuePair<string, string> kvp in item.Item.Effects) {
                labDetailEffect.text = kvp.Key + " " + kvp.Value;
            }
        } else {
            labDetailEffect.text = "--";
        }

        //check if there is CO first
        uint CO;
        if (!item.Item.Prices.TryGetValue("CO", out CO))
        {
            //if there is no price for CO, calculate with CP / 5 instead,  TODO  do this on server
            uint PC;
            item.Item.Prices.TryGetValue("PC", out PC);
            int sellPrice = Mathf.FloorToInt((int)PC / 5);
            labDetailSellPrice.text = sellPrice.ToString();
        }
        else {
            labDetailSellPrice.text = (Mathf.FloorToInt(CO / 2)).ToString();
        }
        checkFavorite.value = item.Item.IsFavorite;

        if (!item.Item.IsItem()) {
            checkEquipped.gameObject.SetActive(true);
            checkEquipped.value = item.Item.IsEquipped;
        }        

        selectedItem = item;
        DisplayDetailScreen();
    }

    public void HideDetailScreen() {
        if (selectedItem != null) {
            selectedItem.Unselect();
        }
        detailPanel.SetActive(false);
    }

    void DisplayDetailScreen() {
        detailPanel.SetActive(true);
    }

    public void SellItem() {
        if (selectedItem.Item.Amount > 0) {
            Debug.Log("trying to sell instance "+selectedItem.Item.InstanceID);           
            PlayfabUtils.Instance.SellItem(selectedItem.Item.InstanceID, "CO", OnSoldItems, OnError);
        }
    }

    //TODO put in a different result class
    void OnSoldItems(ExecuteCloudScriptResult result) {

        //for some weird reason, result is not retutning so we put the result in a log, wth
        Debug.Log("result "+PlayFabSimpleJson.SerializeObject(result));
        //JSONNode json = JSON.Parse(result.FunctionResult.ToString());
        JSONNode json = PlayFabSimpleJson.SerializeObject(result.FunctionResult);

        //if (json == null || json["status"].Equals("error")) {
        //    string msg = json["message"] != null ? json["message"].Value : "";
        //    Debug.Log("error selling the item, "+ msg);
        //}
        //ClientSessionData.Instance.currencyCO += json["sell_price"].AsInt;
        //labCO.text = ClientSessionData.Instance.currencyCO.ToString();

        string logResult = result.Logs.Count > 0 ? result.Logs[0].Message : "";

        if (string.IsNullOrEmpty(logResult)) {
            Debug.Log("no result");
            return;
        }

        if (logResult.Contains("success")) {
            string sellPrice = logResult.Split(' ')[1];
            Debug.Log("sell price string "+sellPrice);
            ClientSessionData.Instance.currencyCO += int.Parse(sellPrice);
            labCO.text = ClientSessionData.Instance.currencyCO.ToString();
        }

        //remove item if amount is 0
        selectedItem.Item.Amount -= 1;
        labDetailAmount.text = "x " + selectedItem.Item.Amount;

        if (selectedItem.Item.Amount <= 0) {
            string itemID = selectedItem.Item.InstanceID;
            ClientSessionData.Instance.InventoryItems.Remove(selectedItem.Item);

            //remove from grid            
            Transform gridItem = grid.GetChildList().Find(x => x.gameObject.GetComponent<FMInventoryItemUI>().Item.InstanceID.Equals(itemID));
            if (gridItem != null) {
                Destroy(gridItem.gameObject);
            }
            grid.Reposition();
            HideDetailScreen();
        }
    }

    void OnError(PlayFabError error) {
        Debug.Log("Error on selling item. "+error.ErrorMessage);
    }

    public void FilterByItem(bool checkValue)
    {
        Filter("Item", checkValue);
    }

    public void FilterByWeapon(bool checkValue)
    {
        Filter("Weapon", checkValue);
    }

    public void FilterByArmor(bool checkValue)
    {
        Filter("Armor", checkValue);
    }

    void Filter(string itemClass, bool check)
    {
        for (int i = 0; i < grid.GetComponentsInChildren<Transform>(true).Length; i++){
            FMInventoryItemUI item = grid.GetComponentsInChildren<Transform>(true)[i].GetComponent<FMInventoryItemUI>();
            if (item != null && item.Item.ItemClass.Equals(itemClass)){
                item.gameObject.SetActive(check);
            }
        }
        grid.Reposition();
    }

    public void SortGrid() {
        Debug.Log("sorting "+sortList.value);
        switch (sortList.value) {
            case "Name":
                grid.onCustomSort = SortByName;
                break;
            case "ID":
                grid.onCustomSort = SortByID;
                break;
            case "Type":
                grid.onCustomSort = SortByType;
                break;                
        }
        grid.Reposition();
    }

    int SortByName(Transform a, Transform b)
    {
        FMInventoryItem c1 = a.GetComponent<FMInventoryItemUI>().Item;
        FMInventoryItem c2 = b.GetComponent<FMInventoryItemUI>().Item;
        return string.Compare(c1.DisplayName, c2.DisplayName);
    }

    int SortByID(Transform a, Transform b)
    {
        FMInventoryItem c1 = a.GetComponent<FMInventoryItemUI>().Item;
        FMInventoryItem c2 = b.GetComponent<FMInventoryItemUI>().Item;
        return string.Compare(c1.CatalogID, c2.CatalogID);
    }

    int SortByType(Transform a, Transform b)
    {
        FMInventoryItem c1 = a.GetComponent<FMInventoryItemUI>().Item;
        FMInventoryItem c2 = b.GetComponent<FMInventoryItemUI>().Item;
        return string.Compare(c1.ItemClass, c2.ItemClass);
    }

    public void BackToHome()
    {
        List<FMInventoryItem> clientItems = ClientSessionData.Instance.InventoryItems;
        JSONArray json = new JSONArray();
        //send to server favorites and eqquiped items
        for (int i = 0; i < grid.GetComponentsInChildren<Transform>(true).Length; i++){
            FMInventoryItemUI item = grid.GetComponentsInChildren<Transform>(true)[i].GetComponent<FMInventoryItemUI>();
            if (item != null) {                
                item.Item.IsFavorite = item.isFavorite;
                JSONNode node = JSON.Parse("{}");
                node.Add("item", item.Item.InstanceID);
                node.Add("is_favorite", item.isFavorite);         
                //json[i]["item"] = item.Item.InstanceID;
                //json[i]["is_favorite"] = item.isFavorite;
                if (item.Item.IsEquipment())
                {
                    node.Add("is_equipped", item.Item.IsEquipped);
                    //json[i]["is_equipped"] = item.Item.IsEquipped;
                }
                //saving on client
                FMInventoryItem savedItem = ClientSessionData.Instance.InventoryItems.Find(x => x.InstanceID.Equals(item.Item.InstanceID));
                if (savedItem != null) {
                    savedItem = item.Item;
                }
                json[i] = node;
            }
        }

        //if there was no changes, just go back
        if (clientItems == ClientSessionData.Instance.InventoryItems) {
            SceneManager.LoadScene("Home");
            Debug.Log("no changes");
            return;
        }

        Debug.Log("el json "+ json.ToString());
        //calling playfab
        PlayfabUtils.Instance.UpdateInventoryEquip(json, 
            result => { Debug.Log("updated successfully"); }, error => { Debug.Log("error on update"); });

        SceneManager.LoadScene("Home");
    }
}
