using PlayFab.ClientModels;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;

public class FMPlayFabInventory : MonoBehaviour
{
    public static List<FMInventoryItem> Items = new List<FMInventoryItem>();

    public static void StoreItemsFromPlayfab(List<CatalogItem> catalogItems, List<ItemInstance> inventoryItems) {
        for (int i = 0; i < inventoryItems.Count; i++)
        {
            ItemInstance iitem = inventoryItems[i];
            CatalogItem cItem = catalogItems.Find(x => x.ItemId.Equals(iitem.ItemId));
            if (cItem != null)
            {                
                Items.Add(CreateInventoryItem(cItem, iitem));
            }
        }
    }

    static FMInventoryItem CreateInventoryItem(CatalogItem cItem, ItemInstance iItem) {
        FMInventoryItem invItem = new FMInventoryItem();
        invItem.DisplayName = cItem.DisplayName;
        invItem.CatalogID = cItem.ItemId;
        invItem.InstanceID = iItem.ItemInstanceId;
        invItem.ItemClass = cItem.ItemClass;
        invItem.Description = cItem.Description;
        invItem.SpriteName = cItem.ItemImageUrl;
        invItem.Amount = cItem.IsStackable ? (int)iItem.RemainingUses : 1;
        invItem.Tags = cItem.Tags;
        invItem.Prices = cItem.VirtualCurrencyPrices;
        invItem.IsStackable = cItem.IsStackable;

        if (!string.IsNullOrEmpty(cItem.CustomData))
        {
            //Debug.Log("item custom json " + cItem.CustomData);
            JSONNode jsonEffects = JSON.Parse(cItem.CustomData);
            Dictionary<string, string> dicEffect = new Dictionary<string, string>();
            dicEffect.Add(jsonEffects["effect"].Value, jsonEffects["value"]);
            invItem.Effects = dicEffect;
        }

        //custom item instance data
        string isFavKey = "false";
        string isEquipKey = "false";
        if (iItem.CustomData != null && iItem.CustomData.TryGetValue("is_favorite", out isFavKey)) //iItem.CustomData.ContainsKey("is_favorite")
        {
            invItem.IsFavorite = bool.Parse(isFavKey); //iItem.CustomData["is_favorite"]
            //Debug.Log(invItem.DisplayName+" has is favorite, "+ invItem.IsFavorite);
        }
        if (iItem.CustomData != null && iItem.CustomData.TryGetValue("is_equipped", out isEquipKey))
        {
            invItem.IsEquipped = bool.Parse(isEquipKey);
            //Debug.Log(invItem.DisplayName + " has is equipped, " + invItem.IsEquipped);
        }
        return invItem;
    }

    public static void AddInventoryItem(ItemInstance newItem, List<FMInventoryItem> invItems, CatalogItem catalogItem) {
        Debug.Log("inventory items count "+invItems.Count);
        //add to inventry
        FMInventoryItem iItem = invItems.Find(x => x.CatalogID.Equals(newItem.ItemInstanceId));

        //if the item was already in inventory
        if (iItem != null && catalogItem.IsStackable) {
            iItem.Amount += 1;
            return;
        }

        //else, we add it to the inventory
        if (iItem == null){
            invItems.Add(CreateInventoryItem(catalogItem, newItem));
            Debug.Log("added to inventory items");
        }        
    }
}
