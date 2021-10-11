using System.Collections.Generic;
using System.IO;
using HomaGames.HomaBelly.Utilities;
using UnityEditor;

#if UNITY_IOS
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
#endif

namespace HomaGames.HomaBelly
{
    /// <summary>
    /// Configure Singular after importing the package
    /// </summary>
    public class SingularPostprocessor
    {
        [InitializeOnLoadMethod]
        static void Configure()
        {
            HomaBellyEditorLog.Debug($"Configuring {HomaBellySingularConstants.ID}");
            PluginManifest pluginManifest = PluginManifest.LoadFromLocalFile();

            if (pluginManifest != null)
            {
                PackageComponent packageComponent = pluginManifest.Packages
                    .GetPackageComponent(HomaBellySingularConstants.ID, HomaBellySingularConstants.TYPE);
                if (packageComponent != null)
                {
                    Dictionary<string, string> configurationData = packageComponent.Data;

                    // Create directory if does not exist
                    string parentPath = Directory.GetParent(HomaBellySingularConstants.CONFIG_FILE).ToString();
                    if (!string.IsNullOrEmpty(parentPath) && !Directory.Exists(parentPath))
                    {
                        Directory.CreateDirectory(parentPath);
                    }

                    File.WriteAllText(HomaBellySingularConstants.CONFIG_FILE, Json.Serialize(configurationData));
                }
            }
            else
            {
                HomaBellyEditorLog.Error($"Singular configuration data not found. Skipping.");
            }

            AndroidProguardUtils.AddProguardRules("\n-keep class com.singular.sdk.** { *; }\r\n-keep public class com.android.installreferrer.** { *; }\r\n-keep public class com.singular.unitybridge.** { *; }");
        }

#if UNITY_IOS
        [PostProcessBuild]
        public static void OnPostprocessBuild(BuildTarget buildTarget, string buildPath)
        {
            iOSPbxProjectUtils.AddFrameworks(buildTarget, buildPath, new string[] {
                "AdSupport.framework",
                "SystemConfiguration.framework",
                "Security.framework",
                "iAd.framework",
                "libsqlite3.0.tbd",
                "libz.tbd",
                "WebKit.framework"
            });

            // Adding 'weak' AdServices.framework
            if (buildTarget == BuildTarget.iOS)
            {
                PBXProject project = new PBXProject();
                string projectPath = PBXProject.GetPBXProjectPath(buildPath);
                project.ReadFromFile(projectPath);

                string targetId;
#if UNITY_2019_3_OR_NEWER
                targetId = project.GetUnityFrameworkTargetGuid();
#else
                targetId = project.TargetGuidByName("Unity-iPhone");
#endif

                // The 'weak'  flag is set to `true`, which makes it optional
                project.AddFrameworkToProject(targetId, "AdServices.framework", true);
                project.WriteToFile(PBXProject.GetPBXProjectPath(buildPath));
            }
        }
#endif
    }
}
