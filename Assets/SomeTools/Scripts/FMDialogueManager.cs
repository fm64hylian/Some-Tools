using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FMDialogueManager : MonoBehaviour
{
    [SerializeField]
    YarnProgram yarnfile;
    [SerializeField]
    Yarn.Unity.DialogueRunner DialogueRunner;
    void Start()
    {
        //TODO  use it as wrapper for DailogueRunner and DialogueUI
        //check nodevisitedtracker.cs for an exmaple
    }

    // Update is called once per frame
    void Update()
    {
        if (DialogueRunner.IsDialogueRunning) { 

        }
    }

    public void StartD() {
        if (!DialogueRunner.IsDialogueRunning)
        {
            DialogueRunner.StartDialogue("Intro"); //test
        }
    }
}
