using PlayFab;
using PlayFab.ClientModels;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DisplayAchievementTest : MonoBehaviour
{
    [SerializeField]
    UIGrid grid;
    [SerializeField]
    GameObject particleSpawner;

    List<FMParticleSpawner> spawners = new List<FMParticleSpawner>();

    void Start()
    {
        spawners.Add(particleSpawner.GetComponentsInChildren<FMParticleSpawner>()[0]); //check
        spawners.Add(particleSpawner.GetComponentsInChildren<FMParticleSpawner>()[1]); //coin CO
        spawners.Add(particleSpawner.GetComponentsInChildren<FMParticleSpawner>()[2]); //coin PC

        if (!FMPlayfabLogin.IsClientLoggedIn())
        {
            FMPlayfabLogin.LoginCustomID("64646464", LoadPlayfabData);
        }
        LoadPlayfabData(null);
    }

    void LoadPlayfabData(LoginResult logResult)
    {
        //getting user statistics
        Debug.Log("on achievements, displaying statistics "+ ClientSessionData.Instance.Statistics.Count);
        if (ClientSessionData.Instance.Statistics.Count == 0) { 
        PlayfabUtils.Instance.GetPlayerStatistics(null, (res) =>
        {
            FMPlayfabUserStatistics.StoreItemsFromJson(res);
        }, PlayFabError);
        }

        //getting user achievements if any
        if (ClientSessionData.Instance.UserAchievements.Count == 0){
            PlayfabUtils.Instance.GetUserReadOnlyData(new List<string> { "fm_user_achievements" },
                result =>
                {
                    FMPlayfabUserAchievement.Instance.StoreItemsFromJson(result);
                    if (FMPlayfabUserAchievement.Items.Count != 0){
                        ClientSessionData.Instance.UserAchievements = FMPlayfabUserAchievement.Items;
                    }

                    //getting reward and achievement list
                    PlayfabUtils.Instance.GetTitleData(new List<string>() { "fm_achievements", "fm_rewards" }, (res) =>
                    {
                        FMPlayfabAchievements.Instance.StoreItemsFromJson(res);
                        FMPlayfabReward.StoreItemsFromJson(res);

                        //define OnGetAchievement to know what to do with the acheivements
                        FMPlayfabAchievements.Instance.OnGetAchievement = DisplayAchievements;
                        FMPlayfabUserAchievement.Instance.OnClaimReward = OnClaimedRewards;
                        FMPlayfabAchievements.Instance.GetUserAchivements();
                    }, PlayFabError);
                }, PlayFabError);
        }
    }

    void PlayFabError(PlayFabError error)
    {
        Debug.Log("cannot hard login, " + error.ErrorMessage);
    }

    void DisplayAchievements(FMPlayfabUserAchievementResult rs)
    {
        if (rs.status == FMAchievementStatus.error)
        {
            Debug.Log(rs.ErrorMessage);
            return;
        }

        Debug.Log("display achievements "+ FMPlayfabAchievements.Items.Count);
        Debug.Log("display user achievements "+FMPlayfabUserAchievement.Items.Count);
        //adding elements in grid
        foreach (FMAchievementItem item in FMPlayfabAchievements.Items)
        {
            GameObject achievementPrefab = Instantiate(Resources.Load("FMAchievementItemUI")) as GameObject;
            FMAchievementItemUI itemData = achievementPrefab.GetComponent<FMAchievementItemUI>();

            int currenIndex = FMPlayfabUserAchievement.Instance.GetUserAchievementFromKey(item.Key) != null ?
                FMPlayfabUserAchievement.Instance.GetUserAchievementFromKey(item.Key).CurrenIndex : 0;
            
            itemData.SetData(item, GetRewardIconName(item, currenIndex));
            itemData.Achievement = item;

            switch (itemData.GetCurrentState())
            {
                case FMAchievementState.Unclaimed:
                    itemData.OnReward = OpenReward;
                    break;
                case FMAchievementState.Claimed:
                    itemData.SetRewardAsClaimed();
                    break;
            }

            //Add to grid
            achievementPrefab.transform.parent = grid.transform;
            achievementPrefab.transform.localScale = Vector3.one;
            grid.Reposition();
        }
    }

    /// <summary>
    /// this could change depending on the game and the atlas used
    /// </summary>
    /// <param name="achivement"></param>
    /// <param name="index"></param>
    /// <returns></returns>
    string GetRewardIconName(FMAchievementItem achivement, int index)
    {

        FMRewardItem reward = FMPlayfabReward.GetRewardFromKey(achivement.RewardKeys[index]);
        //default
        if (index == -1 || reward == null)
        {
            Debug.Log("reward null en "+ achivement.Key);
            return "achievement";
        }
        
        switch (reward.RewardTypeValue)
        {
            case "co":
            case "pc":
                return "coin";
            case "item":
                return "achievement";
        }
        return "";
    }

    public void GetSelectedRewards()
    {
        FMPlayfabUserAchievement.Instance.ClaimRewardsFromPlayfab();
    }

    /// <summary>
    /// if any state of the achievement is "unclaimed" we can get the reward
    /// </summary>
    /// <param name="itemUI"></param>
    void OpenReward(FMAchievementItemUI itemUI)
    {
        int achievementIndex = itemUI.CurrentIndex;
        string rewardKey = itemUI.Achievement.RewardKeys[achievementIndex];
        FMRewardItem reward = FMPlayfabReward.GetRewardFromKey(rewardKey);

        if (!string.IsNullOrEmpty(reward.ItemKey))
        {
            //TODO show item information
            //IVDialogDisplayer.DisplayNotice("you got " + reward.ItemKey, null);
            FMPlayfabUserAchievement.Instance.SetUserAchievementAsClaimedLocal(itemUI.Key, achievementIndex);
            return;
        }

        //spawming particles to currencies
        Debug.Log("reward type "+reward.RewardTypeValue);
        switch (reward.RewardTypeValue) {
            case "co":
                spawners[1].SpawnParticles(UICamera.mainCamera.ScreenToWorldPoint(Input.mousePosition));
                break;
            case "pc":
                spawners[2].SpawnParticles(UICamera.mainCamera.ScreenToWorldPoint(Input.mousePosition));
                break;
            case "item":
                spawners[0].SpawnParticles(UICamera.mainCamera.ScreenToWorldPoint(Input.mousePosition));
                break;
        }
        //particleSpawner.SpawnParticles(UICamera.mainCamera.ScreenToWorldPoint(Input.mousePosition));
        FMPlayfabUserAchievement.Instance.SetUserAchievementAsClaimedLocal(itemUI.Key, achievementIndex);
    }

    /// <summary>
    /// after updating to playfab we have this method in case we want to show something else, returns the rewards received
    /// </summary>
    /// <param name="res"></param>
    void OnClaimedRewards(FMPlayfabRewardsResult res)
    {
        if (res.status != FMRewardStatus.success)
        {
            Debug.Log("bad result");
            return;
        }

        for (int i = 0; i < res.ClaimedRewards.Count; i++)
        {
            Debug.Log("got " + res.ClaimedRewards[i].GetValue() + " " + res.ClaimedRewards[i].GetValue());
        }
    }
    
    public void BackToHome()
    {
        SceneManager.LoadScene("Home");
    }
}
