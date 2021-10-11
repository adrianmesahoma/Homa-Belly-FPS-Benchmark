using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace HomaGames.HomaBelly
{
    public class GeryonPostprocessor
    {
        private static string GERYON_FIRST_TIME_CONFIGURED_KEY = "com.homagames.homabell.geryon_first_time_configured";

        [InitializeOnLoadMethod]
        static void Configure()
        {
            // Only automatically configure the first time. This will alow dev to later
            // customize the ID
            if (!EditorPrefs.GetBool(GERYON_FIRST_TIME_CONFIGURED_KEY, false))
            {
                HomaBellyEditorLog.Debug($"Configuring {HomaBellyGeryonConstants.ID}");
                PluginManifest pluginManifest = PluginManifest.LoadFromLocalFile();

                if (pluginManifest != null)
                {
                    PackageComponent packageComponent = pluginManifest.Packages
                        .GetPackageComponent(HomaBellyGeryonConstants.ID, HomaBellyGeryonConstants.TYPE);
                    if (packageComponent != null)
                    {
                        Dictionary<string, string> configurationData = packageComponent.Data;

                        // Setup Geryon Settings
                        try
                        {
                            Geryon.Settings settings = (Geryon.Settings)Resources.Load("Settings", typeof(Geryon.Settings));
                            if (settings != null)
                            {
                                if (configurationData.ContainsKey("s_ios_app_id") && string.IsNullOrEmpty(settings.GetIOSID()))
                                {
                                    settings.SetiOSID(configurationData["s_ios_app_id"]);
                                }

                                if (configurationData.ContainsKey("s_android_app_id") && string.IsNullOrEmpty(settings.GetAndroidID()))
                                {
                                    settings.SetAndroidID(configurationData["s_android_app_id"]);
                                }
                            }
                        }
                        catch (System.Exception e)
                        {
                            HomaBellyEditorLog.Error($"Error configuring Geryon: {e.Message}");
                        }
                    }
                }

                EditorPrefs.SetBool(GERYON_FIRST_TIME_CONFIGURED_KEY, true);
            }
        }
    }
}
