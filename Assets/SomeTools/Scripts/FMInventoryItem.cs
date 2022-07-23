using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// crossed data between CatalogItems and ItemInstanceID (client only)
/// </summary>
public class FMInventoryItem
{
    public string DisplayName;
    public string CatalogID;
    public string InstanceID;
    public string ItemClass;
    public string Description;
    public string SpriteName;
    public int Amount;
    public List<string> Tags = new List<string>();
    public Dictionary<string, uint> Prices = new Dictionary<string, uint>();
    public Dictionary<string, string> Effects = new Dictionary<string, string>();
    public bool IsStackable;
    public bool IsFavorite;    
    public bool IsEquipped;
    //exclusive to equipment items
    public EquipmentSlotsType SlotType;

    public FMInventoryItem() {

    }

    public bool IsItem() {
        return ItemClass.Equals("Item");
    }

    public bool IsEquipment() {
        return Tags.Contains("equipment");
    }

    public void AssignEquipmentSlotType()
    {
        EquipmentSlotsType[] slots = (EquipmentSlotsType[])System.Enum.GetValues(typeof(EquipmentSlotsType));
        for (int i = 1; i < slots.Length; i++)
        {
            string slot = Tags.Find(x => x.Equals(slots[i].ToString()));
            if (!string.IsNullOrEmpty(slot))
            {
                SlotType = slots[i];
                return;
            }
        }

    }
}
