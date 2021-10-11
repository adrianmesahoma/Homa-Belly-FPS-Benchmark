using System;
using System.Collections;
using System.Collections.Generic;
using HomaGames.GDPR;
using UnityEngine;
using UnityEngine.UI;

public class GDPRSettingsView : View<GDPRSettingsView>
{
    [SerializeField] private Toggle SettingsAnalyticsAcceptanceToggle;
    [SerializeField] private Toggle SettingsTailoredAdsAcceptanceToggle;
    [SerializeField] private Transform _backgroundClickBlocker;
    [SerializeField] private CanvasGroup mainCanvasGroup;
    protected override Transform backgroundClickBlocker => _backgroundClickBlocker;
    
    private void OnEnable()
    {
        SettingsAnalyticsAcceptanceToggle.onValueChanged.AddListener(OnSettingsAnalyticsAcceptanceChanged);
        SettingsTailoredAdsAcceptanceToggle.onValueChanged.AddListener(OnTailoredAdsAcceptanceChanged);
        SettingsAnalyticsAcceptanceToggle.isOn = Manager.Instance.IsAnalyticsGranted();
        SettingsTailoredAdsAcceptanceToggle.isOn = Manager.Instance.IsTailoredAdsGranted();
        MakeCanvasGroupVisible();
    }

#if UNITY_ANDROID
    private void Update()
    {
        if (IsVisible && Input.GetKeyDown(KeyCode.Escape))
        {
            Dismiss();
        }
    }
#endif

    private void MakeCanvasGroupVisible()
    {
        // Sanity check making a double call to this won't make any effect
        if (mainCanvasGroup != null && mainCanvasGroup.alpha == 0)
        {
            mainCanvasGroup.alpha = 1.0f;
        }
    }

    private void OnDisable()
    {
        SettingsAnalyticsAcceptanceToggle.onValueChanged.RemoveListener(OnSettingsAnalyticsAcceptanceChanged);
        SettingsTailoredAdsAcceptanceToggle.onValueChanged.RemoveListener(OnTailoredAdsAcceptanceChanged);
    }

    private void OnTailoredAdsAcceptanceChanged(bool arg0)
    {
        PlayerPrefs.SetInt(Constants.PersistenceKey.TAILORED_ADS, SettingsTailoredAdsAcceptanceToggle.isOn ? 1 : 0);
    }

    private void OnSettingsAnalyticsAcceptanceChanged(bool arg0)
    {
        PlayerPrefs.SetInt(Constants.PersistenceKey.ANALYTICS_TRACKING, arg0 ? 1 : 0);
    }

    protected override void OnEnter()
    {
        
    }

    protected override void OnDismiss()
    {
        // Try to inform any possible instance of Homa Belly with user's choices
        HomaBellyUtilities.InformHomaBellyWithGDPRChoices(Manager.Instance.IsAboveRequiredAge(),
            Manager.Instance.IsTermsAndConditionsAccepted(),
            Manager.Instance.IsAnalyticsGranted(),
            Manager.Instance.IsTailoredAdsGranted());
        PlayerPrefs.Save();
    }
}