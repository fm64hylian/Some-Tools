using PlayFab;
using PlayFab.ClientModels;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using SimpleJSON;

public class FMPlayFabInventory : MonoBehaviour
{
    public static List<FMInventoryItem> Items = new List<FMInventoryItem>();
    public static JSONNode SlotJSON;

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

    /// <summary>
    /// when logging in if User does not have Equipment Json on playfab, it will be created
    /// </summary>
    /// <param name="result"></param>
    /// <param name="error"></param>
    public static void CreateUserEquipment(Action<ExecuteCloudScriptResult> result, Action<PlayFabError> error)
    {
        List<string> slotsStrings = new List<string>();
        int slots = System.Enum.GetValues(typeof(EquipmentSlotsType)).Length;
        for (int i = 1; i < slots; i++)
        {
            string slotType = System.Enum.GetValues(typeof(EquipmentSlotsType)).GetValue(i).ToString();
            slotsStrings.Add(slotType);

        }
        object args = new { slots = slotsStrings };
        for (int i = 0; i < slotsStrings.Count; i++)
        {
            Debug.Log("args " + i + ": " + slotsStrings[i]);
        }
        PlayfabUtils.Instance.ExecuteCloudscript("SetEquipmentSlots", args, result, error);
    }

    public static FMInventoryItem GetInventoryItemFromCatalogID(CatalogItem cItem)
    {
        return Items.Find(x => x.CatalogID.Equals(cItem.ItemId));
    }

    ///returns Item InstanceID from default skin
    public static string GetDefaultSkinInstanceId()
    {
        FMInventoryItem item = Items.Find(x => x.CatalogID.Equals("skin_0000"));
        return item.InstanceID;
    }

    public static bool IsItemOnInventory(CatalogItem cItem)
    {
        return Items.Find(x => x.CatalogID.Equals(cItem.ItemId)) != null;
    }

    public static void StoreSlotsFromJson(GetUserDataResult res)
    {
        JSONNode userEquipmentJSONData = JSON.Parse(res.Data["si_user_equipment"].Value);
        StoreSlotsFromJson(userEquipmentJSONData);
    }

    public static void StoreSlotsFromJson(ExecuteCloudScriptResult res)
    {
        if (JSON.Parse(res.FunctionResult.ToString())["status"] != null)
        {
            JSONNode userEquipmentJSONData = JSON.Parse(res.FunctionResult.ToString())["userEquipment"];
            StoreSlotsFromJson(userEquipmentJSONData);
        }
    }

    static void StoreSlotsFromJson(JSONNode json)
    {
        ClientSessionData.Instance.Slots.Clear();
        JSONNode userEquipmentJSONData = json;
        if (userEquipmentJSONData == null)
        {
            Debug.Log("Error on getting User Equipment");
        }

        List<FMInventorySlot> emptySlots = new List<FMInventorySlot>();
        for (int i = 0; i < userEquipmentJSONData.AsArray.Count; i++)
        {
            FMInventorySlot emptySlot = new FMInventorySlot();
            emptySlot.SlotType = GetSlotsName(userEquipmentJSONData[i]);

            // IfExpressionNotNullValue ?? IfExpressionNullValue   ==   IfExpression ? value1 : value2
            emptySlot.CurrentItem = userEquipmentJSONData[i][emptySlot.SlotType.ToString()].Value ?? "None";
            emptySlots.Add(emptySlot);
        }

        ClientSessionData.Instance.Slots = emptySlots;

        //Asignandole al JSON La informacion de Playfab para comprara si cambia despues
        SlotJSON = json;
    }

    /// <summary>
    /// return EquipmentSlotsType from Json object in Playfab { "SlotType": "etc" }
    /// </summary>
    /// <param name="userSlot"></param>
    /// <returns></returns>
    static EquipmentSlotsType GetSlotsName(JSONNode userSlot)
    {
        //there's only 1 KeyValuePair in the Object but whatever
        foreach (KeyValuePair<string, JSONNode> kvp in userSlot.AsObject)
        {
            //convert String(value from Object) to EquipmentSlotsType
            return (EquipmentSlotsType)System.Enum.Parse(typeof(EquipmentSlotsType), kvp.Key);
        }
        return EquipmentSlotsType.None;
    }
}
