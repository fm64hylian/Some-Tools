using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryItemUI : MonoBehaviour
{
    [SerializeField]
    UILabel labitemName;

    [SerializeField]
    UISprite spriteItem;

    void Start()
    {

    }

    public void SetData(string name, string spriteName){
        labitemName.text = name;
        if (!string.IsNullOrEmpty(spriteName)){
            spriteItem.spriteName = spriteName;
        }
    }

    void Update()
    {

    }
}
