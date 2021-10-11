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
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace HomaGames.GDPR
{
    /// <summary>
    /// Utilities to inform Homa Belly (through reflection) with user's GDPR
    /// choices. If no Homa Belly instance is found, this utilities do nothing.
    /// </summary>
    public class HomaBellyUtilities : MonoBehaviour
    {
        #region Constants
        /*
         *  These constants are `public` to ease testability. A test will ensure
         *  this constant values are the proper ones defined in Homa Belly product.
         */
        public const string HOMA_BELLY_NAMESPACE = "HomaGames.HomaBelly";
        public const string HOMA_BELLY_INSTANCE = "HomaBelly";

        public const string METHOD_SET_USER_ABOVE_REQUIRED_AGE = "SetUserIsAboveRequiredAge";
        public const string METHOD_SET_TERMS_AND_CONDITIONS_ACCEPTANCE = "SetTermsAndConditionsAcceptance";
        public const string METHOD_SET_ANALYTICS_TRACKING = "SetAnalyticsTrackingConsentGranted";
        public const string METHOD_SET_TAILORED_ADS = "SetTailoredAdsConsentGranted";

        #endregion

        /// <summary>
        /// Inform Homa Belly with user's GDPR choices
        /// </summary>
        /// <param name="userAboveRequiredAge">If user informed is above required age or not</param>
        /// <param name="termsAndConditionsAccepted">If user accepted terms and conditions or not</param>
        /// <param name="analyticksTrackingConsent">If user accepted analyticks tracking or not</param>
        /// <param name="tailoredAdsConsent">If user accepted tailored ads or not</param>
        public static void InformHomaBellyWithGDPRChoices(bool userAboveRequiredAge,
            bool termsAndConditionsAccepted,
            bool analyticksTrackingConsent,
            bool tailoredAdsConsent)
        {
            InvokeHomaBellyMethodWithConsentValue(METHOD_SET_USER_ABOVE_REQUIRED_AGE, userAboveRequiredAge);
            InvokeHomaBellyMethodWithConsentValue(METHOD_SET_TERMS_AND_CONDITIONS_ACCEPTANCE, termsAndConditionsAccepted);
            InvokeHomaBellyMethodWithConsentValue(METHOD_SET_ANALYTICS_TRACKING, analyticksTrackingConsent);
            InvokeHomaBellyMethodWithConsentValue(METHOD_SET_TAILORED_ADS, tailoredAdsConsent);
        }

        #region Private methods

        /// <summary>
        /// Invoke Homa Belly's `methodName` method with the given bool value
        /// </summary>
        /// <param name="methodName">The method name to be invoked if found</param>
        /// <param name="consent">The required parameter for the invoked method</param>
        /// <returns>True if successfully inkoved, false otherwise</returns>
        private static bool InvokeHomaBellyMethodWithConsentValue(string methodName, bool consent)
        {
            try
            {
                Debug.Log($"Informing Homa Belly method {methodName} with value: {consent}");
                Type homaBellyIntanceType = (from assembly in AppDomain.CurrentDomain.GetAssemblies()
                                        from type in assembly.GetTypes()
                                        where type.Namespace == HOMA_BELLY_NAMESPACE && type.Name == HOMA_BELLY_INSTANCE
                                        select type).FirstOrDefault();
                if (homaBellyIntanceType != null)
                {
                    PropertyInfo homaBellyInstancePropertyInfo = homaBellyIntanceType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
                    if (homaBellyInstancePropertyInfo != null)
                    {
                        MethodInfo methodInfo = homaBellyInstancePropertyInfo.PropertyType.GetMethod(methodName);
                        if (methodInfo != null)
                        {
                            var homaBellyInstance = homaBellyInstancePropertyInfo.GetValue(null, null);
                            var boolResult = methodInfo.Invoke(homaBellyInstancePropertyInfo.GetValue(null, null), new object[] { consent });
                            Debug.Log($"Homa Belly {methodName} successfully informed");
                            return true;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Homa Belly's {methodName} method failed to invoke: {e.Message}");
            }

            return false;
        }

        #endregion
    }
}