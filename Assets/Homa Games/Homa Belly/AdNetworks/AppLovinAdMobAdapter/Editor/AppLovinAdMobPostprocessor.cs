using System;
using System.Collections.Generic;
using UnityEditor;

namespace HomaGames.HomaBelly
{
    /// <summary>
    /// Creates the configuration json file for AppLovin AdMob Adapter
    ///
    /// See: https://dash.applovin.com/documentation/mediation/unity/mediation-adapters?network=ADMOB_NETWORK
    /// </summary>
    public class AppLovinAdMobPostprocessor
    {
        [InitializeOnLoadMethod]
        static void Configure()
        {

            HomaBellyEditorLog.Debug($"Configuring AppLovin AdMob Ad Network");
            PluginManifest pluginManifest = PluginManifest.LoadFromLocalFile();

            if (pluginManifest != null)
            {
                PackageComponent packageComponent = pluginManifest.Packages
                    .GetPackageComponent(AppLovinAdMobAdapterConstants.ID, AppLovinAdMobAdapterConstants.TYPE);
                if (packageComponent != null)
                {
                    Dictionary<string, string> configurationData = packageComponent.Data;

                    if (configurationData != null)
                    {
                        // Android
                        if (configurationData.ContainsKey("s_android_app_id") && !string.IsNullOrEmpty(configurationData["s_android_app_id"]))
                        {
                            try
                            {
                                AppLovinSettings.Instance.AdMobAndroidAppId = configurationData["s_android_app_id"];
                                HomaBellyEditorLog.Debug($"AppLovin Android AdMob Ad Network configured");
                            }
                            catch (Exception e)
                            {
                                HomaBellyEditorLog.Error($"Could not replace ADMOB_APP_ID: {e.Message}. Please visit https://dash.applovin.com/documentation/mediation/unity/mediation-adapters?network=ADMOB_NETWORK");
                            }
                        }

                        // iOS
                        if (configurationData.ContainsKey("s_ios_app_id") && !string.IsNullOrEmpty(configurationData["s_ios_app_id"]))
                        {
                            try
                            {
                                AppLovinSettings.Instance.AdMobIosAppId = configurationData["s_ios_app_id"];
                                HomaBellyEditorLog.Debug($"IronSource iOS AdMob Ad Network configured");
                            }
                            catch (Exception e)
                            {
                                HomaBellyEditorLog.Error($"Could not replace ADMOB_APP_ID: {e.Message}. Please visit https://dash.applovin.com/documentation/mediation/unity/mediation-adapters?network=ADMOB_NETWORK");
                            }
                        }
                    }
                }
            }
        }
    }
}
