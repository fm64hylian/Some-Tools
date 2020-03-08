using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public struct GridPosition
{
    public int Row; // X
    public float Height; // Y
    public int Col; // Z
}

/// <summary>
/// stageblock will be ground blocks and any grid-like(1x1 space) item for now
/// </summary>
public class StageBlock : MonoBehaviour
{
    public StageItemModel Model;
    public GridPosition GridPosition;
    public float YRotation = 0f;
    public bool IsSelected = false;
    //public List<GameObject> HeightList;
    //public int HeightBlockIndex = 0; //0 for 1x1, 1 for 0.5f, 2 for 0.25
    //public int DamageStateIndex = 0; //0 for no damage
    GameObject activeState;
    //Color tempColor;
    //Color currentMaterialColor;

    private void Start()
    {
        //currentMaterialColor = gameObject.transform.GetChild(0).GetChild(0).gameObject.GetComponentInChildren<MeshRenderer>().material.color;
    }

    public void SetGridPosition(Vector3 pos)
    {
        GridPosition.Row = (int)pos.x;
        GridPosition.Height = pos.y;
        GridPosition.Col = (int)pos.z;
    }

    public float GetCurrengBlockHeight()
    {
        return Model.ItemHeight;
    }

    public void DeleteItem()
    {
        if (gameObject != null)
        {
            Destroy(gameObject);
        }
    }

    public void Rotate()
    {
        Rotate(90f);
    }

    public void Rotate(float newRotation)
    {
        transform.eulerAngles += new Vector3(0, Mathf.Floor(newRotation), 0);
        YRotation = transform.eulerAngles.y;
        //UpdateOccupiedTiles();
    }

    public void HighLight()
    {
        //wrapper.GetActiveBlock().GetComponent<MeshRenderer>().material.color = Color.red;
    }

    public void UnHightlight()
    {
        //wrapper.GetActiveBlock().GetComponent<MeshRenderer>().material.color = currentMaterialColor;
    }


    public void SetNewHeightState(int newIndex)
    {
        /*
        if (!wrapper.IsSolidGround)
        {
            HeightBlockIndex = 0;
            return;
        }
        HeightBlockIndex = newIndex;
        wrapper.SetActiveBlock(HeightBlockIndex, 0, false);
        */
    }

    public int GetActiveDamageIndex()
    {
        /*
        if (!wrapper.IsSolidGround) //DamageStates == null
        {
            return 0;
        }

        for (int i = 0; i < wrapper.GetDamageList().Count; i++)
        {
            if (wrapper.GetDamageList()[i].gameObject.activeInHierarchy)
            {
                DamageStateIndex = i;
                return i;
            }
        }
        return DamageStateIndex;
        */
        return 0;
    }

    /// <summary>
    /// DamageStates are a set of different meshes inside the prefab
    /// which can be toggled to show different levels of damage
    /// </summary>
    //void InitializeBlockDamage()
    //{
    //    //activeState = wrapper.GetActiveBlock();
    //}

    //public List<GameObject> GetDamageStates()
    //{
    //    return null;
    //    //return wrapper.GetDamageList();
    //}

    public void CheckSlope(bool isShowing)
    {
        /*
        IsSlope = isShowing;
        wrapper.SetActiveBlock(HeightBlockIndex, DamageStateIndex, IsSlope);
        */
    }

    //public Mesh GetActiveMesh()
    //{
    //    return gameObject.transform.GetChild(0).GetChild(0).gameObject.GetComponentInChildren<MeshFilter>().mesh;
    //}
}