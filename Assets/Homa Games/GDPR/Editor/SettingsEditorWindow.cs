/*
 * Licensed to the Apache Software Foundation (ASF) under one
 * or more contributor license agreements.  See the NOTICE file
 * distributed with this work for additional information
 * regarding copyright ownership.  The ASF licenses this file
 * to you under the Apache License, Version 2.0 (the
 * "License"); you may not use this file except in compliance
 * with the License.  You may obtain a copy of the License at
 * 
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing,
 * software distributed under the License is distributed on an
 * "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
 * KIND, either express or implied.  See the License for the
 * specific language governing permissions and limitations
 * under the License.
 */

using System;
using UnityEditor;
using UnityEngine;

namespace HomaGames.GDPR
{
    [CustomEditor(typeof(Settings))]
    public class SettingsEditorWindow : Editor
    {
        #region Private properties
        private Material baseBackgroundMaterial;
        private Material fontMaterial;
        private Material textMeshProFontMaterial;
        private Material buttonFontMaterial;
        #endregion

        #region Private serialized properties
        private SerializedProperty gameName;
        private SerializedProperty backgroundColor;
        private SerializedProperty fontColor;
        private SerializedProperty secondaryFontColor;
        private SerializedProperty toggleColor;
        private SerializedProperty buttonFontColor;
        private SerializedProperty useDeviceLanguageWhenPossible;
        private SerializedProperty language;
        private SerializedProperty iOSIdfaPopupMessage;
        private SerializedProperty forceDisableGdpr;
        #endregion

        [InitializeOnLoadMethod]
        static void CreateGDPRSettings()
        {
            bool created = false;
            ScriptableObject asset = CreateOrLoadGDPRSettings(out created);
            if (created)
            {
                ((Settings)asset).ResetToDefaultValues();
                AssetDatabase.SaveAssets();
            }
        }

        [MenuItem("Window/Homa Games/GDPR Settings")]
        static void OpenSettings()
        {
            ScriptableObject asset = CreateOrLoadGDPRSettings(out bool created);

            // Open prefab preview
            /*
            GameObject prefabAsset = (GameObject)AssetDatabase.LoadAssetAtPath(GDPR_PREFAB_ASSET_PATH, typeof(GameObject));
            if (prefabAsset != null)
            {
                AssetDatabase.OpenAsset(prefabAsset);
            }*/

            // Select Settings asset
            Selection.activeObject = asset;
        }

        private static ScriptableObject CreateOrLoadGDPRSettings(out bool created)
        {
            ScriptableObject asset = (ScriptableObject)AssetDatabase.LoadAssetAtPath(Constants.GDPR_SETTINGS_ASSET_PATH, typeof(ScriptableObject));
            created = false;
            if (asset == null)
            {
                // If asset is not found, create it
                asset = ScriptableObject.CreateInstance<Settings>();
                ((Settings)asset).ResetToDefaultValues();
                AssetDatabase.CreateAsset(asset, Constants.GDPR_SETTINGS_ASSET_PATH);
                AssetDatabase.SaveAssets();
                created = true;
            }

            return asset;
        }

        private void OnEnable()
        {
            LoadAssets();
        }

        public override void OnInspectorGUI()
        {
            // Gather properties
            FindProperties();

            // Settings object
            Settings settings = GetTargetAsSettingsObject();

            // Change properties
            EditorGUILayout.LabelField("Configuration", EditorStyles.boldLabel);
            DrawHorizontalLine(Color.gray);
            DisplayProperty(forceDisableGdpr, "Force Disable GDPR", "Set this to true if you never want to show GDPR Screen. This applies, for example, to Chinese builds");
#if UNITY_2019_3_OR_NEWER
            EditorGUILayout.Space(10);
#else
            EditorGUILayout.Space();
#endif

            EditorGUILayout.LabelField("Literals", EditorStyles.boldLabel);
            DrawHorizontalLine(Color.gray);
            bool originalWordWrap = GUI.skin.textField.wordWrap;
            GUI.skin.textField.wordWrap = true;
            DisplayProperty(gameName, "Game Name", "The game name to be visible in the GDPR welcome page");
            DisplayProperty(iOSIdfaPopupMessage, "[iOS only] Privacy Popup Message", "Customizable popup message to be displayed on the native popup", GUILayout.Height(40));
            GUI.skin.textField.wordWrap = originalWordWrap;
#if UNITY_2019_3_OR_NEWER
            EditorGUILayout.Space(10);
#else
            EditorGUILayout.Space();
#endif

            EditorGUILayout.LabelField("Colors", EditorStyles.boldLabel);
            DrawHorizontalLine(Color.gray);
            DisplayProperty(backgroundColor, "Background Color", "Base background color for the GDPR windows");
            DisplayProperty(fontColor, "Font Color", "Base font color for the GDPR text");
            DisplayProperty(secondaryFontColor,"Secondary Font Color","Secondary font color for the GDPR text");
            DisplayProperty(toggleColor, "Toggle Color", "Toggle color when it is ON");
            DisplayProperty(buttonFontColor, "Button Font Color", "Font color for the GDPR buttons");
#if UNITY_2019_3_OR_NEWER
            EditorGUILayout.Space(10);
#else
            EditorGUILayout.Space();
#endif

            // Only enable localization settings if is not configured to fetch values from NTesting
            if (!Localization.FETCH_LITERALS_FROM_NTESTING)
            {
                EditorGUILayout.LabelField("Localization", EditorStyles.boldLabel);
                DrawHorizontalLine(Color.gray);
                DisplayProperty(useDeviceLanguageWhenPossible, "Use Device Language", "Flag informing if GDPR should be displayed in device language in runtime or not");

                // If user decided not to use device language
                if (!settings.UseDeviceLanguageWhenPossible)
                {
                    DisplayProperty(language, "Language", "If `Use Device Language` is set to false, GDPR will be always shown in this specific language");

                    // Detect if language has changed. If so, load new one
                    if ((int)settings.Language != language.enumValueIndex)
                    {
                        Localization.CurrentLanguage = (Settings.SupportedLanguages)language.enumValueIndex;
                    }
                }
            }

#if UNITY_2019_3_OR_NEWER
            EditorGUILayout.Space(40);
#else
            EditorGUILayout.Space();
#endif

            // Apply properties to material
            ApplySettingsToMaterials();

            // Reset to default
            if (GUILayout.Button("Reset to Defaults"))
            {
                settings.ResetToDefaultValues();
                serializedObject.Update();
                AssetDatabase.ForceReserializeAssets(new string[] { Constants.GDPR_SETTINGS_ASSET_PATH });
            }

            // Apply them
            serializedObject.ApplyModifiedProperties();
        }

        #region Editor callbacks

        #endregion

        #region Private helpers

        private void FindProperties()
        {
            gameName = serializedObject.FindProperty("GameName");
            backgroundColor = serializedObject.FindProperty("BackgroundColor");
            fontColor = serializedObject.FindProperty("FontColor");
            secondaryFontColor = serializedObject.FindProperty("SecondaryFontColor");
            toggleColor = serializedObject.FindProperty("ToggleColor");
            buttonFontColor = serializedObject.FindProperty("ButtonFontColor");
            useDeviceLanguageWhenPossible = serializedObject.FindProperty("UseDeviceLanguageWhenPossible");
            language = serializedObject.FindProperty("Language");
            iOSIdfaPopupMessage = serializedObject.FindProperty("iOSIdfaPopupMessage");
            forceDisableGdpr = serializedObject.FindProperty("ForceDisableGdpr");
        }

        private void DisplayProperty(SerializedProperty property, string label, string tooltip = "", params GUILayoutOption[] layouts)
        {
            if (property != null)
            {
                EditorGUILayout.PropertyField(property, new GUIContent(label, tooltip), layouts);
#if UNITY_2019_3_OR_NEWER
            EditorGUILayout.Space(5);
#else
            EditorGUILayout.Space();
#endif
            }
        }

        private void LoadAssets()
        {
            if (baseBackgroundMaterial == null)
            {
                baseBackgroundMaterial = (Material)AssetDatabase.LoadAssetAtPath("Assets/Homa Games/GDPR/Runtime/Materials/GDPR_BaseBackground.mat", typeof(Material));
            }

            if (fontMaterial == null)
            {
                fontMaterial = (Material)AssetDatabase.LoadAssetAtPath("Assets/Homa Games/GDPR/Runtime/Materials/GDPR_Font.mat", typeof(Material));
            }

            if (buttonFontMaterial == null)
            {
                buttonFontMaterial = (Material)AssetDatabase.LoadAssetAtPath("Assets/Homa Games/GDPR/Runtime/Materials/GDPR_ButtonFont.mat", typeof(Material));
            }

            if (textMeshProFontMaterial == null)
            {
                textMeshProFontMaterial = (Material)AssetDatabase.LoadAssetAtPath("Assets/Homa Games/GDPR/Runtime/Fonts/Poppins/Poppins-Regular SDF.mat", typeof(Material));
            }
        }

        private void ApplySettingsToMaterials()
        {
            Settings settings = GetTargetAsSettingsObject();
            if (settings != null)
            {
                baseBackgroundMaterial.color = settings.BackgroundColor;
                fontMaterial.color = settings.FontColor;
                textMeshProFontMaterial.SetColor("_FaceColor", settings.FontColor);
                buttonFontMaterial.color = settings.ButtonFontColor;
            }
        }

        private Settings GetTargetAsSettingsObject()
        {
            try
            {
                Settings settings = serializedObject.targetObject as Settings;
                return settings;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Homa Games GDPR] Could not cast serialized object to Settings: {e.Message}");
            }

            return default;
        }

        private void DrawHorizontalLine(Color color, int thickness = 2, int padding = 10)
        {
            Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(padding + thickness));
            r.height = thickness;
            r.y += padding / 2;
            r.x -= 2;
            r.width += 6;
            EditorGUI.DrawRect(r, color);
        }

        #endregion
    }
}
