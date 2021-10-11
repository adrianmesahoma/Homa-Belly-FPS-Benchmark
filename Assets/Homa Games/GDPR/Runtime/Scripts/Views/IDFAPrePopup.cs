using System.Collections;
using System.Collections.Generic;
using HomaGames.GDPR;
using UnityEngine;
using UnityEngine.UI;

namespace HomaGames.GDPR
{
    public class IDFAPrePopup : View<IDFAPrePopup>
    {
        [SerializeField] private Transform _backgroundClickBlocker;
        protected override Transform backgroundClickBlocker => _backgroundClickBlocker;

        [SerializeField] private GameObject progressBarFillThree;
        [SerializeField] private GameObject prepopupAfterGdpr;
        [SerializeField] private GameObject prepopupAfterGdprCallToActionButton;
        [SerializeField] private GameObject prepopupWithoutGdpr;
        [SerializeField] private GameObject prepopupWithoutGdprCallToActionButton;
        [SerializeField] private GameObject authorizationBackground;
        

        protected override void OnEnter()
        {
            InitializationHelper.TrackDesignEvent("prepopup_open");
            InitializeUIStatus();
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

            Dismiss();
        }

        public void RequestAuthorization()
        {
            UpdateUIForAuthorizationPopup();

#if !UNITY_EDITOR
            if (GDPRGeryonUtils.GetGeryonDynamicVariableValue("enable_idfa", true))
            {
                AppTrackingTransparency.OnAuthorizationRequestDone+= OnAuthorizationRequestDone;
                AppTrackingTransparency.RequestTrackingAuthorization();
            }
#endif
            InitializationHelper.TrackDesignEvent("native_idfa_popup_request");
            InitializationHelper.TrackAttributionEvent("native_idfa_popup_request");
        }

        protected override void OnDismiss()
        {
            AppTrackingTransparency.OnAuthorizationRequestDone -= OnAuthorizationRequestDone;
            PlayerPrefs.SetInt(Constants.PersistenceKey.IOS_ADS_TRACKING_ASKED, 1);
            PlayerPrefs.Save();
            authorizationBackground.SetActive(false);

            // Initialize Homa Belly after user's decision
            InitializationHelper.InitializeHomaBelly();
        }

        private void UpdateUIForAuthorizationPopup()
        {
            progressBarFillThree.SetActive(true);
            authorizationBackground.SetActive(true);
            prepopupAfterGdprCallToActionButton.SetActive(false);
            prepopupWithoutGdprCallToActionButton.SetActive(false);
        }

        private void InitializeUIStatus()
        {
            progressBarFillThree.SetActive(false);
            authorizationBackground.SetActive(false);
            prepopupAfterGdprCallToActionButton.SetActive(true);
            prepopupWithoutGdprCallToActionButton.SetActive(true);

            if (Manager.IsGdprProtectedRegion && GDPRGeryonUtils.GetGeryonDynamicVariableValue("enable_gdpr", Application.internetReachability == NetworkReachability.NotReachable ? false : true))
            {
                // Show default design GDPR continuation explanation prepopup
                prepopupAfterGdpr.SetActive(true);
                prepopupWithoutGdpr.SetActive(false);
            }
            else
            {
                // Show detailed explanation prepopup, as GDPR was not shown before
                prepopupAfterGdpr.SetActive(false);
                prepopupWithoutGdpr.SetActive(true);
            }
        }
    }
}