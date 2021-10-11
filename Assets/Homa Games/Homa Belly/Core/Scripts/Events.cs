using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HomaGames.HomaBelly
{
    public class Events
    {
        private static event Action _onInitialized;
        /// <summary>
        /// Invoked when Homa Belly has been fully initialized
        /// </summary>
        public static event Action onInitialized
        {
            add
            {
                if (_onInitialized == null || !_onInitialized.GetInvocationList().Contains(value))
                {
                    _onInitialized += value;
                }
            }

            remove
            {
                if (_onInitialized.GetInvocationList().Contains(value))
                {
                    _onInitialized -= value;
                }
            }
        }
        public void OnInitialized()
        {
            if (_onInitialized != null)
            {
                _onInitialized();
            }
        }

        #region Rewarded Video Ad Events
        private static event Action<string> _onRewardedVideoAdClosedEvent;
        /// <summary>
        /// Invoked when the RewardedVideo ad view is about to be closed.
        /// </summary>
        public static event Action<string> onRewardedVideoAdClosedEvent
        {
            add
            {
                if (_onRewardedVideoAdClosedEvent == null || !_onRewardedVideoAdClosedEvent.GetInvocationList().Contains(value))
                {
                    _onRewardedVideoAdClosedEvent += value;
                }
            }

            remove
            {
                if (_onRewardedVideoAdClosedEvent.GetInvocationList().Contains(value))
                {
                    _onRewardedVideoAdClosedEvent -= value;
                }
            }
        }
        public void OnRewardedVideoAdClosedEvent(string placement)
        {
            if (_onRewardedVideoAdClosedEvent != null)
            {
                _onRewardedVideoAdClosedEvent(placement);
            }
        }

        private static event Action<bool,string> _onRewardedVideoAvailabilityChangedEvent;
        /// <summary>
        /// Invoked when there is a change in the ad availability status.
        /// @param - available - value will change to true when rewarded videos are available
        /// </summary>
        public static event Action<bool,string> onRewardedVideoAvailabilityChangedEvent
        {
            add
            {
                if (_onRewardedVideoAvailabilityChangedEvent == null || !_onRewardedVideoAvailabilityChangedEvent.GetInvocationList().Contains(value))
                {
                    _onRewardedVideoAvailabilityChangedEvent += value;
                }
            }

            remove
            {
                if (_onRewardedVideoAvailabilityChangedEvent.GetInvocationList().Contains(value))
                {
                    _onRewardedVideoAvailabilityChangedEvent -= value;
                }
            }
        }
        public void OnRewardedVideoAvailabilityChangedEvent(bool available,string placement)
        {
            if (_onRewardedVideoAvailabilityChangedEvent != null)
            {
                _onRewardedVideoAvailabilityChangedEvent(available,placement);
            }
        }

        private static event Action<string> _onRewardedVideoAdStartedEvent;
        /// <summary>
        /// Invoked when the video ad has opened. 
        /// </summary>
        public static event Action<string> onRewardedVideoAdStartedEvent
        {
            add
            {
                if (_onRewardedVideoAdStartedEvent == null || !_onRewardedVideoAdStartedEvent.GetInvocationList().Contains(value))
                {
                    _onRewardedVideoAdStartedEvent += value;
                }
            }

            remove
            {
                if (_onRewardedVideoAdStartedEvent.GetInvocationList().Contains(value))
                {
                    _onRewardedVideoAdStartedEvent -= value;
                }
            }
        }
        public void OnRewardedVideoAdStartedEvent(string placement)
        {
            if (_onRewardedVideoAdStartedEvent != null)
            {
                _onRewardedVideoAdStartedEvent(placement);
            }
        }

        private static event Action _onRewardedVideoAdEndedEvent;
        /// <summary>
        /// Invoked when the video ad finishes playing.
        /// </summary>
        public static event Action onRewardedVideoAdEndedEvent
        {
            add
            {
                if (_onRewardedVideoAdEndedEvent == null || !_onRewardedVideoAdEndedEvent.GetInvocationList().Contains(value))
                {
                    _onRewardedVideoAdEndedEvent += value;
                }
            }

            remove
            {
                if (_onRewardedVideoAdEndedEvent.GetInvocationList().Contains(value))
                {
                    _onRewardedVideoAdEndedEvent -= value;
                }
            }
        }
        public void OnRewardedVideoAdEndedEvent()
        {
            if (_onRewardedVideoAdEndedEvent != null)
            {
                _onRewardedVideoAdEndedEvent();
            }
        }

        private static event Action<VideoAdReward> _onRewardedVideoAdRewardedEvent;
        /// <summary>
        /// Invoked when the user completed the video and should be rewarded.
        /// </summary>
        public static event Action<VideoAdReward> onRewardedVideoAdRewardedEvent
        {
            add
            {
                if (_onRewardedVideoAdRewardedEvent == null || !_onRewardedVideoAdRewardedEvent.GetInvocationList().Contains(value))
                {
                    _onRewardedVideoAdRewardedEvent += value;
                }
            }

            remove
            {
                if (_onRewardedVideoAdRewardedEvent.GetInvocationList().Contains(value))
                {
                    _onRewardedVideoAdRewardedEvent -= value;
                }
            }
        }
        public void OnRewardedVideoAdRewardedEvent(VideoAdReward videoAdReward)
        {
            if (_onRewardedVideoAdRewardedEvent != null)
            {
                _onRewardedVideoAdRewardedEvent(videoAdReward);
            }
        }

        private static event Action<string> _onRewardedVideoAdShowFailedEvent;
        /// <summary>
        /// Invoked when the Rewarded Video failed to show
        /// </summary>
        public static event Action<string> onRewardedVideoAdShowFailedEvent
        {
            add
            {
                if (_onRewardedVideoAdShowFailedEvent == null || !_onRewardedVideoAdShowFailedEvent.GetInvocationList().Contains(value))
                {
                    _onRewardedVideoAdShowFailedEvent += value;
                }
            }

            remove
            {
                if (_onRewardedVideoAdShowFailedEvent.GetInvocationList().Contains(value))
                {
                    _onRewardedVideoAdShowFailedEvent -= value;
                }
            }
        }
        public void OnRewardedVideoAdShowFailedEvent(string placement)
        {
            if (_onRewardedVideoAdShowFailedEvent != null)
            {
                _onRewardedVideoAdShowFailedEvent(placement);
            }
        }

        private static event Action<string> _onRewardedVideoAdClicked;
        /// <summary>
        /// Invoked when the video ad is clicked.
        /// </summary>
        public static event Action<string> onRewardedVideoAdClickedEvent
        {
            add
            {
                if (_onRewardedVideoAdClicked == null || !_onRewardedVideoAdClicked.GetInvocationList().Contains(value))
                {
                    _onRewardedVideoAdClicked += value;
                }
            }

            remove
            {
                if (_onRewardedVideoAdClicked.GetInvocationList().Contains(value))
                {
                    _onRewardedVideoAdClicked -= value;
                }
            }
        }
        public void OnRewardedVideoAdClickedEvent(string placement)
        {
            if (_onRewardedVideoAdClicked != null)
            {
                _onRewardedVideoAdClicked(placement);
            }
        }
        #endregion

        #region Interstitial Events
        private static event Action _onInterstitialAdReadyEvent;
        /// <summary>
        /// Invoked when the Interstitial is Ready to shown after load function is called
        /// </summary>
        public static event Action onInterstitialAdReadyEvent
        {
            add
            {
                if (_onInterstitialAdReadyEvent == null || !_onInterstitialAdReadyEvent.GetInvocationList().Contains(value))
                {
                    _onInterstitialAdReadyEvent += value;
                }
            }

            remove
            {
                if (_onInterstitialAdReadyEvent.GetInvocationList().Contains(value))
                {
                    _onInterstitialAdReadyEvent -= value;
                }
            }
        }
        public void OnInterstitialAdReadyEvent()
        {
            if (_onInterstitialAdReadyEvent != null)
            {
                _onInterstitialAdReadyEvent();
            }
        }

        private static event Action _onInterstitialAdLoadFailedEvent;
        /// <summary>
        /// Invoked when the initialization process has failed.
        ///  @param description - string - contains information about the failure.
        /// </summary>
        public static event Action onInterstitialAdLoadFailedEvent
        {
            add
            {
                if (_onInterstitialAdLoadFailedEvent == null || !_onInterstitialAdLoadFailedEvent.GetInvocationList().Contains(value))
                {
                    _onInterstitialAdLoadFailedEvent += value;
                }
            }

            remove
            {
                if (_onInterstitialAdLoadFailedEvent.GetInvocationList().Contains(value))
                {
                    _onInterstitialAdLoadFailedEvent -= value;
                }
            }
        }
        public void OnInterstitialAdLoadFailedEvent()
        {
            if (_onInterstitialAdLoadFailedEvent != null)
            {
                _onInterstitialAdLoadFailedEvent();
            }
        }

        private static event Action<string> _onInterstitialAdShowSucceededEvent;
        /// <summary>
        /// Invoked right before the Interstitial screen is about to open.
        /// </summary>
        public static event Action<string> onInterstitialAdShowSucceededEvent
        {
            add
            {
                if (_onInterstitialAdShowSucceededEvent == null || !_onInterstitialAdShowSucceededEvent.GetInvocationList().Contains(value))
                {
                    _onInterstitialAdShowSucceededEvent += value;
                }
            }

            remove
            {
                if (_onInterstitialAdShowSucceededEvent.GetInvocationList().Contains(value))
                {
                    _onInterstitialAdShowSucceededEvent -= value;
                }
            }
        }
        public void OnInterstitialAdShowSucceededEvent(string placement)
        {
            if (_onInterstitialAdShowSucceededEvent != null)
            {
                _onInterstitialAdShowSucceededEvent(placement);
            }
        }

        private static event Action<string> _onInterstitialAdShowFailedEvent;
        /// <summary>
        /// Invoked when the ad fails to show.
        /// @param description - string - contains information about the failure.
        /// </summary>
        public static event Action<string> onInterstitialAdShowFailedEvent
        {
            add
            {
                if (_onInterstitialAdShowFailedEvent == null || !_onInterstitialAdShowFailedEvent.GetInvocationList().Contains(value))
                {
                    _onInterstitialAdShowFailedEvent += value;
                }
            }

            remove
            {
                if (_onInterstitialAdShowFailedEvent.GetInvocationList().Contains(value))
                {
                    _onInterstitialAdShowFailedEvent -= value;
                }
            }
        }
        public void OnInterstitialAdShowFailedEvent(string placement)
        {
            if (_onInterstitialAdShowFailedEvent != null)
            {
                _onInterstitialAdShowFailedEvent(placement);
            }
        }

        private static event Action<string> _onInterstitialAdClickedEvent;
        /// <summary>
        /// Invoked when end user clicked on the interstitial ad
        /// </summary>
        public static event Action<string> onInterstitialAdClickedEvent
        {
            add
            {
                if (_onInterstitialAdClickedEvent == null || !_onInterstitialAdClickedEvent.GetInvocationList().Contains(value))
                {
                    _onInterstitialAdClickedEvent += value;
                }
            }

            remove
            {
                if (_onInterstitialAdClickedEvent.GetInvocationList().Contains(value))
                {
                    _onInterstitialAdClickedEvent -= value;
                }
            }
        }
        public void OnInterstitialAdClickedEvent(string placement)
        {
            if (_onInterstitialAdClickedEvent != null)
            {
                _onInterstitialAdClickedEvent(placement);
            }
        }

        private static event Action _onInterstitialAdOpenedEvent;
        /// <summary>
        /// Invoked when the Interstitial Ad Unit has opened
        /// </summary>
        public static event Action onInterstitialAdOpenedEvent
        {
            add
            {
                if (_onInterstitialAdOpenedEvent == null || !_onInterstitialAdOpenedEvent.GetInvocationList().Contains(value))
                {
                    _onInterstitialAdOpenedEvent += value;
                }
            }

            remove
            {
                if (_onInterstitialAdOpenedEvent.GetInvocationList().Contains(value))
                {
                    _onInterstitialAdOpenedEvent -= value;
                }
            }
        }
        public void OnInterstitialAdOpenedEvent()
        {
            if (_onInterstitialAdOpenedEvent != null)
            {
                _onInterstitialAdOpenedEvent();
            }
        }

        private static event Action _onInterstitialAdClosedEvent;
        /// <summary>
        /// Invoked when the interstitial ad closed and the user goes back to the application screen.
        /// </summary>
        public static event Action onInterstitialAdClosedEvent
        {
            add
            {
                if (_onInterstitialAdClosedEvent == null || !_onInterstitialAdClosedEvent.GetInvocationList().Contains(value))
                {
                    _onInterstitialAdClosedEvent += value;
                }
            }

            remove
            {
                if (_onInterstitialAdClosedEvent.GetInvocationList().Contains(value))
                {
                    _onInterstitialAdClosedEvent -= value;
                }
            }
        }
        public void OnInterstitialAdClosedEvent()
        {
            if (_onInterstitialAdClosedEvent != null)
            {
                _onInterstitialAdClosedEvent();
            }
        }
        #endregion

        #region Banner Events
        private static event Action _onBannerAdLeftApplicationEvent;
        /// <summary>
        /// Invoked when the user leaves the app
        /// </summary>
        public static event Action onBannerAdLeftApplicationEvent
        {
            add
            {
                if (_onBannerAdLeftApplicationEvent == null || !_onBannerAdLeftApplicationEvent.GetInvocationList().Contains(value))
                {
                    _onBannerAdLeftApplicationEvent += value;
                }
            }

            remove
            {
                if (_onBannerAdLeftApplicationEvent.GetInvocationList().Contains(value))
                {
                    _onBannerAdLeftApplicationEvent -= value;
                }
            }
        }
        public void OnBannerAdLeftApplicationEvent()
        {
            if (_onBannerAdLeftApplicationEvent != null)
            {
                _onBannerAdLeftApplicationEvent();
            }
        }

        private static event Action _onBannerAdScreenDismissedEvent;
        /// <summary>
        /// Notifies the presented screen has been dismissed
        /// </summary>
        public static event Action onBannerAdScreenDismissedEvent
        {
            add
            {
                if (_onBannerAdScreenDismissedEvent == null || !_onBannerAdScreenDismissedEvent.GetInvocationList().Contains(value))
                {
                    _onBannerAdScreenDismissedEvent += value;
                }
            }

            remove
            {
                if (_onBannerAdScreenDismissedEvent.GetInvocationList().Contains(value))
                {
                    _onBannerAdScreenDismissedEvent -= value;
                }
            }
        }
        public void OnBannerAdScreenDismissedEvent()
        {
            if (_onBannerAdScreenDismissedEvent != null)
            {
                _onBannerAdScreenDismissedEvent();
            }
        }

        private static event Action<string> _onBannerAdScreenPresentedEvent;
        /// <summary>
        /// Notifies the presentation of a full screen content following user click
        /// </summary>
        public static event Action<string> onBannerAdScreenPresentedEvent
        {
            add
            {
                if (_onBannerAdScreenPresentedEvent == null || !_onBannerAdScreenPresentedEvent.GetInvocationList().Contains(value))
                {
                    _onBannerAdScreenPresentedEvent += value;
                }
            }

            remove
            {
                if (_onBannerAdScreenPresentedEvent.GetInvocationList().Contains(value))
                {
                    _onBannerAdScreenPresentedEvent -= value;
                }
            }
        }
        public void OnBannerAdScreenPresentedEvent(string placement)
        {
            if (_onBannerAdScreenPresentedEvent != null)
            {
                _onBannerAdScreenPresentedEvent(placement);
            }
        }

        private static event Action<string> _onBannerAdClickedEvent;
        /// <summary>
        /// Invoked when end user clicks on the banner ad
        /// </summary>
        public static event Action<string> onBannerAdClickedEvent
        {
            add
            {
                if (_onBannerAdClickedEvent == null || !_onBannerAdClickedEvent.GetInvocationList().Contains(value))
                {
                    _onBannerAdClickedEvent += value;
                }
            }

            remove
            {
                if (_onBannerAdClickedEvent.GetInvocationList().Contains(value))
                {
                    _onBannerAdClickedEvent -= value;
                }
            }
        }
        public void OnBannerAdClickedEvent(string placementId)
        {
            if (_onBannerAdClickedEvent != null)
            {
                _onBannerAdClickedEvent(placementId);
            }
        }

        private static event Action<string> _onBannerAdLoadFailedEvent;
        /// <summary>
        /// Invoked when the banner loading process has failed.
        /// @param description - string - contains information about the failure.
        /// </summary>
        public static event Action<string> onBannerAdLoadFailedEvent
        {
            add
            {
                if (_onBannerAdLoadFailedEvent == null || !_onBannerAdLoadFailedEvent.GetInvocationList().Contains(value))
                {
                    _onBannerAdLoadFailedEvent += value;
                }
            }

            remove
            {
                if (_onBannerAdLoadFailedEvent.GetInvocationList().Contains(value))
                {
                    _onBannerAdLoadFailedEvent -= value;
                }
            }
        }
        public void OnBannerAdLoadFailedEvent(string placement)
        {
            if (_onBannerAdLoadFailedEvent != null)
            {
                _onBannerAdLoadFailedEvent(placement);
            }
        }

        private static event Action<string> _onBannerAdLoadedEvent;
        /// <summary>
        /// Invoked once the banner has loaded
        /// </summary>
        public static event Action<string> onBannerAdLoadedEvent
        {
            add
            {
                if (_onBannerAdLoadedEvent == null || !_onBannerAdLoadedEvent.GetInvocationList().Contains(value))
                {
                    _onBannerAdLoadedEvent += value;
                }
            }

            remove
            {
                if (_onBannerAdLoadedEvent.GetInvocationList().Contains(value))
                {
                    _onBannerAdLoadedEvent -= value;
                }
            }
        }
        public void OnBannerAdLoadedEvent(string placement)
        {
            if (_onBannerAdLoadedEvent != null)
            {
                _onBannerAdLoadedEvent(placement);
            }
        }
        #endregion
    }
}
