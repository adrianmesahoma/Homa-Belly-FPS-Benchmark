using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Android;
using UnityEngine;
using System;
using System.IO;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.PackageManager.Requests;

#if UNITY_IOS
using UnityEditor.Callbacks;
#endif

namespace HomaGames.HomaBelly.IAP
{
    public class PurchasesPostprocessor : IPostGenerateGradleAndroidProject, IPreprocessBuildWithReport
    {
        private const string UNITY_PURCHASING_PACKAGE_NAME = "com.unity.purchasing";
        private static ListRequest unityPackageManagerListRequest;

        [InitializeOnLoadMethod]
        static void Configure()
        {
            // If Unity is executed in batch mode (CI or build machines), do not
            // modify nor configure settings, as this is only intended
            // when the developer integrates the SDK for the very first time
            if (UnityEditorInternal.InternalEditorUtility.inBatchMode)
            {
                return;
            }

            UpdateCscFileWithHomaStoreDefine();
            HomaBellyEditorLog.Debug($"Configuring {HomaBellyPurchasesConstants.ID}");
            CatalogManager.TryCreateAndConfigureCatalog();

            // Always resolve Unity Purchasing
            TryRemoveUnityPurchasing();
        }

        /// <summary>
        /// When using Homa Games Purchases and Singular attribution,
        /// Android dependencies will resolve play-service-ads to it version
        /// 17.0.0, which makes the com.google.android.gms.ads.APPLICATION_ID meta-tag
        /// mandatory. If not present, we add the AdMob Test App ID
        /// </summary>
        /// <param name="basePath"></param>
        public void OnPostGenerateGradleAndroidProject(string basePath)
        {
#if UNITY_ANDROID
            try
            {
                UnityEngine.Debug.Log($"OnPostGenerateGradleAndroidProject");
                string manifestContents = System.IO.File.ReadAllText(System.IO.Path.Combine(basePath, "src/main/AndroidManifest.xml"));
                if (manifestContents != null && !manifestContents.Contains("com.google.android.gms.ads.APPLICATION_ID"))
                {
                    UnityEngine.Debug.Log($"Manifest not containing com.google.android.gms.ads.APPLICATION_ID. Setting Homa Test App ID");
                    AndroidManifest androidManifest = AndroidManifest.FromPostGenerateGradleAndroidProject(basePath);
                    androidManifest.AddMetadata("com.google.android.gms.ads.APPLICATION_ID", "ca-app-pub-8265605982789845~3666659913");
                }
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.Log($"Exception while reading AndroidManifest.xml: {e.Message}");
            }
#endif
        }

        public int callbackOrder { get { return 99; } }
        public void OnPreprocessBuild(BuildReport report)
        {
            // Update CSC file before building to cover possible use cases where
            // user does not commits the file to version control
            UpdateCscFileWithHomaStoreDefine();

            // Restore Catalog from Manifest if it is empty
            CatalogManager.TryCreateAndConfigureCatalog();
        }

#if UNITY_IOS
        [PostProcessBuild]
        public static void OnPostprocessBuild(BuildTarget buildTarget, string buildPath)
        {
            iOSPbxProjectUtils.AddBuildProperties(buildTarget, buildPath, new KeyValuePair<string, string>[]
            {
                new KeyValuePair<string, string>("CLANG_ENABLE_MODULES", "YES")
            });
        }
#endif


        /// <summary>
        /// Try to find 'csc.rsp' file under 'Assets' folder
        /// and write HOMA_STORE define on it.
        ///
        /// If not found, it is created
        /// </summary>
        private static void UpdateCscFileWithHomaStoreDefine()
        {
            try
            {
                // Create file if it does not exist
                string cscFilePath = Path.Combine(Application.dataPath, "csc.rsp");
                if (!File.Exists(cscFilePath))
                {
                    HomaGamesLog.Debug("csc.rsp file does not exist. Creating it...");
                    File.Create(cscFilePath).Close();
                }

                // Append HOMA_STORE define
                string cscFileContents = File.ReadAllText(cscFilePath);
                if (!cscFileContents.Contains("-define:HOMA_STORE"))
                {
                    HomaGamesLog.Debug("Adding HOMA_STORE definition");
                    cscFileContents += "\n-define:HOMA_STORE";
                    File.WriteAllText(cscFilePath, cscFileContents);
                }
            }
            catch (Exception e)
            {
                HomaGamesLog.WarningFormat("Could not create csc.rsp file: ", e.Message);
            }
        }

        /// <summary>
        /// Try to automatically resolve any IAP library conflict within
        /// Homa Belly Purchases and Unity IAP
        /// </summary>
        private static void TryRemoveUnityPurchasing()
        {
            RemoveUnityPurchasingFromUpm();

            // Try to solve any potential library duplication (just in case)
            ResolveAndroidBillingLibraryConflicts();

        }

        /// <summary>
        /// Detect and remove Unity Purchasing from UPM (Unity 2019+)
        /// </summary>
        private static void RemoveUnityPurchasingFromUpm()
        {
            // UPM API is only available on Unity 2019+
#if UNITY_2019
            unityPackageManagerListRequest = UnityEditor.PackageManager.Client.List();
            EditorApplication.update += UnityPackageManagerListProgressUpdate;
#endif
        }

#if UNITY_2019
        private static void UnityPackageManagerListProgressUpdate()
        {
            // List request completed and succeeded
            if (unityPackageManagerListRequest != null && unityPackageManagerListRequest.IsCompleted)
            {
                if (unityPackageManagerListRequest.Status == UnityEditor.PackageManager.StatusCode.Success)
                {
                    foreach (var package in unityPackageManagerListRequest.Result)
                    {
                        if (package.name == UNITY_PURCHASING_PACKAGE_NAME)
                        {
                            bool removePackage = EditorUtility.DisplayDialog("Homa Belly Purchases", "We have detected Unity Purchasing package installed through Unity Package Manager. " +
                                "Homa Belly Purchases require this package to be uninstalled.\n\nDo you want us to remove it for you?", "Yes", "I will do it");

                            if (removePackage)
                            {
                                HomaGamesLog.Debug("Removing Unity Purchasing package from UPM...");
                                UnityEditor.PackageManager.Client.Remove(UNITY_PURCHASING_PACKAGE_NAME);
                            }
                            else
                            {
                                HomaGamesLog.Error("Homa Belly Purchases conflicting with Unity Purchasing. The project may not work nor compile until Unity Purchasing is uninstalled.");
                            }
                        }
                    }
                }

                EditorApplication.update -= UnityPackageManagerListProgressUpdate;
            }
        }
#endif

        /// <summary>
        /// If the project contains Unity IAP, we need to get rid of
        /// Android's com.android.billingclient.billing library, as it is
        /// already included by RevenueCat
        /// </summary>
        private static void ResolveAndroidBillingLibraryConflicts()
        {
            try
            {
                AssetDatabase.DeleteAsset("Assets/Plugins/UnityPurchasing");
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.Log($"Exception deleting Plugins/UnityPurchasing: {e.Message}");
            }

            try
            {
                var unityConnectSettings = Path.GetFullPath("ProjectSettings/UnityConnectSettings.asset");
                if (File.Exists(unityConnectSettings))
                {
                    string unityConnectSettingsContents = File.ReadAllText(unityConnectSettings);
                    if (unityConnectSettingsContents.Contains("UnityPurchasingSettings:\n\tm_Enabled: 1"))
                    {
                        UnityEngine.Debug.Log($"Removing Unity Purchasing from Services");
                        unityConnectSettingsContents.Replace("UnityPurchasingSettings:\n\tm_Enabled: 1", "UnityPurchasingSettings:\n\tm_Enabled: 0");
                        File.WriteAllText("ProjectSettings/UnityConnectSettings.asset", unityConnectSettingsContents);
                    }
                }
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.Log($"Exception deleting Plugins/UnityPurchasing: {e.Message}");
            }

            try
            {
                string[] billingFiles = System.IO.Directory.GetFiles(Application.dataPath, "com.android.billingclient.billing*", System.IO.SearchOption.AllDirectories);
                billingFiles = billingFiles.Concat(System.IO.Directory.GetFiles(Application.dataPath, "billing-*", System.IO.SearchOption.AllDirectories)).ToArray();
                if (billingFiles != null && billingFiles.Length > 0)
                {
                    // Android billing library found
                    for (int i = 0; i < billingFiles.Length; i++)
                    {
                        string billingFile = billingFiles[i];

                        // If the found billing library is not in the Plugins/Android folder,
                        // delete it
                        if (!billingFile.StartsWith(System.IO.Path.Combine(Application.dataPath, "Plugins/Android")))
                        {
                            UnityEngine.Debug.Log($"Deleting Android billing file: {billingFile}");
                            System.IO.File.Delete(billingFile);

                            // Refresh asset database
                            AssetDatabase.Refresh();
                        }

                    }
                }
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.Log($"Exception resolving Android Billing: {e.Message}");
            }

            try
            {
                var unityPluginsPath = Path.GetFullPath("Packages/com.unity.purchasing");
                if (Directory.Exists(unityPluginsPath))
                {
                    Directory.Delete(unityPluginsPath, true);

                    // Refresh asset database
                    AssetDatabase.Refresh();
                }
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.Log($"Exception resolving Android Billing: {e.Message}");
            }
        }
    }
}
