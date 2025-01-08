using UnityEngine;
using UnityEngine.UI;

public class ShopDialog : MonoBehaviour
{
    public Text[] numHintTexts, contentTexts, priceTexts;
    public Text removeAdPriceText;
    public Text balanceText;
    public GameObject removeAdButton, restoreButton;

    private void OnEnable()
    {
        removeAdButton.SetActive(!CUtils.IsAdsRemoved());
        restoreButton.SetActive(!CUtils.IsAdsRemoved());
    }

    private void Start()
    {
        Purchaser.instance.onItemPurchased += OnItemPurchased;

        for(int i = 0; i < numHintTexts.Length; i++)
        {
            numHintTexts[i].text = Purchaser.instance.iapItems[i+2].value.ToString();
            contentTexts[i].text = Purchaser.instance.iapItems[i+2].value + " hints";
            priceTexts[i].text = "$" + Purchaser.instance.iapItems[i+2].price;
        }

        removeAdPriceText.text = "$" + Purchaser.instance.iapItems[0].price;
    }

    public void OnBuyProduct(int index)
	{
        Sound.instance.PlayButton();
        Purchaser.instance.BuyProduct(index);
    }

    public void Show()
    {
        gameObject.SetActive(true);
        UpdateBalance();
    }

    public void Close()
    {
        gameObject.SetActive(false);
        Sound.instance.PlayButton();
    }

    public void RestorePurchases()
    {
        Sound.instance.PlayButton();
        Purchaser.instance.RestorePurchases();
    }

    public void UpdateBalance()
    {
        balanceText.text = "" + PlayerData.instance.NumberOfHints;
    }

    private void OnItemPurchased(IAPItem item, int index)
    {
        // A consumable product has been purchased by this user.
        if (item.productType == PType.Consumable)
        {
            if (index != 1)
            {
                PlayerData.instance.NumberOfHints += item.value;
                PlayerData.instance.SaveData();

                UpdateBalance();
                var controller = FindObjectOfType<UIControllerForGame>();
                if (controller != null) controller.UpdateHint();

                Toast.instance.ShowMessage("Your purchase is successful", 2.5f);
                CUtils.SetBuyItem();
            }
        }
        // Or ... a non-consumable product has been purchased by this user.
        else if (item.productType == PType.NonConsumable)
        {
            if (index == 0)
            {
                CUtils.SetRemoveAds(true);
                removeAdButton.SetActive(false);
                restoreButton.SetActive(false);
                Toast.instance.ShowMessage("Your purchase is successful");
            }
        }
        // Or ... a subscription product has been purchased by this user.
        else if (item.productType == PType.Subscription)
        {
            // TODO: The subscription item has been successfully purchased, grant this to the player.
        }
    }

    private void OnDestroy()
    {
        Purchaser.instance.onItemPurchased -= OnItemPurchased;
    }
}
