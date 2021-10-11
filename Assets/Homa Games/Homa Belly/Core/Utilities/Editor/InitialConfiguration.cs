using System.IO;
using UnityEditor;
using UnityEngine;

namespace HomaGames.HomaBelly
{
    public class InitialConfiguration
    {
        [InitializeOnLoadMethod]
        static void Configure()
        {
            #region Android settings
            // Gradle build system
            EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
            PlayerSettings.Android.minSdkVersion = (AndroidSdkVersions) Mathf.Max((int) PlayerSettings.Android.minSdkVersion, (int) AndroidSdkVersions.AndroidApiLevel21);
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARMv7 | AndroidArchitecture.ARM64;
            ConfigureGradleTemplate();
            #endregion

            HomaBellyEditorLog.Debug("Project configured");
        }

        private static void ConfigureGradleTemplate()
        {
            string mainTemplateGradlePath = Application.dataPath + "/Plugins/Android/mainTemplate.gradle";
            if (File.Exists(mainTemplateGradlePath))
            {
                HomaBellyEditorLog.Debug("Gradle file detected");
            }
        }
    }
}
