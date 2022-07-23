using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FMInventoryObject : MonoBehaviour
{
    public FMInventoryItem Model;
    public void SetData(FMInventoryItem _model, Transform parent)
    {
        Model = _model;
        //GameObject itemMesh = Instantiate(Resources.Load("Prefabs/Items/" + _model.SpriteName)) as GameObject;
        //itemMesh.transform.SetParent(transform);
        //itemMesh.transform.localPosition = Vector3.zero;
        transform.SetParent(parent);
        transform.localPosition = Vector3.zero;

        //Debug.Log("En InventoryObject, en posicion: " + transform.position + "," + Model.DisplayName);         
    }
}
