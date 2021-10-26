using System;
using System.Collections.Generic;

namespace HomaGames.HomaBelly
{
    public abstract class IMediator
    {
        // For Homa Belly prior 1.2.0
        public abstract void Initialize();
        public abstract void Initialize(Action onInitialized = null);
        public abstract void OnApplicationPause(bool pause);
        public abstract  void ValidateIntegration();

        #region GDPR/CCPA
        /// <summary>
        /// Specifies if the user asserted being above the required age
        /// </summary>
        /// <param name="consent">true if user accepted, false otherwise</param>
        public abstract  void SetUserIsAboveRequiredAge(bool consent);

        /// <summary>
        /// Specifies if the user accepted privacy policy and terms and conditions
        /// </summary>
        /// <param name="consent">true if user accepted, false otherwise</param>
        public abstract void SetTermsAndConditionsAcceptance(bool consent);

        /// <summary>
        /// Specifies if the user granted consent for analytics tracking
        /// </summary>
        /// <param name="consent">true if user accepted, false otherwise</param>
        public abstract void SetAnalyticsTrackingConsentGranted(bool consent);

        /// <summary>
        /// Specifies if the user granted consent for showing tailored ads
        /// </summary>
        /// <param name="consent">true if user accepted, false otherwise</param>
        public abstract void SetTailoredAdsConsentGranted(bool consent);

        #endregion

        /// <summary>
        /// Register all events and callbacks required for the
        /// mediation implementation
        /// </summary>
        public abstract void RegisterEvents();

        // Rewarded Video Ads
        public abstract void ShowRewardedVideoAd(string placement = null);
        public abstract  bool IsRewardedVideoAdAvailable(string placement = null);

        // Banners
        public abstract void LoadBanner(BannerSize size, BannerPosition position, string placement = null, UnityEngine.Color bannerBackgroundColor = default);
        public abstract  void ShowBanner(string placement = null);
        public abstract  void HideBanner(string placement = null);
        public abstract void DestroyBanner(string placement = null);
        
        // Interstitial
        public abstract  void ShowInterstitial(string placement = null);
        public abstract bool IsInterstitialAvailable(string placement = null);
    }
}
