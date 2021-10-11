using System;
using System.Collections;
using System.Collections.Generic;
using HomaGames.HomaBelly;
using UnityEngine;

public class PerformanceController : MonoBehaviour
{
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
}
