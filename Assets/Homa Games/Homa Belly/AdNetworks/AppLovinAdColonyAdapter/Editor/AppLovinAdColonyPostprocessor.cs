using System.Collections.Generic;
using UnityEditor;

#if UNITY_IOS
using UnityEditor.Callbacks;
#endif

namespace HomaGames.HomaBelly
{
    /// <summary>
    /// Creates the configuration json file for AppLovin AdColony Adapter
    /// </summary>
    public class AppLovinAdColonyPostprocessor
    {
        [InitializeOnLoadMethod]
        static void Configure()
        {
            ConfigureiOSSettings();
            AndroidProguardUtils.AddProguardRules("\n# For communication with AdColony's WebView\r\n-keepclassmembers class * { \r\n    @android.webkit.JavascriptInterface <methods>; \r\n}\r\n# Keep ADCNative class members unobfuscated\r\n-keepclassmembers class com.adcolony.sdk.ADCNative** {\r\n    *;\r\n }");
        }

        /// <summary>
        /// Configure somre required iOS settings to work with AdColony.
        /// See: https://dash.applovin.com/documentation/mediation/unity/mediation-adapters?network=ADCOLONY_NETWORK
        /// </summary>
        private static void ConfigureiOSSettings()
        {
            PlayerSettings.iOS.cameraUsageDescription = "Taking selfies";
        }

#if UNITY_IOS
        [PostProcessBuild]
        public static void OnPostprocessBuild(BuildTarget buildTarget, string buildPath)
        {
            iOSPlistUtils.SetAppTransportSecurity(buildTarget, buildPath);
            iOSPlistUtils.SetApplicationQueriesSchemes(buildTarget, buildPath, new string[] {
                "fb",
                "instagram",
                "twitter",
                "tumblr"
            });

            iOSPlistUtils.SetRootStrings(buildTarget, buildPath, new KeyValuePair<string, string>[]
            {
                new KeyValuePair<string, string>("NSPhotoLibraryUsageDescription", "Taking selfies"),
                new KeyValuePair<string, string>("NSCameraUsageDescription", "Taking selfies"),
                new KeyValuePair<string, string>("NSMotionUsageDescription", "Interactive ad controls")
            });
        }
#endif
    }
}
