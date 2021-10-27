using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using HomaGames.HomaBelly;
using UnityEngine;
using Facebook.Unity;
using GameAnalyticsSDK.Events;
using Newtonsoft.Json;
using UnityEngine.Networking;

public class PerformanceController : MonoBehaviour
{
    private static bool status = false;
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

    private async void ReadAsync()
    {
        Debug.Log("ReadAsync");
        string result = null;
        try	
        {
            var path = HomaBellyAppLovinMaxConstants.CONFIG_FILE;
            
            if (path.Contains("://") || path.Contains(":///"))
            {
                // Wait until async operation has finished
                UnityWebRequest www = UnityWebRequest.Get(path);
                www.SendWebRequest();
                await Task.Run(delegate
                {
                    while(!www.isDone)
                    {
                        continue;
                    }
                });
                result =  www.downloadHandler.text;
            }
            else
            {
                result = File.ReadAllText(path);
            }
        }
        catch(HttpRequestException e)
        {
            Debug.LogError("\nException Caught!");	
            Debug.LogErrorFormat("Message :{0} ",e.Message);
        }

        Debug.Log("RESULT: "+result);
    }
    
    private async void ReadAsync1()
    {
        Debug.Log("ReadAsync1");
        string result = null;
        try	
        {
            var path = HomaBellyAppLovinMaxConstants.CONFIG_FILE;
            
            if (path.Contains("://") || path.Contains(":///"))
            {
                // Wait until async operation has finished
                UnityWebRequest www = UnityWebRequest.Get(path);
                www.SendWebRequest();
                await Task.Run(delegate
                {
                    while(!www.isDone)
                    {
                        continue;
                    }
                    result =  www.downloadHandler.text;
                });
            }
            else
            {
                result = File.ReadAllText(path);
            }
        }
        catch(HttpRequestException e)
        {
            Debug.LogError("\nException Caught!");	
            Debug.LogErrorFormat("Message :{0} ",e.Message);
        }

        Debug.Log("RESULT: "+result);
    }
    
    private async void ReadAsync2()
    {
        Debug.Log("ReadAsync2");
        string result = null;
        try	
        {
            var path = HomaBellyAppLovinMaxConstants.CONFIG_FILE;
            
            if (path.Contains("://") || path.Contains(":///"))
            {
                // Wait until async operation has finished
                UnityWebRequest www = UnityWebRequest.Get(path);
                await Task.Run(delegate
                {
                    www.SendWebRequest();
                    while(!www.isDone)
                    {
                        continue;
                    }
                    result =  www.downloadHandler.text;
                });
            }
            else
            {
                result = File.ReadAllText(path);
            }
        }
        catch(HttpRequestException e)
        {
            Debug.LogError("\nException Caught!");	
            Debug.LogErrorFormat("Message :{0} ",e.Message);
        }

        Debug.Log("RESULT: "+result);
    }
    
    private async void ReadAsync3()
    {
        Debug.Log("ReadAsync3");
        string result = null;
        try	
        {
            var path = HomaBellyAppLovinMaxConstants.CONFIG_FILE;
            
            if (path.Contains("://") || path.Contains(":///"))
            {
                // Wait until async operation has finished
                await Task.Run(delegate
                {
                    UnityWebRequest www = UnityWebRequest.Get(path);
                    www.SendWebRequest();
                    while(!www.isDone)
                    {
                        continue;
                    }
                    result =  www.downloadHandler.text;
                });
            }
            else
            {
                result = File.ReadAllText(path);
            }
        }
        catch(HttpRequestException e)
        {
            Debug.LogError("\nException Caught!");	
            Debug.LogErrorFormat("Message :{0} ",e.Message);
        }

        Debug.Log("RESULT: "+result);
    }

    public void GPDRTest()
    {
        HomaGames.GDPR.Manager.Instance.Show(true,false);
    }
}
