using System;
using System.Reflection;
using System.Threading.Tasks;
using HomaGames.HomaBelly;
using UnityEditor;
using UnityEngine;

public class HomaBellyInstallWindow : HomaBellyBaseWindow
{
    private string appToken;
    private string errorMessage;
    private bool requestingManifest = false;
    private PluginManifest manifest;

    public HomaBellyInstallWindow() : base()
    {
        // Initialize app token
        manifest = PluginManifest.LoadFromLocalFile();
        appToken = manifest != null ? manifest.AppToken : "";
    }

    protected override void Draw(Rect windowPosition)
    {
        Vector2 originalIconSize = EditorGUIUtility.GetIconSize();
        EditorGUIUtility.SetIconSize(new Vector2(32, 32));

        GUILayout.BeginArea(new Rect(0, HomaGamesStyle.HOMA_GAMES_LOGO_HEIGHT + 80, windowPosition.width, 500));

        GUILayout.Label("Enter your app token:", HomaGamesStyle.MainLabelStyle);

        // Input field
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        appToken = GUILayout.TextField(appToken, HomaGamesStyle.MainInputFieldStyle);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        // `Setup` button
        GUIContent buttonContent = new GUIContent();
        buttonContent.image = HomaGamesStyle.DownloadIcon;
        buttonContent.text = manifest == null ? "Install Packages" : (HomaBellyUpdateChecker.UpdateAvailable ? "Update Packages" : "Refresh Packages");

        if (requestingManifest)
        {
            GUILayout.Label("Fetching...", HomaGamesStyle.SecondaryLabelStyle);
        }
        else
        {
            if (GUILayout.Button(buttonContent, HomaGamesStyle.ButtonStyleTexts))
            {
                if (IsValidAppToken(appToken))
                {
                    requestingManifest = true;

                    // Clear previous log traces
                    HomaBellyEditorLog.ResetLog();
                    errorMessage = "";

                    homaBellyWindowController.RequestPluginManifest(appToken).ContinueWith((taskResult) =>
                    {
                        if (taskResult.Result != null)
                        {
                            homaBellyWindowController.ShowWindow(ID.INSTALLATION_PROGRESS);
                            homaBellyWindowController.InstallPluginManifest(taskResult.Result);
                        }
                        else
                        {
                            // Show error
                            errorMessage = $"Could not fetch manifest for app token '{appToken}'\nEnsure your app token is valid";
                        }

                        requestingManifest = false;
                    }, TaskScheduler.FromCurrentSynchronizationContext());
                }
                else
                {
                    errorMessage = $"Invalid app token";
                }
            }
        }

        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        // Status message (error)
        if (!string.IsNullOrEmpty(errorMessage))
        {
            GUILayout.Label(errorMessage, HomaGamesStyle.ErrorLabelStyle);
        }        

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        GUILayout.EndArea();

        EditorGUIUtility.SetIconSize(originalIconSize);
    }

    private bool IsValidAppToken(string appToken)
    {
        return !string.IsNullOrEmpty(appToken)
            && !appToken.Contains(" ");
    }

    protected override void OnVisibleFocus()
    {
        // NO-OP
    }
}
