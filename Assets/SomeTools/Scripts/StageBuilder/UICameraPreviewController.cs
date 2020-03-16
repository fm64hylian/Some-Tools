using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// this class moves the preview camera so the items can be seen in the preview menu
/// </summary>
public class UICameraPreviewController : MonoBehaviour
{
    public float spaceBetweenItems = 1.5f;
    int MAX_BLOCKS = StageItemModel.Blocks.Count;
    int MAX_OBJECTS = StageItemModel.Objects.Count;
    //int MAX_DECOS = StageItemModel.Decorations.Count;
    int currentBlockIndex = 0;
    int currentObjectIndex = 0;
    Vector3 startingPoint;
    Vector3 startingPointObject;
    //Vector3 startingPointDeco;
    GroundBlockType selectedBlockType;
    ObjectType selectedObject;
    //DecorationType selectedDecoration;
    StageBuildMode currentMode;

    void Start()
    {
        startingPoint = transform.localPosition;
        startingPointObject = transform.localPosition + Vector3.right * 4;
        //startingPointDeco = transform.localPosition + Vector3.right * 8;
        selectedBlockType = GroundBlockType.Grass;
        selectedObject = ObjectType.Wall;
        //selectedDecoration = DecorationType.Grass_Small;
    }

    public void MoveUp()
    {
        SetItemColumn();
        switch (currentMode)
        {
            case StageBuildMode.Stacking:
                transform.position += Vector3.up * spaceBetweenItems;
                currentBlockIndex++;
                if (currentBlockIndex >= MAX_BLOCKS)
                {
                    transform.localPosition = startingPoint;
                    currentBlockIndex = 0;
                }
                selectedBlockType = (GroundBlockType)currentBlockIndex;
                break;

            case StageBuildMode.FreePlacing:
                transform.position += Vector3.up * spaceBetweenItems;
                currentObjectIndex++;
                if (currentObjectIndex >= MAX_OBJECTS)
                {
                    transform.localPosition = startingPointObject;
                    currentObjectIndex = 0;
                }
                selectedObject = (ObjectType)currentObjectIndex;
                break;
        }
    }

    public void Movedown()
    {
        SetItemColumn();
        switch (currentMode)
        {
            case StageBuildMode.Stacking:
                currentBlockIndex--;
                transform.position += Vector3.down * spaceBetweenItems;
                if (currentBlockIndex < 0)
                {
                    transform.localPosition = startingPoint + (Vector3.up * (MAX_BLOCKS - 1) * spaceBetweenItems);
                    currentBlockIndex = MAX_BLOCKS - 1;
                }
                selectedBlockType = (GroundBlockType)currentBlockIndex;
                break;
            case StageBuildMode.FreePlacing:
                currentObjectIndex--;
                transform.position += Vector3.down * spaceBetweenItems;
                if (currentObjectIndex < 0)
                {
                    transform.localPosition = startingPointObject + (Vector3.up * (MAX_OBJECTS - 1) * spaceBetweenItems);
                    currentObjectIndex = MAX_OBJECTS - 1;
                }
                selectedObject = (ObjectType)currentObjectIndex;
                break;
        }
    }

    public GroundBlockType GetSelectedMouseBlock()
    {
        return selectedBlockType;
    }

    public ObjectType GetSelectedObject()
    {
        return selectedObject;
    }

    public int GetObjectIndex()
    {
        return currentObjectIndex;
    }

    //public DecorationType GetSelectedDecoration()
    //{
    //    return selectedDecoration;
    //}

    /// <summary>
    /// displays the block or decorations eletced at that monent
    /// </summary>
    /// <returns></returns>
    public string DisplaySelectedItemName()
    {
        switch (currentMode)
        {
            case StageBuildMode.Stacking:
                return selectedBlockType.ToString();
            case StageBuildMode.FreePlacing:
                return selectedObject.ToString();
            default:
                return selectedBlockType.ToString();
        }
    }

    public void SetCurrentMode(StageBuildMode mode)
    {
        currentMode = mode;
        SetItemColumn();
    }

    /// <summary>
    /// depending on the selection (decoration, stacking)
    /// it will position the camera at the blocks column or the decoration colum
    /// </summary>
    void SetItemColumn()
    {
        switch (currentMode)
        {
            case StageBuildMode.Stacking:
                transform.localPosition = new Vector3(startingPoint.x,
                 startingPoint.y + currentBlockIndex * spaceBetweenItems,
                 transform.localPosition.z);
                return;
            case StageBuildMode.FreePlacing:
                transform.localPosition = new Vector3(startingPointObject.x,
                  startingPoint.y + currentObjectIndex * spaceBetweenItems,
                  transform.localPosition.z);
                return;
        }
    }
}
