using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageMap : MonoBehaviour
{
    public List<StageBlock> groundBlocks = new List<StageBlock>();
    public List<StageObject> StageObjects = new List<StageObject>();
    //public List<StageDecoration> Decorations = new List<StageDecoration>();
    public List<StageObject> PlayerSpawns = new List<StageObject>();
    public List<StageObject> EnemySpawns = new List<StageObject>();
    //public List<InventoryItem> BattleItems = new List<InventoryItem>();
    public int SkyboxIndex = 0;

    public void AddStageBlock(StageBlock block)
    {
        groundBlocks.Add(block);
    }
    public void AddStageObject(StageObject obt)
    {
        StageObjects.Add(obt);

        if (obt.Model.JsonKey.Equals(StageItemModel.PlayerSpawn.JsonKey))
        {
            PlayerSpawns.Add(obt);
        }
        else if (obt.Model.JsonKey.Equals(StageItemModel.EnemySpawn.JsonKey))
        {
            EnemySpawns.Add(obt);
        }
        //else if (obt.Model.JsonKey.Equals(StageItemModel.Item.JsonKey))
        //{
        //    BattleItems.Add(obt.gameObject.GetComponent<InventoryItem>());
        //}
    }
    //public void AddStageDecoration(StageDecoration deco)
    //{
    //    Decorations.Add(deco);
    //}

    /// <summary>
    /// gets position from either Player or Enemy list
    /// </summary>
    /// <param name="spawnList"></param>
    /// <returns></returns>
    public List<Transform> GetSpawnPositions(List<StageObject> spawnList)
    {
        List<Transform> positions = new List<Transform>();
        //adding 1 from the spawming position
        for (int i = 0; i < spawnList.Count; i++)
        {
            Transform higherPosition = spawnList[i].transform;
            //Debug.Log(higherPosition.name + " new pos " + higherPosition.position);
            positions.Add(higherPosition);
        }
        //spawnList.ForEach(x => positions.Add(x.transform));
        return positions;
    }

    /// <summary>
    /// true for players, false for enemies
    /// </summary>
    /// <param name="isPlayer"></param>
    public void HideSpawnPoints(List<StageObject> spawnList)
    {
        spawnList.ForEach(x => x.gameObject.SetActive(false));
        //PlayerSpawns.ForEach(x => x.gameObject.SetActive(false));
    }

    public void MergeBlocks(GameObject blocks)
    {
        var cmc = gameObject.AddComponent<CustomMeshCombiner>();
        cmc.MergeObjects(blocks);

        //adding collider to new object
        blocks.AddComponent<MeshCollider>();
    }

    public void Clean()
    {
        if (groundBlocks.Count > 0)
        {
            for (int i = groundBlocks.Count - 1; i >= 0; i--)
            {
                StageBlock item = groundBlocks[i];
                groundBlocks.Remove(item);
                item.DeleteItem();
            }
        }
        //objects
        if (StageObjects.Count > 0)
        {
            for (int i = StageObjects.Count - 1; i >= 0; i--)
            {
                StageObject item = StageObjects[i];
                StageObjects.Remove(item);
                item.DeleteItem();
            }
        }
        //for (int i = Decorations.Count - 1; i >= 0; i--)
        //{
        //    StageDecoration item = Decorations[i];
        //    Decorations.Remove(item);
        //    item.DeleteItem();
        //}

        PlayerSpawns.Clear();
        EnemySpawns.Clear();
    }

    /// <summary>
    /// the positions on stagebuilder starts in 20x0x20 so this will reposition to 0,0,0 on battle scenes
    /// </summary>
    public void RepositionToInGame()
    {
        transform.position = (Vector3.back * 20) + (Vector3.left * 20) + Vector3.down + (Vector3.right * 0.5f);
    }
}
