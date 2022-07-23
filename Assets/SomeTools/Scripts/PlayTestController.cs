using System.Collections;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PlayTestController : MonoBehaviour
{
    [SerializeField]
    UIPopupList stageNames;
    [SerializeField]
    FMPlayerController player;
    [SerializeField]
    GameObject noStagesWarning;
    [SerializeField]
    UILabel labCurrency;

    string filePath = StageBuilder.STAGE_PATH;
    List<string> stagePaths = new List<string>();
    int selectedIndex = 0;
    string SelectedStageName;
    StageMap map;
    

    void Start()
    {
        labCurrency.text = ClientSessionData.Instance.currencyCO.ToString();
        noStagesWarning.SetActive(false);
        LoadStageNames();

        //loading player
        player.gameObject.SetActive(false);
    }

     void LoadStageNames(){

        stageNames.Clear();
        string[] stages = System.IO.Directory.GetFiles(filePath, "*.json");

        if (stages.Length == 0) {
            stageNames.AddItem("NO STAGES");
            selectedIndex = -1;

            //display warning
            noStagesWarning.SetActive(true);
            return;
        }

        for (int i = 0; i < stages.Length; i++)
        {
            stagePaths.Add(stages[i]);
            string[] trimmedName = stages[i].Split('/'); //TODO check for other OS
            string stageNameJson = trimmedName[trimmedName.Length - 1];
            string stageNameNoJson = stageNameJson.Substring(0, stageNameJson.Length - 5);
            stageNames.AddItem(stageNameNoJson);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateStageIndex() {
        selectedIndex = stageNames.items.IndexOf(stageNames.value);
    }

    public void LoadSelectedStage() {

        if (selectedIndex == -1) {
            Debug.Log("please select a stage");
            return;
        }

        var fileContent = File.ReadAllText(stagePaths[selectedIndex]);
       map =StageBuilder.Instance.LoadStageFromJsonInGame(fileContent.ToString());


        player.StartPosition = (map.PlayerSpawns != null || map.PlayerSpawns.Count == 0) ?
            Vector3.zero + Vector3.up * 3: map.PlayerSpawns[0].transform.position + (Vector3.up) ;
        map.RepositionToInGame();

        player.ResetPosition();        
    }

    public void BackToHome() {
            SceneManager.LoadScene("Home");
    }
}
