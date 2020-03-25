using PlayFab;
using PlayFab.ClientModels;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FMInventoryController : MonoBehaviour
{
    [SerializeField]
    GameObject header;
    [SerializeField]
    UIGrid grid;
    [SerializeField]
    GameObject detailPanel;

    UILabel labDetailName;
    UISprite detailItemSPrite;
    UILabel labDetailDescription;
    UILabel labDetailEffect;
    UILabel labDetailSellPrice;
    UIToggle checkFavorite;
    UIToggle checkEquipped;

    FMInventoryItemUI selectedItem;

    void Start()
    {
        //header
        UILabel labCO = header.GetComponentsInChildren<UILabel>()[2];
        UILabel labPC = header.GetComponentsInChildren<UILabel>()[3];
        labCO.text = ClientSessionData.Instance.currencyCO.ToString();
        labPC.text = ClientSessionData.Instance.currencyPC.ToString();

        //detail Panel
        labDetailName = detailPanel.GetComponentsInChildren<UILabel>()[1];
        detailItemSPrite = detailPanel.GetComponentsInChildren<UISprite>()[8];
        labDetailDescription = detailPanel.GetComponentsInChildren<UILabel>()[2];
        labDetailEffect = detailPanel.GetComponentsInChildren<UILabel>()[4]; 
        labDetailSellPrice = detailPanel.GetComponentsInChildren<UILabel>()[6];
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
        labDetailName.text = item.Item.DisplayName;
        detailItemSPrite.spriteName = item.Item.SpriteName;
        labDetailDescription.text = item.Item.Description;

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
        checkEquipped.value = item.Item.IsEquipped;

        selectedItem = item;

        DisplayDetailScreen();
    }

    void Update()
    {
        
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

    public void BackToHome()
    {
        SceneManager.LoadScene("Home");
    }
}
