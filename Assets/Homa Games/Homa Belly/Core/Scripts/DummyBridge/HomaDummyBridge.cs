#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HomaGames.HomaBelly.Utilities;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace HomaGames.HomaBelly
{
    /// <summary>
    /// Homa Bridge is the main connector between the public facade (HomaBelly)
    /// and all the inner behaviour of the Homa Belly library. All features
    /// and callbacks will be centralized within this class.
    /// </summary>
    public class HomaDummyBridge : IHomaBellyBridge
    {
        private const string RES_PATH = "Assets/Homa Games/Homa Belly/Core/Prefabs/DummyAds/";
        #region Private properties
        private bool BannerLoaded = false;
        private bool RewardedVideoLoaded = false;
        private GameObject DummyBanner;
        private Events m_events = new Events();
        private bool initialized = false;
        private AnalyticsHelper analyticsHelper = new AnalyticsHelper();

        public bool IsInitialized
        {
            get
            {
                return initialized;
            }
        }
        #endregion

        public void Initialize()
        {
            HomaGamesLog.Debug("[Homa Belly] Initializing Homa Belly Dummy for Unity Editor");
            RemoteConfiguration.FetchRemoteConfiguration().ContinueWith((remoteConfiguration) =>
            {
                HomaGamesLog.Debug("[Homa Belly] Initializing Homa Belly after Remote Configuration fetch");
                InitializeRemoteConfigurationDependantComponents(remoteConfiguration.Result);

            }, TaskScheduler.FromCurrentSynchronizationContext());

            InitializeRemoteConfigurationIndependentComponents();
        }

        /// <summary>
        /// Initializes all those components that can be initialized
        /// before the Remote Configuration data is fetched
        /// </summary>
        private void InitializeRemoteConfigurationIndependentComponents()
        {
            analyticsHelper.Start();
            LoadRewardedVideoAd();

            // Notify initialized after some dummy delay
            Task.Delay(3000)
                .ContinueWith((unused) =>
                {
                    initialized = true;
                    m_events.OnInitialized();
                }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        /// <summary>
        /// Initializes all those components that require from Remote Configuration
        /// data in order to initialize
        /// </summary>
        private void InitializeRemoteConfigurationDependantComponents(RemoteConfiguration.RemoteConfigurationSetup remoteConfigurationSetup)
        {
            CrossPromotionManager.Initialize(remoteConfigurationSetup);
        }

        public void SetDebug(bool enabled)
        {

        }

        public void ValidateIntegration()
        {

        }

        public void OnApplicationPause(bool pause)
        {
            // Analytics Helper
            analyticsHelper.OnApplicationPause(pause);
        }

        #region IHomaBellyBridge

        public void ShowRewardedVideoAd(string placementId = null)
        {
            if (!RewardedVideoLoaded)
            {
                HomaGamesLog.Warning("[Homa Belly] Rewarded Video Ad not yet loaded.");
            }
            else
            {
                m_events.OnRewardedVideoAdStartedEvent(placementId);
                ShowDummyRewardedAd(placementId);
            }
        }

        public bool IsRewardedVideoAdAvailable(string placementId = null)
        {
            return RewardedVideoLoaded;
        }

        private void LoadRewardedVideoAd(string placementId = null)
        {
            ExecuteWithDelay(1f, () =>
            {
                RewardedVideoLoaded = true;
                m_events.OnRewardedVideoAvailabilityChangedEvent(true,placementId);
            });
        }

        private void ShowDummyRewardedAd(string placementId)
        {
            GameObject rewardedPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(RES_PATH + "Rewarded.prefab");
            GameObject dummyRewardedAd = InstantiateDummyAd(rewardedPrefab, Vector3.zero, Quaternion.identity);
            bool grantedReward = false;
            DummyRewardBehaviour dummy = dummyRewardedAd.GetComponent<DummyRewardBehaviour>();
            dummy.HomaRewardedCloseButton.onClick.AddListener(() =>
            {
                if (grantedReward)
                {
                    m_events.OnRewardedVideoAdRewardedEvent(new VideoAdReward(dummy.NameInput.text, int.Parse(dummy.QuantityInput.text)));
                }
                m_events.OnRewardedVideoAdClosedEvent(placementId);
                LoadRewardedVideoAd(placementId);
                UnityEngine.Object.Destroy(dummyRewardedAd);
            });
            dummy.HomaRewardButton.onClick.AddListener(() =>
            {
                grantedReward = true;
                dummy.HomaRewardStatus.text = "Reward granted. Will send reward callback on ad close.";
            });
            m_events.OnRewardedVideoAvailabilityChangedEvent(false,placementId);
            RewardedVideoLoaded = false;

            analyticsHelper.OnRewardedVideoAdWatched();
        }

        // Banners
        public void LoadBanner(BannerSize size, BannerPosition position, string placementId = null, Color bannerBackgroundColor = default)
        {
            if (!BannerLoaded)
            {
                // Only support BottomCenter and TopCenter for now
                string bannerPrefabName = position == BannerPosition.BOTTOM ? "BannerBottom" : "BannerTop";
                GameObject bannerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(RES_PATH + bannerPrefabName + ".prefab");
                GameObject dummyBanner = InstantiateDummyAd(bannerPrefab, Vector3.zero, Quaternion.identity);
                dummyBanner.SetActive(false); // Hidden by default

                DummyBanner = dummyBanner;
                BannerLoaded = true;
                ExecuteWithDelay(0.1f, () => m_events.OnBannerAdLoadedEvent(placementId));
            }
        }

        public void ShowBanner(string placementId = null)
        {
            if (!BannerLoaded)
            {
                HomaGamesLog.Warning("[Homa Belly] Banner was not created, can not show it");
            }
            else
            {
                if (DummyBanner)
                {
                    DummyBanner.SetActive(true);
                }
            }
        }

        public void HideBanner(string placementId = null)
        {
            if (DummyBanner)
            {
                DummyBanner.SetActive(false);
            }
        }

        public void DestroyBanner(string placementId = null)
        {
            if (DummyBanner)
            {
                UnityEngine.Object.Destroy(DummyBanner);
            }
        }

        public void ShowInsterstitial(string placementId = null)
        {
            m_events.OnInterstitialAdOpenedEvent();
            GameObject interstitialPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(RES_PATH + "/Interstitial.prefab");
            GameObject dummyInterstitial = InstantiateDummyAd(interstitialPrefab, Vector3.zero, Quaternion.identity);
            DummyInterstitialBehaviour dummy = dummyInterstitial.GetComponent<DummyInterstitialBehaviour>();

            dummy.HomaInterstitialCloseButton.onClick.AddListener(() =>
            {
                m_events.OnInterstitialAdClosedEvent();
                UnityEngine.Object.Destroy(dummyInterstitial);
            });
            m_events.OnInterstitialAdShowSucceededEvent(placementId);

            analyticsHelper.OnInterstitialAdWatched();
        }

        public bool IsInterstitialAvailable(string placementId = null)
        {
            return true;
        }

        public void SetUserIsAboveRequiredAge(bool consent)
        {

        }

        public void SetTermsAndConditionsAcceptance(bool consent)
        {

        }

        public void SetAnalyticsTrackingConsentGranted(bool consent)
        {

        }

        public void SetTailoredAdsConsentGranted(bool consent)
        {

        }

#if UNITY_PURCHASING
        public void TrackInAppPurchaseEvent(UnityEngine.Purchasing.Product product, bool isRestored = false)
        {
            DummyEvent("InAppPurchase", "product=" + product, "restored=" + isRestored);
        }
#endif

        public void TrackInAppPurchaseEvent(string productId, string currencyCode, double unitPrice, string transactionId = null, string payload = null, bool isRestored = false)
        {
            DummyEvent("InAppPurchase", "productId=" + productId, "currencyCode=" + currencyCode, "unitPrice=" + unitPrice, "transactionId=" + transactionId, "payload=" + payload, "restored=" + isRestored);
        }

        public void TrackResourceEvent(ResourceFlowType flowType, string currency, float amount, string itemType, string itemId)
        {
            DummyEvent("ResourceEvent", "ResourceFlowType=" + flowType.ToString(), "currency=" + currency, "amount=" + amount, "itemType=" + itemType, "itemId=" + itemId);
        }

        public void TrackProgressionEvent(ProgressionStatus progressionStatus, string progression01, int score = 0)
        {
            DummyEvent("Progression", "ProgressionStatus=" + progressionStatus.ToString(), "progression01=" + progression01, "score=" + score);
        }

        public void TrackProgressionEvent(ProgressionStatus progressionStatus, string progression01, string progression02, int score = 0)
        {
            DummyEvent("Progression", "ProgressionStatus=" + progressionStatus.ToString(), "progression01=" + progression01, "progression02=" + progression02, "score=" + score);
        }

        public void TrackProgressionEvent(ProgressionStatus progressionStatus, string progression01, string progression02, string progression03, int score = 0)
        {
            DummyEvent("Progression", "ProgressionStatus=" + progressionStatus.ToString(), "progression01=" + progression01, "progression02=" + progression02, "progression03=" + progression03, "score=" + score);
        }

        public void TrackErrorEvent(ErrorSeverity severity, string message)
        {
            DummyEvent("Error", "ErrorSeverity=" + severity.ToString(), "message=" + message);
        }

        public void TrackDesignEvent(string eventName, float eventValue = 0f)
        {
            DummyEvent("Design", "eventName=" + eventName, "eventValue=" + eventValue);
        }

        public void TrackAdEvent(AdAction adAction, AdType adType, string adNetwork, string adPlacementId)
        {
            DummyEvent("Ad", "AdAction=" + adAction.ToString(), "AdType=" + adType.ToString(), "adNetwork=" + adNetwork, "adPlacementId=" + adPlacementId);
        }

        public void TrackAdRevenue(AdRevenueData adRevenueData)
        {
            DummyEvent("AdRevenue", "AdRevenueData=" + adRevenueData.ToString());
        }

        public void TrackAttributionEvent(string eventName, Dictionary<string, object> arguments = null)
        {
            DummyEvent(eventName, "Arguments=" + Json.Serialize(arguments));
        }

        public void SetCustomDimension01(string customDimension)
        {
            // NO-OP
        }

        public void SetCustomDimension02(string customDimension)
        {
            // NO-OP
        }

        public void SetCustomDimension03(string customDimension)
        {
            // NO-OP
        }

        private void DummyEvent(string eventName, params string[] p)
        {
            var str = "[Homa Belly] Tracking " + eventName + " Event : ";
            foreach (string param in p)
                str += " " + param;
            HomaGamesLog.Debug(str);
        }

        private void ExecuteWithDelay(float seconds, Action action)
        {
            Task.Delay((int)(seconds * 1000)).ContinueWith((result) =>
              {
                  action();
              }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private GameObject InstantiateDummyAd(GameObject prefab, Vector3 position,Quaternion rotation)
        {
            GameObject ad = UnityEngine.Object.Instantiate(prefab, position, rotation);
            UnityEngine.Object.DontDestroyOnLoad(ad);
            return ad;
        }
        #endregion
    }
}
#endif