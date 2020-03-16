using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using SimpleJSON;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif

public enum StageBuildMode
{
    Stacking,
    FreePlacing,
    Decoration
}

public enum TouchMode
{
    Selecting,
    Drawing,
    TileDraw,
    Erasing,
    TileErase,
}

public class StageBuilder : MonoBehaviour
{
    public static int MAX_SIZE = 40;
    public static int MAX_HEIGHT = 20;
    public static int MAX_PLAYERS = 8;
    public static string STAGE_PATH = "Assets/SomeTools/Resources/Stages/";

    [SerializeField]
    UIMenuController menuController;
    [SerializeField]
    SelectionController selectController;
    [SerializeField]
    GameObject prefabPreview;
    [SerializeField]
    UICameraPreviewController cameraPreviewController;
    [SerializeField]
    GameObject tileSelector;
    static StageBuilder instance;

    GroundBlockType selectedBlockType;
    ObjectType selectedObjectType;
    //DecorationType selectedDecoType;
    StageBuildMode Mode = StageBuildMode.Stacking;
    TouchMode touchMode = TouchMode.Selecting;
    StageObject SelectedMouseObject;
    int mousedownHeight = 0;
    bool mouseHold = false;

    List<StageBlock> blocks = new List<StageBlock>();
    List<StageObject> objects = new List<StageObject>();
    //List<StageDecoration> decorations = new List<StageDecoration>();
    int spawnCount = 0;
    //string STAGE_PATH = "Assets/BOUNCING_STRIKE_STAGEBUILDER/Resources/Stages/";
    bool isAligned = true;

    KeyCode addKey = KeyCode.Z;
    KeyCode deleteKey = KeyCode.X;
    KeyCode clearKey = KeyCode.C;
    KeyCode selectKey = KeyCode.S;

    StageAction[] PerformedActions = new StageAction[2];
    Vector3 clickedDownPos;
    float slowButonsPressed = 3;// "slow motion"
    float presedShiftInTime = 0; // Counter time for pressed button
    int maxCubeRange = 3;
    bool isFilling = false;

    public static StageBuilder Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject obj = new GameObject("StageBuilder");
                instance = obj.AddComponent<StageBuilder>();
            }

            return instance;
        }
        set
        {
            instance = value;
        }
    }

    void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (menuController == null || selectController == null || prefabPreview == null)
        {
            enabled = false;
            return;
        }

        LoadPreviewBlocks();
        LoadPreviewObjects();
        UpdateSelectedItemType();
        //LoadPreviewItems();

        InitializeTileSelector();
    }

    private void Update()
    {
        if (selectController.transform.position.y == 0)
        {
            selectController.UpdateHeight();
        }

        //toggle mode hotkey
        if (Input.GetKeyDown(KeyCode.M))
        {
            ToggleMode();
            return;
        }

        //checking if mouse is not being pressed anymore, for tile draw/eraser and slowing normal eraser down
        if (Input.GetMouseButtonUp(0))
        {
            mouseHold = false;
            presedShiftInTime = 0;

            //Filling blocks if selected
            if ((touchMode == TouchMode.TileDraw || touchMode == TouchMode.TileErase) && isFilling == true) // && !CheckUIClick()
            {
                Vector3 clickedpoint = GetRaycastFromClick().point;
                TileBlocks(clickedDownPos, new Vector3(Mathf.RoundToInt(clickedpoint.x),
                    clickedDownPos.y, Mathf.RoundToInt(clickedpoint.z)), touchMode == TouchMode.TileDraw);
                ResetTileSelector();
                isFilling = false;
            }
        }

        //clear all
        if (Input.GetKeyDown(clearKey))
        {
            menuController.DisplayClearGridMenu();
            return;
        }

        switch (Mode)
        {
            //used for ground blocks 1x1 (fixed position using cube selector)
            case StageBuildMode.Stacking:

                //add blocks
                if (selectController.IsContinuous ? Input.GetKey(addKey) : Input.GetKeyDown(addKey))
                {
                    ToggleDrawingMode();
                    UpdateSelectedItemType();
                    selectController.GetActiveCubes().ForEach(x => AddGroundBlock(x, selectedBlockType));
                    AddGroundBlock(selectController.transform.position, selectedBlockType);
                    selectController.UpdateHeight();
                    return;
                }

                //remove blocks
                if (selectController.IsContinuous ? Input.GetKey(deleteKey) : Input.GetKeyDown(deleteKey))
                {
                    ToggleEraseMode();
                    menuController.UnselelectItem();
                    selectController.GetActiveCubes().ForEach(x => RemoveGroundBlock(x));
                    selectController.UpdateHeight();
                    return;
                }

                //drawing/painting/erasing with mouse hold
                if (Input.GetMouseButton(0))
                {
                    RaycastHit hitInfo = GetRaycastFromClick();
                    //anchoring height until mouse is released
                    if (!mouseHold)
                    {
                        mouseHold = true;
                        //var hittedObject = hitInfo.transform.gameObject
                        if (hitInfo.transform != null && hitInfo.transform.gameObject.GetComponent<StageBlock>() != null) //&& !CheckUIClick()
                        {
                            var hittedObject = hitInfo.transform.gameObject;
                            mousedownHeight = Mathf.RoundToInt(hittedObject.transform.position.y);
                        }

                        mousedownHeight = Mathf.RoundToInt(hitInfo.point.y);
                        clickedDownPos = new Vector3(Mathf.RoundToInt(hitInfo.point.x),
                    mousedownHeight, Mathf.RoundToInt(hitInfo.point.z));
                        //repositioning tileSelector, it will be at the same height if tileDrawing, -1 height if it's erasing
                        int hei = touchMode == TouchMode.TileDraw || mousedownHeight > 0f ? -1 : 0;
                        tileSelector.transform.position = clickedDownPos - (Vector3.left * 0.5f) -
                            (Vector3.back * 0.5f) + (Vector3.up * hei);
                    }

                    Vector3 roundedPosition = new Vector3(Mathf.RoundToInt(hitInfo.point.x),
                    mousedownHeight, Mathf.RoundToInt(hitInfo.point.z));
                    List<Vector3> placePositions = GetPositionsAroundClick(roundedPosition);
                    switch (touchMode)
                    {
                        case TouchMode.Drawing:
                            if (!CheckUIClick() && !CheckLowerBlock(roundedPosition))
                            {
                                UpdateSelectedItemType();
                                placePositions.ForEach(x => AddGroundBlock(x, selectedBlockType));
                                //AddGroundBlock(roundedPosition, selectedBlockType);
                            }
                            break;

                        case TouchMode.TileDraw:
                        case TouchMode.TileErase:
                            isFilling = true;
                            tileSelector.SetActive(true);
                            ClampTileClick(roundedPosition);

                            StageBlock selectBlock = GetBlockInPosition(tileSelector.transform.position);
                            if (touchMode == TouchMode.TileErase && selectBlock != null)
                            {
                                Debug.Log("selected block at" + tileSelector.transform.position + " " + selectBlock);
                                //selectBlock.HighLight();
                            }
                            break;

                        case TouchMode.Erasing:
                            if (!CheckUIClick() && CheckCointinuousPress())
                            {
                                placePositions = GetPositionsAroundClick(roundedPosition);
                                placePositions.ForEach(x => RemoveStageBlockFromClick(GetBlockInPosition(x)));
                                //selectController.UpdateHeight();
                            }
                            break;
                    }
                    return;
                }

                //mouse selection (used for block rotarion, damage, etc)
                if (Input.GetMouseButtonDown(0))
                {
                    //touchMode = TouchMode.Selecting;
                    ToggleItemMenuFromClick();
                }
                return;

            //used for objects in different sizes (fixed and freehhand placement using both mouse and cube selector)
            case StageBuildMode.FreePlacing:
                UpdateSelectedItemType();
                //add objects on selector position
                if (Input.GetKeyDown(addKey))
                {
                    ToggleDrawingMode();
                    AddStageObject(selectController.transform.position, selectedObjectType);
                    selectController.UpdateHeight();
                    return;
                }

                //remove object on selector position
                if (Input.GetKeyDown(deleteKey))
                {
                    ToggleEraseMode();
                    RemoveStageObjectFromClick(selectController.GetHighestObject());
                    selectController.UpdateHeight();
                    return;
                }

                //mouse input will change depending on the Touchmode selected from buttons/keys
                if (Input.GetMouseButtonDown(0))
                {
                    switch (touchMode)
                    {
                        case TouchMode.Selecting:
                            ToggleItemMenuFromClick();
                            break;

                        case TouchMode.Drawing:
                            UpdateSelectedItemType();

                            RaycastHit clickedObject = GetRaycastFromClick();
                            if (clickedObject.transform && clickedObject.transform.gameObject.GetComponent<StageBlock>() != null)
                            {
                                //AddDecoration(hitInfo.point, selectedDecoType); //hitInfo.transform.position
                                AddStageObject(clickedObject.point, selectedObjectType);
                                selectController.UpdateHeight();
                            }
                            break;
                        case TouchMode.Erasing:
                            RemoveStageObjectFromClick(GetObjectFromClick());
                            selectController.UpdateHeight();
                            break;
                    }
                }

                //select mode for objects
                if (Input.GetKeyDown(selectKey))
                {
                    ToggleSelectMode();
                }
                break;
        }
    }

    /// <summary>
    /// will generate decoration (tiny grass, pebbles, etc thuought the stage)
    /// </summary>
    //public void GenerateRandomDeco()
    //{
    //    ClearDecorations();

    //    for (int i = 0; i < MAX_SIZE; i++)
    //    {
    //        for (int j = 0; j < MAX_SIZE; j++)
    //        {
    //            //up top 3 decorations can be instantiated on each tile
    //            for (int k = 0; k < 3; k++)
    //            {
    //                StageBlock block = GetHighestBlockOnTile(i, j);
    //                float randomPercent = Random.Range(1, 20);

    //                if (block != null && randomPercent == 5)
    //                {
    //                    float randomX = Random.Range(0f, 0.4f) + i;
    //                    float randomy = Random.Range(0f, 0.4f) + j;
    //                    int randomDeco = Random.Range(0, 5);

    //                    //testing greater density for grass than other elements
    //                    float randonDensity = Random.value;
    //                    randomDeco = randonDensity <= .85f ? Random.Range(0, 2) : Random.Range(3, 4); //no bush
    //                    //

    //                    DecorationType decoType = StageItemModel.Decorations[randomDeco].DecoType;
    //                    AddDecoration(new Vector3(randomX, block.GridPosition.Height + 1f, randomy), decoType);
    //                }
    //            }
    //        }
    //    }
    //}

    ////////////////////--- UI---//////////////////////


    /// <summary>
    /// will get the adjacent positions from mouse click accoring to the Selector's range option (1x1, 2x2, 3x3, etc) 
    /// </summary>
    /// <returns></returns>
    List<Vector3> GetPositionsAroundClick(Vector3 clickPosition)
    {
        //0,0  0,1  0,2 
        //1,0  1,1  1,2
        //2,0  2,1  2,2
        //these are fixed positions around the main cube selector
        List<Vector3> positions = new List<Vector3>() {
            new Vector3(clickPosition.x+1f, clickPosition.y, clickPosition.z+1f),
            new Vector3(clickPosition.x, clickPosition.y, clickPosition.z+1f),
            clickPosition,
            new Vector3(clickPosition.x+1f, clickPosition.y, clickPosition.z),
            new Vector3(clickPosition.x, clickPosition.y, clickPosition.z+2f),
            new Vector3(clickPosition.x+1f, clickPosition.y, clickPosition.z+2f),
            new Vector3(clickPosition.x+2f, clickPosition.y, clickPosition.z+2f),
            new Vector3(clickPosition.x+2f, clickPosition.y, clickPosition.z+1f),
            new Vector3(clickPosition.x+2f, clickPosition.y, clickPosition.z),
        };

        int totalRange = selectController.CubeRange * selectController.CubeRange;
        return totalRange == 1 ? new List<Vector3>() { clickPosition } : positions.GetRange(0, totalRange);
    }

    /// <summary>
    /// destroys all objebts from the list
    /// </summary>
    /// <param name="toBeDeleted"></param>
    void CleanList(List<Transform> toBeDeleted)
    {
        for (int i = toBeDeleted.Count - 1; i >= 0; i--)
        {
            Transform delte = toBeDeleted[i];
            toBeDeleted.Remove(delte);
            Destroy(delte.gameObject);
        }
    }


    /// <summary>
    /// sets drawing mode from UI button
    /// </summary>
    public void ToggleDrawingMode()
    {
        touchMode = TouchMode.Drawing;
        menuController.HighlightPressedButtonFromIndex((int)touchMode);
    }

    /// <summary>
    /// sets drawing mode from UI button
    /// </summary>
    public void ToggleSelectMode()
    {
        touchMode = TouchMode.Selecting;
        menuController.HighlightPressedButtonFromIndex((int)touchMode);
    }

    /// <summary>
    /// sets tile drawing mode from UI button
    /// </summary>
    public void ToggleTileDrawMode()
    {
        touchMode = TouchMode.TileDraw;
        menuController.HighlightPressedButtonFromIndex((int)touchMode);
        SetTileSelectorColor(true);

    }
    /// <summary>
    /// sets tile eraser mode from UI button
    /// </summary>
    public void ToggleTileEraseMode()
    {
        touchMode = TouchMode.TileErase;
        menuController.HighlightPressedButtonFromIndex((int)touchMode);
        SetTileSelectorColor(false);
    }

    /// <summary>
    /// sets drawing mode from UI button
    /// </summary>
    public void ToggleEraseMode()
    {
        touchMode = TouchMode.Erasing;
        menuController.HighlightPressedButtonFromIndex((int)touchMode);
    }

    /// <summary>
    /// gives the RaycastHit from click, with infromation about the StageItem and position
    /// </summary>
    /// <returns></returns>
    RaycastHit GetRaycastFromClick()
    {
        RaycastHit hitInfo;
        Ray rayMainCamera = Camera.main.ScreenPointToRay(Input.mousePosition);
        int everythingbutUIMask = (1 >> LayerMask.NameToLayer("UI"));

        Physics.Raycast(rayMainCamera, out hitInfo, 100f, everythingbutUIMask);
        Debug.Log("pos "+Camera.main.ScreenToWorldPoint(Input.mousePosition) +" hit object "+hitInfo.transform);
        return hitInfo;

        /*
         *         Ray rayCastUI = UICamera.currentCamera.ScreenPointToRay(Input.mousePosition);
         RaycastHit hitUI;
         int uiMask = (1 << LayerMask.NameToLayer("UI"));
         return Physics.Raycast(rayCastUI, out hitUI, 100f, uiMask);
         */
    }

    bool CheckCointinuousPress()
    {
        presedShiftInTime += Time.deltaTime * 10;
        bool iscontinuous = presedShiftInTime > slowButonsPressed;
        presedShiftInTime = presedShiftInTime > slowButonsPressed ? 1.4f : presedShiftInTime;
        return iscontinuous;
    }

    //public void ToggleDecorations()
    //{
    //    switch (menuController.GetSelectedDeco())
    //    {
    //        case 0:
    //            GenerateRandomDeco();
    //            menuController.SetSelectedDecoFromButton();
    //            break;
    //        case 1:
    //            ClearDecorations();
    //            menuController.SetSelectedDecoFromButton();
    //            break;
    //    }
    //}

    void InitializeTileSelector()
    {
        tileSelector.SetActive(false);
        //tileSelector.GetComponentInChildren<MeshRenderer>().material = selectController.GetSelectShaders()[1];
    }

    void ResetTileSelector()
    {
        tileSelector.transform.localScale = new Vector3(1f, 1f, 1f); //0.1f 
        tileSelector.transform.position = Vector3.zero;
        tileSelector.SetActive(false);
    }

    void ClampTileClick(Vector3 clickedPos)
    {
        int signedRow = (tileSelector.transform.position.x - clickedPos.x) > 0 ? 1 : -1;
        int signedCol = (tileSelector.transform.position.z - clickedPos.z) > 0 ? 1 : -1;
        tileSelector.transform.localScale =
        new Vector3(tileSelector.transform.position.x - clickedPos.x
        , 1, tileSelector.transform.position.z - clickedPos.z) //0
        + (Vector3.left * 0.5f * signedRow) + (Vector3.back * 0.5f * signedCol);

        float clampRow = Mathf.Clamp(tileSelector.transform.position.x, 0, MAX_SIZE);
        float clampCol = Mathf.Clamp(tileSelector.transform.position.z, 0, MAX_SIZE);

        tileSelector.transform.position = new Vector3(clampRow, tileSelector.transform.position.y, clampCol);
    }

    public void SetTileSelectorColor(bool isDraw)
    {
        //int index = isDraw ? 1 : 2;
        //tileSelector.GetComponentInChildren<MeshRenderer>().material = selectController.GetSelectShaders()[index];
    }


    void UpdateSelectedItemFromClick(StageObject newSelection)
    {
        if (SelectedMouseObject != newSelection)
        {
            menuController.UnselelectItem();
            SelectedMouseObject = newSelection;
        }
        menuController.SelectItem(SelectedMouseObject);
    }

    /// <summary>
    /// used as a wrapper to call from inspector
    /// </summary>
    public void ClearGridFromButton()
    {
        UpdateSelectedItemType();
        ClearGrid();
        selectController.UpdateHeight();
        menuController.HideClearGriMenu();
    }

    void UpdateSelectedItemType()
    {
        switch (Mode)
        {
            case StageBuildMode.Stacking:
                selectedBlockType = selectController.GetSelectedPreviewBlock();
                break;
            case StageBuildMode.FreePlacing:
                selectedObjectType = selectController.GetSelectedPreviewObject();
                break;
        }
    }

    void LoadPreviewBlocks()
    {
        float spaceBetween = cameraPreviewController.spaceBetweenItems;

        for (int i = 0; i < StageItemModel.Blocks.Count; i++)
        {
            StageItemModel itemModel = StageItemModel.GetModelFromKey(StageItemModel.Blocks[i].JsonKey);
            GameObject prefabBlock = Resources.Load(itemModel.PrefabPath) as GameObject;
            StageBlock item = prefabBlock.GetComponent<StageBlock>();
            item.Model = itemModel;

            //setting position
            Vector3 previewPos = prefabPreview.transform.position;
            Vector3 indexPos = new Vector3(previewPos.x, previewPos.y + (i * spaceBetween), 3.5f);

            GameObject BlockInstance = Instantiate(prefabBlock, indexPos, Quaternion.identity);

            //item.SetBlockSizeData();
            //scale to fit in preview screen
            BlockInstance.transform.localScale = new Vector3(
                itemModel.PreviewScale, itemModel.PreviewScale, itemModel.PreviewScale);
            BlockInstance.transform.SetParent(prefabPreview.transform);
            SetAllChildrenLayer(BlockInstance.transform, "previewItems");
            cameraPreviewController.SetCurrentMode(Mode);
        }
    }

    void LoadPreviewObjects()
    {
        float spaceBetween = cameraPreviewController.spaceBetweenItems;

        for (int i = 0; i < StageItemModel.Objects.Count; i++)
        {
            StageItemModel itemModel = StageItemModel.GetModelFromKey(StageItemModel.Objects[i].JsonKey);
            GameObject prefabObject = Resources.Load(itemModel.PrefabPath) as GameObject;

            //setting position TODO link 4f with variable in cameraPreviewController
            Vector3 previewPos = prefabPreview.transform.position;
            Vector3 indexPos = new Vector3(previewPos.x + 4f + (itemModel.PreviewScale * itemModel.PreviewOffSet), //offset
                previewPos.y + (i * spaceBetween),  // +0.5f
                3.5f);

            GameObject ObtInstance = Instantiate(prefabObject, indexPos, Quaternion.identity);
            ObtInstance.transform.localScale = new Vector3(
                itemModel.PreviewScale, itemModel.PreviewScale, itemModel.PreviewScale);
            ObtInstance.transform.SetParent(prefabPreview.transform);
            ObtInstance.gameObject.layer = LayerMask.NameToLayer("previewItems");
            SetAllChildrenLayer(ObtInstance.transform, "previewItems");
        }

        ///TEST LOADING ITEMS
        //    for (int i = 0; i < StageItemModel.Objects.Count; i++)
        //    {
        //        GameObject ObtInstance;

        //            StageItemModel itemModel = StageItemModel.GetModelFromKey(StageItemModel.Objects[i].JsonKey);
        //            GameObject prefabObject = Resources.Load(itemModel.PrefabPath) as GameObject;

        //            //setting position TODO link 4f with variable in cameraPreviewController
        //            Vector3 previewPos = prefabPreview.transform.position;
        //            Vector3 indexPos = new Vector3(previewPos.x + 4f + (itemModel.PreviewScale * itemModel.PreviewOffSet), //offset
        //                previewPos.y + (i * spaceBetween),  // +0.5f
        //                3.5f);

        //            ObtInstance = Instantiate(prefabObject, indexPos, Quaternion.identity);
        //            ObtInstance.transform.localScale = new Vector3(
        //itemModel.PreviewScale, itemModel.PreviewScale, itemModel.PreviewScale);

        //        ObtInstance.transform.SetParent(prefabPreview.transform);
        //        ObtInstance.gameObject.layer = LayerMask.NameToLayer("previewItems");
        //        SetAllChildrenLayer(ObtInstance.transform, "previewItems");
        //    }
    }

    void SetAllChildrenLayer(Transform trans, string name)
    {
        foreach (Transform child in trans)
        {
            child.gameObject.layer = LayerMask.NameToLayer(name);
            SetAllChildrenLayer(child, name);
        }
    }

    void ToggleItemMenuFromClick()
    {
        ToggleSelectMode();
        UpdateSelectedItemType();
        if (CheckUIClick())
        {
            Debug.Log("clicked UI");
            return;
        }

        StageObject so = GetObjectFromClick();
        if (so != null)
        {
            menuController.DisplayTileMenu();
            return;
        }
        menuController.HideTileMenu();
    }

    StageObject GetObjectFromClick()
    {
        UpdateSelectedItemType();
        if (CheckUIClick())
        {
            return null;
        }

        RaycastHit hitInfo = GetRaycastFromClick();
        //Ray rayMainCamera = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (hitInfo.transform != null) //Physics.Raycast(rayMainCamera, out hitInfo)
        {
            var hittedObject = hitInfo.transform.gameObject;
        Debug.Log("hit info object " + hittedObject);
        if (hittedObject.GetComponent<StageObject>() != null)
        {            
            StageObject sim = hittedObject.GetComponent<StageObject>();
            Debug.Log("hitted object  " + sim.name);
            UpdateSelectedItemFromClick(sim);
            return sim;
        }
        }
        return null;
    }

    /// <summary>
    /// true if click is touching UI and not objects
    /// </summary>
    /// <returns></returns>
    bool CheckUIClick()
    {
        Ray rayCastUI = UICamera.currentCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitUI;
        int uiMask = (1 << LayerMask.NameToLayer("UI"));
        return Physics.Raycast(rayCastUI, out hitUI, 100f, uiMask);
    }

    public void UpdateMode()
    {
        Mode = menuController.GetSelectedMode();
        Debug.Log("seleced mode "+Mode);
        cameraPreviewController.SetCurrentMode(Mode);
    }

    public void ToggleMode()
    {
    Mode = Mode == StageBuildMode.FreePlacing? StageBuildMode.Stacking : StageBuildMode.FreePlacing;
    menuController.SetSelectedModeFromButton();
    UpdateMode();
    }

    /// <summary>
    /// TESTING UNDO comming soon
    /// </summary>
    /// <param name="actionType"></param>
    /// <param name="afectedPositions"></param>
    void UpdateActions(StageActionType actionType, List<Vector3> afectedPositions)
    {
        StageAction latestAction = new StageAction(actionType, afectedPositions);
        if (PerformedActions[0] == null)
        {
            PerformedActions[0] = latestAction;
            return;
        }

        Queue<StageAction> actionqueue = new Queue<StageAction>();
        actionqueue.Enqueue(latestAction);
        if (actionqueue.Count > 2)
        {
            actionqueue.Dequeue();
        }
    }

    ////////////////////--- BLOCK CRUD---//////////////////////

    StageBlock AddGroundBlock(Vector3 placePosition, GroundBlockType block)
    {
        //for blocks, the position is always rounded
        placePosition = new Vector3((int)placePosition.x, placePosition.y, (int)placePosition.z);

        //check if position is occupied
        if (CheckSameBlockPosition(placePosition)) //IsTileOccupied(placePosition)
        {
            return null;
        }

        //check if highest position wont surpass the height limit
        if (CheckHighestPlacinPosition(placePosition))
        {
            return null;
        }
        //check out of bounds (from click)
        if (CheckOutOfBounds(placePosition))
        {
            return null;
        }

        //remove any decorations in that position
        //RemoveDecorationsFromtile(placePosition);

        StageItemModel itemModel = StageItemModel.GetModelFromKey(block.ToString().ToLower());
        GameObject prefabBlock = Resources.Load(itemModel.PrefabPath) as GameObject;
        GameObject BlockInstance = Instantiate(prefabBlock, placePosition, Quaternion.identity) as GameObject;//stackedPosition

        StageBlock item = BlockInstance.GetComponent<StageBlock>();
        item.Model = itemModel;
        item.SetGridPosition(placePosition);

        BlockInstance.name = itemModel.JsonKey + "_" + item.GridPosition.Row + "_" +
            item.GridPosition.Height.ToString("F1") + "_" + item.GridPosition.Col;

        blocks.Add(item);
        return item;
    }

    /// <summary>
    /// will apply tile rawing or tile erase to the designated area, true if it's drawing
    /// </summary>
    /// <param name="pos1"></param>
    /// <param name="pos2"></param>
    /// <param name="isDrawing"></param>
    void TileBlocks(Vector3 pos1, Vector3 pos2, bool isDrawing)
    {
        UpdateSelectedItemType();
        Vector3 diff = pos1 - pos2;
        int startPosRow = (int)pos1.x;
        int startPosCol = (int)pos1.z;

        int fillRow = (int)Mathf.Abs(diff.x);
        int fillCol = (int)Mathf.Abs(diff.z);
        int rowSigned = diff.x >= 0 ? -1 : 1;
        int colSigned = diff.z >= 0 ? -1 : 1;

        for (int i = 0; i < fillRow; i++) //row
        {
            for (int j = 0; j < fillCol; j++) //col
            {
                if (isDrawing)
                {
                    AddGroundBlock(new Vector3(startPosRow + (i * rowSigned), pos1.y
                        , startPosCol + (j * colSigned)), selectedBlockType);
                }
                else
                {
                    RemoveGroundBlock(new Vector3(startPosRow + (i * rowSigned), pos1.y + 1
                        , startPosCol + (j * colSigned)));
                }
            }
        }
    }

    StageBlock GetBlockFromClick()
    {
        UpdateSelectedItemType();
        if (CheckUIClick())
        {
            return null;
        }

        RaycastHit hitInfo = GetRaycastFromClick();
        //Ray rayMainCamera = Camera.main.ScreenPointToRay(Input.mousePosition);

        //if (Physics.Raycast(rayMainCamera, out hitInfo))
        //{
        var hittedObject = hitInfo.transform.gameObject;
        if (hittedObject != null && hittedObject.GetComponent<StageBlock>() != null)
        {
            StageBlock sbo = hittedObject.GetComponent<StageBlock>();
            //SelectNewItemFromClick(sim);
            return sbo;
        }
        //}
        return null;
    }

    /// <summary>
    /// checks if the block to place is a Player spawn, only a certain amount can be placed
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    bool CheckSpawnNumber(ObjectType item)
    {
        return item.Equals(ObjectType.Player_Spawn) && spawnCount > MAX_PLAYERS;
    }

    /// <summary>
    /// checks if the block surpassed the grid bounds, true if out of bounds
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    bool CheckOutOfBounds(Vector3 pos)
    {
        return pos.x > MAX_SIZE - 1 || pos.x < 0 || pos.z > MAX_SIZE - 1 || pos.z < 0;
    }

    /// <summary>
    /// check if current position wont surpas the height limit to place block
    /// true if it's higher than max height
    /// </summary>
    /// <returns></returns>
    bool CheckHighestPlacinPosition(Vector3 placePosition)
    {
        //return GetTotalHeight((int)placePosition.x, (int)placePosition.z) + 1 > MAX_HEIGHT;
        return placePosition.y + 1 > MAX_HEIGHT;
    }

    /// <summary>
    /// used as a wrapper to call from inspector, will remove blocks in Stacking mode and enable removing on click for freeplacing mode
    /// </summary>
    public void RemoveItemFromButton()
    {
        ToggleEraseMode();
    }

    void RemoveGroundBlock(Vector3 selectPosition)
    {
        StageBlock blockToDelete = GetHighestBlockOnTile((int)selectPosition.x, (int)selectPosition.z);
        //StageBlock blockToDelete = GetBlockInPosition(selectController.transform.position);
        if (blockToDelete == null)
        {
            return;
        }

        //RemoveDecorationsFromtile(blockToDelete.transform.position);
        blocks.Remove(blockToDelete);
        if (blockToDelete.Model.ObjectType != null && blockToDelete.Model.ObjectType.Equals(ObjectType.Player_Spawn))
        {
            spawnCount--;
        }
        blockToDelete.DeleteItem();
    }

    public void ClearGrid()
    {
        //blocks
        if (blocks.Count > 0)
        {
            for (int i = blocks.Count - 1; i >= 0; i--)
            {
                StageBlock item = blocks[i];
                blocks.Remove(item);
                item.DeleteItem();
            }
        }
        //objects
        if (objects.Count > 0)
        {
            for (int i = objects.Count - 1; i >= 0; i--)
            {
                StageObject item = objects[i];
                objects.Remove(item);
                item.DeleteItem();
            }
        }
        //decoration
        //ClearDecorations();
    }

    /// <summary>
    /// TODO figure out if we want height or not
    /// </summary>
    public void CreateBlocksByInput()
    {
        selectedBlockType = cameraPreviewController.GetSelectedMouseBlock();

        if (!menuController.CheckGridSizeInput(MAX_SIZE))
        {
            Debug.LogWarning("the size must be greater than 0 and less than " + MAX_SIZE);
            return;
        }

        int[] gridsize = menuController.GetGridInputValues();

        //the center is MAX_SIZE /2, so depending on the input size, it should be positioned with the center as pivot
        //ej: 10x10 size is from 5,5 to 15,15 , 37x19 is from 2,11 to 38,29 etc
        int startingPosX = (int)(MAX_SIZE / 2) - (int)(gridsize[0] / 2);
        int startingPosY = (int)(MAX_SIZE / 2) - (int)(gridsize[1] / 2);

        for (int i = startingPosX; i < gridsize[0] + startingPosX; i++) //row
        {
            for (int j = startingPosY; j < gridsize[1] + startingPosY; j++) //col
            {
                AddGroundBlock(new Vector3(i, 0, j), selectedBlockType);
            }
        }
        menuController.DisplayMainMenu();
        selectController.UpdateHeight();
    }

    public void CreateRandomStageFromButton()
    {
        CreateBlocksByInput();
        GenerateRandomObjects();
    }

    /// <summary>
    /// adds a list of StageItemModel.RandomObjects randomly to the stage (enemy spaen included)
    /// </summary>
    void GenerateRandomObjects()
    {
        int randomEnemy = Random.Range(3, 6);
        int enemyCount = 0;

        for (int i = 0; i < MAX_SIZE; i++)
        {
            for (int j = 0; j < MAX_SIZE; j++)
            {
                StageBlock block = GetHighestBlockOnTile(i, j);
                float randomPercent = Random.Range(1, 100);

                if (block != null)
                {
                    float randomRow = Random.Range(0f, 0.4f) + i;
                    float randomCol = Random.Range(0f, 0.4f) + j;
                    ObjectType type;
                    StageObject obt;
                    switch (randomPercent)
                    {
                        //objects
                        case 5:
                        case 10:
                            int randomObt = Random.Range(0, StageItemModel.RandomObjects.Count);
                            type = StageItemModel.RandomObjects[randomObt].ObjectType;
                            obt = AddStageObject(new Vector3(randomRow, block.GridPosition.Height + 1f, randomCol), type);
                            obt.Rotate(Random.Range(-180f, 180f));
                            break;
                        //enemies
                        case 6:
                            if (enemyCount <= randomEnemy)
                            {
                                type = ObjectType.Enemy_Spawn;
                                obt = AddStageObject(new Vector3(randomRow, block.GridPosition.Height + 1f, randomCol), type);
                                enemyCount++;
                            }
                            break;
                    }
                }
            }
        }
    }

    public StageBlock GetHighestBlockOnTile(int row, int col)
    {
        List<StageBlock> tileBlocks = new List<StageBlock>();
        StageBlock highestBlock = null;
        tileBlocks = blocks.FindAll(x => (row == x.GridPosition.Row && col == x.GridPosition.Col));
        if (tileBlocks.Count > 0)
        {
            float height = 0;

            foreach (StageBlock item in tileBlocks)
            {
                if (height <= item.GridPosition.Height)
                {
                    highestBlock = item;
                }
            }
        }
        return highestBlock;
    }

    public float GetTotalHeight(int row, int col)
    {
        float height = 0;

        foreach (StageBlock item in blocks)
        {
            if ((row == item.GridPosition.Row && col == item.GridPosition.Col))
            {
                height += item.GetCurrengBlockHeight();
            }
        }
        return height;
    }

    void RemoveStageBlockFromClick(StageBlock block)
    {
        if (block == null)
        {
            Debug.Log("null block");
            return;
        }

        //check if there are any objects in the way
        RemoveObjectsFromtile(block.transform.position);

        blocks.Remove(block);
        block.DeleteItem();
    }

    StageBlock GetBlockInPosition(Vector3 pos)
    {
        //if (!IsTileOccupied(pos) && pos.y < 0f)
        //{
        //    return null;
        //}


        ///////
        Vector3 fromTopPosition = new Vector3(pos.x, StageBuilder.MAX_HEIGHT, pos.z);
        Vector3 toBottomPosition = pos;//new Vector3(pos.x, 0, pos.z);
        Vector3 direction = toBottomPosition - fromTopPosition;
        RaycastHit hit;

        //Debug.Log("from top " + fromTopPosition + " to pos " + toBottomPosition + " direction " + direction);
        //Debug.DrawRay(fromTopPosition, direction, Color.red, 50f);
        if (Physics.Raycast(fromTopPosition, direction, out hit) && hit.transform.gameObject.GetComponent<StageBlock>() != null)
        {
            return hit.transform.gameObject.GetComponent<StageBlock>();
        }
        return null;
        ///////

        return blocks.Find(x =>
        x.GridPosition.Row == pos.x &&
        x.GridPosition.Col == pos.z &&
        x.GridPosition.Height == pos.y);
    }

    /// <summary>
    /// checks if the current position has something instanciated already (will also detect blocks bigger than 1x1)
    /// </summary>
    /// <param name="currentPos"></param>
    /// <returns></returns>
    bool IsTileOccupied(Vector3 currentPos)
    {
        if (blocks.Count == 0)
        {
            return false;
        }

        StageBlock block = blocks.Find(x =>
        x.GridPosition.Row == currentPos.x &&
        x.GridPosition.Col == currentPos.z &&
        x.GridPosition.Height == currentPos.y);

        //for (int i = 0; i < blocks.Count; i++)
        //{

        //Vector3 found = blocks[i].OccupiedTiles.Find(x => currentPos == x);
        //if (found != null)
        //{
        //    return true;
        //}

        //    for (int j = 0; j < blocks[i].OccupiedTiles.Count; j++)
        //    {
        //        if (currentPos == blocks[i].OccupiedTiles[j])
        //        {
        //            return true;
        //        }
        //    }

        //}
        //return false;
        Debug.Log("block is tile occupied is " + block != null);

        return block != null;
    }

    /// <summary>
    /// reue if there is no block at the same position
    /// </summary>
    /// <param name="placedPosition"></param>
    /// <returns></returns>
    bool CheckSameBlockPosition(Vector3 placedPosition)
    {
        StageBlock blockFound = blocks.Find(x => x.transform.position == placedPosition);
        return blockFound != null;
    }

    /// <summary>
    /// checks if there is another block below, we will not instantiate if there is another
    /// true if there is a lower block
    /// </summary>
    /// <returns></returns>
    bool CheckLowerBlock(Vector3 selectPos)
    {
        if (selectPos.y == 0f)
        {
            return false;
        }

        int hei = selectPos.y == 0 ? 0 : Mathf.FloorToInt(selectPos.y) - 1;
        Vector3 lowerPosition = new Vector3(selectPos.x, hei, selectPos.z);
        StageBlock lowerBlock = GetBlockInPosition(lowerPosition);
        return lowerBlock == null;
    }

    List<StageBlock> GetNeightbours(StageBlock currentBlock)
    {
        List<StageBlock> neighbours = new List<StageBlock>();
        //the cube we want to compare is just below the selector
        //Vector3 positionUnderSelector = selectController.transform.position + Vector3.down;
        Vector3 blockPos = currentBlock.transform.position;

        StageBlock blockNorth = blocks.Find(x =>
        blockPos == x.transform.position + Vector3.forward);
        if (blockNorth != null)
        {
            neighbours.Add(blockNorth);
        }

        StageBlock blockSouth = blocks.Find(x =>
        blockPos == x.transform.position + Vector3.back);
        if (blockSouth != null)
        {
            neighbours.Add(blockSouth);
        }

        StageBlock blockEast = blocks.Find(x =>
        blockPos == x.transform.position + Vector3.right);
        if (blockEast != null)
        {
            neighbours.Add(blockEast);
        }

        StageBlock blockWest = blocks.Find(x =>
        blockPos == x.transform.position + Vector3.left);
        if (blockWest != null)
        {
            neighbours.Add(blockWest);
        }
        return neighbours;
    }

    ////////////////////--- OBJECTS CRUD---//////////////////////

    StageObject AddStageObject(Vector3 placePosition, ObjectType obtType)
    {
        placePosition = isAligned ?
           new Vector3(Mathf.RoundToInt(placePosition.x), placePosition.y, Mathf.RoundToInt(placePosition.z)) : placePosition;

        if (CheckSameObjectPosition(placePosition))
        {
            return null;
        }

        //check maximum spawn number
        if (CheckSpawnNumber(obtType))
        {
            return null;
        }

        StageItemModel itemModel = StageItemModel.GetModelFromKey(obtType.ToString().ToLower());
        GameObject prefabObt = Resources.Load(itemModel.PrefabPath) as GameObject;
        GameObject obtInstance = Instantiate(prefabObt);
        /////TESTING ADDING ITEMS
        //if (obtType == ObjectType.Item)
        //{
        //    itemModel = StageItemModel.Item;
        //    InventoryItemModel inventoryItemModel = InventoryItemModel.GetRandomItem();

        //    InventoryItem iitem = obtInstance.GetComponent<InventoryItem>();
        //    iitem.amount = 1;
        //    iitem.Model = inventoryItemModel;
        //}

        obtInstance.transform.position = placePosition;

        StageObject item = obtInstance.GetComponent<StageObject>();
        item.SetPosition(placePosition);
        item.Model = itemModel;

        obtInstance.name = "Obt_" + itemModel.JsonKey + "_" + item.transform.position.x.ToString("F2") + "_" +
            item.transform.position.z.ToString("F2") + "_" + item.transform.position.y.ToString("F1");

        objects.Add(item);

        //TODO put in items
        if (item.Model.DecoType.Equals(ObjectType.Player_Spawn))
        {
            spawnCount++;
        }
        return item;
    }

    void RemoveStageObjectFromClick(StageObject obt)
    {
        if (obt == null)
        {
            Debug.Log("null obt");
            return;
        }

        objects.Remove(obt);
        obt.DeleteItem();
    }

    /// <summary>
    /// used when a block is removed and there was an object above
    /// </summary>
    /// <param name="blockPos"></param>
    void RemoveObjectsFromtile(Vector3 blockPos)
    {
        Vector3 fromTopPosition = new Vector3(transform.position.x, StageBuilder.MAX_HEIGHT, transform.position.z);
        Vector3 direction = blockPos - fromTopPosition;
        RaycastHit hit;

        if (Physics.Raycast(fromTopPosition, direction, out hit))
        {
            Debug.DrawRay(fromTopPosition, direction, Color.red, 50f);
            if (hit.transform.gameObject.GetComponent<StageObject>() != null)
            {
                //height will be the highest block in that tile plus that block's height, same if it's an object
                StageObject obt = hit.transform.gameObject.GetComponent<StageObject>();
                RemoveStageObjectFromClick(obt);
                //obt.Remove(obts[i]);
                obt.DeleteItem();
                Debug.Log("collisioned with object");
            }
        }


        //List<StageObject> obts = objects.FindAll(x => new Vector3((int)x.transform.position.x, blockPos.y,
        //    (int)x.transform.position.z) == blockPos);
        //if (obts != null)
        //{
        //    for (int i = obts.Count - 1; i >= 0; i--)
        //    {
        //        objects.Remove(obts[i]);
        //        obts[i].DeleteItem();
        //    }
        //}
    }

    /// <summary>
    /// true if there is no item at the same position
    /// </summary>
    /// <param name="placedPosition"></param>
    /// <returns></returns>
    bool CheckSameObjectPosition(Vector3 placedPosition)
    {
        StageObject objectFound = objects.Find(x => x.transform.position == placedPosition);
        return objectFound != null;
    }


    ////////////////////--- DECORATION CRUD---//////////////////////

    //StageDecoration AddDecoration(Vector3 placePosition, DecorationType decoType)
    //{
    //    placePosition = isAligned ?
    //       new Vector3(Mathf.RoundToInt(placePosition.x), placePosition.y, Mathf.RoundToInt(placePosition.z)) : placePosition;

    //    if (CheckSameDecoPosition(placePosition)) //CheckSameDecoPosition(placePosition)
    //    {
    //        return null;
    //    }

    //    StageItemModel itemModel = StageItemModel.GetModelFromKey(decoType.ToString().ToLower());

    //    GameObject prefabBlock = Resources.Load(itemModel.PrefabPath) as GameObject;
    //    GameObject decoInstance = Instantiate(prefabBlock);
    //    decoInstance.transform.position = placePosition;

    //    StageDecoration item = decoInstance.GetComponent<StageDecoration>();
    //    item.SetPosition(placePosition.x, placePosition.y, placePosition.z);
    //    item.Model = itemModel;

    //    decoInstance.name = "Deco" + itemModel.JsonKey + "_" + item.transform.position.x.ToString("F2") + "_" +
    //        item.transform.position.z.ToString("F2") + "_" + item.transform.position.y.ToString("F1");

    //    //random rotation for now
    //    item.Rotate(Random.Range(0, 360));

    //    decorations.Add(item);
    //    return item;
    //}

    /// <summary>
    /// true if there is a decoration in that position already
    /// </summary>
    /// <param name="placedPosition"></param>
    /// <returns></returns>
    //bool CheckSameDecoPosition(Vector3 placedPosition)
    //{
    //    StageDecoration found = decorations.Find(x => x.transform.position == placedPosition);
    //    return found != null;
    //}

    /// <summary>
    /// removes any deco in the block that is being removed or added above it
    /// </summary>
    /// <param name="position"></param>
    //void RemoveDecorationsFromtile(Vector3 position)
    //{
    //    List<StageDecoration> decos = decorations.FindAll(x => new Vector3((int)x.transform.position.x, position.y,
    //        (int)x.transform.position.z) == position);
    //    if (decos != null)
    //    {
    //        for (int i = decos.Count - 1; i >= 0; i--)
    //        {
    //            decorations.Remove(decos[i]);
    //            decos[i].DeleteItem();
    //        }
    //    }
    //}

    //void ClearDecorations()
    //{
    //    if (decorations != null)
    //    {
    //        for (int i = decorations.Count - 1; i >= 0; i--)
    //        {
    //            StageDecoration item = decorations[i];
    //            decorations.Remove(item);
    //            item.DeleteItem();
    //        }
    //    }
    //}

    /// <summary>
    /// if aligned, objects will be placed in the middle of the block (grid-like), blocks are always aligned
    /// and therefore not affected by this Align
    /// </summary>
    /// <param name="value"></param>
    public void OnChangeAlign(bool value)
    {
        isAligned = value;
    }


    ///////////////////////--- SAVE LOAD STAGE---//////////////////////
    public void SaveCurrentStage()
    {
#if UNITY_EDITOR
        var path = EditorUtility.SaveFilePanel("Save Stage", STAGE_PATH, "", "json");
        if (path.Length != 0)
        {
            WriteDataToFile(path, GetJsonFromStage(Path.GetFileName(path)));
        }
#endif
    }

    JSONNode GetJsonFromStage(string name)
    {
        JSONNode json = JSON.Parse("{}");

        //map properties
        json["stage"]["name"] = name;
        //json["stage"]["base"] = menuController == null ? "0" : menuController.GetTerrainIndex().ToString();
        json["stage"]["base"] = "0";
        json["stage"]["rows"] = MAX_SIZE.ToString();
        json["stage"]["cols"] = MAX_SIZE.ToString();

        //blocks
        for (int i = 0; i < blocks.Count; i++)
        {
            StageItemModel model = blocks[i].Model;
            json["stage"]["tiles"][i]["block"]["json_key"] = model.JsonKey;
            json["stage"]["tiles"][i]["block"]["row"] = blocks[i].GridPosition.Row.ToString();
            json["stage"]["tiles"][i]["block"]["col"] = blocks[i].GridPosition.Col.ToString();
            //json["stage"]["tiles"][i]["block"]["damage_index"] = blocks[i].GetActiveDamageIndex().ToString();
            json["stage"]["tiles"][i]["block"]["height"] = blocks[i].GridPosition.Height.ToString();
            json["stage"]["tiles"][i]["block"]["rotation"] = blocks[i].YRotation.ToString();
            json["stage"]["tiles"][i]["block"]["is_ground"] = model.IsGround.ToString();
            json["stage"]["tiles"][i]["block"]["is_stackable"] = model.IsStackable.ToString();
            json["stage"]["tiles"][i]["block"]["is_destructible"] = model.IsDestructible.ToString();
            //json["stage"]["tiles"][i]["block"]["is_slope"] = blocks[i].IsSlope.ToString();
            json["stage"]["tiles"][i]["block"]["block_height"] = model.ItemHeight.ToString();
        }


        //objects
        for (int i = 0; i < objects.Count; i++)
        {
            StageItemModel model = objects[i].Model;
            json["stage"]["objects"][i]["object"]["json_key"] = model.JsonKey;
            json["stage"]["objects"][i]["object"]["row"] = objects[i].GridPosition.Row.ToString();
            json["stage"]["objects"][i]["object"]["col"] = objects[i].GridPosition.Col.ToString();
            json["stage"]["objects"][i]["object"]["height"] = objects[i].GridPosition.Height.ToString();
            json["stage"]["objects"][i]["object"]["damage_index"] = blocks[i].GetActiveDamageIndex().ToString();
            json["stage"]["objects"][i]["object"]["rotation"] = objects[i].YRotation.ToString();
            json["stage"]["objects"][i]["object"]["is_stackable"] = model.IsStackable.ToString();
            json["stage"]["objects"][i]["object"]["is_destructible"] = model.IsDestructible.ToString();
            json["stage"]["objects"][i]["object"]["block_height"] = model.ItemHeight.ToString();
            //if (model.ObjectType.Equals(ObjectType.Item))
            //{
            //    string itemKey = objects[i].GetComponent<InventoryItem>().Model.JsonKey;
            //    json["stage"]["objects"][i]["object"]["item_key"] = itemKey;
            //}
        }

        //decorations
        //for (int i = 0; i < decorations.Count; i++)
        //{
        //    StageItemModel model = decorations[i].Model;
        //    json["stage"]["decos"][i]["decoration"]["json_key"] = model.JsonKey;
        //    json["stage"]["decos"][i]["decoration"]["x"] = decorations[i].transform.position.x.ToString();
        //    json["stage"]["decos"][i]["decoration"]["y"] = decorations[i].transform.position.y.ToString();
        //    json["stage"]["decos"][i]["decoration"]["z"] = decorations[i].transform.position.z.ToString();
        //    json["stage"]["decos"][i]["decoration"]["rotation"] = decorations[i].YRotation.ToString();
        //}
        return json;
    }

    public void WriteDataToFile(string path, JSONNode jsonStage)
    {
        string jsonString = jsonStage.ToString();
        using (FileStream fs = new FileStream(path, FileMode.Create))
        {
            using (StreamWriter writer = new StreamWriter(fs))
            {
                writer.Write(jsonString);
            }
        }
    }

    public void LoadStageFromFile()
    {
#if UNITY_EDITOR
        string path = EditorUtility.OpenFilePanel("Load Stage", STAGE_PATH, "json");
        if (path.Length != 0)
        {
            var fileContent = File.ReadAllText(path);
            LoadStageFromJson(fileContent.ToString());
            //setting other map properties (base terrain)
            //this must be done outside the static method to access other clases

            var json = JSON.Parse(fileContent.ToString());
            int baseLevelIndex = json["stage"]["base"] == null ? 0 : json["stage"]["base"].AsInt;
            //menuController.SetSkyboxIndex(baseLevelIndex);

            //setting grid boundaries
            //selectController.RepositionToCenterOfBlocks(maxRows, maxCols);
            menuController.DisplayMainMenu();
        }
#endif
    }

    /// <summary>
    /// will add colliders to the edges and merge meshes to optimize performance
    /// (editing not allowed)
    /// </summary>
    /// <param name="jsonString"></param>
    /// <returns></returns>
    public StageMap LoadStageFromJsonInGame(string jsonString)
    {
        return LoadStageFromJsonInGame(LoadStageFromJson(jsonString));
    }
    public StageMap LoadStageFromJsonInGame(StageMap map)
    {
        //merging blocks TODO double check
        //blocks.ForEach(x => Destroy(x.gameObject.GetComponent<BoxCollider>()));
        //map.MergeBlocksByMaterial(GameObject.Find("groundBlocks")); //MergeBlocks

        GameObject wallColliders = GameObject.Find("wallColliders");
        if (wallColliders == null)
        {
            wallColliders = new GameObject("wallColliders");
            wallColliders.transform.parent = map.gameObject.transform;
        }
        else
        {
            CleanList(new List<Transform>(wallColliders.GetComponentsInChildren<Transform>()));
        }
        wallColliders.transform.parent = map.gameObject.transform;
        AddCollidersOnEdges(wallColliders);
        return map;
    }

    /// <summary>
    /// used to load the stage on Stagebuilder (allows editing)
    /// </summary>
    /// <param name="jsonString"></param>
    /// <returns></returns>
    public StageMap LoadStageFromJson(string jsonString)
    {
        ClearGrid();
        spawnCount = 0;
        var json = JSON.Parse(jsonString);

        int countBlocks = json["stage"]["tiles"].Count;
        int countObjects = json["stage"]["objects"].Count;
        int countDecos = json["stage"]["decos"] == null ? 0 : json["stage"]["decos"].Count;
        int baseLevelIndex = json["stage"]["base"] == null ? 0 : json["stage"]["base"].AsInt;
        //menuController.SetSkyboxIndex(baseLevelIndex);

        //creating gameobjects on hierarchy
        GameObject map = GameObject.Find("StageMap");
        GameObject groundBlocks = GameObject.Find("groundBlocks");
        GameObject stageObjects = GameObject.Find("stageObjects");
        StageMap stageMap = new StageMap();

        if (map == null)
        {
            map = new GameObject("StageMap");
            map.transform.position = Vector3.zero;
            map.transform.localScale = Vector3.one;
            stageMap = map.AddComponent<StageMap>();
        }
        else
        {
            stageMap = map.GetComponent<StageMap>();
            stageMap.Clean();
            CleanList(new List<Transform>(groundBlocks.GetComponentsInChildren<Transform>()));
            CleanList(new List<Transform>(stageObjects.GetComponentsInChildren<Transform>()));
        }
        groundBlocks = new GameObject("groundBlocks");
        stageObjects = new GameObject("stageObjects");
        //adding subfolders
        groundBlocks.transform.parent = map.transform;
        stageObjects.transform.parent = map.transform;

        //skybox
        stageMap.SkyboxIndex = baseLevelIndex;

        //blocks
        for (int i = 0; i < countBlocks; i++)
        {
            string jsonKey = json["stage"]["tiles"][i]["block"]["json_key"].Value;
            int row = json["stage"]["tiles"][i]["block"]["row"].AsInt;
            int col = json["stage"]["tiles"][i]["block"]["col"].AsInt;
            float height = json["stage"]["tiles"][i]["block"]["height"].AsFloat;
            //int damageIndex = json["stage"]["tiles"][i]["block"]["damage_index"].AsInt;
            //int heiIndex = json["stage"]["tiles"][i]["block"]["height_index"] == null ?
            //   0 : json["stage"]["tiles"][i]["block"]["height_index"].AsInt;
            int rotation = json["stage"]["tiles"][i]["block"]["rotation"].AsInt;
            bool isGround = json["stage"]["tiles"][i]["block"]["is_ground"].AsBool;
            bool isStackable = json["stage"]["tiles"][i]["block"]["is_stackable"].AsBool;
            bool isDestructible = json["stage"]["tiles"][i]["block"]["is_destructible"].AsBool;
            bool isSlope = json["stage"]["tiles"][i]["block"]["is_slope"].AsBool;
            float blockHeight = json["stage"]["tiles"][i]["block"]["block_height"].AsFloat;

            StageItemModel itemModel = StageItemModel.GetModelFromKey(jsonKey);
            StageBlock item = AddGroundBlock(new Vector3(row, height, col), itemModel.GroundBlockType);

            if (item != null)
            {
                itemModel.IsGround = isGround;
                itemModel.IsStackable = isStackable;
                itemModel.IsDestructible = isDestructible;
                itemModel.ItemHeight = blockHeight;
                item.Model = itemModel;

                item.Rotate(rotation);
                item.transform.parent = groundBlocks.transform;
                item.transform.localScale = Vector3.one;

                stageMap.AddStageBlock(item);
            }
        }

        //objects
        for (int i = 0; i < countObjects; i++)
        {
            string jsonKey = json["stage"]["objects"][i]["object"]["json_key"].Value;
            int row = json["stage"]["objects"][i]["object"]["row"].AsInt;
            int col = json["stage"]["objects"][i]["object"]["col"].AsInt;
            float height = json["stage"]["objects"][i]["object"]["height"].AsFloat;
            int damageIndex = json["stage"]["objects"][i]["object"]["damage_index"] == null ?
                 -1 : json["stage"]["objects"][i]["object"]["damage_index"].AsInt;
            int heiIndex = json["stage"]["objects"][i]["object"]["height_index"] == null ?
               0 : json["stage"]["objects"][i]["object"]["height_index"].AsInt;
            int rotation = json["stage"]["objects"][i]["object"]["rotation"].AsInt;
            bool isGround = json["stage"]["objects"][i]["object"]["is_ground"].AsBool;
            bool isStackable = json["stage"]["objects"][i]["object"]["is_stackable"].AsBool;
            bool isDestructible = json["stage"]["objects"][i]["object"]["is_destructible"].AsBool;
            float blockHeight = json["stage"]["objects"][i]["object"]["block_height"].AsFloat;
            string itemKey = json["stage"]["objects"][i]["object"]["item_key"] == null ? "" :
            json["stage"]["objects"][i]["object"]["item_key"].Value;

            StageItemModel itemModel = StageItemModel.GetModelFromKey(jsonKey);
            StageObject item = AddStageObject(new Vector3(row, height, col), itemModel.ObjectType);

            if (item != null)
            {
                itemModel.IsGround = isGround;
                itemModel.IsStackable = isStackable;
                itemModel.IsDestructible = isDestructible;
                itemModel.ItemHeight = blockHeight;
                item.Model = itemModel;

                item.AddRotation(rotation);
                item.transform.parent = stageObjects.transform;
                item.transform.localScale = Vector3.one;
                //item.SetNewDamageState(damageIndex);

                //ITEM
                //if (!itemKey.Equals(""))
                //{
                //    InventoryItem iitem = item.GetComponent<InventoryItem>();
                //    iitem.SetData(InventoryItemModel.GetModelFromKey(itemKey));
                //    iitem.amount = 1;
                //}

                stageMap.AddStageObject(item);
            }
        }

        //decorations
        //for (int j = 0; j < countDecos; j++)
        //{
        //    string jsonKey = json["stage"]["decos"][j]["decoration"]["json_key"].Value;
        //    float row = json["stage"]["decos"][j]["decoration"]["x"].AsFloat;
        //    float col = json["stage"]["decos"][j]["decoration"]["z"].AsFloat;
        //    float height = json["stage"]["decos"][j]["decoration"]["y"].AsFloat;
        //    float rotation = json["stage"]["decos"][j]["decoration"]["rotation"].AsFloat;
        //    //float blockHeight = json["stage"]["tiles"][j]["decoration"]["block_height"].AsFloat;

        //    StageItemModel itemModel = StageItemModel.GetModelFromKey(jsonKey);
        //    StageDecoration item = AddDecoration(new Vector3(row, height, col), itemModel.DecoType);

        //    if (item != null)
        //    {
        //        //itemModel.BlockHeight = blockHeight;
        //        item.Model = itemModel;

        //        item.Rotate(rotation);
        //        item.transform.parent = map.transform;
        //        item.transform.localScale = Vector3.one;

        //        stageMap.AddStageDecoration(item);
        //    }
        //}
        return stageMap;
    }

    void AddCollidersOnEdges(GameObject wallColliders)
    {
        List<BoxCollider> colliders = new List<BoxCollider>();
        List<StageBlock> edges = GetStageEdges();
        for (int i = 0; i < edges.Count; i++)
        {
            StageBlock edgeBlock = edges[i];
            DetectEdges(edgeBlock).ForEach(x => AddColliderToEdge(edgeBlock, x, colliders, wallColliders));
        }
    }

    /// <summary>
    /// gets the blocks on the edge to apply the external collider
    /// </summary>
    /// <returns></returns>
    List<StageBlock> GetStageEdges()
    {
        List<StageBlock> edges = new List<StageBlock>();
        for (int i = 0; i < blocks.Count; i++)
        {
            StageBlock block = blocks[i];
            //only detecting on first level
            if (block.transform.position.y == 0f && GetNeightbours(block).Count < 4)
            {
                edges.Add(block);
            }
        }
        return edges;
    }

    /// <summary>
    /// gives the position where the edge collider should go
    /// </summary>
    /// <returns></returns>
    List<Vector3> DetectEdges(StageBlock block)
    {
        Vector3 blockPos = block.transform.position;
        List<Vector3> edgePositions = new List<Vector3>();

        StageBlock blockNorth = blocks.Find(x =>
        blockPos == x.transform.position + Vector3.forward);
        if (blockNorth == null)
        {
            edgePositions.Add(blockPos + Vector3.forward);
        }

        StageBlock blockSouth = blocks.Find(x =>
        blockPos == x.transform.position + Vector3.back);
        if (blockSouth == null)
        {
            edgePositions.Add(blockPos + Vector3.back);
        }

        StageBlock blockEast = blocks.Find(x =>
        blockPos == x.transform.position + Vector3.right);
        if (blockEast == null)
        {
            edgePositions.Add(blockPos + Vector3.right);
        }

        StageBlock blockWest = blocks.Find(x =>
        blockPos == x.transform.position + Vector3.left);
        if (blockWest == null)
        {
            edgePositions.Add(blockPos + Vector3.left);
        }
        return edgePositions;
    }

    /// <summary>
    /// /// adds the edge collider to that position
    /// </summary>
    /// <param name="edgeBlock"></param>
    /// <param name="colliderPosition"></param>
    /// <param name="addedColliders"></param>
    /// <param name="wallsObt"></param>
    void AddColliderToEdge(StageBlock edgeBlock, Vector3 colliderPosition, List<BoxCollider> addedColliders, GameObject wallsObt)
    {
        float row = edgeBlock.transform.position.x - colliderPosition.x;
        float col = edgeBlock.transform.position.z - colliderPosition.z;
        //we are not adding colliders if they are on that position already
        if (addedColliders.Find(x => (edgeBlock.transform.position + new Vector3(row, 2, col)) ==
        (x.transform.position + x.center)) == null)
        {
            //we'll attach the box collider to a gameobject, so we can see it on hierarchy
            GameObject wallObject = new GameObject("wallCollider_" + (edgeBlock.transform.position + new Vector3(row, 2, col)));
            wallObject.transform.position = edgeBlock.transform.position + new Vector3(row, 2, col);

            BoxCollider edgeCollider = wallObject.gameObject.AddComponent<BoxCollider>();
            //edgeCollider.center = new Vector3(row, 2, col);
            edgeCollider.size = new Vector3(1, 5, 1);
            addedColliders.Add(edgeCollider);
            //adding to hierarchy
            wallObject.transform.parent = wallsObt.transform;
        }
    }

    /// <summary>
    /// will generate a Random stage on the fly
    /// </summary>
    public StageMap GenerateRandomStage(int wid, int hei, GroundBlockType groundType)
    {
        int[] gridsize = new int[] { wid, hei };
        StageMap stageMap = new StageMap();

        //gameobject hierarchy
        GameObject mapObt = new GameObject("StageMap");
        GameObject groundBlocks = new GameObject("groundBlocks");
        GameObject stageObjects = new GameObject("stageObjects");
        mapObt.transform.position = Vector3.zero;
        mapObt.transform.localScale = Vector3.one;
        stageMap = mapObt.AddComponent<StageMap>();

        //adding subfolders
        groundBlocks.transform.parent = mapObt.transform;
        stageObjects.transform.parent = mapObt.transform;

        stageMap.Clean();

        //the center is MAX_SIZE /2, so depending on the input size, it should be positioned with the center as pivot
        //ej: 10x10 size is from 5,5 to 15,15 , 37x19 is from 2,11 to 38,29 etc
        int startingPosX = (int)(MAX_SIZE / 2) - (int)(gridsize[0] / 2);
        int startingPosY = (int)(MAX_SIZE / 2) - (int)(gridsize[1] / 2);

        for (int i = startingPosX; i < gridsize[0] + startingPosX; i++) //row
        {
            for (int j = startingPosY; j < gridsize[1] + startingPosY; j++) //col
            {
                StageBlock block = AddGroundBlock(new Vector3(i, 0, j), groundType);
                stageMap.AddStageBlock(block);
                block.transform.parent = groundBlocks.transform;
            }
        }

        GenerateRandomObjects();
        objects.ForEach(x => {
            x.transform.parent = stageObjects.transform;
            stageMap.AddStageObject(x);
        });

        return LoadStageFromJsonInGame(stageMap);
    }

    public void UploadToPlayfab()
    {
        //coming soon
    }

    public void BackToHome() {
        SceneManager.LoadScene("Home");
    }
} 