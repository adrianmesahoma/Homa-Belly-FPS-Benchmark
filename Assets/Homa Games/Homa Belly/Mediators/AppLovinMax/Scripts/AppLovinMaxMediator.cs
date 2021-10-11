using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using HomaGames.HomaBelly.Utilities;
using UnityEngine;

namespace HomaGames.HomaBelly
{
    public class AppLovinMaxMediator : IMediatorWithInitializationCallback
    {
        private Dictionary<string, object> configurationData;
        private Events events = new Events();

        // Retry attempts
        private NetworkHelper networkHelper = new NetworkHelper();
        private int bannerLoadRetryAttempt = 0;
        private int interstitialLoadRetryAttempt = 0;
        private int rewardedVideoLoadRetryAttempt = 0;
        private bool bannerReloadScheduled = false;
        private bool interstitialReloadScheduled = false;
        private bool rewardedReloadScheduled = false;

        /// <summary>
        /// Dictionary containing the default Ad IDs for the current platform.
        /// </summary>
        private Dictionary<AdType, string> defaultAdIds = new Dictionary<AdType, string>();

        private enum AdType
        {
            REWARDED_VIDEO,
            INTERSTITIAL,
            BANNER
        }

        public void Initialize(Action onInitialized = null)
        {
            configurationData = LoadConfigurationData();
            if (configurationData != null)
            {
                string sdkKey = configurationData.ContainsKey("s_sdk_key") ? (string)configurationData["s_sdk_key"] : "";
                if (!string.IsNullOrEmpty(sdkKey))
                {
                    // Gather default Ad Unity IDs
                    GetDefaultAdIds();

                    // Initialize AppLovin SDK
                    MaxSdkCallbacks.OnSdkInitializedEvent += (MaxSdkBase.SdkConfiguration sdkConfiguration) => {
#if UNITY_IOS || UNITY_IPHONE
                        InvokeFacebookAudienceNetworkAdvertiserFlag();
#endif

                        // AppLovin SDK is initialized
                        // Preload interstitial and rewarded video ads to be cached
                        MaxSdk.LoadInterstitial(GetAdIdOrDefault(AdType.INTERSTITIAL, null));
                        MaxSdk.LoadRewardedAd(GetAdIdOrDefault(AdType.REWARDED_VIDEO, null));
                        MaxSdk.CreateBanner(GetAdIdOrDefault(AdType.BANNER, null), MaxSdkBase.BannerPosition.BottomCenter);
                        MaxSdk.SetBannerExtraParameter(GetAdIdOrDefault(AdType.BANNER, null), "adaptive_banner", "true");

                        networkHelper.OnNetworkReachabilityChange += OnNetworkReachabilityChange;
                        networkHelper.StartListening();

                        if (onInitialized != null)
                        {
                            onInitialized.Invoke();
                        }
                    };

                    MaxSdk.SetSdkKey(sdkKey);
                    MaxSdk.InitializeSdk();
                    MaxSdk.SetVerboseLogging(false);
                }
                else
                {
                    HomaGamesLog.Warning($"[AppLovin Max Mediator] Could not find sdk_key for AppLovin Max");
                }
            }
            else
            {
                HomaGamesLog.Warning($"[AppLovin Max Mediator] Could not find configuration data for AppLovin Max");
            }
        }

        public void Initialize()
        {
            Initialize(() =>
            {
                HomaGamesLog.Debug($"[AppLovin Max Mediator] Initialized successfully");
            });
        }

        public void DestroyBanner(string placementId = null)
        {
            if (MaxSdk.IsInitialized())
            {
                HideBanner(placementId);
            }
            else
            {
                HomaGamesLog.Warning("[AppLovin Max Mediator] Not initialized");
            }
        }

        public void HideBanner(string placementId = null)
        {
            if (MaxSdk.IsInitialized())
            {
                MaxSdk.HideBanner(GetAdIdOrDefault(AdType.BANNER, placementId));
            }
            else
            {
                HomaGamesLog.Warning("[AppLovin Max Mediator] Not initialized");
            }
        }

        public bool IsInterstitialAvailable(string placementId = null)
        {
            if (MaxSdk.IsInitialized())
            {
                return MaxSdk.IsInterstitialReady(GetAdIdOrDefault(AdType.INTERSTITIAL, placementId));
            }
            else
            {
                HomaGamesLog.Warning("[AppLovin Max Mediator] Not initialized");
            }

            return false;
        }

        public bool IsRewardedVideoAdAvailable(string placementId = null)
        {
            if (MaxSdk.IsInitialized())
            {
                return MaxSdk.IsRewardedAdReady(GetAdIdOrDefault(AdType.REWARDED_VIDEO, placementId));
            }
            else
            {
                HomaGamesLog.Warning("[AppLovin Max Mediator] Not initialized");
            }

            return false;
        }

        public void LoadBanner(BannerSize size, BannerPosition position, string placementId = null, UnityEngine.Color bannerBackgroundColor = default)
        {
            if (MaxSdk.IsInitialized())
            {
                MaxSdkBase.BannerPosition finalPosition = MaxSdkBase.BannerPosition.BottomCenter;
                switch (position)
                {
                    case BannerPosition.TOP:
                        finalPosition = MaxSdkBase.BannerPosition.TopCenter;
                        break;
                }
                var finalPlacement = GetAdIdOrDefault(AdType.BANNER, placementId);

                MaxSdk.CreateBanner(finalPlacement, finalPosition);
                MaxSdk.SetBannerExtraParameter(finalPlacement, "adaptive_banner", "true");

                // If background color is WHITE with ALPHA to 0f, do not set it so it
                // will be fully transparent (no background at all)
                if (bannerBackgroundColor != new Color(1f, 1f, 1f, 0f))
                {
                    // Otherwise, set background banner color, being white as default
                    if (bannerBackgroundColor == default)
                    {
                        bannerBackgroundColor = Color.white;
                    }

                    MaxSdk.SetBannerBackgroundColor(finalPlacement, bannerBackgroundColor);
                }

                // Applovin MAX SDK won't call BannerAdLoaded event upon creation
                HomaGamesLog.Debug($"[AppLovin Max Mediator] BannerAdLoadedEvent");
                events.OnBannerAdLoadedEvent(finalPlacement);
            }
            else
            {
                HomaGamesLog.Warning("[AppLovin Max Mediator] Not initialized");
            }
        }

        public void OnApplicationPause(bool pause)
        {
            // NO-OP
        }

        public void RegisterEvents()
        {
            // Attach callbacks based on the ad format(s) you are using
            MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent += OnAdRevenuePaidEvent;
            MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent += OnAdRevenuePaidEvent;
            MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent += OnAdRevenuePaidEvent;
            MaxSdkCallbacks.MRec.OnAdRevenuePaidEvent += OnAdRevenuePaidEvent;

            // Banner
            MaxSdkCallbacks.Banner.OnAdClickedEvent += BannerAdClickedEvent;
            MaxSdkCallbacks.Banner.OnAdLoadFailedEvent += BannerAdLoadFailedEvent;
            MaxSdkCallbacks.Banner.OnAdLoadedEvent += BannerAdLoadedEvent;

            // Video Ads
            MaxSdkCallbacks.Rewarded.OnAdLoadedEvent += OnRewardedAdLoadedEvent;
            MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent += OnRewardedAdFailedEvent;
            MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent += OnRewardedAdFailedToDisplayEvent;
            MaxSdkCallbacks.Rewarded.OnAdDisplayedEvent += OnRewardedAdDisplayedEvent;
            MaxSdkCallbacks.Rewarded.OnAdClickedEvent += OnRewardedAdClickedEvent;
            MaxSdkCallbacks.Rewarded.OnAdHiddenEvent += OnRewardedAdDismissedEvent;
            MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent += OnRewardedAdReceivedRewardEvent;

            // Interstitials
            MaxSdkCallbacks.Interstitial.OnAdClickedEvent += OnInterstitialClickedEvent;
            MaxSdkCallbacks.Interstitial.OnAdDisplayedEvent += OnInterstitialShownEvent;
            MaxSdkCallbacks.Interstitial.OnAdLoadedEvent += OnInterstitialLoadedEvent;
            MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent += OnInterstitialFailedEvent;
            MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent += InterstitialFailedToDisplayEvent;
            MaxSdkCallbacks.Interstitial.OnAdHiddenEvent += OnInterstitialDismissedEvent;

        }

        public void SetUserIsAboveRequiredAge(bool consent)
        {
            MaxSdk.SetIsAgeRestrictedUser(!consent);
        }

        public void SetTermsAndConditionsAcceptance(bool consent)
        {
            // NO-OP
        }

        public void SetAnalyticsTrackingConsentGranted(bool consent)
        {
            // NO-OP
        }

        public void SetTailoredAdsConsentGranted(bool consent)
        {
            MaxSdk.SetHasUserConsent(consent);
        }

        public void ShowBanner(string placementId = null)
        {
            if (MaxSdk.IsInitialized())
            {
                MaxSdk.ShowBanner(GetAdIdOrDefault(AdType.BANNER, placementId));
            }
            else
            {
                HomaGamesLog.Warning("[AppLovin Max Mediator] Not initialized");
            }
        }

        public void ShowInterstitial(string placementId = null)
        {
            if (MaxSdk.IsInitialized())
            {
                HomaGamesLog.Debug($"[AppLovin Max Mediator] Request show interstitial");
                if (MaxSdk.IsInterstitialReady(GetAdIdOrDefault(AdType.INTERSTITIAL, placementId)))
                {
                    HomaGamesLog.Debug($"[AppLovin Max Mediator] Interstitial available");
                    MaxSdk.ShowInterstitial(GetAdIdOrDefault(AdType.INTERSTITIAL, placementId));
                }
                else
                {
                    HomaGamesLog.Debug($"[AppLovin Max Mediator] Interstitial not available");
                    OnInterstitialFailedEvent("Interstitial not available", new MaxSdkBase.ErrorInfo(new Dictionary<string, string> {
                        { "errorCode", "999" },
                        { "errorMessage", "Interstitial not available" }
                    }));
                }
            }
            else
            {
                HomaGamesLog.Warning("[AppLovin Max Mediator] Not initialized");
                OnInterstitialFailedEvent("Not initialized", new MaxSdkBase.ErrorInfo(new Dictionary<string, string> {
                        { "errorCode", "999" },
                        { "errorMessage", "Not initialized" }
                }));
            }
        }

        public void ShowRewardedVideoAd(string placementId = null)
        {
            if (MaxSdk.IsInitialized())
            {
                // If rewarded video ad is ready, show it
                string finalID = GetAdIdOrDefault(AdType.REWARDED_VIDEO, placementId);
                if (MaxSdk.IsRewardedAdReady(finalID))
                {
                    HomaGamesLog.Debug($"[AppLovin Max Mediator] Video Ad available. Showing...");
                    MaxSdk.ShowRewardedAd(finalID);
                }
                else
                {
                    HomaGamesLog.Debug($"[AppLovin Max Mediator] Video Ad not available");
                    OnRewardedAdFailedEvent("Rewarded video not available", new MaxSdkBase.ErrorInfo(new Dictionary<string, string> {
                        { "errorCode", "999" },
                        { "errorMessage", "Rewarded video not available" }
                    }));
                }
            }
            else
            {
                HomaGamesLog.Warning("[AppLovin Max Mediator] Not initialized");
                OnRewardedAdFailedEvent("Not initialized", new MaxSdkBase.ErrorInfo(new Dictionary<string, string> {
                        { "errorCode", "999" },
                        { "errorMessage", "Not initialized" }
                }));
            }
        }

        public void ValidateIntegration()
        {
            // Show Mediation Debugger
            MaxSdk.ShowMediationDebugger();
        }

        #region Private helpers

        private void OnNetworkReachabilityChange(NetworkReachability reachability)
        {
            if (reachability != NetworkReachability.NotReachable)
            {
                HomaGamesLog.Debug("[AppLovin Max Mediator] Internet reachable. Reloading ads if necessary");
                ReloadAdAfterFailure(null, AdType.BANNER);
                ReloadAdAfterFailure(null, AdType.REWARDED_VIDEO);
                ReloadAdAfterFailure(null, AdType.INTERSTITIAL);
            }
        }

        /// <summary>
        /// Callback invoked for ULRD
        /// </summary>
        /// <param name="adUnitId"></param>
        /// <param name="adInfo"></param>
        private void OnAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            AdRevenueData data = new AdRevenueData();

            data.AdPlatform = "AppLovin";
            data.Currency = "USD";
            data.Revenue = Convert.ToDouble(adInfo.Revenue, CultureInfo.InvariantCulture);
            data.AdUnitId = adInfo.AdUnitIdentifier;
            data.NetworkName = adInfo.NetworkName;
            data.AdPlacamentName = adInfo.Placement;

            HomaBelly.Instance.TrackAdRevenue(data);
        }

        /// <summary>
        /// Call AudienceNetwork.AdSettings.SetAdvertiserTrackingFlag by reflection
        /// to avoid crashes if the integration does not contain FacebookAudienceNetwork adapter
        /// </summary>
        private void InvokeFacebookAudienceNetworkAdvertiserFlag()
        {
            try
            {
                Type adSettingsType = (from assembly in AppDomain.CurrentDomain.GetAssemblies()
                                       from type in assembly.GetTypes()
                                       where type.Namespace == "AudienceNetwork" && type.Name == "AdSettings"
                                       select type).FirstOrDefault();
                if (adSettingsType != null)
                {
                    MethodInfo methodInfo = adSettingsType.GetMethod("SetAdvertiserTrackingFlag", BindingFlags.Static | BindingFlags.Public);
                    if (methodInfo != null)
                    {
                        methodInfo.Invoke(null, null);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"AudienceNetwork.AdSettings.SetAdvertiserTrackingFlag() method failed to invoke: {e.Message}");
            }
        }

        /// <summary>
        /// Obtain the default Ad IDs depending on the running platform. If the running
        /// platform IDs are not found, fallack ones will be the IDs from the other
        /// platform.
        /// </summary>
        private void GetDefaultAdIds()
        {
            if (configurationData != null)
            {
                // Gather Android Ad IDs
                Dictionary<AdType, string> androidAdIds = new Dictionary<AdType, string>();
                if (configurationData.ContainsKey("s_android_default_rewarded_video_ad_unit_id") && !string.IsNullOrEmpty((string)configurationData["s_android_default_rewarded_video_ad_unit_id"]))
                {
                    androidAdIds.Add(AdType.REWARDED_VIDEO, (string)configurationData["s_android_default_rewarded_video_ad_unit_id"]);
                }

                if (configurationData.ContainsKey("s_android_default_interstitial_ad_unit_id") && !string.IsNullOrEmpty((string)configurationData["s_android_default_interstitial_ad_unit_id"]))
                {
                    androidAdIds.Add(AdType.INTERSTITIAL, (string)configurationData["s_android_default_interstitial_ad_unit_id"]);
                }

                if (configurationData.ContainsKey("s_android_default_banner_ad_unit_id") && !string.IsNullOrEmpty((string)configurationData["s_android_default_banner_ad_unit_id"]))
                {
                    androidAdIds.Add(AdType.BANNER, (string)configurationData["s_android_default_banner_ad_unit_id"]);
                }

                // Gather iOS Ad IDs
                Dictionary<AdType, string> iOSAdIds = new Dictionary<AdType, string>();
                if (configurationData.ContainsKey("s_ios_default_rewarded_video_ad_unit_id") && !string.IsNullOrEmpty((string)configurationData["s_ios_default_rewarded_video_ad_unit_id"]))
                {
                    iOSAdIds.Add(AdType.REWARDED_VIDEO, (string)configurationData["s_ios_default_rewarded_video_ad_unit_id"]);
                }

                if (configurationData.ContainsKey("s_ios_default_interstitial_ad_unit_id") && !string.IsNullOrEmpty((string)configurationData["s_ios_default_interstitial_ad_unit_id"]))
                {
                    iOSAdIds.Add(AdType.INTERSTITIAL, (string)configurationData["s_ios_default_interstitial_ad_unit_id"]);
                }

                if (configurationData.ContainsKey("s_ios_default_banner_ad_unit_id") && !string.IsNullOrEmpty((string)configurationData["s_ios_default_banner_ad_unit_id"]))
                {
                    iOSAdIds.Add(AdType.BANNER, (string)configurationData["s_ios_default_banner_ad_unit_id"]);
                }

                // Dump to default depending on the platform
#if UNITY_ANDROID
                // If Android IDs found, use them
                if (androidAdIds.Count > 0)
                {
                    defaultAdIds = new Dictionary<AdType, string>(androidAdIds);
                }
                else
                {
                    // If not, try to use iOS ones
                    defaultAdIds = new Dictionary<AdType, string>(iOSAdIds);
                }
#elif UNITY_IOS
                // If iOS IDs found, use them
                if (iOSAdIds.Count > 0)
                {
                    defaultAdIds = new Dictionary<AdType, string>(iOSAdIds);
                }
                else
                {
                    // If not, try to use Android ones
                    defaultAdIds = new Dictionary<AdType, string>(androidAdIds);
                }
#endif
            }
        }

        private Dictionary<string, object> LoadConfigurationData()
        {
#if UNITY_EDITOR
            if (!File.Exists(HomaBellyAppLovinMaxConstants.CONFIG_FILE))
            {
                return null;
            }
#endif

            string configJson = FileUtilities.ReadAllText(HomaBellyAppLovinMaxConstants.CONFIG_FILE);
            return Json.Deserialize(configJson) as Dictionary<string, object>;
        }

        private string GetAdIdOrDefault(AdType adType, string placement)
        {
            switch (adType)
            {
                case AdType.REWARDED_VIDEO:
                    return string.IsNullOrEmpty(placement) ? defaultAdIds[AdType.REWARDED_VIDEO] : placement;
                case AdType.INTERSTITIAL:
                    return string.IsNullOrEmpty(placement) ? defaultAdIds[AdType.INTERSTITIAL] : placement;
                case AdType.BANNER:
                    return string.IsNullOrEmpty(placement) ? defaultAdIds[AdType.BANNER] : placement;
            }

            return "";
        }

        /// <summary>
        /// Reloads an ad after it failed to be loaded. This method
        /// will trigger a reload with a cretain delay in time, increasing
        /// that delay up to a max of 64 seconds (6 retries)
        /// </summary>
        /// <param name="placement"></param>
        /// <param name="adType"></param>
        private void ReloadAdAfterFailure(string placement, AdType adType)
        {
            if ((adType == AdType.BANNER && bannerReloadScheduled)
                || (adType == AdType.INTERSTITIAL && interstitialReloadScheduled)
                || (adType == AdType.REWARDED_VIDEO && rewardedReloadScheduled))
            {
                HomaGamesLog.Debug($"[AppLovin Max Mediator] {adType} reload already scheduled");
                return;
            }

            int maxRetries = 6;
            int reloadAttempt = 1;
            switch (adType)
            {
                case AdType.BANNER:
                    bannerLoadRetryAttempt++;
                    bannerReloadScheduled = true;
                    reloadAttempt = bannerLoadRetryAttempt;
                    break;
                case AdType.INTERSTITIAL:
                    interstitialLoadRetryAttempt++;
                    interstitialReloadScheduled = true;
                    reloadAttempt = interstitialLoadRetryAttempt;
                    break;
                case AdType.REWARDED_VIDEO:
                    rewardedVideoLoadRetryAttempt++;
                    rewardedReloadScheduled = true;
                    reloadAttempt = rewardedVideoLoadRetryAttempt;
                    break;
            }

            // Calculate the delay in ms
            int retryDelayInMs = (int)Math.Pow(2, Math.Min(maxRetries, reloadAttempt)) * 1000;
            HomaGamesLog.Debug($"[AppLovin Max Mediator] Scheduling {adType} reload after {retryDelayInMs / 1000} seconds");
            Task.Delay(retryDelayInMs).ContinueWith((result) =>
            {
                switch (adType)
                {
                    case AdType.BANNER:
                        bannerReloadScheduled = false;
                        HomaGamesLog.Debug($"[AppLovin Max Mediator] {adType} reload triggered");
                        MaxSdk.CreateBanner(GetAdIdOrDefault(AdType.BANNER, placement), MaxSdkBase.BannerPosition.BottomCenter);
                        break;
                    case AdType.INTERSTITIAL:
                        interstitialReloadScheduled = false;
                        if (!IsInterstitialAvailable(placement))
                        {
                            HomaGamesLog.Debug($"[AppLovin Max Mediator] {adType} reload triggered");
                            MaxSdk.LoadInterstitial(GetAdIdOrDefault(AdType.INTERSTITIAL, placement));
                        }
                        else
                        {
                            HomaGamesLog.Debug($"[AppLovin Max Mediator] {adType} reload not triggered because ad is already available");
                        }
                        break;
                    case AdType.REWARDED_VIDEO:
                        rewardedReloadScheduled = false;
                        if (!IsRewardedVideoAdAvailable(placement))
                        {
                            HomaGamesLog.Debug($"[AppLovin Max Mediator] {adType} reload triggered");
                            MaxSdk.LoadRewardedAd(GetAdIdOrDefault(AdType.REWARDED_VIDEO, placement));
                        }
                        else
                        {
                            HomaGamesLog.Debug($"[AppLovin Max Mediator] {adType} reload not triggered because ad is already available");
                        }
                        break;
                }

            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        #endregion

        #region Banner Ad Events

        private void BannerAdLoadedEvent(string placement, MaxSdkBase.AdInfo arg2)
        {
            // Reset reload attempts after load successful
            bannerLoadRetryAttempt = 0;

            HomaGamesLog.Debug($"[AppLovin Max Mediator] OnBannerAdLoadedEvent");
            events.OnBannerAdLoadedEvent(placement);
        }

        private void BannerAdClickedEvent(string placement, MaxSdkBase.AdInfo adInfo)
        {
            HomaGamesLog.Debug($"[AppLovin Max Mediator] BannerAdClickedEvent");
            events.OnBannerAdClickedEvent(placement);
        }

        private void BannerAdLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        {
            HomaGamesLog.Debug($"[AppLovin Max Mediator] BannerAdLoadFailedEvent with error code {errorInfo?.Code}: {errorInfo?.Message}");
            events.OnBannerAdLoadFailedEvent(adUnitId);

            ReloadAdAfterFailure(adUnitId, AdType.BANNER);
        }

        #endregion

        #region Rewarded Video Ad Events

        private void OnRewardedAdReceivedRewardEvent(string arg1, MaxSdkBase.Reward reward, MaxSdkBase.AdInfo adInfo)
        {
            HomaGamesLog.Debug($"[AppLovin Max Mediator] OnRewardedAdReceivedRewardEvent");
            events.OnRewardedVideoAdRewardedEvent(new VideoAdReward(
                reward.Label,
                reward.Amount));
        }

        private void OnRewardedAdDismissedEvent(string obj, MaxSdkBase.AdInfo adInfo)
        {
            // Request a new rewarded video ad to be cached
            MaxSdk.LoadRewardedAd(GetAdIdOrDefault(AdType.REWARDED_VIDEO, null));

            HomaGamesLog.Debug($"[AppLovin Max Mediator] OnRewardedAdDismissedEvent");
            events.OnRewardedVideoAdClosedEvent(obj);
        }

        private void OnRewardedAdClickedEvent(string obj, MaxSdkBase.AdInfo adInfo)
        {
            HomaGamesLog.Debug($"[AppLovin Max Mediator] OnRewardedAdClickedEvent");
            events.OnRewardedVideoAdClickedEvent(obj);
        }

        private void OnRewardedAdDisplayedEvent(string obj, MaxSdkBase.AdInfo adInfo)
        {
            HomaGamesLog.Debug($"[AppLovin Max Mediator] OnRewardedAdDisplayedEvent");
            events.OnRewardedVideoAdStartedEvent(obj);
        }

        private void OnRewardedAdFailedToDisplayEvent(string arg1, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
        {
            // Request a new rewarded video ad to be cached
            MaxSdk.LoadRewardedAd(GetAdIdOrDefault(AdType.REWARDED_VIDEO, null));

            HomaGamesLog.Debug($"[AppLovin Max Mediator] OnRewardedAdFailedToDisplayEvent with error code {errorInfo?.Code}: {errorInfo?.Message}");
            events.OnRewardedVideoAdShowFailedEvent(arg1);
        }

        private void OnRewardedAdFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        {
            HomaGamesLog.Debug($"[AppLovin Max Mediator] OnRewardedAdFailedEvent with error code {errorInfo?.Code}: {errorInfo?.Message}");
            events.OnRewardedVideoAdShowFailedEvent(adUnitId);

            ReloadAdAfterFailure(adUnitId, AdType.REWARDED_VIDEO);
        }

        private void OnRewardedAdLoadedEvent(string placement, MaxSdkBase.AdInfo adInfo)
        {
            // Reset reload attempts after load successful
            rewardedVideoLoadRetryAttempt = 0;

            HomaGamesLog.Debug($"[AppLovin Max Mediator] OnRewardedAdLoadedEvent");
            events.OnRewardedVideoAvailabilityChangedEvent(true, placement);
        }

        #endregion

        #region Interstitial Events

        private void OnInterstitialDismissedEvent(string placement, MaxSdkBase.AdInfo adInfo)
        {
            HomaGamesLog.Debug($"[AppLovin Max Mediator] OnInterstitialDismissedEvent");
            MaxSdk.LoadInterstitial(GetAdIdOrDefault(AdType.INTERSTITIAL, placement));

            events.OnInterstitialAdClosedEvent();
        }

        private void InterstitialFailedToDisplayEvent(string placement, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
        {
            HomaGamesLog.Debug($"[AppLovin Max Mediator] InterstitialAdShowFailedEvent with error code {errorInfo?.Code}: {errorInfo?.Message}");
            events.OnInterstitialAdShowFailedEvent(placement);
            MaxSdk.LoadInterstitial(GetAdIdOrDefault(AdType.INTERSTITIAL, placement));
        }

        private void OnInterstitialFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        {
            HomaGamesLog.Debug($"[AppLovin Max Mediator] OnInterstitialFailedEvent with error code {errorInfo?.Code}: {errorInfo?.Message}");
            events.OnInterstitialAdLoadFailedEvent();

            ReloadAdAfterFailure(adUnitId, AdType.BANNER);
        }

        private void OnInterstitialLoadedEvent(string placement, MaxSdkBase.AdInfo adInfo)
        {
            // Reset reload attempts after load successful
            interstitialLoadRetryAttempt = 0;

            HomaGamesLog.Debug($"[AppLovin Max Mediator] OnInterstitialLoadedEvent");
            events.OnInterstitialAdReadyEvent();
        }

        private void OnInterstitialClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            HomaGamesLog.Debug($"[AppLovin Max Mediator] InterstitialAdClickedEvent");
            events.OnInterstitialAdClickedEvent(adUnitId);
        }

        private void OnInterstitialShownEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            HomaGamesLog.Debug($"[AppLovin Max Mediator] InterstitialAdShowSucceededEvent");
            events.OnInterstitialAdShowSucceededEvent(adUnitId);
        }
        #endregion
    }
}