using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using GameAnalyticsSDK;
using GameAnalyticsSDK.Validators;
using HomaGames.HomaBelly.Utilities;
using UnityEngine;

namespace HomaGames.HomaBelly
{
    public class GameAnalyticsImplementation : IAnalyticsWithInitializationCallback, ICustomDimensions
    {
        #region Public methods

        public void Initialize()
        {
            Initialize(null);
        }

        public void Initialize(Action onInitialized = null)
        {
            // Try to find any previous GameAnalytics object and destroy it
            UnityEngine.Object gameAnalyticsObject = UnityEngine.Object.FindObjectOfType(typeof(GameAnalytics));
            if (gameAnalyticsObject != null)
            {
                UnityEngine.Object.Destroy(gameAnalyticsObject);
            }

            // Create GameObject for GameAnalytics in runtime and attach script
            GameObject gameAnalyticsGameObject = new GameObject("Game Analytics");
            gameAnalyticsGameObject.AddComponent<GameAnalytics>();
            GameAnalytics.Initialize();

            // Invoke intialization
            if (onInitialized != null)
            {
                onInitialized.Invoke();
            }
        }

        public void OnApplicationPause(bool pause)
        {
            // N/A
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
            GameAnalytics.SetEnabledEventSubmission(consent);
        }

        public void SetTailoredAdsConsentGranted(bool consent)
        {
            // NO-OP
        }

        public void ValidateIntegration()
        {
#if UNITY_EDITOR
            string gameAnalyticsSettingsPath = Application.dataPath + "/Resources/GameAnalytics/Settings.asset";
            if (File.Exists(gameAnalyticsSettingsPath))
            {
#endif
                if (GameAnalytics.SettingsGA != null && GameAnalytics.SettingsGA.Platforms != null)
                {
                    if (GameAnalytics.SettingsGA.Platforms.Count > 0)
                    {
                        HomaGamesLog.Debug($"[Validate Integration] Game Analytics successfully integrated");
                    }
                }
                else
                {
                    HomaGamesLog.Warning($"[Validate Integration] Wrong configuration for Game Analytics");
                }
#if UNITY_EDITOR
            }
            else
            {
                HomaGamesLog.Warning($"[Validate Integration] Game Analytics Settings not found. Please see {gameAnalyticsSettingsPath}");
            }
#endif
        }

#if UNITY_PURCHASING
        public void TrackInAppPurchaseEvent(UnityEngine.Purchasing.Product product, bool isRestored = false)
        {
            TrackInAppPurchaseEvent(product.definition.id, product.metadata.isoCurrencyCode, Convert.ToDouble(product.metadata.localizedPrice, CultureInfo.InvariantCulture), product.transactionID, product.receipt, isRestored);
        }
#endif

        public void TrackInAppPurchaseEvent(string productId, string currencyCode, double unitPrice, string transactionId = null, string receipt = null, bool isRestored = false)
        {
            // For the seek of Homa Belly standarization, we do not
            // inform `itemType` nor `cartType`, as attribution products
            // do not take care of them
            int unitPriceInCents = isRestored ? 0 : (int)(unitPrice * 100);
            string itemType = "";
            string cartType = "";

#if UNITY_ANDROID
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

            if (GAValidator.ValidateBusinessEvent(currencyCode, 1, cartType, itemType, productId))
            {
                GameAnalytics.NewBusinessEventGooglePlay(currencyCode, unitPriceInCents, itemType, productId, cartType, transactionId, googlePlaySignature);
            }

#elif UNITY_IOS
            if (GAValidator.ValidateBusinessEvent(currencyCode, 1, cartType, itemType, productId))
            {
                GameAnalytics.NewBusinessEvent(currencyCode, unitPriceInCents, itemType, productId, cartType);
            }
#endif
        }

        public void TrackResourceEvent(ResourceFlowType flowType, string currency, float amount, string itemType, string itemId)
        {
            #if UNITY_EDITOR || HOMA_DEVELOPMENT
            if (GAValidator.ValidateResourceEvent((GAResourceFlowType)flowType, currency, amount, itemType, itemId))
            #endif
            {
                GameAnalytics.NewResourceEvent((GAResourceFlowType)flowType, currency, amount, itemType, itemId);
            }
        }

        public void TrackProgressionEvent(ProgressionStatus progressionStatus, string progression01, int score = 0)
        {
            #if UNITY_EDITOR || HOMA_DEVELOPMENT
            if (GAValidator.ValidateProgressionEvent((GAProgressionStatus)progressionStatus, progression01, "", ""))
            #endif
            {
                GameAnalytics.NewProgressionEvent((GAProgressionStatus)progressionStatus, progression01, score);
            }
        }

        public void TrackProgressionEvent(ProgressionStatus progressionStatus, string progression01, string progression02, int score = 0)
        {
            #if UNITY_EDITOR || HOMA_DEVELOPMENT
            if (GAValidator.ValidateProgressionEvent((GAProgressionStatus)progressionStatus, progression01, progression02, ""))
            #endif
            {
                GameAnalytics.NewProgressionEvent((GAProgressionStatus)progressionStatus, progression01, progression02, score);
            }
        }

        public void TrackProgressionEvent(ProgressionStatus progressionStatus, string progression01, string progression02, string progression03, int score = 0)
        {
            #if UNITY_EDITOR || HOMA_DEVELOPMENT
            if (GAValidator.ValidateProgressionEvent((GAProgressionStatus)progressionStatus, progression01, progression02, progression03))
            #endif
            {
                GameAnalytics.NewProgressionEvent((GAProgressionStatus)progressionStatus, progression01, progression02, progression03, score);
            }
        }

        public void TrackErrorEvent(ErrorSeverity severity, string message)
        {
            if (GAValidator.ValidateErrorEvent((GAErrorSeverity)severity, message))
            {
                GameAnalytics.NewErrorEvent((GAErrorSeverity)severity, message);
            }
        }

        public void TrackDesignEvent(string eventName, float eventValue = 0f)
        {
            #if UNITY_EDITOR || HOMA_DEVELOPMENT
            if (GAValidator.ValidateDesignEvent(eventName))
            #endif
            {
                GameAnalytics.NewDesignEvent(eventName, eventValue);
            }
        }

        public void TrackAdEvent(AdAction adAction, AdType adType, string adNetwork, string adPlacementId)
        {
            #if UNITY_EDITOR || HOMA_DEVELOPMENT
            if (GAValidator.ValidateAdEvent((GAAdAction)adAction, (GAAdType)adType, adNetwork, string.IsNullOrEmpty(adPlacementId) ? "default" : adPlacementId))
            #endif
            {
                GameAnalytics.NewAdEvent((GAAdAction)adAction, (GAAdType)adType, adNetwork, string.IsNullOrEmpty(adPlacementId) ? "default" : adPlacementId);
            }
        }

        #endregion

        #region ICustomDimensions

        public void SetCustomDimension01(string customDimension)
        {
            GameAnalytics.SetCustomDimension01(customDimension);
        }

        public void SetCustomDimension02(string customDimension)
        {
            GameAnalytics.SetCustomDimension02(customDimension);
        }

        public void SetCustomDimension03(string customDimension)
        {
            GameAnalytics.SetCustomDimension03(customDimension);
        }

        #endregion
    }
}

