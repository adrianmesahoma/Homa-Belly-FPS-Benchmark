using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using System.ComponentModel;
using System.IO;
using System;

#if UNITY_IOS || UNITY_IPHONE
using UnityEditor.iOS.Xcode;
#endif

namespace HomaGames.HomaBelly
{
    public class FirebasePostProcessor
    {
        [InitializeOnLoadMethod]
        static void Configure()
        {
            // Ensure API Compatibiliy level is NET 4 as it is required for Firebase libraries
            PlayerSettings.SetApiCompatibilityLevel(BuildTargetGroup.Android, ApiCompatibilityLevel.NET_4_6);
            PlayerSettings.SetApiCompatibilityLevel(BuildTargetGroup.iOS, ApiCompatibilityLevel.NET_4_6);

            // Configure any remote data
            HomaBellyEditorLog.Debug($"Configuring {HomaBellyFirebaseConstants.ID}");
            PluginManifest pluginManifest = PluginManifest.LoadFromLocalFile();

            if (pluginManifest != null)
            {
                PackageComponent packageComponent = pluginManifest.Packages
                    .GetPackageComponent(HomaBellyFirebaseConstants.ID, HomaBellyFirebaseConstants.TYPE);
                if (packageComponent != null)
                {
                    Dictionary<string, string> configurationData = packageComponent.Data;
                    if (configurationData.ContainsKey("b_enable_push_notifications"))
                    {
                        bool enablePushNotifications = true;
                        bool.TryParse(configurationData["b_enable_push_notifications"], out enablePushNotifications);
                        UpdateCscFileWithHomaPushNotificationsDefine(enablePushNotifications);
                    }
                }
            }
        }

        /// <summary>
        /// Try to find 'csc.rsp' file under 'Assets' folder
        /// and write HOMA_PUSH_NOTIFICATIONS define on it.
        ///
        /// If not found, it is created
        /// </summary>
        private static void UpdateCscFileWithHomaPushNotificationsDefine(bool enablePushNotifications)
        {
            try
            {
                // Create file if it does not exist
                string cscFilePath = Path.Combine(Application.dataPath, "csc.rsp");
                if (!File.Exists(cscFilePath) && enablePushNotifications)
                {
                    HomaGamesLog.Debug("csc.rsp file does not exist. Creating it...");
                    File.Create(cscFilePath).Close();
                }

                // Append HOMA_PUSH_NOTIFICATIONS define
                string cscFileContents = File.ReadAllText(cscFilePath);
                if (enablePushNotifications)
                {
                    if (!cscFileContents.Contains("-define:HOMA_PUSH_NOTIFICATIONS"))
                    {
                        HomaGamesLog.Debug("Adding HOMA_PUSH_NOTIFICATIONS definition");
                        cscFileContents += "\n-define:HOMA_PUSH_NOTIFICATIONS";
                        File.WriteAllText(cscFilePath, cscFileContents);
                    }
                }
                else
                {
                    if (cscFileContents.Contains("-define:HOMA_PUSH_NOTIFICATIONS"))
                    {
                        HomaGamesLog.Debug("Adding HOMA_PUSH_NOTIFICATIONS definition");
                        cscFileContents = cscFileContents.Replace("\n-define:HOMA_PUSH_NOTIFICATIONS", "\n");
                        File.WriteAllText(cscFilePath, cscFileContents);
                    }
                }
            }
            catch (Exception e)
            {
                HomaGamesLog.WarningFormat("Could not create csc.rsp file: ", e.Message);
            }
        }

#if UNITY_IOS || UNITY_IPHONE
        [PostProcessBuild]
        public static void OnPostprocessBuild(BuildTarget buildTarget, string buildPath)
        {
#if HOMA_PUSH_NOTIFICATIONS
            AddPushNotificationsCapability(buildTarget, buildPath);
#endif
        }

#if HOMA_PUSH_NOTIFICATIONS
        private static void AddPushNotificationsCapability(BuildTarget buildTarget, string buildPath)
        {
            // Add capabilities: https://firebase.google.com/docs/cloud-messaging/unity/client
            PBXProject project = new PBXProject();
            string projectPath = PBXProject.GetPBXProjectPath(buildPath);
            project.ReadFromFile(projectPath);

            string targetId;
#if UNITY_2019_3_OR_NEWER
            targetId = project.GetUnityMainTargetGuid();
#else
            targetId = project.TargetGuidByName("Unity-iPhone");
#endif

            var entitlementsFileName = project.GetBuildPropertyForAnyConfig(targetId, "CODE_SIGN_ENTITLEMENTS") ?? Application.identifier + ".entitlements";
            var capManager = new ProjectCapabilityManager(projectPath, entitlementsFileName, "Unity-iPhone");

            try
            {
                capManager.AddPushNotifications(false);
            }
            catch (WarningException e)
            {
                HomaBellyEditorLog.Debug($"WarningException for AddPushNotifications: {e.Message}");
            }

            try
            {
                capManager.AddBackgroundModes(BackgroundModesOptions.BackgroundFetch);
            }
            catch (WarningException e)
            {
                HomaBellyEditorLog.Debug($"WarningException for capability BackgroundModesOptions.BackgroundFetch: {e.Message}");
            }

            try
            {
                capManager.AddBackgroundModes(BackgroundModesOptions.RemoteNotifications);
            }
            catch (WarningException e)
            {
                HomaBellyEditorLog.Debug($"WarningException for capability BackgroundModesOptions.RemoteNotifications: {e.Message}");
            }

            capManager.WriteToFile();

            iOSPbxProjectUtils.AddFrameworks(buildTarget, buildPath, new string[] {
                "UserNotifications.framework"
            });
        }
#endif
#endif
    }
}