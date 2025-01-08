﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class BaseController : MonoBehaviour {
    public GameObject gameMaster;
    public string sceneName;

    protected virtual void Awake()
    {
        if (GameMaster.instance == null && gameMaster != null)
            Instantiate(gameMaster);
    }

    protected virtual void Start()
    {
        if (JobWorker.instance.onEnterScene != null)
        {
            JobWorker.instance.onEnterScene(sceneName);
        }

#if UNITY_WSA && !UNITY_EDITOR
        StartCoroutine(SavePrefs());
#endif
    }

    private IEnumerator SavePrefs()
    {
        while (true)
        {
            yield return new WaitForSeconds(5);
            PlayerPrefs.Save();
        }
    }
}
