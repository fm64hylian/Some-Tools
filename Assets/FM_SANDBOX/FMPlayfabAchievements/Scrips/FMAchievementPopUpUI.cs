using PlayFab;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Used for the AnimationPreset can be attached to the gameObject, and 
/// </summary>
public class FMAchievementPopUpUI : MonoBehaviour
{
    [SerializeField]
    UILabel labTitle;
    [SerializeField]
    UILabel labDescription;
    [SerializeField]
    UILabel labNumber;
    //[SerializeField]
    //IVTweenPopUp popUpAnimation;

    void Start()
    {
        //testin statistics
        PlayfabUtils.Instance.GetPlayerStatistics(null, (res) =>
        {
            FMPlayfabUserStatistics.StoreItemsFromJson(res);
        }, OnError);
    }

    void OnError(PlayFabError error)
    {
        Debug.Log("achievement error");
    }


    public void PopUpNotification(FMAchievementItem item, int progress)
    {
        labTitle.text = item.Title;
        //labDescription.text = item.Description;
        labNumber.text = progress.ToString();

        //AsyncWait.Wait(2f, () =>
        //{
        //    popUpAnimation.BeginAnimation();
        //});
    }
}
