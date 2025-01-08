using UnityEngine;

public class RewardedVideoButton : MonoBehaviour
{
    public void OnClick()
    {
#if UNITY_EDITOR
        OnUserEarnedReward();
#else
        if (IsAdAvailable())
        {
            AdmobController.onUserEarnedReward = OnUserEarnedReward;
            AdmobController.instance.ShowRewardedAd();
        }
        else
        {
            Toast.instance.ShowMessage("Ad is not available now, please wait..");
        }
#endif

        Sound.instance.PlayButton();
    }

    public void OnUserEarnedReward()
    {
        Toast.instance.ShowMessage("You've received a free hint", 2.5f);
        PlayerData.instance.NumberOfHints += 1;
        PlayerData.instance.SaveData();

        var controller = FindObjectOfType<UIControllerForGame>();
        if (controller != null) controller.UpdateHint();

        var shopDialog = FindObjectOfType<ShopDialog>();
        if (shopDialog != null) shopDialog.UpdateBalance();
    }

    private bool IsAdAvailable()
    {
        return AdmobController.instance.rewardedAd.IsLoaded();
    }
}
