using UnityEditor;
using UnityEngine;
using System.IO;
using System.Text.RegularExpressions;
using System;
using System.Threading.Tasks;

namespace HomaGames.Geryon
{
    /// <summary>
    /// Editor window to allow developer to configure:
    /// - IOS App ID
    /// - Android App ID
    ///
    /// Once data is fulfilled, the `Setup` button will request
    /// server the default remote config values.
    /// </summary>
    internal sealed class SettingsEditor : EditorWindow
    {
        #region Input detection
        private const string VALID_APP_ID_REGEX = "^([0-9]+)$|^([a-zA-Z]+[0-9]*[a-zA-Z]*\\.{1})+([a-zA-Z]+[0-9]*[a-zA-Z]*\\.*)+";
        #endregion

        private HttpCaller httpCaller = new HttpCaller();
        private int requesting;
        private string DVRTemplate;
        private Settings settings;
        private SerializedObject settingsSerializedObject;
        private string androidAppId;
        private string iOSAppId;

        [MenuItem(Constants.SETTINGS_MENU_ITEM_PATH)]
        internal static void CreateSettingsAndFocus()
        {
            GetWindow(typeof(SettingsEditor), false, Constants.SDK_NAME, true);
        }

        private void Initialize()
        {
            LoadSettings();
        }

        private void LoadSettings()
        {
            settings = Resources.Load<Settings>(Constants.SETTINGS_FILENAME);
            if (settings == null)
            {
                // Create the directory to hold Settings asset (if not already created)
                string resourcesFolder = string.Format("{0}/{1}", Application.dataPath,
                    Constants.SETTINGS_ASSET_RELATIVE_PATH.Substring(0, Constants.SETTINGS_ASSET_RELATIVE_PATH.LastIndexOf('/')));
                if (!Directory.Exists(resourcesFolder))
                {
                    Directory.CreateDirectory(resourcesFolder);
                }

                // Create the Settings scriptable object with default app ids
                settings = ScriptableObject.CreateInstance<Settings>();
                settings.SetAndroidID(PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Android));
                settings.SetiOSID(PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.iOS));

                AssetDatabase.CreateAsset(settings, Constants.ABSOLUTE_SETTINGS_PATH);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            settingsSerializedObject = new SerializedObject(settings);
            androidAppId = settingsSerializedObject.FindProperty("androidAppID").stringValue;
            iOSAppId = settingsSerializedObject.FindProperty("iOSAppID").stringValue;
        }

        void OnGUI()
        {
            Initialize();

            // Draw background color
            GUI.DrawTexture(new Rect(0, 0, maxSize.x, maxSize.y), HomaGamesStyle.BackgroundTexture, ScaleMode.StretchToFill);

            // ####################################
            // HEADER
            // ####################################

            // Draw Homa Games logo
            float homaGameLogoXPosition = position.width / 2 - HomaGamesStyle.HOMA_GAMES_LOGO_WITH / 2;
            GUI.DrawTexture(new Rect(homaGameLogoXPosition, 40,
                HomaGamesStyle.HOMA_GAMES_LOGO_WITH, HomaGamesStyle.HOMA_GAMES_LOGO_HEIGHT),
                HomaGamesStyle.LogoTexture, ScaleMode.ScaleToFit, true);
            GUILayout.Space(HomaGamesStyle.HOMA_GAMES_LOGO_HEIGHT + 80);

            // UI Box with application IDs
            GUIStyle contentBoxStyle = new GUIStyle();
            contentBoxStyle.padding = new RectOffset(20, 20, 0, 0);
            contentBoxStyle.normal.background = null;

            GUILayout.BeginVertical(contentBoxStyle);

            GUILayout.BeginHorizontal();
            GUILayout.Label("iOS bundle ID", HomaGamesStyle.MainLabelStyle);
            iOSAppId = GUILayout.TextField(iOSAppId, HomaGamesStyle.MainInputFieldStyle, GUILayout.ExpandWidth(true));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Android bundle ID", HomaGamesStyle.MainLabelStyle);
            androidAppId = GUILayout.TextField(androidAppId, HomaGamesStyle.MainInputFieldStyle, GUILayout.ExpandWidth(true));
            GUILayout.EndHorizontal();

            // `Debug` options
            GUILayout.BeginHorizontal();
            GUIStyle secondaryLeftAlignet = new GUIStyle(HomaGamesStyle.SecondaryLabelStyle);
            secondaryLeftAlignet.alignment = TextAnchor.MiddleLeft;
            GUILayout.Label("Debug Log enabled", secondaryLeftAlignet, GUILayout.Width(160), GUILayout.Height(20));
            bool debugEnabled = EditorGUILayout.Toggle(settings.IsDebugEnabled());
            HomaGamesLog.debugEnabled = settings.IsDebugEnabled();
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            GUILayout.EndVertical();

            settingsSerializedObject.FindProperty("iOSAppID").stringValue = iOSAppId;
            settingsSerializedObject.FindProperty("androidAppID").stringValue = androidAppId;
            settingsSerializedObject.FindProperty("debugEnabled").boolValue = debugEnabled;
            
            if (settingsSerializedObject.ApplyModifiedProperties())
            {
                EditorUtility.SetDirty(settings);
            }

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            // `Setup` button
            bool isRequesting = (requesting > 0);
            GUI.enabled = CanSetup() && !isRequesting;
            if (GUILayout.Button(isRequesting ? "Talking to server..." : "Setup", HomaGamesStyle.ButtonStyleTexts))
            {
                DVRTemplate = EditorUtils.DYNAMIC_VARIABLES_REGISTER_TEMPLATE;
                requesting = 2;

                // Fetch iOS and Android configuration variables
                FetchIOS(settings.GetIOSID());
                FetchAndroid(settings.GetAndroidID());
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            // ####################################
            // FOOTER
            // ####################################

            // Product name and version
            GUILayout.BeginArea(new Rect(0, position.height - 40, position.width, position.height));
            GUILayout.Label(string.Format("{0} v{1}", Constants.SDK_NAME, Constants.VERSION), HomaGamesStyle.SecondaryLabelStyle);
            GUILayout.EndArea();
        }

        /// <summary>
        /// Determines if the `Setup` button can be clicked, doing some
        /// input validations before.
        /// </summary>
        /// <returns>True if at least one App ID is well formed, false otherwise</returns>
        private bool CanSetup()
        {
            string iOSAppId = settings.GetIOSID();
            string androidAppId = settings.GetAndroidID();
            Match iosRegexMatch = Regex.Match(iOSAppId, VALID_APP_ID_REGEX);
            Match androidRegexMatch = Regex.Match(iOSAppId, VALID_APP_ID_REGEX);
            // TODO: Apply Regex match check one final formats completely defined
            bool iosValid = !string.IsNullOrWhiteSpace(iOSAppId);// && iosRegexMatch.Success;
            bool androidValid = !string.IsNullOrWhiteSpace(androidAppId);// && androidRegexMatch.Success;
            return iosValid || androidValid;
        }
        
        private void OnDestroy()
        {
            if (EditorUtility.IsDirty(settings))
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }

        #region iOS

        /// <summary>
        /// Obtains the iOS default configuration variables
        /// </summary>
        /// <param name="appID">The iOS App ID</param>
        private void FetchIOS(string appID)
        {
            if (string.IsNullOrEmpty(appID))
            {
                // iOS App ID not configured by the user: do nothing.
                HomaGamesLog.Warning(string.Format("[iOS] {0}", Constants.ErrorMessages[400]));
                OnRequestDone();
            }
            else
            {
                // Fetch from server the iOS configuration variables
                httpCaller.Get(string.Format(Constants.APP_BASE_URL, appID, "IPHONE"))
                    .ContinueWith((result) =>
                    {
                        if (!string.IsNullOrEmpty(result.Result))
                        {
                            // Fetch success. Deserialize and create DVR Template
                            ConfigurationResponse configurationResponse = ConfigurationResponse.FromServerResponse(result.Result);
                            if (EditorUtils.TryFormatConfigJSON(configurationResponse, out string dvrFileValues))
                            {
                                DVRTemplate = EditorUtils.IOS_DVR_FINDER.Replace(DVRTemplate, dvrFileValues);
                            }
                        }
                        else
                        {
                            HomaGamesLog.Error(string.Format("[N-Testing] Error fetching iOS dynamic variables"));
                        }

                        // Fetch is done
                        OnRequestDone();
                    }, TaskScheduler.FromCurrentSynchronizationContext());
            }
        }

        #endregion

        #region Android

        /// <summary>
        /// Obtains the Android default configuration variables
        /// </summary>
        /// <param name="appID">The Android App ID</param>
        private void FetchAndroid(string appID)
        {
            if (string.IsNullOrEmpty(appID))
            {
                // Android App ID not configured by the user: do nothing.
                HomaGamesLog.Warning(string.Format("[Android] {0}", Constants.ErrorMessages[400]));
                OnRequestDone();

            }
            else
            {
                // Fetch from server the iOS configuration variables
                httpCaller.Get(string.Format(Constants.APP_BASE_URL, appID, "ANDROID"))
                    .ContinueWith((result) =>
                    {
                        if (!string.IsNullOrEmpty(result.Result))
                        {
                            // Fetch success. Deserialize and create DVR Template
                            ConfigurationResponse configurationResponse = ConfigurationResponse.FromServerResponse(result.Result);
                            if (EditorUtils.TryFormatConfigJSON(configurationResponse, out string dvrFileValues))
                            {
                                DVRTemplate = EditorUtils.ANDROID_DVR_FINDER.Replace(DVRTemplate, dvrFileValues);
                            }
                        }
                        else
                        {
                            HomaGamesLog.Error(string.Format("[N-Testing] Error fetching Android dynamic variables"));
                        }

                        // Fetch is done
                        OnRequestDone();
                    }, TaskScheduler.FromCurrentSynchronizationContext());
            }
        }
        
        #endregion

        /// <summary>
        /// The request has finished (either with successfull result or not)
        /// </summary>
        private void OnRequestDone()
        {
            requesting--;

            if (requesting > 0)
            {
                return;
            }

            DeleteDvrFiles();
            CreateDvrDirectory();
            WriteDvrFile();
            HomaGamesLog.Debug("Homa Games SDK setup finished.");
        }

        #region DVR File helpers

        /// <summary>
        /// Writes the `DVRTemplate` to the `DVR.cs` file
        /// </summary>
        private void WriteDvrFile()
        {
            string dvrFilePath = string.Format("{0}/{1}", Application.dataPath, Constants.ABSOLUTE_DVR_PATH);
            HomaGamesLog.DebugFormat("Writing file: {0}", dvrFilePath);
            File.WriteAllText(dvrFilePath, DVRTemplate);
            DVRTemplate = null;

            AssetDatabase.Refresh();
        }

        /// <summary>
        /// Creates `Homa Games/Scripts` directory in case it does not exist
        /// </summary>
        private void CreateDvrDirectory()
        {
            string dvrDirectory = string.Format("{0}/{1}", Application.dataPath, Constants.HOMA_GAMES_SCRIPTS_DIR_PATH);
            if (!Directory.Exists(dvrDirectory))
            {
                Directory.CreateDirectory(dvrDirectory);
            }
        }

        /// <summary>
        /// Delete any DVR.cs file
        /// </summary>
        private void DeleteDvrFiles()
        {
            string[] dvrFiles = AssetDatabase.FindAssets("DVR");
            
            if (dvrFiles != null)
            {
                try
                {
                    for (int i = 0; i < dvrFiles.Length; i++)
                    {
                        string assetPath = AssetDatabase.GUIDToAssetPath(dvrFiles[i]);
                        HomaGamesLog.DebugFormat("Deleting DVR file: {0}", assetPath);
                        AssetDatabase.DeleteAsset(assetPath);
                    }
                }
                catch (Exception e)
                {
                    HomaGamesLog.WarningFormat("Exception deleting DVR files: ", e.Message);
                }
            }
        }

        #endregion
    }
}
