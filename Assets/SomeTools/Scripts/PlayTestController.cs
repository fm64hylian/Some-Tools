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
    PlayerController player;

    string filePath = StageBuilder.STAGE_PATH;
    List<string> stagePaths = new List<string>();
    int selectedIndex = 0;
    string SelectedStageName;
    StageMap map;
    

    void Start()
    {
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
            return;
        }

        for (int i = 0; i < stages.Length; i++)
        {
            stagePaths.Add(stages[i]);
            string trimmedName = stages[i].Replace(filePath, "");
            stageNames.AddItem(stages[i]);
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

        player.transform.position = (map.PlayerSpawns != null || map.PlayerSpawns.Count == 0) ?
            Vector3.zero + Vector3.up * 3: map.PlayerSpawns[0].transform.position + (Vector3.up) ;
        map.RepositionToInGame();

        player.gameObject.SetActive(true);
    }

    public void BackToHome() {
            SceneManager.LoadScene("Home");
    }
}
