using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SelectorMode
{
    Default,
    MultiSelect,
    NotAllowed
}

public class SelectionController : MonoBehaviour
{
    [SerializeField]
    UICameraPreviewController blockPreviewController;
    [SerializeField]
    UILabel labCurrentPos;
    [SerializeField]
    Material[] selectBoxMaterials;
    [SerializeField]
    GameObject selectCube;
    [SerializeField]
    GameObject[] selectCubes;

    public GroundBlockType selectedBlockType; //used for mouse button
    public ObjectType selectedObject; //used for mouse button
    public bool IsContinuous { get; set; }
    public int CubeRange = 1;
    float currentHeight;
    CameraOrientation orientation = CameraOrientation.North;
    int MaxGridSize;
    bool isMultiSelection = false;
    Vector3 selection1;
    Vector3 selection2;
    float presedShiftInTime = 0; // Counter time for pressed button
    float slowButonsPressed = 3;// "slow motion"
    int maxCubeRange = 3;

    void Start()
    {
        MaxGridSize = StageBuilder.MAX_SIZE;
        transform.position = new Vector3((int)(MaxGridSize / 2), 0, (int)(MaxGridSize / 2));
        selectedBlockType = blockPreviewController.GetSelectedMouseBlock();
        //selectedObject = blockPreviewController.GetSelectedObject();

        InitializeRange();
    }

    void Update()
    {
        SlowDownPressedKeys();

        MoveInGrid();

        //ModeHeightInGrid();      
        labCurrentPos.text = "Current Position: [ "
            + transform.position.x + ", " + transform.position.z + ", " + transform.position.y + "]";
    }

    void MoveInGrid()
    {
        if (IsContinuous ? Input.GetKey(KeyCode.LeftArrow) : Input.GetKeyDown(KeyCode.LeftArrow)) //left
        {
            GoLeft();
            UpdateHeight();
        }
        else if (IsContinuous ? Input.GetKey(KeyCode.RightArrow) : Input.GetKeyDown(KeyCode.RightArrow)) //right
        {
            GoRight();
            UpdateHeight();
        }
        else if (IsContinuous ? Input.GetKey(KeyCode.UpArrow) : Input.GetKeyDown(KeyCode.UpArrow)) //forward
        {

            GoUp();
            UpdateHeight();
        }
        else if (IsContinuous ? Input.GetKey(KeyCode.DownArrow) : Input.GetKeyDown(KeyCode.DownArrow)) // back
        {
            GoDown();
            UpdateHeight();
        }
    }

    void GoUp()
    {
        switch (orientation)
        {
            case CameraOrientation.North:
                if (!CheckOutOfBounds(Vector3.forward))
                {
                    transform.position += Vector3.forward;
                }
                break;
            case CameraOrientation.South:
                if (!CheckOutOfBounds(Vector3.back))
                {
                    transform.position += Vector3.back;
                }
                break;
            case CameraOrientation.East:
                if (!CheckOutOfBounds(Vector3.right))
                {
                    transform.position += Vector3.right;
                }
                break;
            case CameraOrientation.West:
                if (!CheckOutOfBounds(Vector3.left))
                {
                    transform.position += Vector3.left;
                }
                break;
        }
    }

    void GoDown()
    {
        switch (orientation)
        {
            case CameraOrientation.North:
                if (!CheckOutOfBounds(Vector3.back))
                {
                    transform.position += Vector3.back;
                }
                break;
            case CameraOrientation.South:
                if (!CheckOutOfBounds(Vector3.forward))
                {
                    transform.position += Vector3.forward;
                }
                break;
            case CameraOrientation.East:
                if (!CheckOutOfBounds(Vector3.left))
                {
                    transform.position += Vector3.left;
                }
                break;
            case CameraOrientation.West:
                if (!CheckOutOfBounds(Vector3.right))
                {
                    transform.position += Vector3.right;
                }
                break;
        }
    }

    void GoLeft()
    {
        switch (orientation)
        {
            case CameraOrientation.North:
                if (!CheckOutOfBounds(Vector3.left))
                {
                    transform.position += Vector3.left;
                }
                break;
            case CameraOrientation.South:
                if (!CheckOutOfBounds(Vector3.right))
                {
                    transform.position += Vector3.right;
                }
                break;
            case CameraOrientation.East:
                if (!CheckOutOfBounds(Vector3.forward))
                {
                    transform.position += Vector3.forward;
                }
                break;
            case CameraOrientation.West:
                if (!CheckOutOfBounds(Vector3.back))
                {
                    transform.position += Vector3.back;
                }
                break;
        }
    }

    void GoRight()
    {
        switch (orientation)
        {
            case CameraOrientation.North:
                if (!CheckOutOfBounds(Vector3.right))
                {
                    transform.position += Vector3.right;
                }
                break;
            case CameraOrientation.South:
                if (!CheckOutOfBounds(Vector3.left))
                {
                    transform.position += Vector3.left;
                }
                break;
            case CameraOrientation.East:
                if (!CheckOutOfBounds(Vector3.back))
                {
                    transform.position += Vector3.back;
                }
                break;
            case CameraOrientation.West:
                if (!CheckOutOfBounds(Vector3.forward))
                {
                    transform.position += Vector3.forward;
                }
                break;
        }
    }

    public void RepositionToCenterOfBlocks(int row, int col)
    {
        //reposition selector to center
        transform.position = new Vector3(Mathf.FloorToInt(row / 2), 0f, Mathf.FloorToInt(col / 2));
    }

    /// <summary>
    /// true if it goes over the row/column limit
    /// </summary>
    /// <param name="plusMove"></param>
    /// <returns></returns>
    bool CheckOutOfBounds(Vector3 plusMove)
    {
        bool outBounds = false;
        //Vector3 newPos = transform.position + plusMove;
        //return newPos.x > MaxGridSize - 1 || newPos.x < 0 || newPos.z > MaxGridSize - 1 || newPos.z < 0;
        List<Vector3> positions = GetActiveCubes();
        for (int i = 0; i < positions.Count; i++)
        {
            Vector3 newPos = positions[i] + plusMove;
            outBounds = newPos.x > MaxGridSize - 1 || newPos.x < 0 || newPos.z > MaxGridSize - 1 || newPos.z < 0;
            if (outBounds)
            {
                return outBounds;
            }
        }
        return outBounds;
    }

    void InitializeRange()
    {
        for (int i = 0; i < selectCubes.Length; i++)
        {
            selectCubes[i].SetActive(i == 0);
        }
    }

    public void SetMoreRange()
    {
        if (!CheckOutOfBounds(Vector3.zero))
        {
            SetRange(1);
        }
    }

    public void SetLessRange()
    {
        SetRange(-1);
    }

    void SetRange(int addedRange)
    {
        CubeRange += addedRange;
        CubeRange = CubeRange < 1 ? 1 : CubeRange;
        CubeRange = CubeRange > maxCubeRange ? maxCubeRange : CubeRange;

        int enabledCubeSize = CubeRange * CubeRange;

        //selectPositions.Clear();

        for (int i = 0; i < selectCubes.Length; i++)
        {
            selectCubes[i].gameObject.SetActive(i < enabledCubeSize);
        }
    }

    /// <summary>
    /// used in freehand mode
    /// </summary>
    void ModeHeightInGrid()
    {
        if (Input.GetKeyDown(KeyCode.W)) //higher
        {
            SetHeight(0.25f);
        }
        if (Input.GetKeyDown(KeyCode.S)) //lower
        {
            SetHeight(-0.25f);
        }
    }

    /// <summary>
    /// slows actions down a bit when keeping keys pressed
    /// </summary>
    void SlowDownPressedKeys()
    {
        IsContinuous = Input.GetKey(KeyCode.LeftShift);

        if (Input.GetKey(KeyCode.LeftShift))
        {
            presedShiftInTime += Time.deltaTime * 10;
            IsContinuous = presedShiftInTime > slowButonsPressed;
            presedShiftInTime = presedShiftInTime > slowButonsPressed ? 1.4f : presedShiftInTime;
        }
        else if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            presedShiftInTime = 0;
            IsContinuous = false;
        }
    }

    public GroundBlockType GetSelectedPreviewBlock()
    {
        selectedBlockType = blockPreviewController.GetSelectedMouseBlock();
        return selectedBlockType;
    }
    //public ObjectType GetSelectedPreviewObject()
    //{
    //    selectedObject = blockPreviewController.GetSelectedObject();
    //    return selectedObject;
    //}
    //public DecorationType GetSelectedDecoration()
    //{
    //    selectedObject = blockPreviewController.GetSelectedDecoration();
    //    return selectedObject;
    //}

    public void AddHeightFromInput()
    {
        SetHeight(1f);
    }
    public void RemoveHeightFromInput()
    {
        SetHeight(1f);
    }

    /// <summary>
    /// lets the user set the height for item
    /// the parameter can be negative but not less than 0 (used in freehand)
    /// </summary>
    /// <param name="addedHeight"></param>
    void SetHeight(float addedHeight)
    {
        if ((transform.position.y + addedHeight) < 0)
        {
            return;
        }

        transform.position += Vector3.up * addedHeight;
        currentHeight = transform.position.y;
    }

    /// <summary>
    /// updates the height in that tile using a raycast from maximum height to the selector's height
    /// </summary>
    /// <param name="newHeight"></param>
    public void UpdateHeight()
    {
        currentHeight = Mathf.Clamp(currentHeight, 0, StageBuilder.MAX_HEIGHT);
        float highestHei = 0;
        List<Vector3> activeCubes = GetActiveCubes();

        for (int i = 0; i < activeCubes.Count; i++)
        {
            float newHei = transform.position.y;
            Vector3 fromTopPosition = new Vector3(activeCubes[i].x, StageBuilder.MAX_HEIGHT, activeCubes[i].z);
            Vector3 toBottomPosition = new Vector3(activeCubes[i].x, 0, activeCubes[i].z);
            Vector3 direction = toBottomPosition - fromTopPosition;
            RaycastHit hit;

            if (Physics.Raycast(fromTopPosition, direction, out hit))
            {
                if (hit.transform.gameObject.GetComponent<StageObject>() != null)
                {
                    //height will be the highest block in that tile plus that block's height, same if it's an object
                    StageObject obt = hit.transform.gameObject.GetComponent<StageObject>();
                    newHei = obt.GridPosition.Height + obt.GetCurrentItemHeight();
                }
                else if (hit.transform.gameObject.GetComponent<StageBlock>() != null)
                {
                    StageBlock block = hit.transform.gameObject.GetComponent<StageBlock>();
                    newHei = block.GridPosition.Height + block.GetCurrengBlockHeight();
                }
                else
                {
                    newHei = 0;
                }

                highestHei = newHei > highestHei ? newHei : highestHei;
            }

            currentHeight = highestHei;//newHei;            
        }
        transform.position = new Vector3(transform.position.x, highestHei, transform.position.z); //currentHeight
    }

    public void UpdateHeightTEST() //this is the old one, with obly 1 cube selector
    {
        currentHeight = Mathf.Clamp(currentHeight, 0, StageBuilder.MAX_HEIGHT);

        float newHei = transform.position.y;
        Vector3 fromTopPosition = new Vector3(transform.position.x, StageBuilder.MAX_HEIGHT, transform.position.z);
        Vector3 toBottomPosition = new Vector3(transform.position.x, 0, transform.position.z);
        Vector3 direction = toBottomPosition - fromTopPosition;
        RaycastHit hit;

        if (Physics.Raycast(fromTopPosition, direction, out hit))
        {
            if (hit.transform.gameObject.GetComponent<StageObject>() != null)
            {
                //height will be the highest block in that tile plus that block's height, same if it's an object
                StageObject obt = hit.transform.gameObject.GetComponent<StageObject>();
                newHei = obt.GridPosition.Height + obt.GetCurrentItemHeight();
            }
            else if (hit.transform.gameObject.GetComponent<StageBlock>() != null)
            {
                StageBlock block = hit.transform.gameObject.GetComponent<StageBlock>();
                newHei = block.GridPosition.Height + block.GetCurrengBlockHeight();
            }
            else
            {
                newHei = 0;
            }
        }

        currentHeight = newHei;
        transform.position = new Vector3(transform.position.x, currentHeight, transform.position.z);
    }

    public StageBlock GetHighestBlock()
    {
        //currentHeight = currentHeight < 0 ? 0 : currentHeight;
        currentHeight = Mathf.Clamp(currentHeight, 0, StageBuilder.MAX_HEIGHT);

        Vector3 fromTopPosition = new Vector3(transform.position.x, StageBuilder.MAX_HEIGHT, transform.position.z);
        Vector3 toBottomPosition = new Vector3(transform.position.x, 0, transform.position.z);
        Vector3 direction = toBottomPosition - fromTopPosition;
        RaycastHit hit;

        if (Physics.Raycast(fromTopPosition, direction, out hit) && hit.transform.gameObject.GetComponent<StageBlock>() != null)
        {
            Debug.Log("block?");
            return hit.transform.gameObject.GetComponent<StageBlock>();
        }
        return null;
    }

    public StageObject GetHighestObject()
    {
        //currentHeight = currentHeight < 0 ? 0 : currentHeight;
        currentHeight = Mathf.Clamp(currentHeight, 0, StageBuilder.MAX_HEIGHT);

        Vector3 fromTopPosition = new Vector3(transform.position.x, StageBuilder.MAX_HEIGHT, transform.position.z);
        Vector3 toBottomPosition = new Vector3(transform.position.x, 0, transform.position.z);
        Vector3 direction = toBottomPosition - fromTopPosition;
        RaycastHit hit;
        bool isHit = Physics.Raycast(fromTopPosition, direction, out hit);
        Debug.Log("raycast hei " + transform.position.y + ", is hit " + isHit + " stageobject " + hit.transform.gameObject.GetComponent<StageObject>());
        Debug.DrawRay(fromTopPosition, direction, Color.green, 50f);
        if (isHit && hit.transform.gameObject.GetComponent<StageObject>() != null)
        {
            Debug.Log("object?");
            Debug.Log(hit.transform.gameObject.GetComponent<StageObject>().name);
            return hit.transform.gameObject.GetComponent<StageObject>();
        }
        Debug.Log("object " + hit.transform.gameObject.name);
        return null;
    }

    public void UpdateOrientation(CameraOrientation newOriewntation)
    {
        orientation = newOriewntation;
    }

    /// <summary>
    /// 0 normal (yellow)
    /// 1 multi select (green)
    /// 2 can't instanciate (red)
    /// </summary>
    /// <param name="index"></param>
    public void SetSelectorColor(SelectorMode mode)
    {
        switch (mode)
        {
            case SelectorMode.Default:
                selectCubes[0].GetComponent<MeshRenderer>().material = selectBoxMaterials[0];
                break;
            case SelectorMode.MultiSelect:
                selectCubes[0].GetComponent<MeshRenderer>().material = selectBoxMaterials[1];
                break;
            case SelectorMode.NotAllowed:
                selectCubes[0].GetComponent<MeshRenderer>().material = selectBoxMaterials[2];
                break;
        }
    }

    /// <summary>
    /// depending on the range selected, it will return a list of positions with the enabled select cubes (1x1, 2x2, etc)
    /// </summary>
    /// <returns></returns>
    public List<Vector3> GetActiveCubes()
    {
        List<Vector3> selectPositions = new List<Vector3>();
        for (int i = 0; i < selectCubes.Length; i++)
        {
            //if (selectCubes[i].gameObject.GetComponent<Renderer>().enabled == true)
            if (selectCubes[i].gameObject.activeInHierarchy)
            {
                //the cubes have a tiny offset in height, we actually use the Selector(parent object) position
                Vector3 selectPosition = new Vector3(selectCubes[i].transform.position.x, transform.position.y, selectCubes[i].transform.position.z);
                selectPositions.Add(selectPosition);
            }
        }
        return selectPositions;
    }

    /// <summary>
    /// 0 yellow, 1 green, 2 red
    /// </summary>
    /// <returns></returns>
    public Material[] GetSelectShaders()
    {
        return selectBoxMaterials;
    }
}