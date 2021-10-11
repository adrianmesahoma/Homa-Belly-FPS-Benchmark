using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HomaGames.GDPR
{
    public class TrackingSettingsView : View<TrackingSettingsView>
    {
        [SerializeField] private Transform _backgroundClickBlocker;
        protected override Transform backgroundClickBlocker => _backgroundClickBlocker;

#if UNITY_EDITOR
        private void OnGUI()
        {
            if (!IsVisible) return;
            if (GUI.Button(new Rect(0, 0, Screen.width, Screen.height), "Allow tracking in settings"))
            {
                Dismiss();
            }
        }
#endif
        private void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus && IsVisible)
            {
                Dismiss();
            }
        }

        protected override void OnEnter()
        {
            AppTrackingTransparency.OpenPrivacySettings();
        }

        protected override void OnDismiss()
        {
            if (AppTrackingTransparency.TrackingAuthorizationStatus ==
                AppTrackingTransparency.AuthorizationStatus.AUTHORIZED)
            {
                if(ParentView)
                    ParentView.Dismiss();
            }
        }
    }
}