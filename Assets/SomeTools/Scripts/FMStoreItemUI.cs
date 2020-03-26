using PlayFab.ClientModels;
using System;
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
    [HideInInspector]
    public CatalogItem Item;
    public Action<FMStoreItemUI> OnSelected;
    bool isSelected = false;

    public bool IsSelected {
        get {
            return isSelected;
        }
        set {
            if (value != isSelected)
            {
                Debug.Log("chaging color ");
                GetComponent<UISprite>().color = isSelected ? selectedColor : unSelectedColor;
                value = isSelected;                
            }
        }
    }

    Color32 selectedColor = new Color32(15,94,55,255);
    Color32 unSelectedColor; // 63, 65, 65

    uint CO;
    uint PC;
    bool hasCO;
    bool hasPC;

    // Start is called before the first frame update
    void Start()
    {
        unSelectedColor = GetComponent<UISprite>().color;
    }

    public void SetData(CatalogItem item) {
        Item = item;
        itemName.text = item.DisplayName;
        itemSprite.spriteName = item.ItemImageUrl;

        hasCO = item.VirtualCurrencyPrices.TryGetValue("CO", out CO);
        hasPC = item.VirtualCurrencyPrices.TryGetValue("PC", out PC);

        //COButton.GetComponentInChildren<UILabel>().text = isCO ? CO.ToString() : "--";
        //PCButton.GetComponentInChildren<UILabel>().text = isPC ? PC.ToString() : "--";

        ////disable buttons if there is no currency
        //if (!isCO) {
        //    COButton.isEnabled = false;
        //}
        //if (!isPC) {
        //    PCButton.isEnabled = false;
        //}
    }

    public void Unselect() {
        isSelected = false;
    }

    public void OnItemSelected() {
        if (OnSelected != null) {
            isSelected = true;
            OnSelected(this);
        }
    }
}
