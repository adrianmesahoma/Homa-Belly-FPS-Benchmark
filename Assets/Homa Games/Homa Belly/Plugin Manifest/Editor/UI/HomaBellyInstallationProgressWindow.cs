using System.Threading.Tasks;
using HomaGames.HomaBelly;
using UnityEditor;
using UnityEngine;

public class HomaBellyInstallationProgressWindow : HomaBellyBaseWindow
{
    private const float PROGRESS_BAR_START_Y = 30f;

    private bool showingLogs = false;

    private HomaBellyBaseWindow logWindow;
    private HomaBellyBaseWindow packagesWindow;
    private string statusLabel;

    public HomaBellyInstallationProgressWindow() : base()
    {
        if (logWindow == null)
        {
            logWindow = new HomaBellyLogWindow();
            logWindow.SetVisible(false);
        }

        if (packagesWindow == null)
        {
            packagesWindow = new HomaBellyPackagesWindow();
            packagesWindow.SetVisible(true);
        }
    }

    public override void SetWindowController(IHomaBellyWindowController controller)
    {
        base.SetWindowController(controller);
        logWindow.SetWindowController(controller);
        packagesWindow.SetWindowController(controller);
    }

    protected override void Draw(Rect windowPosition)
    {
        GUILayout.BeginArea(new Rect(0, HomaGamesStyle.HOMA_GAMES_LOGO_HEIGHT + 60, windowPosition.width, windowPosition.height));

        // ####################################
        // HEADER
        // ####################################
        GUILayout.BeginHorizontal(new GUIStyle()
        {
            alignment = TextAnchor.MiddleCenter
        });
        GUILayout.FlexibleSpace();
        GUILayout.Label(statusLabel, HomaGamesStyle.SecondaryLabelStyle, GUILayout.MaxWidth(180));
        showingLogs = GUILayout.Toggle(showingLogs, showingLogs ? "(Hide logs)" : "(Show logs)", HomaGamesStyle.ButtonOnlyTextStyle);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.Space(20);

        if (!homaBellyWindowController.IsInstalling())
        {
            ShowFinishedStatus();
        }
        else
        {
            // Progress bar
            statusLabel = "Installing packages...";
            ShowInstallationProgressBar(windowPosition);
        }

        GUILayout.Space(20);

        // ####################################
        // WINDOWS
        // ####################################
        logWindow.SetVisible(showingLogs);
        logWindow.OnGUI(windowPosition);

        packagesWindow.SetVisible(!showingLogs);
        packagesWindow.OnGUI(windowPosition);

        GUILayout.EndArea();
    }

    protected override void OnVisibleFocus()
    {
        // NO-OP
    }

    private void ShowFinishedStatus()
    {
        string buttonLabel;

        // Installation completed
        if (HomaBellyUpdateChecker.UpdateAvailable)
        {
            // If has a new version
            statusLabel = "New version available";
            buttonLabel = "Update to latest version";
        }
        else
        {
            statusLabel = "Installation finished";
            buttonLabel = "Refresh packages";
        }

        GUILayout.BeginHorizontal(new GUIStyle()
        {
            alignment = TextAnchor.MiddleCenter
        });
        GUILayout.FlexibleSpace();
        if (GUILayout.Button(buttonLabel, HomaGamesStyle.ButtonOnlyTextCyanStyle))
        {
            homaBellyWindowController.ShowWindow(ID.INSTALL);
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
    }

    private void ShowInstallationProgressBar(Rect windowPosition)
    {
        float progressBarXStart = (windowPosition.width / 2)
            - (HomaGamesStyle.ProgressBarBackgroundStyle.fixedWidth / 2)
            - (HomaGamesStyle.ProgressBarBackgroundStyle.padding.left * 2);
        float progressBarWidth = HomaGamesStyle.ProgressBarBackgroundStyle.fixedWidth + HomaGamesStyle.ProgressBarBackgroundStyle.padding.left * 2;
        float progressBarHeight = HomaGamesStyle.ProgressBarBackgroundStyle.fixedHeight;
        GUILayout.BeginArea(new Rect(progressBarXStart, PROGRESS_BAR_START_Y, progressBarWidth, progressBarHeight),
            HomaGamesStyle.ProgressBarBackgroundStyle);
        GUILayout.Box("", HomaGamesStyle.ProgressBarForegroundStyle,
            GUILayout.Width(HomaGamesStyle.ProgressBarBackgroundStyle.fixedWidth * homaBellyWindowController.GetInstallationProgress()));
        GUILayout.EndArea();
    }
}
