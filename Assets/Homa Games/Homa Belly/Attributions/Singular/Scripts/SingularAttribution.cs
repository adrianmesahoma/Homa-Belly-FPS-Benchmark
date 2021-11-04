using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using HomaGames.HomaBelly.Utilities;
using UnityEngine;

namespace HomaGames.HomaBelly
{
    public class SingularAttribution : IAttributionWithInitializationCallback
    {
        private Dictionary<string, object> configurationData;

        #region Public methods

        public void Initialize(string appSubversion = "")
        {
            Initialize(appSubversion, null);
        }

        public async void Initialize(string appSubversion = "", Action onInitialized = null)
        {
            var loadTask = LoadConfigurationData();
            await loadTask;
            configurationData = loadTask.Result;
            if (configurationData != null)
            {
                InstantiateSingularSdkGameObject((string)configurationData["s_api_key"], (string)configurationData["s_api_secret"]);

                // Set the app subversion as a Global Property.
                // Always after instantiation but before intialization.
                // See https://support.singular.net/hc/en-us/articles/360038355151#Adding_Global_Properties
                if (!string.IsNullOrEmpty(appSubversion) && appSubversion != "0")
                {
                    bool result = SingularSDK.SetGlobalProperty("s_ntesting_version", appSubversion, false);
                    HomaGamesLog.Debug($"Singular Global Property s_ntesting_version set to {appSubversion}: {result}");
                }

                // Set application version at install. With `override` flag to false, it will
                // remain the very first value set
                string appVersionAtInstall = PlayerPrefs.GetString(HomaBellySingularConstants.APP_VERSION_AT_INSTALL, Application.version);
                bool resultVersionAtInstall = SingularSDK.SetGlobalProperty("s_install_app_version", appVersionAtInstall, false);
                HomaGamesLog.Debug($"Singular Global Property s_install_app_version set to {appVersionAtInstall}: {resultVersionAtInstall}");

                // Save APP_VERSION_AT_INSTALL
                if (string.IsNullOrEmpty(PlayerPrefs.GetString(HomaBellySingularConstants.APP_VERSION_AT_INSTALL)))
                {
                    PlayerPrefs.SetString(HomaBellySingularConstants.APP_VERSION_AT_INSTALL, appVersionAtInstall);
                    PlayerPrefs.Save();
                }
            }

#if UNITY_EDITOR
            HomaGamesLog.Debug("Singular SDK initialized");
#else
            SingularSDK.InitializeSingularSDK();
#endif

            if (onInitialized != null)
            {
                onInitialized.Invoke();
            }
        }

        public void OnApplicationPause(bool pause)
        {
            // N/A
        }

        public void ValidateIntegration()
        {
            // TODO: Validate Singular integration
        }

        public void SetUserIsAboveRequiredAge(bool consent)
        {
            // NO-OP
        }

        public void SetTermsAndConditionsAcceptance(bool consent)
        {
            // NO-OP
        }

        public void SetAnalyticsTrackingConsentGranted(bool consent)
        {
#if UNITY_EDITOR
            return;
#endif

#pragma warning disable CS0162 // Remove inaccessible code warning
            SingularSDK.LimitDataSharing(!consent);

            if (consent)
            {
                if (SingularSDK.IsAllTrackingStopped())
                {
                    SingularSDK.ResumeAllTracking();
                }

                SingularSDK.TrackingOptIn();
            }
            else
            {
                SingularSDK.StopAllTracking();
            }
#pragma warning restore CS0162
        }

        public void SetTailoredAdsConsentGranted(bool consent)
        {
            // NO-OP
        }

#if UNITY_PURCHASING
        public void TrackInAppPurchaseEvent(UnityEngine.Purchasing.Product product, bool isRestored = false)
        {
            SingularSDK.InAppPurchase(product, null, isRestored);
        }
#endif
        public void TrackInAppPurchaseEvent(string productId, string currencyCode, double unitPrice, string transactionId = null, string receipt = null, bool isRestored = false)
        {
#if UNITY_EDITOR
            return;
#elif UNITY_ANDROID
            string googlePlaySignature = "";

            if (receipt != null)
            {
                Dictionary<string, object> receiptDictionary = Json.Deserialize(receipt) as Dictionary<string, object>;
                if (receiptDictionary != null && receiptDictionary.ContainsKey("Payload"))
                {
                    string googlePlayPayloadString = receiptDictionary["Payload"] as string;
                    Dictionary<string, object> googlePlayPayloadDictionary = Json.Deserialize(googlePlayPayloadString) as Dictionary<string, object>;
                    if (googlePlayPayloadDictionary != null)
                    {
                        googlePlaySignature = googlePlayPayloadDictionary.ContainsKey("signature") ? (string)googlePlayPayloadDictionary["signature"] : "";
                    }
                }
            }

            SingularSDK.CustomRevenue("__iap__", currencyCode, isRestored ? 0.0 : unitPrice, receipt, googlePlaySignature);
#elif UNITY_IOS
            SingularSDK.CustomRevenue("__iap__", currencyCode, isRestored ? 0.0 : unitPrice, productId, "", "", 1, isRestored ? 0.0 : unitPrice);
#endif
        }

        public void TrackAdRevenue(AdRevenueData adRevenueData)
        {
            HomaGamesLog.Debug($"Tracking Ad Revenue for Singular: {adRevenueData}");
#if UNITY_EDITOR
            return;
#endif

#pragma warning disable CS0162 // Remove inaccessible code warning
            SingularAdData data = new SingularAdData(adRevenueData.AdPlatform, adRevenueData.Currency, adRevenueData.Revenue);

            data.WithAdGroupId(adRevenueData.AdGroupId)
                .WithAdGroupName(adRevenueData.AdGroupName)
                .WithAdGroupType(adRevenueData.AdGroupType)
                .WithAdGroupPriority(adRevenueData.AdGroupPriority)
                .WithAdUnitId(adRevenueData.AdUnitId)
                .WithAdUnitName(adRevenueData.AdUnitName)
                .WithImpressionId(adRevenueData.ImpressionId)
                .WithNetworkName(adRevenueData.NetworkName)
                .WithPlacementId(adRevenueData.PlacementId)
                .WithAdType(adRevenueData.AdType)
                .WithAdPlacmentName(adRevenueData.AdPlacamentName)
                .WithPrecision(adRevenueData.Precision);

            SingularSDK.AdRevenue(data);
#pragma warning restore CS0162
        }

        public void TrackEvent(string eventName, Dictionary<string, object> arguments = null)
        {
            HomaGamesLog.Debug($"Tracking Event on Singular: {eventName}");
            if (arguments != null)
            {
                HomaGamesLog.Debug($"With arguments: {JsonUtility.ToJson(arguments)}");
            }

#if UNITY_EDITOR
            return;
#endif

#pragma warning disable CS0162 // Remove inaccessible code warning
            if (arguments != null)
            { 
                SingularSDK.Event(arguments, eventName);
            }
            else
            {
                SingularSDK.Event(eventName);
            }
#pragma warning restore CS0162
        }

#endregion

#region Private methods

        private async Task<Dictionary<string, object>> LoadConfigurationData()
        {
            return await FileUtilities.LoadAndDeserializeJsonFromResources<Dictionary<string, object>>(HomaBellySingularConstants
                .CONFIG_FILE_RESOURCES);
        }

        /// <summary>
        /// Creates a GameObject attaching the Singular SDK and setting keys
        /// </summary>
        /// <param name="apiKey">Singular API key</param>
        /// <param name="apiSecret">Singular API secret</param>
        private static void InstantiateSingularSdkGameObject(string apiKey, string apiSecret)
        {
            SingularSDK singularSdk = GameObject.FindObjectOfType<SingularSDK>();
            if (singularSdk == null)
            {
                GameObject singularSdkGameObject = new GameObject("SingularSDK");
                singularSdkGameObject.SetActive(false);
                singularSdk = singularSdkGameObject.AddComponent<SingularSDK>();

                if (singularSdk != null)
                {
                    singularSdk.SingularAPIKey = apiKey;
                    singularSdk.SingularAPISecret = apiSecret;
                    singularSdk.InitializeOnAwake = false;
#if UNITY_IOS
                    singularSdk.SKANEnabled = true;
                    singularSdk.manualSKANConversionManagement = false;

                    // Try to delay Singular initialization
                    string geryonIdfaConsentPopupDelayInSecodsString = GeryonUtils.GetGeryonDynamicVariableValue("IDFA_CONSENT_POPUP_DELAY_S");
                    if (!string.IsNullOrEmpty(geryonIdfaConsentPopupDelayInSecodsString))
                    {
                        int delayInSeconds = 0;
                        int.TryParse(geryonIdfaConsentPopupDelayInSecodsString, out delayInSeconds);
                        singularSdk.waitForTrackingAuthorizationWithTimeoutInterval = delayInSeconds + 30;  // Give 30 seconds margin
                    }
#endif
                }

                singularSdkGameObject.SetActive(true);
            }
        }

#endregion
    }
}
