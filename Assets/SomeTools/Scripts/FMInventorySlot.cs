using PlayFab.ClientModels;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EquipmentSlotsType
{
    None,
    //HeadGear
    Head_Hair,
    Head_UpperFace,
    Head_LowerFace,
    //UpperBody
    UBody_Clothes,
    UBody_HandL,
    UBody_HandR,
    Skin_Color
}

public class FMInventorySlot : MonoBehaviour
{

    public string CurrentItem = "None";
    public EquipmentSlotsType SlotType = EquipmentSlotsType.None;

    /// <summary>
    /// Esto se llama cuando se está equipando o comprando (No al asignarse a none y desequiparse)
    /// </summary>
    /// <param name="_item"></param>
    public void UpdateCurrentItem(CatalogItem _item)
    {
        FMInventoryItem invItem = FMPlayFabInventory.GetInventoryItemFromCatalogID(_item);

        //Comprobar si el item se encuentra en el inventario
        bool isInInventory = FMPlayFabInventory.IsItemOnInventory(_item);

        string instanceId = invItem != null ? invItem.InstanceID : "";
        //si parte con "Store", significa que no ha sido comprado aun
        CurrentItem = isInInventory ? instanceId : "Store_" + _item.ItemId;
        FMInventorySlot clientSlot = ClientSessionData.Instance.Slots.Find(x => x.SlotType.Equals(SlotType));
        clientSlot.CurrentItem = CurrentItem;
    }
}
