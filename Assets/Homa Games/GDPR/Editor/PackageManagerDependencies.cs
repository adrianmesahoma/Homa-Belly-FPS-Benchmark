using UnityEditor;
using System;
using System.Collections.Generic;

#if UNITY_2018_4_OR_NEWER
using UnityEditor.PackageManager.Requests;
using UnityEditor.PackageManager;
#endif
using UnityEngine;

namespace HomaGames.GDPR
{
    public class PackageManagerDependencies
    {
        private const string DEPENDENCIES_MET_KEY = "homagames.gdpr.dependencies_met";
        private const string HOMA_GAMES_GDPR_TMP_DEFINE = "homagames_gdpr_textmeshproavailable";
        private const string TMP_PACKAGE_ID = "com.unity.textmeshpro";

#if UNITY_2018_4_OR_NEWER
        private static AddRequest installationRequest;
        private static ListRequest listRequest;

        [InitializeOnLoadMethod]
        static void CheckDependencies()
        {
            bool dependenciesMet = EditorPrefs.GetBool(DEPENDENCIES_MET_KEY, false);
            if (!dependenciesMet)
            {
                listRequest = Client.List();
                EditorApplication.update += ListProgress;

                EditorPrefs.SetBool(DEPENDENCIES_MET_KEY, true);
            }
            else
            {
                // Ensure define is always present
                UpdateDefines(HOMA_GAMES_GDPR_TMP_DEFINE, true, new BuildTargetGroup[] { BuildTargetGroup.iOS, BuildTargetGroup.Android });
            }
        }

        static void ListProgress()
        {
            if (listRequest.IsCompleted)
            {
                if (listRequest.Status == StatusCode.Success)
                {
                    bool installed = false;
                    foreach (var package in listRequest.Result)
                    {
                        if (package.name == TMP_PACKAGE_ID)
                        {
                            // TMP is installed, do nothing
                            Debug.Log("Detected text mesh pro package");
                            UpdateDefines(HOMA_GAMES_GDPR_TMP_DEFINE, true, new BuildTargetGroup[] { BuildTargetGroup.iOS, BuildTargetGroup.Android });
                            installed = true;
                        }
                    }

                    // If it is not installed, add it
                    if (!installed)
                    {
                        RequestInstallation();
                    }
                }
                else if (listRequest.Status >= StatusCode.Failure)
                {
                    Debug.Log(listRequest.Error.message);
                }

                EditorApplication.update -= ListProgress;
            }
        }

        static void InstallationProgress()
        {
            if (installationRequest.IsCompleted)
            {
                if (installationRequest.Status == StatusCode.Success)
                {
                    Debug.Log("Installed: " + installationRequest.Result.packageId);
                    UpdateDefines(HOMA_GAMES_GDPR_TMP_DEFINE, true, new BuildTargetGroup[] { BuildTargetGroup.iOS, BuildTargetGroup.Android });
                }
                else if (installationRequest.Status >= StatusCode.Failure)
                {
                    Debug.Log(installationRequest.Error.message);
                    RequestInstallation();
                }

                EditorApplication.update -= InstallationProgress;
            }
        }

        private static void RequestInstallation()
        {
            bool result = EditorUtility.DisplayDialog("Homa Games GDPR", "Text Mesh Pro is required for GDPR module. It will be automatically imported now", "Accept", "Cancel");
            if (result)
            {
                installationRequest = Client.Add(TMP_PACKAGE_ID);
                EditorApplication.update += InstallationProgress;
            }
            else
            {
                Debug.LogError("Homa Games GDPR module requires Text Mesh Pro package. Please install it through Package Manager to avoid compilation errors");
            }
        }

        private static void UpdateDefines(string entry, bool enabled, BuildTargetGroup[] groups)
        {
            foreach (var group in groups)
            {
                var defines = new List<string>(PlayerSettings.GetScriptingDefineSymbolsForGroup(group).Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries));
                var edited = false;
                if (enabled && !defines.Contains(entry))
                {
                    defines.Add(entry);
                    edited = true;
                }
                else if (!enabled && defines.Contains(entry))
                {
                    defines.Remove(entry);
                    edited = true;
                }
                if (edited)
                {
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(group, string.Join(";", defines.ToArray()));
                }
            }
        }
#endif
    }
}
