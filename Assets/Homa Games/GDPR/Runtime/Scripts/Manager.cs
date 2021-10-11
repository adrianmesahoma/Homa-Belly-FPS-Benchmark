/*
 * Licensed to the Apache Software Foundation (ASF) under one
 * or more contributor license agreements.  See the NOTICE file
 * distributed with this work for additional information
 * regarding copyright ownership.  The ASF licenses this file
 * to you under the Apache License, Version 2.0 (the
 * "License"); you may not use this file except in compliance
 * with the License.  You may obtain a copy of the License at
 * 
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing,
 * software distributed under the License is distributed on an
 * "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
 * KIND, either express or implied.  See the License for the
 * specific language governing permissions and limitations
 * under the License.
 */

using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_IOS
using UnityEngine.iOS;
#endif

namespace HomaGames.GDPR
{
    /// <summary>
    /// Entry point for Homa Games GDPR features
    /// </summary>
    public sealed class Manager
    {
        #region Singleton pattern

        private static readonly Manager instance = new Manager();

        public static Manager Instance
        {
            get { return instance; }
        }

        #endregion

        #region Public properties

        /// <summary>
        /// Callback invoked when the GDPR UI is shown
        /// </summary>
        public static event System.Action OnShow
        {
            add => View.OnFirstViewShown += value;
            remove => View.OnFirstViewShown -= value;
        }

        /// <summary>
        /// Callback invoked when the GDPR UI is dismissed. When this
        /// method gets invoked, all user decisions can be retrieved
        /// through corresponding Manager accessors.
        /// </summary>
        public static event System.Action OnDismiss
        {
            add => View.OnAllViewDismissed += value;
            remove => View.OnAllViewDismissed -= value;
        }

        private static PrivacyResponse privacyResponse = default;
        public static bool IsGdprProtectedRegion
        {
            get
            {
                return privacyResponse != null ? privacyResponse.Protected : true;
            }
        }

        public bool IsiOS14_5OrHigher
        {
            get
            {
#if UNITY_IOS && !UNITY_EDITOR
                Version currentVersion = new Version(Device.systemVersion); // Parse the version of the current OS
                Version ios14_5 = new Version("14.5"); // Parse the iOS 14.5 version constant

                if (currentVersion >= ios14_5)
                {
                    return true;
                }

                return false;
#elif UNITY_EDITOR
                return true;
#else
                return false;
#endif
            }
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Show the GDPR UI
        /// </summary>
        public void Show()
        {
            // Load settings to determine if GDPR screen should be never shown
            Settings settings = (Settings)Resources.Load(Constants.SETTINGS_RESOURCE_NAME, typeof(Settings));
            Show(Application.internetReachability == NetworkReachability.NotReachable, settings.ForceDisableGdpr);
        }

        /// <summary>
        /// Show the GDPR UI
        /// </summary>
        public async void Show(bool internetReachable, bool forceDisableGdpr)
        {
            if (IsTermsAndConditionsAccepted())
            {
                Debug.Log($"[Homa Games GDPR] Showing GDPR Settings page");

                // If TOS and PP are already accepted, skip welcome page
                GDPRSettingsView.Instance.Enter();
            }
            // If GDPR is disabled through NTesting, skip GDPRWelcome page setting all decisions to `true`
            else if (forceDisableGdpr || !internetReachable || !GDPRGeryonUtils.GetGeryonDynamicVariableValue("enable_gdpr", true))
            {
                Debug.Log($"[Homa Games GDPR] Skipping GDPR. Disabled: {(forceDisableGdpr ? "locally" : "NTesting")}");
                GDPRWelcomeView.Instance.SkipGdprWelcomeView();
            }
            else
            {
                Debug.Log($"[Homa Games GDPR] Showing GDPR");

                // On first run, fetch privacy response
                privacyResponse = default;
                await PrivacyResponse.FetchPrivacyForCurrentRegion()
                    .ContinueWith((result) =>
                    {
                        privacyResponse = result.Result;
                        Debug.Log($"[Homa Games GDPR] Region detection. Protected region? {privacyResponse.Protected}");
                    }, TaskScheduler.FromCurrentSynchronizationContext());

                // If the privacy region detection informs it is not Protected, skip GDPRWelcome page setting all decisions to `true`
                if (!IsGdprProtectedRegion)
                {
                    Debug.Log($"[Homa Games GDPR] Skipping GDPR");
                    GDPRWelcomeView.Instance.SkipGdprWelcomeView();
                }
                else
                {
                    Debug.Log($"[Homa Games GDPR] GDPR shown");
                    // If not, show welcome page
                    GDPRWelcomeView.Instance.Enter();
                }
            }
        }

        /// <summary>
        /// Obtain either if user is above required age or not.
        /// </summary>
        /// <returns>True if user explicitly asserted being above the required age. False otherwise</returns>
        public bool IsAboveRequiredAge()
        {
            return PlayerPrefs.GetInt(Constants.PersistenceKey.ABOVE_REQUIRED_AGE, 0) == 1;
        }

        /// <summary>
        /// Obtain either if user accepted Terms & Conditions or not.
        /// </summary>
        /// <returns>True if user accepted Terms & Conditions. False otherwise</returns>
        public bool IsTermsAndConditionsAccepted()
        {
            return PlayerPrefs.GetInt(Constants.PersistenceKey.TERMS_AND_CONDITIONS, 0) == 1;
        }

        /// <summary>
        /// Obtain either if user granted Analytics tracking or not.
        /// </summary>
        /// <returns>True if user granted Analytics tracking. False otherwise</returns>
        public bool IsAnalyticsGranted()
        {
            return PlayerPrefs.GetInt(Constants.PersistenceKey.ANALYTICS_TRACKING, 0) == 1;
        }

        /// <summary>
        /// Obtain either if user granted Tailored Ads permission or not.
        /// </summary>
        /// <returns>True if user granted Tailored Ads permission. False otherwise</returns>
        public bool IsTailoredAdsGranted()
        {
            return PlayerPrefs.GetInt(Constants.PersistenceKey.TAILORED_ADS, 0) == 1;
        }

        /// <summary>
        /// Return if the IOS IDFA onboarding flow has been asked already.
        /// </summary>
        /// <returns>True if already asked.</returns>
        public bool IsIOSIDFAFlowDone()
        {
            return PlayerPrefs.GetInt(Constants.PersistenceKey.IOS_ADS_TRACKING_ASKED, 0) == 1;
        }

        #endregion
    }
}