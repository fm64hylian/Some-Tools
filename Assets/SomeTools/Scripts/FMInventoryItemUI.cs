using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FMInventoryItemUI : MonoBehaviour
{
    [SerializeField]
    UILabel labitemName;
    [SerializeField]
    UISprite spriteItem;
    [SerializeField]
    GameObject equipedIcon;
    [SerializeField]
    UIToggle checkFavorite;
    [SerializeField]
    UILabel labAmount;
    public Action<FMInventoryItemUI> OnSelected;
    [HideInInspector]
    public bool isFavorite;
    public FMInventoryItem Item;
    bool isSelected;

    void Start()
    {
        equipedIcon.SetActive(false);
        checkFavorite.value = false;
    }

    public void SetData(FMInventoryItem iitem){
        Item = iitem;
        labitemName.text = iitem.DisplayName;
        if (!string.IsNullOrEmpty(iitem.SpriteName)){
            spriteItem.spriteName = iitem.SpriteName;
        }

        labAmount.text = iitem.IsStackable ? "x "+ iitem.Amount.ToString() : "";

        //if equipment, show E
        equipedIcon.SetActive(!iitem.IsItem() && iitem.IsEquipped);
    }

    public void OnCLicked() {
        if (OnSelected != null) {
            isSelected = true;
            OnSelected(this);
        }
    }

    public void UpdateIsFavorite() {
        isFavorite = checkFavorite.value;
        Item.IsFavorite = isFavorite;
    }

    public void Unselect() {
        isSelected = false;
    }

    void Update()
    {

    }
}
