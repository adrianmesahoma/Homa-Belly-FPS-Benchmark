using System;
using System.Threading.Tasks;
using UnityEngine;
#if UNITY_IOS
using UnityEngine.iOS;
#endif
using UnityEngine.UI;

namespace HomaGames.GDPR
{
    public class GDPRWelcomeView : View<GDPRWelcomeView>
    {
        #region Private properties
        [SerializeField] private Transform _backgroundClickBlocker;
        protected override Transform backgroundClickBlocker => _backgroundClickBlocker;
        [SerializeField] private GameObject progressBar;
        [SerializeField] private CanvasGroup mainCanvasGroup;

        #endregion

        [SerializeField] private Button AcceptAndPlayButton;
        [SerializeField] private GameObject authorizationBackground;

        public void OnTailoredAdsAcceptanceChanged(bool arg0)
        {
            PlayerPrefs.SetInt(Constants.PersistenceKey.TAILORED_ADS, arg0 ? 1 : 0);
            UpdateAcceptAndPlayButton();
        }

        public void OnAnalyticsAcceptanceChanged(bool arg0)
        {
            PlayerPrefs.SetInt(Constants.PersistenceKey.ANALYTICS_TRACKING, arg0 ? 1 : 0);
            UpdateAcceptAndPlayButton();
        }

        public void OnPrivacyPolicyChanged(bool arg0)
        {
            PlayerPrefs.SetInt(Constants.PersistenceKey.ABOVE_REQUIRED_AGE, arg0 ? 1 : 0);
            PlayerPrefs.SetInt(Constants.PersistenceKey.TERMS_AND_CONDITIONS, arg0 ? 1 : 0);
            UpdateAcceptAndPlayButton();
        }

        public void UpdateAcceptAndPlayButton()
        {
            AcceptAndPlayButton.interactable = Manager.Instance.IsAnalyticsGranted() &&
                                               Manager.Instance.IsTailoredAdsGranted()
                                               && Manager.Instance.IsTermsAndConditionsAccepted();

        }

        public void SkipGdprWelcomeView()
        {
            // By default, accept all
            OnTailoredAdsAcceptanceChanged(true);
            OnAnalyticsAcceptanceChanged(true);
            OnPrivacyPolicyChanged(true);

            // Bypass the `Dismiss` call and move to next View
            Dismiss();
        }

        protected override void OnEnter()
        {
            MakeCanvasGroupVisibleAfterSomeDelay();

            UpdateAcceptAndPlayButton();

            // Only show the progress bar image if IDFA Prepopup is enabled
            bool shouldShowProgressBar = GDPRGeryonUtils.GetGeryonDynamicVariableValue("enable_idfa_prepopup", Application.internetReachability == NetworkReachability.NotReachable ? false : true) // If enabled on NTesting
                && Manager.Instance.IsiOS14_5OrHigher   // Only for iOS 14.5+
                && AppTrackingTransparency.TrackingAuthorizationStatus == AppTrackingTransparency.AuthorizationStatus.NOT_DETERMINED; // If global setting for tracking is enabled

            progressBar.SetActive(shouldShowProgressBar);
        }

        /// <summary>
        /// Upon enter, make canvas visible after 2 seconds.
        /// This is done to create the illusion of instant
        /// show after all variables have been loaded
        /// to configure GDPR screens
        /// </summary>
        private void MakeCanvasGroupVisibleAfterSomeDelay()
        {
            Task.Delay(2000).ContinueWith((result) =>
            {
                // Sanity check making a double call to this won't make any effect
                if (mainCanvasGroup != null && mainCanvasGroup.alpha == 0)
                {
                    mainCanvasGroup.alpha = 1.0f;
                }
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        protected override void OnDismiss()
        {
            PlayerPrefs.Save();

#if UNITY_ANDROID

            // Initialize Homa Belly
            InitializationHelper.InitializeHomaBelly();

#elif UNITY_IOS

            bool idfaPrepopupEnabled = GDPRGeryonUtils.GetGeryonDynamicVariableValue("enable_idfa_prepopup", Application.internetReachability == NetworkReachability.NotReachable ? false : true);
            bool idfaEnabled = GDPRGeryonUtils.GetGeryonDynamicVariableValue("enable_idfa", true);

            // If we are on iOS lower than 14.5 or IDFA is disabled, initialize Homa Belly (won't show IDFA)
            if (!Manager.Instance.IsiOS14_5OrHigher || !idfaEnabled)
            {
                InitializationHelper.InitializeHomaBelly();
            }
            // For iOS 14.5+ where IDFA is enabled and is not asked yet
            else if (!Manager.Instance.IsIOSIDFAFlowDone())
            {
                // The global setting for tracking is enabled and has not been asked yet
                if (AppTrackingTransparency.TrackingAuthorizationStatus == AppTrackingTransparency.AuthorizationStatus.NOT_DETERMINED)
                {
                    // We make canvas group visible here to handle
                    // the use case where GDPR screen is skipped
                    MakeCanvasGroupVisibleAfterSomeDelay();

                    // If the prepopup is enable through NTesting, show it
                    if (idfaPrepopupEnabled)
                    {
                        IDFAPrePopup.Instance.Enter();
                    }
                    else
                    {
                        authorizationBackground.SetActive(true);

                        // Otherwise, handle native IDFA popup directly
    #if !UNITY_EDITOR
                        AppTrackingTransparency.OnAuthorizationRequestDone+= OnAuthorizationRequestDone;
                        AppTrackingTransparency.RequestTrackingAuthorization();
    #endif
                        InitializationHelper.TrackDesignEvent("native_idfa_popup_request");
                        InitializationHelper.TrackAttributionEvent("native_idfa_popup_request");
                    }
                }
                else
                {
                    // If ATT is DENIED or AUTHORIZED, move forward with initialization
                    InitializationHelper.InitializeHomaBelly();
                }

                // Remember what the global tracking setting is
                PlayerPrefs.SetInt(Constants.PersistenceKey.IOS_GLOBAL_ADS_TRACKING_SETTING,
                    AppTrackingTransparency.TrackingAuthorizationStatus ==
                    AppTrackingTransparency.AuthorizationStatus.DENIED
                        ? 0
                        : 1);

                // If it's already authorized we can directly go to the game
            }
            else
            {
                // If ATT has already been asked on iOS 14.5+, move forward with initialization
                InitializationHelper.InitializeHomaBelly();
            }
#endif
        }

        private void OnAuthorizationRequestDone(AppTrackingTransparency.AuthorizationStatus obj)
        {
            authorizationBackground.SetActive(false);

            if (obj != AppTrackingTransparency.AuthorizationStatus.AUTHORIZED)
            {
                InitializationHelper.TrackDesignEvent("native_idfa_popup_tracking_not_allowed");
                InitializationHelper.TrackAttributionEvent("native_idfa_popup_tracking_not_allowed");
            }
            else
            {
                InitializationHelper.TrackDesignEvent("native_idfa_popup_tracking_allowed");
                InitializationHelper.TrackAttributionEvent("native_idfa_popup_tracking_allowed");
            }

            // Mark IDFA popup as already asked
            AppTrackingTransparency.OnAuthorizationRequestDone -= OnAuthorizationRequestDone;
            PlayerPrefs.SetInt(Constants.PersistenceKey.IOS_ADS_TRACKING_ASKED, 1);
            PlayerPrefs.Save();

            // Initialize Homa Belly after user's decision
            InitializationHelper.InitializeHomaBelly();
        }
    }
}