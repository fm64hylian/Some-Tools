using PlayFab.ClientModels;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FMStoreItemUI : MonoBehaviour
{
    [SerializeField]
    UILabel itemName;
    [SerializeField]
    UISprite itemSprite;
    [SerializeField]
    UIButton COButton;
    [SerializeField]
    UIButton PCButton;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void SetData(CatalogItem item) {
        itemName.text = item.DisplayName;
        itemSprite.spriteName = item.ItemImageUrl;

        uint CO;
        uint PC;

        bool isCO = item.VirtualCurrencyPrices.TryGetValue("CO", out CO);
        bool isPC = item.VirtualCurrencyPrices.TryGetValue("PC", out PC);

        COButton.GetComponentInChildren<UILabel>().text = isCO ? CO.ToString() : "--";
        PCButton.GetComponentInChildren<UILabel>().text = isPC ? PC.ToString() : "--";

        //disable buttons if there is no currency
        if (!isCO) {
            COButton.isEnabled = false;
        }
        if (!isPC) {
            PCButton.isEnabled = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
