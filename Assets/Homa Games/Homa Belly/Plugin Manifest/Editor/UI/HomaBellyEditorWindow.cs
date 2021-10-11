using System;
using System.Threading.Tasks;
using HomaGames.HomaBelly.Utilities;
using UnityEditor;
using UnityEngine;

namespace HomaGames.HomaBelly
{
    [Serializable]
    internal sealed class HomaBellyEditorWindow : EditorWindow, IHomaBellyWindowController
    {
        #region Private properties
        private HomaBellyBaseWindow installWindow;
        private HomaBellyBaseWindow installationProgressWindow;
        private PluginController pluginController;
        [SerializeField]
        private HomaBellyBaseWindow.ID currentWindowId = HomaBellyBaseWindow.ID.INSTALL;
        #endregion

        [MenuItem("Window/Homa Games/Homa Belly/Setup", false, 1)]
        internal static void CreateSettingsAndFocus()
        {
            GetWindow(typeof(HomaBellyEditorWindow), false, "Homa Belly", true);
        }

        [MenuItem("Window/Homa Games/Homa Belly/Full Implementation Guide", false, 15)]
        internal static void OpenHomaBellyDocumentation()
        {
            Application.OpenURL("https://www.notion.so/homagames/Full-implementation-guide-13eb58192648433bb78707be3537e521");
        }

        [MenuItem("Window/Homa Games/Homa Belly/Troubleshooting", false, 15)]
        internal static void OpenHomaBellyTroubleshooting()
        {
            Application.OpenURL("https://www.notion.so/homagames/Troubleshooting-Known-Issues-98747034856b41c79ae3caed102dd12f");
        }

        private void Awake()
        {
            // When opening new window, if there is a manifest stored,
            // show installation result window
            PluginManifest oldPluginManifest = PluginManifest.LoadFromLocalFile();
            if (oldPluginManifest != null)
            {
                currentWindowId = HomaBellyBaseWindow.ID.INSTALLATION_PROGRESS;
            }
            else
            {
                currentWindowId = HomaBellyBaseWindow.ID.INSTALL;
            }
        }

        void OnEnable()
        {
            AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
            AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReload;
        }

        void OnDisable()
        {
            AssemblyReloadEvents.beforeAssemblyReload -= OnBeforeAssemblyReload;
            AssemblyReloadEvents.afterAssemblyReload -= OnAfterAssemblyReload;
        }

        private void Update()
        {
            if (IsInstalling())
            {
                Repaint();
            }
        }

        private void OnFocus()
        {
            if (installWindow != null)
            {
                installWindow.OnFocus();
            }

            if (installationProgressWindow != null)
            {
                installationProgressWindow.OnFocus();
            }
        }

        public void OnBeforeAssemblyReload()
        {
            HomaBellyEditorLog.BeforeAssemblyReload();
        }

        public void OnAfterAssemblyReload()
        {
            HomaBellyEditorLog.AfterAssemblyReload();
        }

        private void Initialize()
        {
            if (installWindow == null)
            {
                installWindow = new HomaBellyInstallWindow();
                installWindow.SetWindowController(this);
            }

            if (installationProgressWindow == null)
            {
                installationProgressWindow = new HomaBellyInstallationProgressWindow();
                installationProgressWindow.SetWindowController(this);
            }

            if (pluginController == null)
            {
                pluginController = new PluginController();
            }

            // Show current window
            ShowWindow(currentWindowId);
        }

        void OnGUI()
        {
            // Global Defaults
            //GUISkin originalSkin = GUI.skin;
            // TODO: Customize scrollbars and uncomment below line
            //GUI.skin = HomaGamesStyle.skin;

            EditorGUIUtility.SetIconSize(new Vector2(32, 32));

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


            // ####################################
            // WINDOWS
            // ####################################
            installWindow.OnGUI(position);
            installationProgressWindow.OnGUI(position);

            // ####################################
            // FOOTER
            // ####################################

            // Product name and version
            GUILayout.BeginArea(new Rect(0, position.height - 40, position.width, position.height));
            GUILayout.Label($"{HomaBellyConstants.PRODUCT_NAME} v{HomaBellyConstants.PRODUCT_VERSION}", HomaGamesStyle.SecondaryLabelStyle);
            GUILayout.EndArea();

            //GUI.skin = originalSkin;
        }

        #region IHomaBellyWindowController

        public async Task<PluginManifest> RequestPluginManifest(string appToken)
        {
            if (pluginController != null)
            {
                return await pluginController.RequestPluginManifest(appToken);
            }

            return null;
        }

        public void InstallPluginManifest(PluginManifest newPluginManifest)
        {
            if (newPluginManifest != null && pluginController != null)
            {
                pluginController.InstallPluginManifest(newPluginManifest);
            }
        }

        public float GetInstallationProgress()
        {
            float progress = 0f;
            if (pluginController != null)
            {
                progress = pluginController.GetInstallationProgress();
            }

            return progress;
        }

        public void ShowWindow(HomaBellyBaseWindow.ID id)
        {
            currentWindowId = id;
            installWindow.SetVisible(id == HomaBellyBaseWindow.ID.INSTALL);
            installationProgressWindow.SetVisible(id == HomaBellyBaseWindow.ID.INSTALLATION_PROGRESS);
        }

        public bool IsInstalling()
        {
            if (pluginController != null)
            {
                return pluginController.IsInstalling();
            }

            return false;
        }

        #endregion
    }
}