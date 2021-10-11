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
using HomaGames.HomaBelly.Utilities;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HomaGames.GDPR
{
    /// <summary>
    /// Script for default behavior showing the GDPR screens within "Homa Games GDPR Scene".
    /// Once user accepts to play, the next scene in Build Settings gets loaded.
    /// </summary>
    public class DefaultGDPRBehaviour : MonoBehaviour
    {

        [SerializeField]
        private GameObject loadingGameObject;

        private void OnEnable()
        {
#if UNITY_IOS
            // Track authorization initial status for iOS 14.5+
            if (Manager.Instance.IsiOS14_5OrHigher)
            {
                switch (AppTrackingTransparency.TrackingAuthorizationStatus)
                {
                    case AppTrackingTransparency.AuthorizationStatus.NOT_DETERMINED:
                        InitializationHelper.TrackDesignEvent("app_start_tracking_not_determined");
                        InitializationHelper.TrackAttributionEvent("app_start_tracking_not_determined");
                        break;
                    case AppTrackingTransparency.AuthorizationStatus.AUTHORIZED:
                        InitializationHelper.TrackDesignEvent("app_start_tracking_allowed");
                        InitializationHelper.TrackAttributionEvent("app_start_tracking_allowed");
                        break;
                    case AppTrackingTransparency.AuthorizationStatus.DENIED:
                        InitializationHelper.TrackDesignEvent("app_start_tracking_denied");
                        InitializationHelper.TrackAttributionEvent("app_start_tracking_denied");
                        break;
                    case AppTrackingTransparency.AuthorizationStatus.RESTRICTED:
                        InitializationHelper.TrackDesignEvent("app_start_tracking_restricted");
                        InitializationHelper.TrackAttributionEvent("app_start_tracking_restricted");
                        break;
                }
            }
#endif
            // Load settings to determine if GDPR screen should be never shown
            Settings settings = (Settings)Resources.Load(Constants.SETTINGS_RESOURCE_NAME, typeof(Settings));

            // If TOS and PP are already accepted, skip GDPR
            if (Manager.Instance.IsTermsAndConditionsAccepted())
            {
                ActivateNextScene();
            }
            else if (Application.internetReachability == NetworkReachability.NotReachable || settings.ForceDisableGdpr)
            {
                // Show GDPR without waiting for NTesting
                Manager.OnDismiss += OnDismiss;
                Manager.Instance.Show(false, settings.ForceDisableGdpr);
            }
            else
            {
                // Wait for Geryon to be initialized in case there is some remote configuration data
                GeryonUtils.WaitForInitialization().ContinueWith((taskResult) =>
                {
                    // Show GDPR
                    Manager.OnDismiss += OnDismiss;
                    Manager.Instance.Show(true, false);
                }, TaskScheduler.FromCurrentSynchronizationContext());
            }
        }

        private void OnDisable()
        {
            Manager.OnDismiss -= OnDismiss;
        }

        private void OnDismiss()
        {
            // Hide loading upon dismiss (it will prevent it being visible after finishing the flow)
            loadingGameObject?.SetActive(false);

            ActivateNextScene();
        }

        /// <summary>
        /// Loads the next scene available in Build Settings (if any)
        /// </summary>
        private void ActivateNextScene()
        {
            // No GDPR nor IDFA required. Initialize
            InitializationHelper.InitializeHomaBelly();

            int nextSceneBuildIndex = SceneManager.GetActiveScene().buildIndex + 1;
            if (nextSceneBuildIndex < SceneManager.sceneCountInBuildSettings)
            {
                SceneManager.LoadScene(nextSceneBuildIndex);
            }
            else
            { 
                Debug.LogWarning("[Homa Games GDPR] There is no next scene available in Build Settings");
            }
        }
    }
}