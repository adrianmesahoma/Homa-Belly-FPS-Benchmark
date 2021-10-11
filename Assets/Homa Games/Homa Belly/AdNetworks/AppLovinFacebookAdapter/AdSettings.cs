using System.Runtime.InteropServices;
using System;
using System.Linq;
using HomaGames.HomaBelly;

#if UNITY_IOS || UNITY_IPHONE
using UnityEngine.iOS;
#endif

namespace AudienceNetwork
{
    public static class AdSettings
    {
#if UNITY_IOS || UNITY_IPHONE
        [DllImport("__Internal")]
        private static extern void FBAdSettingsBridgeSetAdvertiserTrackingEnabled(bool advertiserTrackingEnabled);
#endif

        [HomaGames.HomaBelly.PreserveAttribute]
        public static void SetAdvertiserTrackingFlag()
        {
#if UNITY_IOS || UNITY_IPHONE
            Version currentVersion = new Version(Device.systemVersion); // Parse the version of the current OS
            Version ios14 = new Version("14.0"); // Parse the iOS 14.0 version constant

            // FAN AM confirmed we should inform ATE flag starting with iOS 14.0
            // otherwise FAN will default it to false
            if (currentVersion >= ios14)
            {
                // Obtain autorhization from AppTrackingTransparency with reflection
                bool advertiserTrackingEnabled = GetAppTrackingTransparencyAuthorizationStatus();
                HomaGamesLog.Debug($"Setting FacebookAudienceNetwork ATE Flag to: {advertiserTrackingEnabled}");
                FBAdSettingsBridgeSetAdvertiserTrackingEnabled(advertiserTrackingEnabled);
            }
            else
            {
                HomaGamesLog.Debug($"FacebookAudienceNetwork ATE Flag not required due to iOS version: {currentVersion}");
            }
#endif
        }

        /// <summary>
        /// Obtains ATT authorization from the GDPR package through reflection
        /// </summary>
        /// <returns></returns>
        [HomaGames.HomaBelly.PreserveAttribute]
        private static bool GetAppTrackingTransparencyAuthorizationStatus()
        {
#if !UNITY_IOS
            return true;
#else
            bool authorized = false;
            try
            {
                Type appTrackingTransparencyType = (from assembly in AppDomain.CurrentDomain.GetAssemblies()
                                       from type in assembly.GetTypes()
                                       where type.Namespace == "HomaGames.GDPR" && type.Name == "AppTrackingTransparency"
                                       select type).FirstOrDefault();
                if (appTrackingTransparencyType != null)
                {
                    System.Reflection.PropertyInfo authorizationStatusPropertyInfo = appTrackingTransparencyType.GetProperty("TrackingAuthorizationStatus", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                    if (authorizationStatusPropertyInfo != null)
                    {
                        var authorizationStatusValue = authorizationStatusPropertyInfo.GetValue(null, null);
                        if (authorizationStatusValue != null)
                        {
                            authorized = authorizationStatusValue.ToString() == "AUTHORIZED";
                            HomaGamesLog.Debug($"AppTrackingTransparency TrackingAuthorizationStatus found: {authorizationStatusValue}");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                HomaGamesLog.Warning($"HomaGames.GDPR.AppTrackingTransparency TrackingAuthorizationStatus property failed to invoke: {e.Message}");
            }

            return authorized;
#endif
        }
    }
}