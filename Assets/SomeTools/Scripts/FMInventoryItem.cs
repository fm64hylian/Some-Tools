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

    public FMInventoryItem() {

    }

    public bool IsItem() {
        return ItemClass.Equals("Item");
    }
}
