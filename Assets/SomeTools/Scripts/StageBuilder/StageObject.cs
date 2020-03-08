using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageObjectState
{
    public int HP;
}
public class StageObject : MonoBehaviour
{
    public StageItemModel Model;
    public GridPosition GridPosition;
    public float YRotation = 0f;
    public bool IsSelected = false;
    public int DamageStateIndex; //0 for full damage(disappear), meshWrapper size for no damage
    GameObject activeState;
    StageObjectState state = new StageObjectState();
    //Color tempColor;
    //[SerializeField]
    //StageObjectMeshWrapper wrapper;
    Color currentMaterialColor;

    private void Start()
    {
        //wrapper = GetComponent<StageObjectMeshWrapper>();
        //DamageStateIndex = wrapper.GetDamageList().Count - 1;
        state.HP = DamageStateIndex;
        //TODO link to wrapper when we have the meshes
        currentMaterialColor = gameObject.transform.GetChild(0).gameObject.GetComponent<MeshRenderer>() == null ?
            currentMaterialColor = gameObject.transform.GetChild(0).GetChild(0).gameObject.GetComponent<MeshRenderer>().material.color
        :  gameObject.transform.GetChild(0).gameObject.GetComponent<MeshRenderer>().material.color;

        //Model is initialized on instances only. it won't be initialized on prefab previews
        if (Model != null)
        {
            InitializeBlockDamage();
        }        
    }

    public void SetPosition(Vector3 pos)
    {
        GridPosition.Row = (int)pos.x;
        GridPosition.Height = pos.y;
        GridPosition.Col = (int)pos.z;
    }

    public float GetCurrentItemHeight()
    {
        return Model != null ? Model.ItemHeight : 0;
    }

    public void DeleteItem()
    {
        if (gameObject != null)
        {
            Destroy(gameObject);
        }
    }

    public void AddRotation()
    {
        AddRotation(90f);
    }

    public void AddRotation(float newRotation)
    {
        transform.eulerAngles += new Vector3(0, Mathf.Floor(newRotation), 0);
        YRotation = transform.eulerAngles.y;
        //UpdateOccupiedTiles();
    }

    /// <summary>
    /// rotates to the given value
    /// </summary>
    public void Rotate(float newRotation)
    {
        transform.eulerAngles = new Vector3(0, Mathf.Floor(newRotation), 0);
        YRotation = transform.eulerAngles.y;
    }

    /// <summary>
    /// will rotate by 10 degrees to left or right
    /// </summary>
    /// <param name="isRight"></param>
    public void RotateFreely(bool isRight)
    {
        transform.eulerAngles += (isRight ? Vector3.up : Vector3.down) * 10;
        YRotation = transform.eulerAngles.y;
    }

    public void HighLight() {
        //if (wrapper.GetActiveBlock().GetComponent<MeshRenderer>()!= null) {
        //    wrapper.GetActiveBlock().GetComponent<MeshRenderer>().material.color = Color.yellow;
        //}
    }

    public void UnHightlight() {
        //if (wrapper.GetActiveBlock().GetComponent<MeshRenderer>() != null)
        //{
        //    wrapper.GetActiveBlock().GetComponent<MeshRenderer>().material.color = currentMaterialColor;
        //}
    }

    public void DamageObject(int damage = 1)
    {        
        if (state.HP == 0)
            return;

        state.HP -= damage;
        //wrapper.SetActiveBlock(state.HP);
    }


    public void SetNewDamageState(int newIndex)
    {
        //Debug.Log("wrapper null? "+ wrapper);
        //if (wrapper == null || newIndex == -1)
        //{            
        //    //DamageStateIndex = 0;
        //    return;
        //}
        //DamageStateIndex = newIndex;
        //wrapper.SetActiveBlock(DamageStateIndex);
    }


    //public int GetActiveDamageIndex()
    //{
    //    if (wrapper == null) //DamageStates == null
    //    {
    //        return 0;
    //    }

    //    for (int i = 0; i < wrapper.GetDamageList().Count; i++)
    //    {
    //        if (wrapper.GetDamageList()[i].gameObject.activeInHierarchy)
    //        {
    //            DamageStateIndex = i;
    //            return i;
    //        }
    //    }
    //    return DamageStateIndex;
    //}

    /// <summary>
    /// DamageStates are a set of different meshes inside the prefab
    /// which can be toggled to show different levels of damage
    /// </summary>
    void InitializeBlockDamage()
    {
        //activeState = wrapper.GetActiveBlock();
    }

    public List<GameObject> GetDamageStates()
    {
        return null;
        //return wrapper.GetDamageList();
    }
}
