﻿using UnityEngine.SceneManagement;
using System;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using UnityEngine.Networking;

#pragma warning disable 0618

public class CUtils
{
    public static void OpenStore()
    {
#if UNITY_EDITOR || UNITY_ANDROID
        Application.OpenURL("https://play.google.com/store/apps/details?id=" + Application.identifier);
#elif UNITY_IPHONE
		Application.OpenURL("https://itunes.apple.com/app/id" + GameConfig.instance.iosAppID);
#elif UNITY_STANDALONE_OSX
		Application.OpenURL("https://itunes.apple.com/app/id" + GameConfig.instance.macAppID);
#elif UNITY_WSA
        if (JobWorker.instance.onLink2Store != null) JobWorker.instance.onLink2Store();
#endif
    }

    public static void OpenStore(string id)
    {
#if UNITY_EDITOR || UNITY_ANDROID
        Application.OpenURL("https://play.google.com/store/apps/details?id=" + id);
#elif UNITY_IPHONE
		Application.OpenURL("https://itunes.apple.com/app/id" + id);
#elif UNITY_STANDALONE_OSX
		Application.OpenURL("https://itunes.apple.com/app/id" + id);
#elif UNITY_WSA
        if (JobWorker.instance.onLink2Store != null) JobWorker.instance.onLink2Store();
#endif
    }

    public static void LikeFacebookPage(string faceID)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
	    Application.OpenURL("fb://page/" + faceID);
#else
        Application.OpenURL("https://www.facebook.com/" + faceID);
#endif
        SetLikeFbPage(faceID);
    }

    public static void SetBuyItem()
    {
        SetBool("buy_item", true);
    }

    public static void SetRemoveAds(bool value)
    {
        SetBool("remove_ads", value);
    }

    public static bool IsAdsRemoved()
    {
        return GetBool("remove_ads");
    }

    public static bool IsBuyItem()
    {
        return GetBool("buy_item", false);
    }

    public static void SetRateGame()
    {
        SetBool("rate_game", true);
    }

    public static bool IsGameRated()
    {
        return GetBool("rate_game", false);
    }

    public static void SetLikeFbPage(string id)
    {
        SetBool("like_page_" + id, true);
    }

    public static bool IsLikedFbPage(string id)
    {
        return GetBool("like_page_" + id, false);
    }

    #region Double
    public static void SetDouble(string key, double value)
    {
        PlayerPrefs.SetString(key, DoubleToString(value));
    }

    public static double GetDouble(string key, double defaultValue)
    {
        string defaultVal = DoubleToString(defaultValue);
        return StringToDouble(PlayerPrefs.GetString(key, defaultVal));
    }

    public static double GetDouble(string key)
    {
        return GetDouble(key, 0d);
    }

    private static string DoubleToString(double target)
    {
        return target.ToString("R");
    }

    private static double StringToDouble(string target)
    {
        if (string.IsNullOrEmpty(target))
            return 0d;

        return double.Parse(target);
    }
    #endregion

    #region Bool
    public static void SetBool(string key, bool value)
    {
        PlayerPrefs.SetInt(key, value ? 1 : 0);
    }

    public static bool GetBool(string key, bool defaultValue = false)
    {
        int defaultVal = defaultValue ? 1 : 0;
        return PlayerPrefs.GetInt(key, defaultVal) == 1;
    }
    #endregion


    public static double GetCurrentTime()
    {
        TimeSpan span = DateTime.Now.Subtract(new DateTime(1970, 1, 1, 0, 0, 0));
        return span.TotalSeconds;
    }

    public static double GetCurrentTimeInDays()
    {
        TimeSpan span = DateTime.Now.Subtract(new DateTime(1970, 1, 1, 0, 0, 0));
        return span.TotalDays;
    }

    public static double GetCurrentTimeInMills()
    {
        TimeSpan span = DateTime.Now.Subtract(new DateTime(1970, 1, 1, 0, 0, 0));
        return span.TotalMilliseconds;
    }

    public static T GetRandom<T>(params T[] arr)
    {
        return arr[UnityEngine.Random.Range(0, arr.Length)];
    }

    public static bool IsActionAvailable(String action, int time, bool availableFirstTime = true)
    {
        if (!PlayerPrefs.HasKey(action + "_time")) // First time.
        {
            if (availableFirstTime == false)
            {
                SetActionTime(action);
            }
            return availableFirstTime;
        }

        int delta = (int)(GetCurrentTime() - GetActionTime(action));
        return delta >= time;
    }

    public static double GetActionDeltaTime(String action)
    {
        if (GetActionTime(action) == 0)
            return 0;
        return GetCurrentTime() - GetActionTime(action);
    }

    public static void SetActionTime(String action)
    {
        SetDouble(action + "_time", GetCurrentTime());
    }

    public static void SetActionTime(String action, double time)
    {
        SetDouble(action + "_time", time);
    }

    public static double GetActionTime(String action)
    {
        return GetDouble(action + "_time");
    }

    public static void ShowInterstitialAd()
    {
        if (IsAdsRemoved()) return;
        if (GameConfig.instance.showInterstitialAdAfterLevel > PlayerData.instance.GetTotalLevelCrossed()) return;

        if (IsActionAvailable("show_ads", GameConfig.instance.adPeriod))
        {
#if UNITY_ANDROID || UNITY_IPHONE
            if (!AdmobController.instance.ShowInterstitial())
            {
                AdmobController.instance.RequestInterstitial();
            }
#else
            
#endif
        }
    }

    public static void LoadScene(int sceneIndex, bool useScreenFader = false)
    {
        if (useScreenFader)
        {
            ScreenFader.instance.GotoScene(sceneIndex);
        }
        else
        {
            SceneManager.LoadScene(sceneIndex);
        }
    }

    public static void ReloadScene(bool useScreenFader = false)
    {
        int currentIndex = SceneManager.GetActiveScene().buildIndex;
        LoadScene(currentIndex, useScreenFader);
    }

    public static void ShowBannerAd()
    {
        if (IsAdsRemoved()) return;

#if UNITY_ANDROID || UNITY_IPHONE
        AdmobController.instance.ShowBanner();
#endif
    }

    public static void CloseBannerAd()
    {
#if UNITY_ANDROID || UNITY_IPHONE
        AdmobController.instance.DestroyBanner();
#endif
    }

    public static string BuildStringFromCollection(ICollection values, char split = '|')
    {
        string results = "";
        int i = 0;
        foreach (var value in values)
        {
            results += value;
            if (i != values.Count - 1)
            {
                results += split;
            }
            i++;
        }
        return results;
    }

    public static List<T> BuildListFromString<T>(string values, char split = '|')
    {
        List<T> list = new List<T>();
        if (string.IsNullOrEmpty(values))
            return list;

        string[] arr = values.Split(split);
        foreach (string value in arr)
        {
            if (string.IsNullOrEmpty(value)) continue;
            T val = (T)Convert.ChangeType(value, typeof(T));
            list.Add(val);
        }
        return list;
    }

    public static IEnumerator LoadPicture(string url, string fileName, Action<Texture2D> callback)
    {
        string localPath = GetLocalPath(fileName);
        bool loaded = LoadFromLocal(callback, localPath);

        if (!loaded)
        {
            UnityWebRequest www = UnityWebRequestTexture.GetTexture(url, false);
            yield return www.SendWebRequest();
            if (!www.isNetworkError && !www.isHttpError)
            {
                Texture2D texture = ((DownloadHandlerTexture)www.downloadHandler).texture;
                callback(texture);
                File.WriteAllBytes(localPath, texture.EncodeToPNG());
            }
            else
            {
                LoadFromLocal(callback, localPath);
            }
        }
    }

    public static string GetLocalPath(string fileName)
    {
        return Path.Combine(Application.persistentDataPath, fileName);
    }

    public static IEnumerator CachePicture(string url, string fileName, Action<bool> result)
    {
        string localPath = GetLocalPath(fileName);
        WWW www = new WWW(url);
        yield return www;
        if (www.isDone && string.IsNullOrEmpty(www.error))
        {
            File.WriteAllBytes(localPath, www.bytes);
            if (result != null) result(true);
        }
        else
        {
            if (result != null) result(false);
        }
    }

    public static bool LoadFromLocal(Action<Texture2D> callback, string localPath)
    {
        if (File.Exists(localPath))
        {
            var bytes = File.ReadAllBytes(localPath);
            var tex = new Texture2D(0, 0, TextureFormat.RGBA32, false);
            tex.LoadImage(bytes);
            if (tex != null)
            {
                callback(tex);
                return true;
            }
        }
        return false;
    }

    public static Sprite CreateSprite(Texture2D texture, int width, int height)
    {
        return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100.0f);
    }

    static float lastChangeTime = int.MinValue;
    static int selectMusicIndex;
    static Music.Type[] musicTypes = { Music.Type.MainMusic1, Music.Type.MainMusic2, Music.Type.MainMusic3 };

    public static void ChangeGameMusic()
    {
        if (Time.time - lastChangeTime > 60)
        {
            Music.instance.Play(musicTypes[selectMusicIndex]);
            selectMusicIndex = (selectMusicIndex + 1) % musicTypes.Length;

            lastChangeTime = Time.time;
        }
    }

    static CUtils()
    {
        selectMusicIndex = UnityEngine.Random.Range(0, musicTypes.Length);
    }
}
