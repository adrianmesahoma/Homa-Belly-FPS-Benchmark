using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace HomaGames.HomaBelly
{
    public class HomaBelly : MonoBehaviour, IHomaBellyBridge
    {
        #region Public properties
        [SerializeField, Tooltip("Enable to see Debug logs")]
        private bool debugEnabled = true;
        public bool IsInitialized
        {
            get
            {
                return homaBridge.IsInitialized;
            }
        }
        #endregion

        #region Singleton pattern

        private static HomaBelly _instance;
        public static HomaBelly Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = GameObject.FindObjectOfType<HomaBelly>();
                    if (_instance == null)
                    {
                        HomaGamesLog.Warning("WARNING! Homa Belly not initialized. Please ensure Homa Belly prefab is present in your scene hierarchy. Prefab can be found under Assets/Homa Games/Homa Belly/Core/Prefabs folder");
                        GameObject homaBellyGameObject = new GameObject("Homa Belly");
                        _instance = homaBellyGameObject.AddComponent<HomaBelly>();
                    }
                }

                return _instance;
            }
        }

#endregion

#region Private properties
#if UNITY_EDITOR && UNITY_2019_1_OR_NEWER
        private HomaDummyBridge homaBridge = new HomaDummyBridge();
#else
        private HomaBridge homaBridge = new HomaBridge();
#endif
        private Events events = new Events();

        /// <summary>
        /// If network is not reachable, API calls will be stored in
        /// this queue. All Actions will be triggered when network is
        /// reachable again
        /// </summary>
        private Queue<Action> unreachableNetworkActionQueue = new Queue<Action>();
        private const int REACHABILITY_WAIT_MS = 3000;
        private Task unreachableNetworkTaskDelay;
        private bool IsNetworkReachable
        {
            get
            {
                return Application.internetReachability != NetworkReachability.NotReachable;
            }
        }
#endregion

        private void Awake()
        {
#region Singleton pattern
            if (_instance != null && _instance != this)
            {
                Destroy(this.gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(this.gameObject);
#endregion

            // Set Log Stack Traces
            /*Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
            Application.SetStackTraceLogType(LogType.Warning, StackTraceLogType.None);
            Application.SetStackTraceLogType(LogType.Error, StackTraceLogType.Full);
            Application.SetStackTraceLogType(LogType.Exception, StackTraceLogType.Full);*/

            // Initialization
            SetDebug(debugEnabled);
            HomaGamesLog.Debug("[Homa Belly] Initializing Homa Belly");
            //CheckNetworkReachabilityBeforeAction(() => homaBridge.Initialize());
        }

        public void ManualInitialize()
        {
            CheckNetworkReachabilityBeforeAction(() => homaBridge.Initialize());
        }

        private void OnApplicationPause(bool pause)
        {
            homaBridge.OnApplicationPause(pause);
        }

        /// <summary>
        /// Method filtering API calls on network reachabiliy.
        /// If reachable, executes it. If not, stores in in the
        /// #unreachableNetworkActionQueue to be executed when
        /// network is reachable again
        /// </summary>
        /// <param name="action">The action to be executed</param>
        private void CheckNetworkReachabilityBeforeAction(Action action)
        {
            if (IsNetworkReachable)
            {
                action.Invoke();
            }
            else
            {
                HomaGamesLog.Warning($"[Homa Belly] Network not reachable. Deferring call to {action} until network available again.");
                unreachableNetworkActionQueue.Enqueue(action);
                WaitForReachability();
            }
        }

        /// <summary>
        /// Method triggered when an API call is done and network is not reachable.
        /// This method will loop until network is reachable and execute
        /// all the deferred actions
        /// </summary>
        private void WaitForReachability()
        {
            // If network recovery task is already running
            if (unreachableNetworkTaskDelay == null
                || unreachableNetworkTaskDelay.IsCanceled
                || unreachableNetworkTaskDelay.IsCompleted
                || unreachableNetworkTaskDelay.IsFaulted)
            {
                // Task waiting REACHABILITY_WAIT_MS to retry any API call
                // If network is not reachable after that time, a new task
                // will be scheduled to try again, until network is reachable
                unreachableNetworkTaskDelay = Task.Delay(REACHABILITY_WAIT_MS)
                    .ContinueWith((result) =>
                    {
                        // If network is reachable and actions need to be
                        // triggered, proceed
                        if (IsNetworkReachable
                            && unreachableNetworkActionQueue != null
                            && unreachableNetworkActionQueue.Count > 0)
                        {
                            HomaGamesLog.Debug($"[Homa Belly] Network recovered. Executing deferred actions");
                            while (unreachableNetworkActionQueue.Count > 0)
                            {
                                Action action = unreachableNetworkActionQueue.Dequeue();
                                if (action != null)
                                {
                                    action.Invoke();
                                }
                            }

                            HomaGamesLog.Debug($"[Homa Belly] Deferred actions successfully invoked");
                        }
                    }, TaskScheduler.FromCurrentSynchronizationContext())
                    .ContinueWith((result) =>
                    {
                        // Network recovery task is done
                        unreachableNetworkTaskDelay = null;

                        // If network is still not reachable, reset task
                        if (!IsNetworkReachable)
                        { 
                            WaitForReachability();
                        }
                    }, TaskScheduler.FromCurrentSynchronizationContext());
            }
        }

        #region IHomaBellyBridge

        public void SetDebug(bool enabled)
        {
            HomaGamesLog.debugEnabled = enabled;
            homaBridge.SetDebug(enabled);
        }

        public void ValidateIntegration()
        {
            homaBridge.ValidateIntegration();
        }

        // Rewarded Video Ads
        public void ShowRewardedVideoAd(string placementId = null)
        {
            if (IsNetworkReachable)
            {
                homaBridge.ShowRewardedVideoAd(placementId);
            }
            else
            {
                events.OnRewardedVideoAdShowFailedEvent(placementId);
            }
        }

        public bool IsRewardedVideoAdAvailable(string placementId = null)
        {
            return homaBridge.IsRewardedVideoAdAvailable();
        }

        // Banners
        public void LoadBanner(string placementId)
        {
            if (IsNetworkReachable)
            {
                LoadBanner(BannerSize.BANNER, BannerPosition.BOTTOM, placementId);
            }
            else
            {
                events.OnBannerAdLoadFailedEvent(placementId);
            }
        }

        public void LoadBanner(UnityEngine.Color bannerBackgroundColor, string placementId = null)
        {
            if (IsNetworkReachable)
            {
                LoadBanner(BannerSize.BANNER, BannerPosition.BOTTOM, placementId, bannerBackgroundColor);
            }
            else
            {
                events.OnBannerAdLoadFailedEvent(placementId);
            }
        }

        public void LoadBanner(BannerPosition position, string placementId = null)
        {
            if (IsNetworkReachable)
            {
                LoadBanner(BannerSize.BANNER, position, placementId);
            }
            else
            {
                events.OnBannerAdLoadFailedEvent(placementId);
            }
        }

        public void LoadBanner(BannerSize size, string placementId = null)
        {
            if (IsNetworkReachable)
            {
                LoadBanner(size, BannerPosition.BOTTOM, placementId);
            }
            else
            {
                events.OnBannerAdLoadFailedEvent(placementId);
            }
        }

        public void LoadBanner(BannerSize size = BannerSize.BANNER, BannerPosition position = BannerPosition.BOTTOM, string placementId = null, Color bannerBackgroundColor = default)
        {
            if (IsNetworkReachable)
            {
                homaBridge.LoadBanner(size, position, placementId, bannerBackgroundColor);
            }
            else
            {
                events.OnBannerAdLoadFailedEvent(placementId);
            }
        }

        public void ShowBanner(string placementId = null)
        {
            homaBridge.ShowBanner();
        }

        public void HideBanner(string placementId = null)
        {
            homaBridge.HideBanner();
        }

        public void DestroyBanner(string placementId = null)
        {
            homaBridge.DestroyBanner();
        }

        public void ShowInsterstitial(string placementId = null)
        {
            if (IsNetworkReachable)
            {
                homaBridge.ShowInsterstitial(placementId);
            }
            else
            {
                events.OnInterstitialAdShowFailedEvent(placementId);
            }
        }

        public bool IsInterstitialAvailable(string placementId = null)
        {
            return homaBridge.IsInterstitialAvailable();
        }

        [HomaGames.HomaBelly.PreserveAttribute]
        public void SetUserIsAboveRequiredAge(bool consent)
        {
            homaBridge.SetUserIsAboveRequiredAge(consent);
        }

        [HomaGames.HomaBelly.PreserveAttribute]
        public void SetTermsAndConditionsAcceptance(bool consent)
        {
            homaBridge.SetTermsAndConditionsAcceptance(consent);
        }

        [HomaGames.HomaBelly.PreserveAttribute]
        public void SetAnalyticsTrackingConsentGranted(bool consent)
        {
            homaBridge.SetAnalyticsTrackingConsentGranted(consent);
        }

        [HomaGames.HomaBelly.PreserveAttribute]
        public void SetTailoredAdsConsentGranted(bool consent)
        {
            homaBridge.SetTailoredAdsConsentGranted(consent);
        }

#if UNITY_PURCHASING
        public void TrackInAppPurchaseEvent(UnityEngine.Purchasing.Product product, bool isRestored = false)
        {
            homaBridge.TrackInAppPurchaseEvent(product);
        }
#endif

        public void TrackInAppPurchaseEvent(string productId, string currencyCode, double unitPrice, string transactionId = null, string payload = null, bool isRestored = false)
        {
            homaBridge.TrackInAppPurchaseEvent(productId, currencyCode, unitPrice, transactionId, payload);
        }

        public void TrackResourceEvent(ResourceFlowType flowType, string currency, float amount, string itemType, string itemId)
        {
            homaBridge.TrackResourceEvent(flowType, currency, amount, itemType, itemId);
        }

        public void TrackProgressionEvent(ProgressionStatus progressionStatus, string progression01, int score = 0)
        {
            homaBridge.TrackProgressionEvent(progressionStatus, progression01, score);
        }

        public void TrackProgressionEvent(ProgressionStatus progressionStatus, string progression01, string progression02, int score = 0)
        {
            homaBridge.TrackProgressionEvent(progressionStatus, progression01, progression02, score);
        }

        public void TrackProgressionEvent(ProgressionStatus progressionStatus, string progression01, string progression02, string progression03, int score = 0)
        {
            homaBridge.TrackProgressionEvent(progressionStatus, progression01, progression02, progression03, score);
        }

        public void TrackErrorEvent(ErrorSeverity severity, string message)
        {
            homaBridge.TrackErrorEvent(severity, message);
        }

        public void TrackDesignEvent(string eventName, float eventValue = 0)
        {
            homaBridge.TrackDesignEvent(eventName, eventValue);
        }

        public void TrackAdEvent(AdAction adAction, AdType adType, string adNetwork, string adPlacementId)
        {
            homaBridge.TrackAdEvent(adAction, adType, adNetwork, adPlacementId);
        }

        public void TrackAdRevenue(AdRevenueData adRevenueData)
        {
            homaBridge.TrackAdRevenue(adRevenueData);
        }

        public void TrackAttributionEvent(string eventName, Dictionary<string, object> arguments = null)
        {
            homaBridge.TrackAttributionEvent(eventName, arguments);
        }

        public void SetCustomDimension01(string customDimension)
        {
            homaBridge.SetCustomDimension01(customDimension);
        }

        public void SetCustomDimension02(string customDimension)
        {
            homaBridge.SetCustomDimension02(customDimension);
        }

        public void SetCustomDimension03(string customDimension)
        {
            homaBridge.SetCustomDimension03(customDimension);
        }

        #endregion
    }
}
