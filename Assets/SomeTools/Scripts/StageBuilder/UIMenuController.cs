using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class UIMenuController : MonoBehaviour
{
    [SerializeField]
    GameObject controlMenu;
    [SerializeField]
    GameObject PreviewMenu;
    [SerializeField]
    GameObject tileMenu;
    [SerializeField]
    UICameraPreviewController previewController;
    //[SerializeField]
    //SkyDomeController baseController;
    [SerializeField]
    GameObject gridSizeMenu;
    [SerializeField]
    GameObject helpMenu;
    [SerializeField]
    GameObject clearConfirmationScreen;
    [SerializeField]
    GameObject[] ItemButtons;
    //[SerializeField]
    //GameObject clearMenu;
    StageBlock selectedBlock;
    StageObject selectedItem;

    //UIPopupList modeList;
    UILabel posLabel;
    UILabel labpreviewBlock;
    //UIPopupList heiList; //TODO disable until figuring height out
    UILabel labGridRow;
    UILabel labGridCol;
    ToggleButtonController toggleModeButton;
    //ToggleButtonController toggleDecoButton;
    //UILabel labGridHei;

    private void Start() {             
        toggleModeButton = controlMenu.GetComponentsInChildren<ToggleButtonController>()[0];
        //toggleDecoButton = controlMenu.GetComponentsInChildren<ToggleButtonController>()[1];
        //modeList = controlMenu.GetComponentsInChildren<UIPopupList>()[1];
        posLabel = tileMenu.GetComponentsInChildren<UILabel>()[0];
        labpreviewBlock = PreviewMenu.GetComponentsInChildren<UILabel>()[0];
        tileMenu.SetActive(false);
        //heiList = tileMenu.GetComponentsInChildren<UIPopupList>()[0];

        //setting gridsize menu
        UIInput rowInput = gridSizeMenu.GetComponentsInChildren<UIInput>()[0];
        UIInput colInput = gridSizeMenu.GetComponentsInChildren<UIInput>()[1];
        //UIInput heiInput = gridSizeMenu.GetComponentsInChildren<UIInput>()[2];
        rowInput.validation = UIInput.Validation.Integer;
        colInput.validation = UIInput.Validation.Integer;
        //heiInput.validation = UIInput.Validation.Integer;

        labGridRow = rowInput.label;
        labGridCol = colInput.label;

        //labGridHei = heiInput.label;
        gridSizeMenu.SetActive(true);
        controlMenu.SetActive(false);

        //hiding menus
        HideTileMenu();
        HideClearGriMenu();

        //set highlight buttons
        for (int i = 0; i < ItemButtons.Length; i++)
        {
            UISprite highlight = ItemButtons[i].GetComponentsInChildren<UISprite>()[1];
            highlight.enabled = false;
        }
        helpMenu.SetActive(false);
    }

    private void Update()
    {
        labpreviewBlock.text = previewController.DisplaySelectedItemName().ToString().Replace("_", " ");

        if (Input.GetKeyDown(KeyCode.H))
        {
            ToggleHelpMenu();
        }
    }

    public void DisplayPosition(int row, int col, float hei)
    {
        posLabel.text = "Row : " + row + "\nCol : " + col + "\nHei : " + hei.ToString("F1");
    }

    public void DisplayTileMenu()
    {
        tileMenu.SetActive(true); //clearMenu
        //CheckIfBlockHasSlope();
    }
    public void HideTileMenu()
    {
        tileMenu.SetActive(false);//clearMenu
        if ( selectedItem != null) {
            selectedItem.UnHightlight();
        }
    }

    //public void DisplayClearConfirmationMenu()
    //{
    //    clearMenu.SetActive(true);
    //}

    //public void HideClearConfirmationMenu()
    //{
    //    clearMenu.SetActive(false);
    //}

    public void DisplayClearGridMenu()
    {
        clearConfirmationScreen.SetActive(true);
    }

    public void HideClearGriMenu()
    {
        clearConfirmationScreen.SetActive(false);
    }

    public void HighlightPressedButtonFromIndex(int index)
    {
        HighlightPressedButton(ItemButtons[index]);
    }

    public void HighlightPressedButton(GameObject pressedButton)
    {
        for (int i = 0; i < ItemButtons.Length; i++)
        {
            UISprite highlight = ItemButtons[i].GetComponentsInChildren<UISprite>()[2]; //1
            highlight.enabled = false;
        }
        pressedButton.GetComponentsInChildren<UISprite>()[2].enabled = true;   //1
    }

    /// <summary>
    // true if all grid values are valid
    /// </summary>
    /// <returns></returns>
    public bool CheckGridSizeInput(int maxsize)
    {
        return labGridRow.text.Length > 0 && int.Parse(labGridRow.text) > 0
            && int.Parse(labGridRow.text) <= maxsize && labGridCol.text.Length > 0
            && int.Parse(labGridCol.text) > 0 && int.Parse(labGridCol.text) <= maxsize;
        //&& labGridHei.text.Length > 0
    }

    /// <summary>
    /// TODO see what to do with height
    /// </summary>
    /// <returns></returns>
    public int[] GetGridInputValues()
    {
        int rowValue = int.Parse(labGridRow.text) > 0 ? int.Parse(labGridRow.text) : 1;
        int colValue = int.Parse(labGridCol.text) > 0 ? int.Parse(labGridCol.text) : 1;
        return new int[] { rowValue, colValue };
    }

    //after selecting the height or loading a stage, controlMenu is toggled
    public void DisplayMainMenu()
    {
        gridSizeMenu.SetActive(false);
        controlMenu.SetActive(true);
    }

    void ToggleHelpMenu()
    {
        helpMenu.SetActive(!helpMenu.activeInHierarchy);
    }

    public void SelectItem(StageObject item)
    {
        selectedItem = item;
        selectedItem.IsSelected = true;
        selectedItem.HighLight();
        DisplayPosition(item.GridPosition.Row, item.GridPosition.Col, item.GridPosition.Height);
        //SetHeightDataFromItem();
    }

    public void UnselelectItem()
    {
        if (selectedItem != null)
        {
            selectedItem.IsSelected = false;
            selectedItem.UnHightlight();
            selectedItem = null;
        }
    }

    /// <summary>
    /// fixed 90 degree rotation
    /// </summary>
    public void RotateItemFromButtonFixed()
    {
        if (selectedItem != null)
        {
            selectedItem.AddRotation();
            float rot = selectedItem.YRotation;
            //rounding rotation to closest angle (0, 90, 180, 270)
            float[] fixedAngles = new float[] { 0f, 90f, 180f, 270f, 360f };

            float distance = 400f;
            float roundedValue = float.NaN;
            foreach (float f in fixedAngles)
            {
                float d = Mathf.Abs(rot - f);
                if (d < distance)
                {
                    distance = d;
                    roundedValue = f;
                }
            }
            roundedValue = roundedValue == 360f ? 0 : roundedValue;
            selectedItem.Rotate(roundedValue);
        }
    }

    public void RotateItemFromButtonLeft()
    {
        selectedItem.RotateFreely(false);
    }

    public void RotateItemFromButtonright()
    {
        selectedItem.RotateFreely(true);
    }

    /// <summary>
    /// IF ITEM IT CHANGES THE ITEM
    /// </summary>
    public void ChangeItemDamage()
    {
        //int selectedIndex = damageList.items.IndexOf(damageList.value);
        //if (selectedItem.GetComponent<InventoryItem>()!= null) {
        //    InventoryItem item= selectedItem.GetComponent<InventoryItem>();
        //    item.SetData(InventoryItemModel.AllItems[selectedIndex]);
        //    return;
        //}
        //selectedItem.SetNewDamageState(selectedIndex);
        ////SetHeightDataFromItem();
    }

    /// <summary>
    /// sets UiPopUPlist height options depending on the selected block
    /// </summary>
    void SetHeightDataFromItem()
    {
        //heiList.Clear();
        //heiList.AddItem("Height 100%");

        //if (!selectedItem.wrapper.IsSolidGround)
        //{
        //    return;
        //}
        //heiList.AddItem("Height 50%");
        //heiList.AddItem("Height 25%");
    }

    //public int GetTerrainIndex()
    //{
    //    return baseController.GetSelectedIndex();
    //}

    //public void SetSkyboxIndex(int index)
    //{
    //    baseController.ChangeMaterial(index);
    //}

    //public SkyDomeController GetbaseController()
    //{
    //    return baseController;
    //}


    /// <summary>
    /// gets the selected index(int) from ToggleButtonController
    /// </summary>
    /// <returns></returns>
    public StageBuildMode GetSelectedMode()
    {
        return (StageBuildMode)toggleModeButton.State;
    }

    /// <summary>
    /// used if the user toggle smode using the hotkey
    /// </summary>
    public void SetSelectedModeFromButton()
    {
        toggleModeButton.ChangeState();
    }

    /// <summary>
    /// gets the selected index(int) from ToggleButtonController
    /// </summary>
    /// <returns></returns>
    //public int GetSelectedDeco()
    //{
    //    return toggleDecoButton.State;
    //}

    /// <summary>
    /// used if the user toggle smode using the hotkey
    /// </summary>
    //public void SetSelectedDecoFromButton()
    //{
    //    toggleDecoButton.ChangeState();
    //}

    /// <summary>
    /// disables SlopeCheck if selected block does not have slope option (only available for block)
    /// TODO use when we have slope meshes
    /// </summary>
    void CheckIfBlockHasSlope()
    {
        //SlopeCheck.transform.gameObject.SetActive(selectedItem.wrapper.HasSlope());
    }

}
