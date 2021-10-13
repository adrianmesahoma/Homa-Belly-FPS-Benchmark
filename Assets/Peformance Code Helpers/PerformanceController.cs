using System;
using System.Collections;
using System.Collections.Generic;
using HomaGames.HomaBelly;
using UnityEngine;
using CielaSpike;
using Newtonsoft.Json;

public class PerformanceController : MonoBehaviour
{
    private static bool status = false;
    private void Awake()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        Application.targetFrameRate = 60;
        //Events.onBannerAdLoadedEvent += OnBannerAdLoadedEvent;
    }

    public void InitializeHomaBellyHandler()
    {
        HomaBelly.Instance.ManualInitialize();
    }

    public void LoadBannerHandler()
    {
        HomaBelly.Instance.LoadBanner(HomaGames.HomaBelly.BannerSize.BANNER, HomaGames.HomaBelly.BannerPosition.BOTTOM);
    }

    public void SHowBannerHandler()
    {
        HomaBelly.Instance.ShowBanner();
    }

    public void HideBannerHandler()
    {
        HomaBelly.Instance.HideBanner();
    }

    private void OnBannerAdLoadedEvent(string obj)
    {
        HomaBelly.Instance.ShowBanner();
    }

    public void ShowIntersitialHandler()
    {
        HomaBelly.Instance.ShowInsterstitial();
    }

    public void TestThread()
    {
        this.StartCoroutineAsync(Thread());
    }

    private IEnumerator Thread()
    {
        Debug.Log("Starting test Thread");
        
        Debug.Log("Test Application.IsEditor");
        Debug.Log("Application.isEditor: "+Application.isEditor);
        Debug.Log("Access to singleton");
        Debug.Log("HomaBelly.Instance: "+(HomaBelly.Instance != null));
        Debug.Log("Serialize Dictionary");
        Dictionary<string, object> args = new Dictionary<string, object>()
        {
            {"Key1", true},
            {"Key2", 1},
            {"Key3", "asasas"},
            {"Key4", 0.5f},
        };
        string json = JsonConvert.SerializeObject(args, Formatting.None);
        Debug.Log("Json: "+json);
        Debug.Log("Create Android java object: "+json);
        Debug.Log("Access to status");
        Debug.Log("Status: "+status);
        yield return Ninja.JumpToUnity;
        AndroidJavaObject androidJavaObject = new AndroidJavaObject("org.json.JSONObject", json);

        yield break;
    }
}
