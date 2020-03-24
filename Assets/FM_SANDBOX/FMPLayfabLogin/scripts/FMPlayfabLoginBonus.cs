using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FMPlayfabLoginBonus : MonoBehaviour
{
    public Action<FMPlayfabLoginBonusResult> OnResult;
    static FMPlayfabLoginBonus instance;

    public static FMPlayfabLoginBonus Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject obj = new GameObject("FMPlayfabLoginBonus");
                instance = obj.AddComponent<FMPlayfabLoginBonus>();
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

    public void CheckLoginBonus() {
        PlayfabUtils.Instance.CheckLoginBonus("regular", OnLoginBonus, OnError);
    }

    public void CheckLoginBonus(string mode) {
        PlayfabUtils.Instance.CheckLoginBonus(mode, OnLoginBonus, OnError);
    }

    void OnLoginBonus(ExecuteCloudScriptResult result) {
        if (OnResult != null) {
            FMPlayfabLoginBonusResult bonusResult = new FMPlayfabLoginBonusResult(result);
            OnResult(bonusResult);
        }
    }

    void OnError(PlayFabError error) {
        Debug.Log("error on getting login bonus, "+ error.ErrorMessage);
    }
}
