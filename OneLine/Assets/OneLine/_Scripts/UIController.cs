using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using TTSDK.UNBridgeLib.LitJson;
using TTSDK;
using StarkSDKSpace;
using UnityEngine.Analytics;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
#if EASY_MOBILE
using EasyMobile;
#endif

public class UIController : MonoBehaviour
{

    public static int totalLevel = LevelData.totalLevelsPerWorld * LevelData.worldNames.Length;
    public static int totalLevelInWorld = LevelData.totalLevelsPerWorld;

    public List<int> UnLock=new List<int>();

    public Transform packagesContent, levelsContent;

    public enum UIMODE : int
    {
        OPENPLAYSCREEN,
        OPENLEVELSCREEN,
        OPENWORLDSCREEN
    }

    public static UIMODE mode = UIMODE.OPENPLAYSCREEN;

    public GameObject playScreen, shopScreen;
    public GameObject playBackGround;

    public GameObject levelScreen;

    public GameObject worldScene;

    public UnlockPackageDialog unlockPackageDialog;

    public Sprite disableSprite;
    public Sprite enableSprite;

    public GameObject soundButton;
    public Sprite soundOn;
    public Sprite soundOf;

    public GameObject musicButton;
    public Sprite musicOn;
    public Sprite musicOff;

    public Image shopImage;
    public Sprite rateSprite;

    public ShopDialog shopDialog;

    public string clickid;
    private StarkAdManager starkAdManager;
    // Use this for initialization
    void Start()
    {
        UnLock.Add(1);
        if (mode == UIMODE.OPENPLAYSCREEN)
        {
            EnablePlayScreen();
        }
        else if (mode == UIMODE.OPENWORLDSCREEN)
        {
            EnableWorldScreen();
        }
        else if (mode == UIMODE.OPENLEVELSCREEN)
        {
            EnableStageScreen(LevelData.worldSelected);
        }

        ChangeMusic(Music.instance.IsEnabled());
        ChangeSound(Sound.instance.IsEnabled());

        CUtils.ChangeGameMusic();

        CUtils.CloseBannerAd();

        if (!Purchaser.instance.isEnabled)
        {
            shopImage.sprite = rateSprite;
        }
        LoadData();

        ShowInterstitialAd("1286cn0mmi1611831j",
            () => {

            },
            (it, str) => {
                Debug.LogError("Error->" + str);
            });
    }

    public void PlayButtonSound()
    {
        Sound.instance.PlayButton();
    }

    public void EnablePlayScreen()
    {
        playScreen.SetActive(true);
        playBackGround.SetActive(true);

        levelScreen.SetActive(false);
        worldScene.SetActive(false);
    }

    public void EnableWorldScreen()
    {
        PrepareWorldScreen();
        playScreen.SetActive(false);
        playBackGround.SetActive(false);

        levelScreen.SetActive(false);
        worldScene.SetActive(true);
    }

    public void EnableStageScreen()
    {
        playScreen.SetActive(false);
        playBackGround.SetActive(false);

        levelScreen.SetActive(true);
        worldScene.SetActive(false);
    }

    // data for worlds
    private void PrepareWorldScreen()
    {
        int cCount = packagesContent.childCount;
        UpdateWorldTitle(worldScene.transform.Find("Title"));

        for (int i = 0; i < (cCount - 1); i++)
        {
            UpdateWorld(packagesContent.GetChild(i), i + 1);
        }
    }

    private void UpdateWorldTitle(Transform title)
    {
        //todo
        TextMeshProUGUI levels = title.GetComponentInChildren<TextMeshProUGUI>();
        levels.text = "" + PlayerData.instance.GetTotalLevelCrossed() + " / " + totalLevel;
    }

    private void UpdateWorld(Transform world, int index)
    {
        int isUnlocked = PlayerData.instance.LEVELUNLOCKED[index];

        var starObj = world.Find("Button/Star").gameObject;
        var progressTextObj = world.Find("Button/ProgressText").gameObject;
        var lockedTextObj = world.Find("Button/Locked").gameObject;
        var packageName = world.Find("PackageName").GetComponent<TextMeshProUGUI>();
        packageName.text = LevelData.worldNames[index - 1];

        if (index > 1 && isUnlocked == 0)
        {
            int prvLevelCrossed = PlayerData.instance.LevelCrossedForOneWorld(index - 1);

            if (prvLevelCrossed < LevelData.prvLevelToCrossToUnLock)
            {
                starObj.SetActive(false);
                progressTextObj.SetActive(false);
                lockedTextObj.SetActive(true);
                return;
            }
        }

        starObj.SetActive(true);
        progressTextObj.SetActive(true);
        lockedTextObj.SetActive(false);

        int levelCrossed = PlayerData.instance.LevelCrossedForOneWorld(index);

        TextMeshProUGUI levels = world.GetComponentInChildren<TextMeshProUGUI>();
        levels.text = "" + levelCrossed + "/" + totalLevelInWorld;
    }

    
    //data for level
    public void EnableStageScreen(int indexWorld)
    {
        bool isLocked=true;
        for(int i=0;i<UnLock.Count;i++)
        {
            if (indexWorld == UnLock[i])
            {
                isLocked = false;
            }
        }
        if (isLocked==false)
        {
            LevelSetup(indexWorld);
            EnableStageScreen();
        }
        else
        {
            ShowVideoAd("9a1hj4eafii1396khd",
            (bol) => {
                if (bol)
                {
                    PlayerData.instance.UnLockedLevelForWorld(indexWorld);
                    LevelSetup(indexWorld);
                    UnLock.Add(indexWorld);
                    SaveData();
                    EnableStageScreen();

                    clickid = "";
                    getClickid();
                    apiSend("game_addiction", clickid);
                    apiSend("lt_roi", clickid);


                }
                else
                {
                    StarkSDKSpace.AndroidUIManager.ShowToast("观看完整视频才能获取奖励哦！");
                }
            },
            (it, str) => {
                Debug.LogError("Error->" + str);
                //AndroidUIManager.ShowToast("广告加载异常，请重新看广告！");
            });
            
            
        }
    }
    void SaveData()
    {
        BinaryFormatter formatter = new BinaryFormatter();
        FileStream fileStream = new FileStream(Application.persistentDataPath + "/save.dat", FileMode.Create);

        formatter.Serialize(fileStream, UnLock);
        fileStream.Close();
    }
    void LoadData()
    {
        string path = Application.persistentDataPath + "/save.dat";
        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream fileStream = new FileStream(path, FileMode.Open);

            UnLock = formatter.Deserialize(fileStream) as List<int>;
            fileStream.Close();
        }
    }

    private void LevelSetup(int indexWorld)
    {
        LevelData.worldSelected = indexWorld;
        LevelScreenReader(indexWorld);
    }

    void LevelScreenReader(int indexWorld)
    {
        Transform top = levelScreen.transform.GetChild(1);
        top.Find("title").GetComponent<Text>().text = LevelData.worldNames[indexWorld - 1];
        top.Find("Score").GetComponent<Text>().text = PlayerData.instance.LevelCrossedForOneWorld(indexWorld) + "/" + totalLevelInWorld;

        // list of all levelssssss
        int largetLevel = PlayerData.instance.GetLargestLevel(indexWorld);
        for (int i = 0; i < LevelData.totalLevelsPerWorld; i++)
        {
            bool isShownHint = true;
            Transform child = levelsContent.GetChild(i);
            child.GetComponentInChildren<Text>().text = "" + (i + 1);
            Transform locked = child.Find("Locked");
            Transform unlocked = child.Find("Unlocked");

            if (i < largetLevel + 3)
            {
                locked.gameObject.SetActive(false);
                unlocked.gameObject.SetActive(true);

                if (PlayerData.instance.IsLevelCrossed(indexWorld, i + 1))
                {
                    isShownHint = false;
                    unlocked.Find("Star1").gameObject.SetActive(true);
                    unlocked.Find("Star2").gameObject.SetActive(false);
                }
                else
                {
                    unlocked.Find("Star1").gameObject.SetActive(false);
                    unlocked.Find("Star2").gameObject.SetActive(true);
                }

                child.GetComponent<Button>().interactable = true;
            }
            else
            {
                locked.gameObject.SetActive(true);
                unlocked.gameObject.SetActive(false);
                child.GetComponent<Button>().interactable = false;
            }
            if (isShownHint && LevelData.isLevelIsHintLevel(indexWorld, i + 1))
            {
                child.Find("Hint").gameObject.SetActive(true);
            }
            else
            {
                child.Find("Hint").gameObject.SetActive(false);
            }
        }
    }

    public void OnPackageUnlocked()
    {
        PrepareWorldScreen();
        unlockPackageDialog.gameObject.SetActive(false);
    }

    public void Share()
    {
#if UNITY_EDITOR
#if EASY_MOBILE
        Toast.instance.ShowMessage("This feature only works in Android and iOS");
#else
        Toast.instance.ShowMessage("See Console for more information");

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.Append("You need to import Easy Mobile Lite (free) to use this function. This is how to import:(Click this log to see full instruction)").AppendLine();
        sb.Append("1. Open the asset store: Window -> General -> Asset Store or Window -> Asset Store").AppendLine();
        sb.Append("2. Search: Easy Mobile Lite").AppendLine();
        sb.Append("3. Download and import it");

        Debug.LogWarning(sb.ToString());
#endif
#elif (UNITY_ANDROID || UNITY_IPHONE) && EASY_MOBILE
        StartCoroutine(DoShare());
#endif
    }

    private IEnumerator DoShare()
    {
        yield return new WaitForEndOfFrame();
#if EASY_MOBILE
        Sharing.ShareScreenshot("screenshot", "");
#endif
    }

    public void LoadLevel(int levelSelected)
    {
        PlayButtonSound();
        LevelData.levelSelected = levelSelected;
        SceneManager.LoadScene(1);
    }

    public void ToggleMusic()
    {
        bool isEnabled = !Music.instance.IsEnabled();
        Music.instance.SetEnabled(isEnabled, true);
        ChangeMusic(isEnabled);
    }

    void ChangeMusic(bool isEnabled)
    {
        if (isEnabled)
        {
            musicButton.GetComponent<Image>().sprite = musicOn;
        }
        else
        {
            musicButton.GetComponent<Image>().sprite = musicOff;
        }
    }

    public void ToggleSound()
    {
        bool isEnabled = !Sound.instance.IsEnabled();
        Sound.instance.SetEnabled(isEnabled);
        ChangeSound(isEnabled);
    }

    void ChangeSound(bool isEnabled)
    {
        if (isEnabled)
        {
            soundButton.GetComponent<Image>().sprite = soundOn;
        }
        else
        {
            soundButton.GetComponent<Image>().sprite = soundOf;
        }
    }

    public void OnShopClick()
    {
        if (Purchaser.instance.isEnabled)
        {
            shopDialog.Show();
        }
        else
        {
            CUtils.OpenStore();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (playScreen.activeSelf && !shopScreen.activeSelf)
            {
#if UNITY_ANDROID
                Application.Quit();
#endif
            }
            else if (worldScene.activeSelf && !unlockPackageDialog.gameObject.activeSelf)
            {
                EnablePlayScreen();
            }
            else if (levelScreen.activeSelf)
            {
                EnableWorldScreen();
            }
        }
    }



    public void getClickid()
    {
        var launchOpt = StarkSDK.API.GetLaunchOptionsSync();
        if (launchOpt.Query != null)
        {
            foreach (KeyValuePair<string, string> kv in launchOpt.Query)
                if (kv.Value != null)
                {
                    Debug.Log(kv.Key + "<-参数-> " + kv.Value);
                    if (kv.Key.ToString() == "clickid")
                    {
                        clickid = kv.Value.ToString();
                    }
                }
                else
                {
                    Debug.Log(kv.Key + "<-参数-> " + "null ");
                }
        }
    }

    public void apiSend(string eventname, string clickid)
    {
        TTRequest.InnerOptions options = new TTRequest.InnerOptions();
        options.Header["content-type"] = "application/json";
        options.Method = "POST";

        JsonData data1 = new JsonData();

        data1["event_type"] = eventname;
        data1["context"] = new JsonData();
        data1["context"]["ad"] = new JsonData();
        data1["context"]["ad"]["callback"] = clickid;

        Debug.Log("<-data1-> " + data1.ToJson());

        options.Data = data1.ToJson();

        TT.Request("https://analytics.oceanengine.com/api/v2/conversion", options,
           response => { Debug.Log(response); },
           response => { Debug.Log(response); });
    }


    /// <summary>
    /// </summary>
    /// <param name="adId"></param>
    /// <param name="closeCallBack"></param>
    /// <param name="errorCallBack"></param>
    public void ShowVideoAd(string adId, System.Action<bool> closeCallBack, System.Action<int, string> errorCallBack)
    {
        starkAdManager = StarkSDK.API.GetStarkAdManager();
        if (starkAdManager != null)
        {
            starkAdManager.ShowVideoAdWithId(adId, closeCallBack, errorCallBack);
        }
    }

    /// <summary>
    /// 播放插屏广告
    /// </summary>
    /// <param name="adId"></param>
    /// <param name="errorCallBack"></param>
    /// <param name="closeCallBack"></param>
    public void ShowInterstitialAd(string adId, System.Action closeCallBack, System.Action<int, string> errorCallBack)
    {
        starkAdManager = StarkSDK.API.GetStarkAdManager();
        if (starkAdManager != null)
        {
            var mInterstitialAd = starkAdManager.CreateInterstitialAd(adId, errorCallBack, closeCallBack);
            mInterstitialAd.Load();
            mInterstitialAd.Show();
        }
    }
}
