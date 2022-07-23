using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FMPlayerSlot : MonoBehaviour
{
    public FMInventoryObject CurrentInventoryItem;
    public FMInventorySlot Model;
    public EquipmentSlotsType SlotType;

    //public FMStoreEquipmentObject CurrentStoreItem;
    public bool IsPreview = false;

    public void EquipInventory(FMInventoryObject _item)
    {
        CurrentInventoryItem = _item;
        _item.transform.SetParent(transform);
        _item.transform.localPosition = Vector3.zero;
        _item.transform.localRotation = Quaternion.identity;
        Debug.Log("Equipando " + _item.Model.DisplayName + ", pos: " + transform.position);

        CurrentInventoryItem.Model.IsEquipped = true;
        //model
        Model.CurrentItem = CurrentInventoryItem.Model.InstanceID;
    }

    public void UnEquipInventory()
    {
        CurrentInventoryItem.Model.IsEquipped = false;
        CurrentInventoryItem = null;

        //model
        Model.CurrentItem = "None";
    }

    public bool IsEquipedInventory()
    {
        return CurrentInventoryItem != null;
    }


    ////////////STORE ITEMS


    //public void EquipStore(FMStoreEquipmentObject _item)
    //{
    //    CurrentStoreItem = _item;
    //    EquipmentSlotsType slotType = FMPlayFabInventory.GetSlotType(_item.ItemModel);


    //    //Equipando item desde la tienda (skin se agrega desde AvatarController, no de pool)
    //    if (!slotType.Equals(EquipmentSlotsType.Skin_Color) && slotType != EquipmentSlotsType.None)
    //    {
    //        _item.transform.SetParent(transform);
    //        _item.transform.localPosition = Vector3.zero;
    //        _item.transform.localRotation = Quaternion.identity;
    //        Debug.Log("Equipando store " + _item.gameObject.name + ", pos: " + transform.position);
    //        CurrentStoreItem.IsEquiped = true;
    //    }

    //    Model.UpdateCurrentItem(_item.ItemModel);
    //    IsPreview = !FMPlayFabInventory.IsItemOnInventory(_item.ItemModel);
    //}

    //public void UnEquipStore()
    //{
    //    if (CurrentStoreItem != null)
    //    {
    //        CurrentStoreItem.IsEquiped = false;
    //        CurrentStoreItem = null;
    //    }

    //    //model TODO vuando se desequipa catalog que no ha sido comprado aun, no dejar en None 
    //    //porque quita selección anterior
    //    Model.CurrentItem = SlotType.Equals(EquipmentSlotsType.Skin_Color) ?
    //        FMPlayFabInventory.GetDefaultSkinInstanceId() : "None";
    //}

    //public bool IsEquipedStore()
    //{
    //    return CurrentStoreItem != null;
    //}
}
