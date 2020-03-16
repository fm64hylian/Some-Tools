using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleButtonController : MonoBehaviour
{
    [SerializeField]
    List<string> Labels = new List<string>();
    [SerializeField]
    List<Material> customColors = new List<Material>();
    public int State = 0;
    UISprite buttonSprite;
    UILabel buttonLab;

    void Start()
    {
        buttonSprite = gameObject.GetComponent<UISprite>();
        buttonLab = gameObject.GetComponentInChildren<UILabel>();

        //setting first positon
        buttonLab.text = Labels[0];
        buttonLab.color = GetButtonColor();
        buttonSprite.color = GetButtonColor();
    }

    public void ChangeState()
    {
        State++;
        State = State > Labels.Count - 1 ? 0 : State;

        buttonLab.text = Labels[State];
        buttonLab.color = GetButtonColor();
        buttonSprite.color = GetButtonColor();
    }

    Color GetButtonColor()
    {
        bool isNull = customColors.Count == 0;
        switch (State)
        {
            default:
                return isNull ? Color.green : customColors[State].color;
            case 1:
                return isNull ? Color.yellow : customColors[State].color;
            case 2:
                return isNull ? Color.red : customColors[State].color;
            case 3:
                return isNull ? Color.blue : customColors[State].color;
            case 4:
                return isNull ? Color.gray : customColors[State].color;
            case 5:
                return isNull ? Color.cyan : customColors[State].color;
        }
    }
}
