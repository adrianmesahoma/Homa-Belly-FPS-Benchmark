using System;
using System.Collections.Generic;
using UnityEngine;

namespace HomaGames.HomaBelly
{
    /// <summary>
    /// Use this class to invoke default Analytic Events for your game. You will
    /// need to invoke the following methods accordingly:
    ///
    /// - LevelStarted(levelId)
    /// - LevelFailed()
    /// - LevelCompleted()
    ///
    /// - TutorialStepStearted(stepId)
    /// - TutorialStepFailed()
    /// - TutorialStepCompleted()
    ///
    /// - SuggestedRewardedAd(rewardedAdName)
    /// - SuggestedRewardedAdTriggered()
    /// - SetNextRewardedAdName(rewardedAdName)
    /// </summary>
    public static class DefaultAnalytics
    {
        private const string LEVEL_STARTED_KEY = "com.homagames.homabelly.level_started_{0}_at_gameplay_seconds";
        private const string LEVEL_COMPLETED_KEY = "com.homagames.homabelly.level_completed_{0}_at_gameplay_seconds";
        private const string CURRENT_LEVEL_KEY = "com.homagames.homabelly.current_level";

        private const string TUTORIAL_STEP_STARTED_KEY = "com.homagames.homabelly.tutorial_step_started_{0}_at_gameplay_seconds";
        private const string TUTORIAL_STEP_COMPLETED_KEY = "com.homagames.homabelly.tutorial_step_completed_{0}_at_gameplay_seconds";
        private const string CURRENT_TUTORIAL_STEP_KEY = "com.homagames.homabelly.current_tutorial_step";

        private const string REWARDED_AD_FIRST_TIME_TAKEN_EVER_KEY = "com.homagames.homabelly.rewarded_ad_first_time_taken_ever_{0}";
        private const string INTERSTITIAL_AD_FIRST_TIME_KEY = "com.homagames.homabelly.interstitial_ad_first_time";

        private const string CURRENT_GAMEPLAY_TIME_KEY = "com.homagames.homavelly.current_gameplay_time_in_seconds";
        private const string MAIN_MENU_LOADED_KEY = "com.homagames.homavelly.main_menu_loaded";
        private const string GAMEPLAY_STARTED_KEY = "com.homagames.homavelly.gameplay_started";

        private static string currentLevelId = "0";
        private static string currentTutorialStep = "0";
        private static string currentGameplayTime;
        private static long gameplayResumedTimestamp;
        private static string currentRewardedAdName = "Default";
        private static Dictionary<string, bool> rewardedAdsTakenThisSession = new Dictionary<string, bool>();

        static DefaultAnalytics()
        {
            Start();
        }

        public static void Start()
        {
            // Recover any previous level or tutorial step stored
            currentLevelId = PlayerPrefs.GetString(CURRENT_LEVEL_KEY, "0");
            currentTutorialStep = PlayerPrefs.GetString(CURRENT_TUTORIAL_STEP_KEY, "0");

            Events.onInitialized += OnHomaBellyInitialized;
        }

        private static void OnHomaBellyInitialized()
        {
            RegisterAdEvents();
        }

        public static void OnApplicationPause(bool pause)
        {
            // Game is paused
            if (pause)
            {
                // Save new current gameplay time
                PlayerPrefs.SetString(CURRENT_GAMEPLAY_TIME_KEY, GetTotalGameplaySecondsAtThisExactMoment().ToString());
                PlayerPrefs.Save();
            }
            else
            {
                // Game is resumed
                currentGameplayTime = PlayerPrefs.GetString(CURRENT_GAMEPLAY_TIME_KEY, "");
                gameplayResumedTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            }
        }

        /// <summary>
        /// Obtains the current gameplay elapsed time in seconds at the
        /// moment of invoking this method.
        ///
        /// This takes into account only the time spent playing, not the time
        /// when the application is paused (the user minimized/exitted the game)
        /// </summary>
        /// <returns></returns>
        private static long GetTotalGameplaySecondsAtThisExactMoment()
        {
            // Calculate time since last resume
            long currentTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            long gameplayTimeSinceApplicationResumed = currentTimestamp - gameplayResumedTimestamp;

            // Parse and add calculated time to current gameplay time
            long currentGameplayTimeLong = string.IsNullOrEmpty(currentGameplayTime) ? 0 : long.Parse(currentGameplayTime);
            currentGameplayTimeLong += gameplayTimeSinceApplicationResumed;

            return currentGameplayTimeLong;
        }

        #region Level Tracking

        /// <summary>
        /// Every time players start the level
        /// </summary>
        /// <param name="levelId"></param>
        public static void LevelStarted(int levelId)
        {
            LevelStarted(levelId.ToString());
        }

        /// <summary>
        /// Every time players start the level
        /// </summary>
        /// <param name="levelId"></param>
        public static void LevelStarted(string levelId)
        {
            // Set current level
            currentLevelId = levelId;
            PlayerPrefs.SetString(CURRENT_LEVEL_KEY, currentLevelId);
            PlayerPrefs.Save();

            HomaBelly.Instance.TrackProgressionEvent(ProgressionStatus.Start, "Level " + currentLevelId);

            // If is the very first time this level is started, track it
            if (string.IsNullOrEmpty(PlayerPrefs.GetString(string.Format(LEVEL_STARTED_KEY, currentLevelId), "")))
            {
                // GameplayTime is the time spent in the game since the first launch, in seconds.
                long totalGameplaySecondsAtThisMoment = GetTotalGameplaySecondsAtThisExactMoment();
                HomaBelly.Instance.TrackDesignEvent("Levels:Reached:" + Sanitize(currentLevelId), totalGameplaySecondsAtThisMoment);
                PlayerPrefs.SetString(string.Format(LEVEL_STARTED_KEY, currentLevelId), totalGameplaySecondsAtThisMoment.ToString());
                PlayerPrefs.Save();
            }
        }

        /// <summary>
        /// Every time players fail the level
        /// </summary>
        public static void LevelFailed()
        {
            HomaBelly.Instance.TrackProgressionEvent(ProgressionStatus.Fail, "Level " + currentLevelId);
        }

        /// <summary>
        /// Every time players complete the level
        /// </summary>
        public static void LevelCompleted()
        {
            HomaBelly.Instance.TrackProgressionEvent(ProgressionStatus.Complete, "Level " + currentLevelId);

            // If is the very first time this level is started, track it
            if (string.IsNullOrEmpty(PlayerPrefs.GetString(string.Format(LEVEL_COMPLETED_KEY, currentLevelId), "")))
            {
                // LevelDuration is the time spent in the level from the start until the completion, in seconds.
                long levelStartAtGameplaySeconds = long.Parse(PlayerPrefs.GetString(LEVEL_STARTED_KEY, "0"));
                long levelDuration = GetTotalGameplaySecondsAtThisExactMoment() - levelStartAtGameplaySeconds;
                HomaBelly.Instance.TrackDesignEvent("Levels:Duration:" + Sanitize(currentLevelId), levelDuration);
                PlayerPrefs.SetString(string.Format(LEVEL_COMPLETED_KEY, currentLevelId), GetTotalGameplaySecondsAtThisExactMoment().ToString());
                PlayerPrefs.Save();
            }
        }

        #endregion

        #region Tutorial Steps Tracking                                                                                                                                                                                                                                  

        /// <summary>
        /// Invoke this method everytime a tutorial step is started. Invoking
        /// it twice for the same step is harmless, as only the very first
        /// one will be taken into account.
        /// </summary>
        /// <param name="step">The tutorial step</param>
        public static void TutorialStepStarted(int step)
        {
            TutorialStepStarted(step.ToString());
        }

        /// <summary>
        /// Invoke this method everytime a tutorial step is started. Invoking
        /// it twice for the same step is harmless, as only the very first
        /// one will be taken into account.
        /// </summary>
        /// <param name="step">The tutorial step</param>
        public static void TutorialStepStarted(string step)
        {
            // Set current level
            currentTutorialStep = step;
            PlayerPrefs.SetString(CURRENT_TUTORIAL_STEP_KEY, currentTutorialStep);
            PlayerPrefs.Save();

            // If is the very first time this tutorial step is started, track it
            if (string.IsNullOrEmpty(PlayerPrefs.GetString(string.Format(TUTORIAL_STEP_STARTED_KEY, currentTutorialStep), "")))
            {
                // GameplayTime is the time spent in the game since the first launch, in seconds.
                long totalGameplaySecondsAtThisMoment = GetTotalGameplaySecondsAtThisExactMoment();
                HomaBelly.Instance.TrackDesignEvent("Tutorial:" + Sanitize(currentTutorialStep) + ":Started", totalGameplaySecondsAtThisMoment);
                PlayerPrefs.SetString(string.Format(TUTORIAL_STEP_STARTED_KEY, currentTutorialStep), totalGameplaySecondsAtThisMoment.ToString());
                PlayerPrefs.Save();
            }
        }

        /// <summary>
        /// When the player does not execute the asked behavior in the current tutorial step
        /// </summary>
        public static void TutorialStepFailed()
        {
            HomaBelly.Instance.TrackDesignEvent("Tutorial:" + Sanitize(currentTutorialStep) + ":Failed");
        }

        /// <summary>
        /// Invoke this method everytime a tutorial step is completed. Invoking
        /// it twice for the same step is harmless, as only the very first
        /// one will be taken into account.
        /// </summary>
        public static void TutorialStepCompleted()
        {
            // If is the very first time this tutorial step is completed, track it
            if (string.IsNullOrEmpty(PlayerPrefs.GetString(string.Format(TUTORIAL_STEP_COMPLETED_KEY, currentTutorialStep), "")))
            {
                // StepDuration is the time spent to complete the step, in seconds.
                long tutorialStepStartAtGameplaySeconds = long.Parse(PlayerPrefs.GetString(TUTORIAL_STEP_STARTED_KEY, "0"));
                long tutorialStepDuration = GetTotalGameplaySecondsAtThisExactMoment() - tutorialStepStartAtGameplaySeconds;
                HomaBelly.Instance.TrackDesignEvent("Tutorial:" + Sanitize(currentTutorialStep) + ":Completed", tutorialStepDuration);
                PlayerPrefs.SetString(string.Format(TUTORIAL_STEP_COMPLETED_KEY, currentTutorialStep), GetTotalGameplaySecondsAtThisExactMoment().ToString());
                PlayerPrefs.Save();
            }   
        }

        #endregion

        #region Ads

        /// <summary>
        /// Invoke this method whenever a rewarded offer is suggested to the player.
        /// </summary>
        /// <param name="rewardedAdName">Please follow the nomenclature in the relevant document</param>
        public static void SuggestedRewardedAd(string rewardedAdName = "")
        {
            SetNextRewardedAdName(rewardedAdName);
            HomaBelly.Instance.TrackDesignEvent("Rewarded:" + Sanitize(rewardedAdName) + ":Suggested:" + Sanitize(currentLevelId));
        }

        /// <summary>
        /// Invoke this method whenever the player is clicking on the button to trigger the suggested Rewarded Video Offer
        /// </summary>
        public static void SuggestedRewardedAdTriggered()
        {
            HomaBelly.Instance.TrackDesignEvent("Rewarded:" + Sanitize(currentRewardedAdName) + ":Triggered:" + Sanitize(currentLevelId));
        }

        /// <summary>
        /// Set the next Rewarded Video Ad Name to be tacked. This Rewarded Video Ad Name
        /// will be used in analytics to identify the Rewarded Video Ad events
        /// until a new rewarded ad name is set (withi this method) or suggested (with #SuggestedRewardedAd)
        /// </summary>
        /// <param name="rewardedAdName">Please follow the nomenclature in the relevant document</param>
        public static void SetNextRewardedAdName(string rewardedAdName = "")
        {
            currentRewardedAdName = rewardedAdName;
        }

        /// <summary>
        /// Registers Ad Events to be tracked
        /// </summary>
        private static void RegisterAdEvents()
        {
            // Banner
            Events.onBannerAdClickedEvent += (id) =>
            {
                HomaBelly.Instance.TrackDesignEvent("Banners:Clicked");
            };

            /*Events.onBannerAdLoadedEvent += (id) =>
            {
                HomaBelly.Instance.TrackDesignEvent("Banners:Loaded");
            };*/

            Events.onBannerAdLoadFailedEvent += (id) =>
            {
                HomaBelly.Instance.TrackDesignEvent("Banners:LoadFailed");
            };

            // Interstitial
            Events.onInterstitialAdReadyEvent += () =>
            {
                HomaBelly.Instance.TrackDesignEvent("Interstitials:Ready:" + Sanitize(currentLevelId));
            };

            Events.onInterstitialAdLoadFailedEvent += () =>
            {
                HomaBelly.Instance.TrackDesignEvent("Interstitials:LoadFailed:" + Sanitize(currentLevelId));
            };

            Events.onInterstitialAdClosedEvent += () =>
            {
                HomaBelly.Instance.TrackDesignEvent("Interstitials:Closed:" + Sanitize(currentLevelId));
            };

            Events.onInterstitialAdClickedEvent += (id) =>
            {
                HomaBelly.Instance.TrackDesignEvent("Interstitials:Clicked:" + Sanitize(currentLevelId));
            };

            Events.onInterstitialAdShowFailedEvent += (id) =>
            {
                HomaBelly.Instance.TrackDesignEvent("Interstitials:ShowFailed:" + Sanitize(currentLevelId));
            };

            Events.onInterstitialAdShowSucceededEvent += (id) =>
            {
                HomaBelly.Instance.TrackDesignEvent("Interstitials:ShowSucceeded:" + Sanitize(currentLevelId));

                if (PlayerPrefs.GetInt(INTERSTITIAL_AD_FIRST_TIME_KEY, 0) == 0)
                {
                    HomaBelly.Instance.TrackDesignEvent("Interstitials:FirstWatched:" + Sanitize(currentLevelId), GetTotalGameplaySecondsAtThisExactMoment());
                    PlayerPrefs.SetInt(INTERSTITIAL_AD_FIRST_TIME_KEY, 1);
                    PlayerPrefs.Save();
                }
            };

            // Rewarded Video
            Events.onRewardedVideoAdRewardedEvent += (reward) =>
            {
                if (!string.IsNullOrEmpty(currentRewardedAdName))
                {
                    // Current rewarded ad has not already been taken this session
                    if (!rewardedAdsTakenThisSession.ContainsKey(currentRewardedAdName))
                    {
                        rewardedAdsTakenThisSession.Add(currentRewardedAdName, true);
                        HomaBelly.Instance.TrackDesignEvent("Rewarded:FirstWatchedSession:" + Sanitize(currentRewardedAdName), GetTotalGameplaySecondsAtThisExactMoment());
                    }

                    // Current rewarded ad has not been taken ever
                    if (PlayerPrefs.GetInt(REWARDED_AD_FIRST_TIME_TAKEN_EVER_KEY, 0) == 0)
                    {
                        HomaBelly.Instance.TrackDesignEvent("Rewarded:FirstWatched:" + Sanitize(currentRewardedAdName), GetTotalGameplaySecondsAtThisExactMoment());
                        PlayerPrefs.SetInt(REWARDED_AD_FIRST_TIME_TAKEN_EVER_KEY, 1);
                        PlayerPrefs.Save();
                    }
                }
                
                HomaBelly.Instance.TrackDesignEvent("Rewarded:" + Sanitize(currentRewardedAdName) + ":Taken:" + Sanitize(currentLevelId));
            };

            Events.onRewardedVideoAdShowFailedEvent += (id) =>
            {
                HomaBelly.Instance.TrackDesignEvent("Rewarded:" + Sanitize(currentRewardedAdName) + ":Failed:" + Sanitize(currentLevelId));
            };

            Events.onRewardedVideoAdStartedEvent += (id) =>
            {
                HomaBelly.Instance.TrackDesignEvent("Rewarded:" + Sanitize(currentRewardedAdName) + ":Opened:" + Sanitize(currentLevelId));
            };

            Events.onRewardedVideoAdClosedEvent += (id) =>
            {
                HomaBelly.Instance.TrackDesignEvent("Rewarded:" + Sanitize(currentRewardedAdName) + ":Closed:" + Sanitize(currentLevelId));
            };
        }

        #endregion

        #region Miscellaneous

        /// <summary>
        /// Invoke this method everytime your Main Menu screen is loaded
        /// </summary>
        public static void MainMenuLoaded()
        {
            if (PlayerPrefs.GetInt(MAIN_MENU_LOADED_KEY, 0) == 0)
            {
                HomaBelly.Instance.TrackDesignEvent("MainMenu_Loaded", GetTotalGameplaySecondsAtThisExactMoment());
                PlayerPrefs.SetInt(MAIN_MENU_LOADED_KEY, 1);
                PlayerPrefs.Save();
            }
        }

        /// <summary>
        /// Invoke this method everytime the user starts the gameplay
        /// </summary>
        public static void GameplayStarted()
        {

            if (PlayerPrefs.GetInt(GAMEPLAY_STARTED_KEY, 0) == 0)
            {
                HomaBelly.Instance.TrackDesignEvent("GamePlay_Started", GetTotalGameplaySecondsAtThisExactMoment());
                PlayerPrefs.SetInt(GAMEPLAY_STARTED_KEY, 1);
                PlayerPrefs.Save();
            }
        }

        #endregion

        /// <summary>
        /// Avoids sending an empty parameter to analytics
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        private static string Sanitize(string parameter)
        {
            return string.IsNullOrEmpty(parameter) ? "_" : parameter;
        }
    }
}
