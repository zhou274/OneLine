using UnityEngine;
using UnityEngine.UI;

public class UnlockPackageDialog : MonoBehaviour
{
    public Text priceText, messageText, worldNameText;
    public GameObject playButton;

    private void Start()
    {
#if IAP && UNITY_PURCHASING
        Purchaser.instance.onItemPurchased += OnItemPurchased;
#endif

        if (!Purchaser.instance.isEnabled)
        {
            playButton.SetActive(false);
        }
    }

    public void Show(string[] worldsName, int showMessageForWorld)
    {
        string message = string.Format("To try {0} you must beat {1} stages on {2}", worldsName[showMessageForWorld - 1], LevelData.prvLevelToCrossToUnLock, worldsName[showMessageForWorld - 2]);
        messageText.text = message;
        worldNameText.text = worldsName[showMessageForWorld - 1];

        priceText.text = "$" + Purchaser.instance.iapItems[1].price;
        gameObject.SetActive(true);
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    public void OnUnlockPackage()
    {
#if IAP && UNITY_PURCHASING
        Purchaser.instance.BuyProduct(1);
#else
        Debug.LogError("Please enable, import and install Unity IAP to use this function");
#endif
    }

#if IAP && UNITY_PURCHASING
    private void OnItemPurchased(IAPItem item, int index)
    {
        // A consumable product has been purchased by this user.
        if (item.productType == PType.Consumable)
        {
            if (index == 1)
            {
                PlayerData.instance.UnLockedLevelForWorld(LevelData.pressedWorld);
                FindObjectOfType<UIController>().OnPackageUnlocked();
                Toast.instance.ShowMessage("Your purchase is successful", 2.5f);
                CUtils.SetBuyItem();
            }
        }
    }
#endif

#if IAP && UNITY_PURCHASING
    private void OnDestroy()
    {
        Purchaser.instance.onItemPurchased -= OnItemPurchased;
    }
#endif

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Close();
        }
    }
}
