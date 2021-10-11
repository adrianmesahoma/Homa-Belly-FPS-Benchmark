using UnityEngine;
using UnityEditor;
using HomaGames.HomaBelly.Utilities;
using System;
using System.Collections.Generic;

#if UNITY_IOS
using UnityEditor.Callbacks;
#endif

namespace HomaGames.HomaBelly
{
    /// <summary>
    /// Postprocessor executed upon iOS build. It fetches any
    /// configuration from servers and applies it to the build:
    /// 
    /// - List of SkAdNetwork IDs to be added to Info.plist
    /// </summary>
    public class iOSPostProcessBuild
    {
#if UNITY_IOS
        [MenuItem("Window/Test/iOS Post Build")]
        public static void Test()
        {
            OnPostprocessBuild(BuildTarget.iOS, "");
        }

        [PostProcessBuild]
        public static void OnPostprocessBuild(BuildTarget buildTarget, string buildPath)
        {
            try
            {
                // Fetch Post Build model
                PostBuildModel model = FetchPostBuildModel();

                // If Homa Belly manifest contains any mediator, try to add SkAdNetworkIDs
                PluginManifest pluginManifest = PluginManifest.LoadFromLocalFile();
                if (pluginManifest != null
                    && pluginManifest.Packages.MediationLayers != null
                    && pluginManifest.Packages.MediationLayers.Count > 0
                    && model.SkAdNetworkIds != null
                    && model.SkAdNetworkIds.Length > 0)
                {
                    UnityEngine.Debug.Log($"[Post Build] Adding SkAdNetworkIDs to Info.plist: {Json.Serialize(model.SkAdNetworkIds)}");
                    iOSPlistUtils.AppendAdNetworkIds(buildTarget, buildPath, model.SkAdNetworkIds);
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogWarning($"[Post Build] Exception thrown while post build processing: {e.Message}");
            }
        }


        #region Private helpers

        private static PostBuildModel FetchPostBuildModel()
        {
            EditorHttpCaller<PostBuildModel> editorHttpCaller = new EditorHttpCaller<PostBuildModel>();
            string appBuild = string.Format(PostBuildConstants.API_APP_POST_BUILD, PluginManifest.LoadFromLocalFile().AppToken, "IPHONE");
            return editorHttpCaller.Get(appBuild, new PostBuildModelDeserializer()).Result;
        }

        #endregion
#endif
    }
}
