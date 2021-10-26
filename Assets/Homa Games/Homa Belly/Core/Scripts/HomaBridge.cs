using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HomaGames.HomaBelly.Utilities;
using UnityEngine;
using UnityEngine.Profiling;

namespace HomaGames.HomaBelly
{
    /// <summary>
    /// Homa Bridge is the main connector between the public facade (HomaBelly)
    /// and all the inner behaviour of the Homa Belly library. All features
    /// and callbacks will be centralized within this class.
    /// </summary>
    public class HomaBridge : IHomaBellyBridge
    {
        #region Private properties
        private List<IMediator> mediators;
        private List<IAttribution> attributions;
        private List<IAnalytics> analytics;
        private InitializationStatus initializationStatus = new InitializationStatus();
        private AnalyticsHelper analyticsHelper = new AnalyticsHelper();
        #endregion

        #region Public properties

        public bool IsInitialized
        {
            get
            {
                return initializationStatus.IsInitialized;
            }
        }

        #endregion

        public void Initialize()
        {
            RemoteConfiguration.FetchRemoteConfiguration().ContinueWith((remoteConfiguration) =>
            {
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
            // Instantiate
            Debug.Log("InstantiateMediators");
            Profiler.BeginSample("[SP] INSTANTIATE MEDIATORS");HomaGamesLog.Debug("[Homa Belly] Initializing Homa Belly after Remote Configuration fetch");
            InstantiateMediators();
            Profiler.EndSample();
            Debug.Log("InstantiateAttributions");
            Profiler.BeginSample("[SP] INSTANTIATE ATTRIBUTION");
            InstantiateAttributions();
            Profiler.EndSample();
            Debug.Log("InstantiateAnalytics");
            Profiler.BeginSample("[SP] INSTANTIATE ANALYTICS");
            InstantiateAnalytics();
            Profiler.EndSample();

            // Auto-track AdEvents
            Debug.Log("RegisterAdEventsForAnalytics");
            Profiler.BeginSample("[SP] REGISTER AD EVENTS ANALYTICS");
            RegisterAdEventsForAnalytics();
            Profiler.EndSample();

            Debug.Log("AutoConfigureAnalyticsCustomDimensionsForNTesting");
            Profiler.BeginSample("[SP] AUTO CONFIG DIMENSIONS");
            // Try to auto configure analytics custom dimensions from NTesting
            // This is done before initializing to ensure all analytic events
            // properly gather the custom dimension
            AutoConfigureAnalyticsCustomDimensionsForNTesting();
            Profiler.EndSample();

            // Initialize
            Debug.Log("InitializeMediators");
            Profiler.BeginSample("[SP] INIT MEDIATORS");
            InitializeMediators();
            Profiler.EndSample();
            Debug.Log("InitializeAttributions");
            Profiler.BeginSample("[SP] INIT ATTRIBUTION");
            InitializeAttributions();
            Profiler.EndSample();
            Debug.Log("InitializeAnalytics");
            Profiler.BeginSample("[SP] INIT ANALYTICS");
            InitializeAnalytics();
            Profiler.EndSample();

            // Start initialization grace period timer
            initializationStatus.StartInitializationGracePeriod();
            analyticsHelper.Start();
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
            // Mediators
            if (mediators != null)
            {
                foreach (IMediator mediator in mediators)
                {
                    mediator.ValidateIntegration();
                }
            }

            // Attributions
            if (attributions != null)
            {
                foreach (IAttribution attribution in attributions)
                {
                    attribution.ValidateIntegration();
                }
            }

            // Analytics
            if (analytics != null)
            {
                foreach (IAnalytics analytic in analytics)
                {
                    analytic.ValidateIntegration();
                }
            }
        }

        public void OnApplicationPause(bool pause)
        {
            // Analytics Helper
            analyticsHelper.OnApplicationPause(pause);

            // Mediators
            if (mediators != null)
            {
                foreach (IMediator mediator in mediators)
                {
                    mediator.OnApplicationPause(pause);
                }
            }

            // Attributions
            if (attributions != null)
            {
                foreach (IAttribution attribution in attributions)
                {
                    attribution.OnApplicationPause(pause);
                }
            }
        }

        #region IHomaBellyBridge

        public void ShowRewardedVideoAd(string placementId = null)
        {
            TrackAdEvent(AdAction.Request, AdType.RewardedVideo, "homagames.homabelly.default", placementId);

            if (mediators != null)
            {
                foreach (IMediator mediator in mediators)
                {
                    mediator.ShowRewardedVideoAd(placementId);
                }
            }
        }

        public bool IsRewardedVideoAdAvailable(string placementId = null)
        {
            bool available = false;
            if (mediators != null)
            {
                foreach (IMediator mediator in mediators)
                {
                    available |= mediator.IsRewardedVideoAdAvailable();
                }
            }

            return available;
        }

        // Banners
        public void LoadBanner(BannerSize size, BannerPosition position, string placementId = null, UnityEngine.Color bannerBackgroundColor = default)
        {
            TrackAdEvent(AdAction.Request, AdType.Banner, "homagames.homabelly.default", placementId);

            if (mediators != null)
            {
                foreach (IMediator mediator in mediators)
                {
                    mediator.LoadBanner(size, position, placementId, bannerBackgroundColor);
                }
            }
        }

        public void ShowBanner(string placementId = null)
        {
            if (mediators != null)
            {
                foreach (IMediator mediator in mediators)
                {
                    mediator.ShowBanner();
                }
            }
        }

        public void HideBanner(string placementId = null)
        {
            if (mediators != null)
            {
                foreach (IMediator mediator in mediators)
                {
                    mediator.HideBanner();
                }
            }
        }

        public void DestroyBanner(string placementId = null)
        {
            if (mediators != null)
            {
                foreach (IMediator mediator in mediators)
                {
                    mediator.DestroyBanner();
                }
            }
        }

        public void ShowInsterstitial(string placementId = null)
        {
            if (mediators != null)
            {
                foreach (IMediator mediator in mediators)
                {
                    mediator.ShowInterstitial(placementId);
                }
            }
        }

        public bool IsInterstitialAvailable(string placementId = null)
        {
            bool available = false;
            if (mediators != null)
            {
                foreach (IMediator mediator in mediators)
                {
                    available |= mediator.IsInterstitialAvailable();
                }
            }

            return available;
        }

        public void SetUserIsAboveRequiredAge(bool consent)
        {
            // For mediators
            if (mediators != null)
            {
                foreach (IMediator mediator in mediators)
                {
                    mediator.SetUserIsAboveRequiredAge(consent);
                }
            }

            // For attributions
            if (attributions != null)
            {
                foreach (IAttribution attribution in attributions)
                {
                    attribution.SetUserIsAboveRequiredAge(consent);
                }
            }

            // For analytics
            if (analytics != null)
            {
                foreach (IAnalytics analytic in analytics)
                {
                    analytic.SetUserIsAboveRequiredAge(consent);
                }
            }
        }

        public void SetTermsAndConditionsAcceptance(bool consent)
        {
            // For mediators
            if (mediators != null)
            {
                foreach (IMediator mediator in mediators)
                {
                    mediator.SetTermsAndConditionsAcceptance(consent);
                }
            }

            // For attributions
            if (attributions != null)
            {
                foreach (IAttribution attribution in attributions)
                {
                    attribution.SetTermsAndConditionsAcceptance(consent);
                }
            }

            // For analytics
            if (analytics != null)
            {
                foreach (IAnalytics analytic in analytics)
                {
                    analytic.SetTermsAndConditionsAcceptance(consent);
                }
            }
        }

        public void SetAnalyticsTrackingConsentGranted(bool consent)
        {
            // For mediators
            if (mediators != null)
            {
                foreach (IMediator mediator in mediators)
                {
                    mediator.SetAnalyticsTrackingConsentGranted(consent);
                }
            }

            // For attributions
            if (attributions != null)
            {
                foreach (IAttribution attribution in attributions)
                {
                    attribution.SetAnalyticsTrackingConsentGranted(consent);
                }
            }

            // For analytics
            if (analytics != null)
            {
                foreach (IAnalytics analytic in analytics)
                {
                    analytic.SetAnalyticsTrackingConsentGranted(consent);
                }
            }
        }

        public void SetTailoredAdsConsentGranted(bool consent)
        {
            // For mediators
            if (mediators != null)
            {
                foreach (IMediator mediator in mediators)
                {
                    mediator.SetTailoredAdsConsentGranted(consent);
                }
            }

            // For attributions
            if (attributions != null)
            {
                foreach (IAttribution attribution in attributions)
                {
                    attribution.SetTailoredAdsConsentGranted(consent);
                }
            }

            // For analytics
            if (analytics != null)
            {
                foreach (IAnalytics analytic in analytics)
                {
                    analytic.SetTailoredAdsConsentGranted(consent);
                }
            }
        }

#if UNITY_PURCHASING
        public void TrackInAppPurchaseEvent(UnityEngine.Purchasing.Product product, bool isRestored = false)
        {
            // For attributions
            if (attributions != null)
            {
                foreach (IAttribution attribution in attributions)
                {
                    attribution.TrackInAppPurchaseEvent(product, isRestored);
                }
            }

            // For analytics
            if (analytics != null)
            {
                foreach (IAnalytics analytic in analytics)
                {
                    analytic.TrackInAppPurchaseEvent(product, isRestored);
                }
            }
        }
#endif

        public void TrackInAppPurchaseEvent(string productId, string currencyCode, double unitPrice, string transactionId = null, string payload = null, bool isRestored = false)
        {
            // IAP events are applicable to Attributions and Analytics

            // For attributions
            if (attributions != null)
            {
                foreach (IAttribution attribution in attributions)
                {
                    attribution.TrackInAppPurchaseEvent(productId, currencyCode, unitPrice, transactionId, payload);
                }
            }

            // For analytics
            if (analytics != null)
            {
                foreach (IAnalytics analytic in analytics)
                {
                    analytic.TrackInAppPurchaseEvent(productId, currencyCode, unitPrice, transactionId, payload);
                }
            }
        }

        public void TrackResourceEvent(ResourceFlowType flowType, string currency, float amount, string itemType, string itemId)
        {
            if (analytics != null)
            {
                foreach (IAnalytics analytic in analytics)
                {
                    analytic.TrackResourceEvent(flowType, currency, amount, itemType, itemId);
                }
            }
        }

        public void TrackProgressionEvent(ProgressionStatus progressionStatus, string progression01, int score = 0)
        {
            if (analytics != null)
            {
                foreach (IAnalytics analytic in analytics)
                {
                    analytic.TrackProgressionEvent(progressionStatus, progression01, score);
                }
            }
        }

        public void TrackProgressionEvent(ProgressionStatus progressionStatus, string progression01, string progression02, int score = 0)
        {
            if (analytics != null)
            {
                foreach (IAnalytics analytic in analytics)
                {
                    analytic.TrackProgressionEvent(progressionStatus, progression01, progression02, score);
                }
            }
        }

        public void TrackProgressionEvent(ProgressionStatus progressionStatus, string progression01, string progression02, string progression03, int score = 0)
        {
            if (analytics != null)
            {
                foreach (IAnalytics analytic in analytics)
                {
                    analytic.TrackProgressionEvent(progressionStatus, progression01, progression02, progression03, score);
                }
            }
        }

        public void TrackErrorEvent(ErrorSeverity severity, string message)
        {
            if (analytics != null)
            {
                foreach (IAnalytics analytic in analytics)
                {
                    analytic.TrackErrorEvent(severity, message);
                }
            }
        }

        public void TrackDesignEvent(string eventName, float eventValue = 0f)
        {
            if (analytics != null)
            {
                foreach (IAnalytics analytic in analytics)
                {
                    analytic.TrackDesignEvent(eventName, eventValue);
                }
            }
        }

        public void TrackAdEvent(AdAction adAction, AdType adType, string adNetwork, string adPlacementId)
        {
            if (analytics != null)
            {
                foreach (IAnalytics analytic in analytics)
                {
                    analytic.TrackAdEvent(adAction, adType, adNetwork, adPlacementId);
                }
            }
        }

        public void TrackAdRevenue(AdRevenueData adRevenueData)
        {
            if (attributions != null)
            {
                foreach (IAttribution attribution in attributions)
                {
                    attribution.TrackAdRevenue(adRevenueData);
                }
            }
        }

        public void TrackAttributionEvent(string eventName, Dictionary<string, object> arguments = null)
        {
            if (attributions != null)
            {
                foreach (IAttribution attribution in attributions)
                {
                    attribution.TrackEvent(eventName, arguments);
                }
            }
        }

        public void SetCustomDimension01(string customDimension)
        {
            if (analytics != null)
            {
                foreach (IAnalytics analytic in analytics)
                {
                    if (typeof(ICustomDimensions).IsInstanceOfType(analytic))
                    {
                        ((ICustomDimensions) analytic).SetCustomDimension01(customDimension);
                    }
                }
            }
        }

        public void SetCustomDimension02(string customDimension)
        {
            if (analytics != null)
            {
                foreach (IAnalytics analytic in analytics)
                {
                    if (typeof(ICustomDimensions).IsInstanceOfType(analytic))
                    {
                        ((ICustomDimensions)analytic).SetCustomDimension02(customDimension);
                    }
                }
            }
        }

        public void SetCustomDimension03(string customDimension)
        {
            if (analytics != null)
            {
                foreach (IAnalytics analytic in analytics)
                {
                    if (typeof(ICustomDimensions).IsInstanceOfType(analytic))
                    {
                        ((ICustomDimensions)analytic).SetCustomDimension03(customDimension);
                    }
                }
            }
        }

        #endregion

        #region Private helpers

        private void RegisterAdEventsForAnalytics()
        {
            // Interstitial
            Events.onInterstitialAdShowSucceededEvent += (id) =>
            {
                analyticsHelper.OnInterstitialAdWatched(id);
            };

            // Rewarded Video
            Events.onRewardedVideoAdRewardedEvent += (reward) =>
            {
                analyticsHelper.OnRewardedVideoAdWatched(reward.getPlacementName());
            };
        }

        private void AutoConfigureAnalyticsCustomDimensionsForNTesting()
        {
            // This is required after implementing Geryon <> Analytics automatic integration
            // and assign ExternalTokens to Custom Dimensions
            string customDimension01 = ""; 
            string customDimension02 = "";

            GeryonUtils.GetNTestingExternalToken("ExternalToken0").ContinueWith((externalToken0TaskResult) =>
            {
                customDimension01 = externalToken0TaskResult.Result;
                GeryonUtils.GetNTestingExternalToken("ExternalToken1").ContinueWith((externalToken1TaskResult) =>
                {
                    customDimension02 = externalToken1TaskResult.Result;

                    if (!string.IsNullOrEmpty(customDimension01))
                    {
                        HomaGamesLog.Debug(string.Format("Setting Game Analytics custom dimension 01 to: {0}", customDimension01));
                        SetCustomDimension01(customDimension01);
                    }

                    if (!string.IsNullOrEmpty(customDimension02))
                    {
                        HomaGamesLog.Debug(string.Format("Setting Game Analytics custom dimension 02 to: {0}", customDimension02));
                        SetCustomDimension02(customDimension02);
                    }
                }, TaskScheduler.FromCurrentSynchronizationContext());
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        #endregion

        #region Mediators

        private void InstantiateMediators()
        {
            /*// LINQ all available classes implementing IMediator interface
            List<Type> availableMediators = Reflection.GetHomaBellyAvailableClasses(typeof(IMediator));

            // If available mediators found, instantiate them
            if (availableMediators != null && availableMediators.Count > 0)
            {
                mediators = new List<IMediator>();
                foreach (Type type in availableMediators)
                {
                    try
                    {
                        HomaGamesLog.Debug($"[Homa Belly] Instantiating {type}");
                        mediators.Add(Activator.CreateInstance(type) as IMediator);
                    }
                    catch (Exception e)
                    {
                        HomaGamesLog.Warning($"[Homa Belly] Exception instantiating {type}: {e.Message}");
                    }
                }
            }
            else
            {
                HomaGamesLog.Warning("No available mediators found");
            }*/
            
            mediators = new List<IMediator>();
            mediators.Add(new AppLovinMaxMediator());
        }

        private void InitializeMediators()
        {
            if (mediators != null)
            {
                foreach (IMediator mediator in mediators)
                {
                    try
                    {
                        // For Homa Belly v1.2.0+
                        if (typeof(IMediator).IsInstanceOfType(mediator))
                        {
                            ((IMediator)mediator).Initialize(initializationStatus.OnInnerComponentInitialized);
                        }
                        else
                        {
                            // For Homa Belly prior 1.2.0
                            mediator.Initialize();
                        }

                        mediator.RegisterEvents();
                    }
                    catch (Exception e)
                    {
                        HomaGamesLog.Warning($"[Homa Belly] Exception initializing {mediator}: {e.Message}");
                    }
                }
            }
        }

#endregion

#region Attributions

        private void InstantiateAttributions()
        {
            /*// LINQ all available classes implementing IAttribution interface
            List<Type> availableAttributions = Reflection.GetHomaBellyAvailableClasses(typeof(IAttribution));

            // If available mediators found, instantiate them
            if (availableAttributions != null && availableAttributions.Count > 0)
            {
                attributions = new List<IAttribution>();
                foreach (Type type in availableAttributions)
                {
                    try
                    {
                        HomaGamesLog.Debug($"[Homa Belly] Instantiating {type}");
                        attributions.Add(Activator.CreateInstance(type) as IAttribution);
                    }
                    catch (Exception e)
                    {
                        HomaGamesLog.Warning($"[Homa Belly] Exception instantiating {type}: {e.Message}");
                    }
                }
            }
            else
            {
                HomaGamesLog.Warning("No available attributions found");
            }*/
            
            attributions = new List<IAttribution>();
            attributions.Add(new SingularAttribution());
        }

        private void InitializeAttributions()
        {
            if (attributions != null)
            {
                // If Geryon Scope and Variant IDs are found, report it to all Attribution
                string scopeId = "";
                string variantId = "";
                GeryonUtils.GetNTestingScopeId().ContinueWith((scopeIdTaskResult) =>
                {
                    scopeId = scopeIdTaskResult.Result;
                    GeryonUtils.GetNTestingVariantId().ContinueWith((variantIdTaskResult) =>
                    {
                        variantId = variantIdTaskResult.Result;

                        foreach (IAttribution attribution in attributions)
                        {
                            try
                            {
                                // For Homa Belly v1.2.0+
                                if (typeof(IAttributionWithInitializationCallback).IsInstanceOfType(attribution))
                                {
                                    ((IAttributionWithInitializationCallback)attribution).Initialize(scopeId + variantId, initializationStatus.OnInnerComponentInitialized);
                                }
                                else
                                {
                                    // For Homa Belly prior 1.2.0
                                    attribution.Initialize(scopeId + variantId);
                                }
                            }
                            catch (Exception e)
                            {
                                HomaGamesLog.Warning($"[Homa Belly] Exception initializing {attribution}: {e.Message}");
                            }
                        }
                    }, TaskScheduler.FromCurrentSynchronizationContext());
                }, TaskScheduler.FromCurrentSynchronizationContext());
            }
        }

#endregion

#region Analytics

        private void InstantiateAnalytics()
        {
            /*// LINQ all available classes implementing IAnalytics interface
            List<Type> availableAnalytics = Reflection.GetHomaBellyAvailableClasses(typeof(IAnalytics));

            // If available mediators found, instantiate them
            if (availableAnalytics != null && availableAnalytics.Count > 0)
            {
                analytics = new List<IAnalytics>();
                foreach (Type type in availableAnalytics)
                {
                    try
                    {
                        HomaGamesLog.Debug($"[Homa Belly] Instantiating {type}");
                        analytics.Add(Activator.CreateInstance(type) as IAnalytics);
                    }
                    catch (Exception e)
                    {
                        HomaGamesLog.Warning($"[Homa Belly] Exception instantiating {type}: {e.Message}");
                    }
                }
            }
            else
            {
                HomaGamesLog.Warning("No available analytics found");
            }*/
            
            analytics = new List<IAnalytics>();
            analytics.Add(new FacebookImplementation());
            analytics.Add(new GameAnalyticsImplementation());
            analytics.Add(new FirebaseAnalyticsImpl());
        }

        private void InitializeAnalytics()
        {
            if (analytics != null)
            {
                foreach (IAnalytics analytic in analytics)
                {
                    try
                    {
                        // For Homa Belly v1.2.0+
                        if (typeof(IAnalyticsWithInitializationCallback).IsInstanceOfType(analytic))
                        {
                            ((IAnalyticsWithInitializationCallback)analytic).Initialize(initializationStatus.OnInnerComponentInitialized);
                        }
                        else
                        {
                            // For Homa Belly prior 1.2.0
                            analytic.Initialize();
                        }
                    }
                    catch (Exception e)
                    {
                        HomaGamesLog.Warning($"[Homa Belly] Exception initializing {analytic}: {e.Message}");
                    }
                }
            }
        }

#endregion
    }
}
