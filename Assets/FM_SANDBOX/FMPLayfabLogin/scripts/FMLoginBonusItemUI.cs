using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum FMLoginBonusState {
    Unclaimed,
    Today,
    Claimed
}

public class FMLoginBonusItemUI : MonoBehaviour
{
    [HideInInspector]
    public FMLoginBonusState State = FMLoginBonusState.Unclaimed;
    [SerializeField]
    UISprite rewardSprite;
    [SerializeField]
    UILabel rewardLab;    

    UISprite bgSprite;
    Color32 claimedColor = new Color32(60, 140,60, 255); //3C8C3C
    Color32 todayColor = new Color32(142, 142, 60, 255); //8E8E3C

    void Start()
    {
        bgSprite = GetComponent<UISprite>();
    }

    public void SetData(FMLoginBonusItem item) {
        string isCurrency = item.Reward.Type == FMRewardType.Currency ? "x " : "";
        rewardLab.text = isCurrency+ item.Reward.GetValue();
        rewardSprite.spriteName = GetRewardSprite(item.Reward);
        State = item.State;

        //sometimes start is called after this
        if (bgSprite == null) {
            bgSprite = GetComponent<UISprite>();
        }

        switch (State) {
            case FMLoginBonusState.Today:
                bgSprite.color = todayColor;
                break;
            case FMLoginBonusState.Claimed:
                bgSprite.color = claimedColor;
                break;
        }
    }

    string GetRewardSprite(FMRewardItem reward)
    {
        switch (reward.RewardTypeValue)
        {
            case "co":
                return "coin_CO";
            case "pc":
                return "coin_PC";
            default:
                return "generic_item";
        }
    }

    void Update()
    {
        //TODO make color tween?
    }
}
