using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
public class UIControllerForGame : MonoBehaviour
{
    public Text hintText;
    public TextMeshProUGUI stageText;
    public TextMeshProUGUI packageName;

    public GameObject pauseScene;
    public GameObject wonUi;

    public GameObject dotAnim;

    void Start()
    {
        UpdateHint();
        CUtils.ShowBannerAd();
        CUtils.ChangeGameMusic();
    }

    public void PlayButtonSound()
    {
        Sound.instance.PlayButton();
    }

    public void UpdateHint()
    {
        hintText.text = "" + PlayerData.instance.NumberOfHints;

        int level = LevelData.levelSelected;
        int world = LevelData.worldSelected;

        stageText.text = "阶段 " + level;
        packageName.text = LevelData.worldNames[world - 1];
    }

    public void ShowPauseScene()
    {
        pauseScene.SetActive(true);
    }

    public void ResumeGame()
    {
        pauseScene.SetActive(false);
    }

    public void OpenStageMode()
    {
        UIController.mode = UIController.UIMODE.OPENLEVELSCREEN;
        SceneManager.LoadScene(0);
    }
    public void OpenHomeMode()
    {
        UIController.mode = UIController.UIMODE.OPENPLAYSCREEN;
        SceneManager.LoadScene(0);
    }

    public void ShowWinUi()
    {
        int world = LevelData.worldSelected;
        int stage = LevelData.levelSelected;

        if (!PlayerData.instance.IsLevelCrossed(world, stage) && LevelData.isLevelIsHintLevel(world, stage))
        {
            var freeHint = LevelData.hintGainForWorld[world - 1];
            PlayerData.instance.NumberOfHints += freeHint;
            PlayerData.instance.SaveData();
            //wonUi.transform.GetChild(0).Find("HintAdded").GetComponent<Text>().text = "Congrats! You got " + freeHint + " free hints";
        }
        else
        {
            //wonUi.transform.GetChild(0).Find("HintAdded").GetComponent<Text>().text = "";
        }

        Sound.instance.Play(Sound.Others.Win);
        PlayerData.instance.SetLevelCrossed(LevelData.worldSelected, stage);
        wonUi.SetActive(true);

        Timer.Schedule(this, 0.3f, () =>
        {
            CUtils.ShowInterstitialAd();
        });
    }

    public void LoadNextLevel()
    {
        Sound.instance.PlayButton();
        int stage = LevelData.levelSelected;

        if (stage == LevelData.totalLevelsPerWorld)
        {
            UIController.mode = UIController.UIMODE.OPENWORLDSCREEN;
            SceneManager.LoadScene(0);
            return;
        }

        LevelData.levelSelected++;
        SceneManager.LoadScene(1);
    }

    public void ShowAnimationOnAllNodes()
    {
        GameObject.FindObjectOfType<DotAnimation>().gameObject.SetActive(false);
        WaysUI[] allUis = GameObject.FindObjectsOfType<WaysUI>();
        List<Vector3> dotAnimations = new List<Vector3>();

        foreach (WaysUI wayUi in allUis)
        {
            for (int i = 0; i < 2; i++)
            {
                Vector3 pos = wayUi.childPos(i);

                if (!dotAnimations.Contains(pos))
                {
                    GameObject an = Instantiate(dotAnim) as GameObject;
                    an.GetComponent<DotAnimation>().setTargetScale(2.5f);
                    an.GetComponent<DotAnimation>().setEnableAtPosition(true, pos);
                    an.GetComponent<DotAnimation>().scalingSpeed = 2.5f;
                    dotAnimations.Add(pos);
                }
            }
        }

        Invoke("ShowWinUi", 1.3f);
    }
}
