using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using HomaGames.HomaBelly;
using UnityEngine;
using Facebook.Unity;
using GameAnalyticsSDK;
using GameAnalyticsSDK.Events;
using GameAnalyticsSDK.Wrapper;
using Newtonsoft.Json;
using UnityEngine.Networking;

public class PerformanceController : MonoBehaviour
{
    static readonly HttpClient client = new HttpClient();

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
        Thread();
    }

    private async void Thread()
    {
        /*Debug.Log("Starting test Thread");
        
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
        
        Debug.Log("Game Analytics Event");
        var tempDictionary = new Dictionary<string, object>() {{"Key1", "hola"}, {"Key2", 10}};
        GA_Design.NewEvent("DummyEvent", 99f,tempDictionary );
        //Debug.Log("Facebook Event");
        //FB.LogAppEvent("DummyEvent", 99f, tempDictionary);
        
        yield return Ninja.JumpToUnity;
        AndroidJavaObject androidJavaObject = new AndroidJavaObject("org.json.JSONObject", json);*/

        Debug.Log("LOADING "+RemoteConfigurationConstants
            .TRACKING_FILE_RESOURCES);
        Debug.Log($"Resources Load "+Resources.Load(RemoteConfigurationConstants
            .TRACKING_FILE_RESOURCES));
        Debug.Log($"Resources Load With Type "+Resources.Load<TextAsset>(RemoteConfigurationConstants
            .TRACKING_FILE_RESOURCES));
        Dictionary<string,object> result = await FileUtilities.LoadAndDeserializeJsonFromResources<Dictionary<string, object>>(RemoteConfigurationConstants
            .TRACKING_FILE_RESOURCES);
        Debug.Log("LOAED "+result.Count);
    }

    public void GPDRTest()
    {
        HomaGames.GDPR.Manager.Instance.Show(true,false);
    }
}
