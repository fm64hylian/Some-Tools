using System;
using UnityEngine;

public class FMAchievementItemUI : MonoBehaviour
{
    [SerializeField]
    UISprite spriteAchievement;
    [SerializeField]
    UILabel labTitle;
    [SerializeField]
    UILabel labProgress;
    [SerializeField]
    UIButton buttonReward;
    [SerializeField]
    UIGrid levelGrid;
    [SerializeField]
    UISlider ProgressBar;
    public Action<FMAchievementItemUI> OnReward;
    public FMAchievementState[] States;

    public string Key { get; set; }

    public FMAchievementItem Achievement;
    string achievementIconName;
    int amountRequired;
    Color32 colorAchievedLv = new Color(0, 0.7843137f, 7921569f, 255); //0,200,202
    Color32 colorUnAchievedLv = new Color(0.49f, 0.49f, 0.49f, 255); //125,125,125
    public int CurrentIndex = 0;

    void Start()
    {
        buttonReward.isEnabled = false;
    }

    public void SetData(FMAchievementItem item, string rewardIcon)
    {
        Achievement = item;
        this.Key = item.Key;
        labTitle.text = item.Title;
        States = new FMAchievementState[item.AmountRequired.Length];
        FMUserAchievement uachv = FMPlayfabUserAchievement.Instance.GetUserAchievementFromKey(Key);
        States = uachv.States;

        achievementIconName = item.iconName;
        //the reward icon will be shown first, and the achievement icon will be shown when the the achievement is unlocked
        spriteAchievement.spriteName = States[States.Length - 1] == FMAchievementState.Claimed ?
           achievementIconName : rewardIcon;

        //setting levels for the first time
        for (int i = 0; i < uachv.States.Length; i++)
        {
            levelGrid.transform.GetChild(i).gameObject.SetActive(true);
            UISprite levelSprite = levelGrid.GetChildList()[i].gameObject.GetComponent<UISprite>();
            levelSprite.color = States[i] == FMAchievementState.Claimed ? colorAchievedLv : colorUnAchievedLv;
        }
        levelGrid.Reposition();
        UpdateData();
    }

    public void UpdateData()
    {
        //update progress bar
        CurrentIndex = FMPlayfabUserAchievement.Instance.GetUserAchievementFromKey(Key).CurrenIndex;
        amountRequired = Achievement.AmountRequired[CurrentIndex];
        int progress = FMPlayfabUserStatistics.GetStatisticProgress(Achievement.Statistic);
        progress = Mathf.Clamp(progress, 0, amountRequired);
        ProgressBar.gameObject.SetActive(true);
        ProgressBar.value = (float)progress / (float)amountRequired;
        labProgress.text = +progress + " / " + amountRequired;
        ProgressBar.gameObject.SetActive(false);

        //update levels
        FMUserAchievement uachv = FMPlayfabUserAchievement.Instance.GetUserAchievementFromKey(Key);
        States = uachv.States;
        for (int i = 0; i < uachv.States.Length; i++)
        {
            UISprite levelSprite = levelGrid.GetChildList()[i].gameObject.GetComponent<UISprite>();
            levelSprite.color = States[i] == FMAchievementState.Claimed ? colorAchievedLv : colorUnAchievedLv;
        }
        levelGrid.Reposition();

        //if we got a new reward
        if (progress >= Achievement.AmountRequired[CurrentIndex])
        {
            buttonReward.isEnabled = true;
            buttonReward.state = UIButtonColor.State.Normal;
            buttonReward.defaultColor = new Color32(188, 179, 84, 255);
            ProgressBar.gameObject.SetActive(false);
            buttonReward.GetComponentInChildren<UILabel>().text = "Get Reward";
        }

        //if it's not enough, display progress bar
        else if (States[CurrentIndex] == FMAchievementState.Locked ||
            (States[CurrentIndex] == FMAchievementState.Claimed && CurrentIndex < States.Length))
        {
            buttonReward.isEnabled = false;
            buttonReward.disabledColor = new Color32(125, 134, 135, 255);
            ProgressBar.gameObject.SetActive(true);
        }

        //if it's the last one, set as claimed
        if (uachv.States[uachv.States.Length - 1] == FMAchievementState.Claimed)
        {
            SetRewardAsClaimed();
        }
    }

    public FMAchievementState GetCurrentState()
    {
        return States[CurrentIndex];
    }


    public void RewardClicked()
    {
        Debug.Log("reward clicked, state  " + GetCurrentState());
        if (GetCurrentState() != FMAchievementState.Unclaimed)
        {
            //we should update in case there are locked achievements to show the progress bar
            UpdateData();
            return;
        }

        //update client only
        FMPlayfabUserAchievement.Instance.GetUserAchievementFromKey(Key).States[CurrentIndex] = FMAchievementState.Claimed;
        States[CurrentIndex] = FMPlayfabUserAchievement.Instance.GetUserAchievementFromKey(Key).States[CurrentIndex];

        if (OnReward != null)
        {
            OnReward(this);
            //changing icon to achievement icon and adding some animation if it's the first time
            TweenAlpha tween1 = spriteAchievement.GetComponents<TweenAlpha>()[0];
            TweenAlpha tween2 = spriteAchievement.GetComponents<TweenAlpha>()[1];

            tween1.PlayForward();
            EventDelegate.Add(tween1.onFinished, () =>
            {
                spriteAchievement.spriteName = achievementIconName;
                tween2.ResetToBeginning();
                tween2.PlayForward();
            });
        }
        UpdateData();
    }

    /// <summary>
    /// this will be the last state when all achievements related to that statistic are obtained
    /// </summary>
    public void SetRewardAsClaimed()
    {
        buttonReward.GetComponentInChildren<UILabel>().text = "Claimed";
        buttonReward.defaultColor = new Color32(94, 188, 84, 200);
        States[CurrentIndex] = FMAchievementState.Claimed;
    }
}
