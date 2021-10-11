using System;
using System.Collections.Generic;
using HomaGames.HomaBelly;
using UnityEngine;

namespace HomaGames.GDPR
{
    /// <summary>
    /// This class allows delaying Homa Belly initialization
    /// after iOS 14.5+ IDFA decision (if applied)
    /// </summary>
    public static class InitializationHelper
    {
        private static Queue<string> analyticsDesignEventsToTrack = new Queue<string>();
        private static Queue<string> attributionEventsToTrack = new Queue<string>();
        private static bool initialized = false;

        /// <summary>
        /// Creates Homa Belly Game Object and initializes it automatically
        /// </summary>
        public static void InitializeHomaBelly()
        {
            if (!initialized)
            {
                // Initialize Homa Belly asynchronously
                HomaBelly.Events.onInitialized += OnHomaBellyInitialized;

                // Create Homa Belly game object
                GameObject homaBellyGameObject = new GameObject("Homa Belly");
                homaBellyGameObject.AddComponent<HomaBelly.HomaBelly>();
            
                initialized = true;
            }
        }

        /// <summary>
        /// Track a design event. If iOS 14.5+, will be queued until
        /// the users makes an IDFA decision. Othwerise will trigger the event.
        /// </summary>
        /// <param name="eventString"></param>
        public static void TrackDesignEvent(string eventString)
        {
            // For iOS 14.5+ enqueue and wait for IDFA decision
            if (Manager.Instance.IsiOS14_5OrHigher)
            {
                analyticsDesignEventsToTrack.Enqueue(eventString);
            }
            else
            {
                HomaBelly.HomaBelly.Instance.TrackDesignEvent(eventString);
            }
        }

        /// <summary>
        /// Track an attribution event. If iOS 14.5+, will be queued until
        /// the users makes an IDFA decision. Othwerise will trigger the event.
        /// </summary>
        /// <param name="eventString"></param>
        public static void TrackAttributionEvent(string eventString)
        {
            // For iOS 14.5+ enqueue and wait for IDFA decision
            if (Manager.Instance.IsiOS14_5OrHigher)
            {
                attributionEventsToTrack.Enqueue(eventString);
            }
            else
            {
                HomaBelly.HomaBelly.Instance.TrackAttributionEvent(eventString);
            }
        }

        /// <summary>
        /// Callback invoked after Homa Belly initialization
        /// </summary>
        private static void OnHomaBellyInitialized()
        {
            // Upon initialization, inform with GDPR decisions
            HomaBellyUtilities.InformHomaBellyWithGDPRChoices(Manager.Instance.IsAboveRequiredAge(),
                Manager.Instance.IsTermsAndConditionsAccepted(),
                Manager.Instance.IsAnalyticsGranted(),
                Manager.Instance.IsTailoredAdsGranted());

#if UNITY_IOS

            // Tracking authorization status upon start for lower iOS 14.5 devices
            if (!Manager.Instance.IsiOS14_5OrHigher)
            {
                switch (AppTrackingTransparency.TrackingAuthorizationStatus)
                {
                    case AppTrackingTransparency.AuthorizationStatus.NOT_DETERMINED:
                        HomaBelly.HomaBelly.Instance.TrackDesignEvent("app_start_tracking_not_determined");
                        HomaBelly.HomaBelly.Instance.TrackAttributionEvent("app_start_tracking_not_determined");
                        break;
                    case AppTrackingTransparency.AuthorizationStatus.AUTHORIZED:
                        HomaBelly.HomaBelly.Instance.TrackDesignEvent("app_start_tracking_allowed");
                        HomaBelly.HomaBelly.Instance.TrackAttributionEvent("app_start_tracking_allowed");
                        break;
                    case AppTrackingTransparency.AuthorizationStatus.DENIED:
                        HomaBelly.HomaBelly.Instance.TrackDesignEvent("app_start_tracking_denied");
                        HomaBelly.HomaBelly.Instance.TrackAttributionEvent("app_start_tracking_denied");
                        break;
                    case AppTrackingTransparency.AuthorizationStatus.RESTRICTED:
                        HomaBelly.HomaBelly.Instance.TrackDesignEvent("app_start_tracking_restricted");
                        HomaBelly.HomaBelly.Instance.TrackAttributionEvent("app_start_tracking_restricted");
                        break;
                }
            }

#if HOMA_STORE
            try
            {
                // Update user subscription attributes after IDFA accepted
                Purchases purchases = GameObject.FindObjectOfType<Purchases>();
                if (purchases != null)
                {
                    purchases.CollectDeviceIdentifiers();
                }
                else
                {
                    HomaGamesLog.Debug("Could not collect device identifiers for Purchases: Game Object not found");
                }
            }
            catch (Exception e)
            {
                HomaGamesLog.DebugFormat("Could not collect device identifiers for Purchases: {0}", e.Message);
            }
#endif
#endif

            // Track 'gdpr_first_accept' as the very first event after initialization
            HomaBelly.HomaBelly.Instance.TrackDesignEvent("gdpr_first_accept");
            HomaBelly.HomaBelly.Instance.TrackAttributionEvent("gdpr_first_accept");

            if (Manager.Instance.IsiOS14_5OrHigher)
            {
                // Send all the tracking events cached
                TriggerAnalyticDesignEvents();
                TriggerAttributionEvents();
            }

            // Deregister event (just in case)
            HomaBelly.Events.onInitialized -= OnHomaBellyInitialized;
        }

        /// <summary>
        /// Triggers al the queued analytic events
        /// </summary>
        private static void TriggerAnalyticDesignEvents()
        {
            if (analyticsDesignEventsToTrack == null || analyticsDesignEventsToTrack.Count == 0)
            {
                return;
            }

            while (analyticsDesignEventsToTrack.Count > 0)
            {
                string eventString = analyticsDesignEventsToTrack.Dequeue();
                HomaBelly.HomaBelly.Instance.TrackDesignEvent(eventString);
            }
        }

        /// <summary>
        /// Triggers all the queued attribution events
        /// </summary>
        private static void TriggerAttributionEvents()
        {
            if (attributionEventsToTrack == null || attributionEventsToTrack.Count == 0)
            {
                return;
            }

            while (attributionEventsToTrack.Count > 0)
            {
                string eventString = attributionEventsToTrack.Dequeue();
                HomaBelly.HomaBelly.Instance.TrackAttributionEvent(eventString);
            }
        }
    }
}