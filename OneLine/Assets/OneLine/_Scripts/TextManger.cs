using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TTSDK.UNBridgeLib.LitJson;
using TTSDK;
using StarkSDKSpace;
using UnityEngine.Analytics;

public class TextManger : MonoBehaviour
{
    public GameObject textPrefab;
    public ShopDialog shopUI;
    public RewardedVideoButton rewardedButton;
    public string clickid;
    private StarkAdManager starkAdManager;

    private int showHint = 0; //with every incr show 3 hints;

    public void showHints()
    {
        ShowVideoAd("192if3b93qo6991ed0",
            (bol) => {
                if (bol)
                {
                    showHint++;

                    int maxPoint = showHint * 3;
                    int minPoint = maxPoint - 2;
                    //1 mean show 1 2 3
                    //2 mean show 4 , 5 ,6;
                    //3 mean show 7, 8 , 9
                    WaysUI[] allWays = GameObject.FindObjectsOfType<WaysUI>();

                    int count = allWays.Length;
                    bool isAnyHintShown = false;
                    for (int i = 0; i < count; i++)
                    {
                        WaysUI wayUi = allWays[i];
                        Ways way = wayUi.getWayModel();

                        string forStart = "";
                        string forEnd = "";

                        if (way.pathTag > 1)
                        {
                            string sol = way.solutions;

                            string[] allSol = sol.Split(new char[] { ',' });
                            int len = allSol.Length;

                            for (int j = 0; j < len; j++)
                            {
                                string[] allSolRead = allSol[j].Split(new char[] { '_' });

                                int solIndex = int.Parse(allSolRead[0]);
                                if (solIndex >= minPoint && solIndex <= maxPoint)
                                {
                                    if (allSolRead[1].Contains("s"))
                                    {
                                        forStart = solIndex + "";
                                        forEnd = (solIndex + 1) + "";
                                    }
                                    else
                                    {
                                        forStart = (solIndex + 1) + "";
                                        forEnd = (solIndex) + "";
                                    }

                                    createOrUpdateTextM(way, forStart, forEnd);
                                    isAnyHintShown = true;
                                }

                            }
                        }
                        else if (way.solutionPosition >= minPoint && way.solutionPosition <= maxPoint)
                        {
                            forStart = way.solutionPosition + "";
                            forEnd = (way.solutionPosition + 1) + "";
                            createOrUpdateTextM(way, forStart, forEnd);
                            isAnyHintShown = true;
                        }
                    }

                    if (isAnyHintShown)
                    {
                        PlayerData.instance.NumberOfHints -= 1;
                        PlayerData.instance.SaveData();

                        GameObject.FindObjectOfType<UIControllerForGame>().UpdateHint();
                    }

                    GameObject.FindObjectOfType<AnimationHandler>().runAnimations();



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

    TextM CreatObj(Vector3 pos)
    {
        GameObject go = Instantiate(textPrefab) as GameObject;
        go.transform.position = pos;
        go.transform.parent = transform;

        return go.GetComponent<TextM>();
    }

    public void createOrUpdateTextM(Ways way, string forStart, string forEnd)
    {
        Vector3 startPos = GridManager.GetGridManger().GetPosForGrid(way.startingGridPosition);
        Vector3 endPos = GridManager.GetGridManger().GetPosForGrid(way.endGridPositon);

        startPos.z = -1;
        endPos.z = -1;
        TextM stPosText = findTextMAtPos(startPos);
        TextM endPosText = findTextMAtPos(endPos);

        if (stPosText == null)
        {
            stPosText = CreatObj(startPos);
        }

        if (endPosText == null)
        {
            endPosText = CreatObj(endPos);
        }

        GameObject.FindObjectOfType<AnimationHandler>().addAnimationToRun(stPosText.transform.position, int.Parse(forStart), stPosText);
        GameObject.FindObjectOfType<AnimationHandler>().addAnimationToRun(endPosText.transform.position, int.Parse(forEnd), endPosText);
    }

    public TextM findTextMAtPos(Vector3 pos)
    {
        TextM[] allMeshe = GameObject.FindObjectsOfType<TextM>();

        if (allMeshe.Length > 0)
        {
            int len = allMeshe.Length;

            for (int i = 0; i < len; i++)
            {
                TextM tM = allMeshe[i];

                if (tM.transform.position.Equals(pos))
                {
                    return tM;
                }
            }
        }
        return null;
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
