#if UNITY_IOS
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Callbacks;
#endif

namespace HomaGames.HomaBelly
{
    /// <summary>
    /// Creates the configuration json file for AppLovin Pangle Adapter
    /// </summary>
    public class AppLovinPanglePostprocessor
    {
#if UNITY_IOS
        [PostProcessBuild]
        public static void OnPostprocessBuild(BuildTarget buildTarget, string buildPath)
        {
            iOSPlistUtils.SetAppTransportSecurity(buildTarget, buildPath);
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
