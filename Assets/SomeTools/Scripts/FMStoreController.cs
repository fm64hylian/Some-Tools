using PlayFab;
using PlayFab.ClientModels;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FMStoreController : MonoBehaviour
{
    [SerializeField]
    UIGrid itemGrid;

    // Start is called before the first frame update
    void Start(){

        if (ClientSessionData.Instance.CatalogItems.Count == 0){
            PlayFabClientAPI.GetCatalogItems(new GetCatalogItemsRequest(), catalogRes =>{
                ClientSessionData.Instance.CatalogItems = catalogRes.Catalog;
                DisplayItems();
                return;
            }, error => { Debug.Log("error on get catalog info"); });
        }
        DisplayItems();
    }

    void DisplayItems(){
        for (int i = 0; i < ClientSessionData.Instance.CatalogItems.Count; i++){

            CatalogItem item = ClientSessionData.Instance.CatalogItems[i];
            GameObject itemPrefab = Instantiate(Resources.Load("Prefabs/FMStoreItemUI")) as GameObject;
            FMStoreItemUI itemUI = itemPrefab.GetComponent<FMStoreItemUI>();

            itemUI.SetData(item);

            itemUI.gameObject.transform.parent = itemGrid.transform;
            itemUI.gameObject.transform.localScale = Vector3.one;
        }

        itemGrid.Reposition();
    }

    public void BackToHome()
    {
        SceneManager.LoadScene("Home");
    }
}
