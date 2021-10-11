using System;
using UnityEngine;

public class GAHyperBidIntegration : MonoBehaviour
{
#if gameanalytics_hyperbid_enabled && !(UNITY_EDITOR)
    private static bool _subscribed = false;
    private class GAInterstitialListener : HyperBid.Api.HBInterstitialAdListener
    {
        private Action<string> callback;

        public GAInterstitialListener(Action<string> callback)
        {
            this.callback = callback;
        }

        public void onInterstitialAdClick(string placementId, HyperBid.Api.HBCallbackInfo callbackInfo) {}

        public void onInterstitialAdClose(string placementId, HyperBid.Api.HBCallbackInfo callbackInfo) {}

        public void onInterstitialAdEndPlayingVideo(string placementId, HyperBid.Api.HBCallbackInfo callbackInfo) {}

        public void onInterstitialAdFailedToPlayVideo(string placementId, string code, string message) {}

        public void onInterstitialAdFailedToShow(string placementId) {}

        public void onInterstitialAdLoad(string placementId) {}

        public void onInterstitialAdLoadFail(string placementId, string code, string message) {}

        public void onInterstitialAdShow(string placementId, HyperBid.Api.HBCallbackInfo callbackInfo)
        {
            callback(callbackInfo.getOriginJSONString());
        }

        public void onInterstitialAdStartPlayingVideo(string placementId, HyperBid.Api.HBCallbackInfo callbackInfo) {}
    }
    private class GABannerListener : HyperBid.Api.HBBannerAdListener
    {
        private Action<string> callback;

        public GABannerListener(Action<string> callback)
        {
            this.callback = callback;
        }

        public void onAdAutoRefresh(string placementId, HyperBid.Api.HBCallbackInfo callbackInfo) {}

        public void onAdAutoRefreshFail(string placementId, string code, string message) {}

        public void onAdClick(string placementId, HyperBid.Api.HBCallbackInfo callbackInfo) {}

        public void onAdClose(string placementId) {}

        public void onAdCloseButtonTapped(string placementId, HyperBid.Api.HBCallbackInfo callbackInfo) {}

        public void onAdImpress(string placementId, HyperBid.Api.HBCallbackInfo callbackInfo)
        {
            callback(callbackInfo.getOriginJSONString());
        }

        public void onAdLoad(string placementId) {}

        public void onAdLoadFail(string placementId, string code, string message) {}
    }
    private class GARewardedListener : HyperBid.Api.HBRewardedVideoListener
    {
        private Action<string> callback;

        public GARewardedListener(Action<string> callback)
        {
            this.callback = callback;
        }

        public void onRewardedVideoAdLoaded(string placementId) {}

        public void onRewardedVideoAdLoadFail(string placementId, string code, string message) {}

        public void onRewardedVideoAdPlayStart(string placementId, HyperBid.Api.HBCallbackInfo callbackInfo)
        {
            callback(callbackInfo.getOriginJSONString());
        }

        public void onRewardedVideoAdPlayEnd(string placementId, HyperBid.Api.HBCallbackInfo callbackInfo) {}

        public void onRewardedVideoAdPlayFail(string placementId, string code, string message) {}

        public void onRewardedVideoAdPlayClosed(string placementId, bool isReward, HyperBid.Api.HBCallbackInfo callbackInfo) {}

        public void onRewardedVideoAdPlayClicked(string placementId, HyperBid.Api.HBCallbackInfo callbackInfo) {}

        public void onReward(string placementId, HyperBid.Api.HBCallbackInfo callbackInfo) {}
    }

    private class GANativeListener : HyperBid.Api.HBNativeAdListener
    {
        private Action<string> callback;

        public GANativeListener(Action<string> callback)
        {
            this.callback = callback;
        }

        public void onAdClicked(string placementId, HyperBid.Api.HBCallbackInfo callbackInfo) {}

        public void onAdCloseButtonClicked(string placementId, HyperBid.Api.HBCallbackInfo callbackInfo) {}

        public void onAdImpressed(string placementId, HyperBid.Api.HBCallbackInfo callbackInfo)
        {
            callback(callbackInfo.getOriginJSONString());
        }

        public void onAdLoaded(string placementId) {}

        public void onAdLoadFail(string placementId, string code, string message) {}

        public void onAdVideoEnd(string placementId) {}

        public void onAdVideoProgress(string placementId, int progress) {}

        public void onAdVideoStart(string placementId) {}
    }
#endif

    public static void ListenForImpressions(Action<string> callback)
    {
#if gameanalytics_hyperbid_enabled && !(UNITY_EDITOR)
        if (_subscribed)
        {
            Debug.Log("Ignoring duplicate gameanalytics subscription");
            return;
        }

        HyperBid.Api.HBInterstitialAd.Instance.setListener(new GAInterstitialListener(callback));
        HyperBid.Api.HBBannerAd.Instance.setListener(new GABannerListener(callback));
        HyperBid.Api.HBRewardedVideo.Instance.setListener(new GARewardedListener(callback));
        HyperBid.Api.HBNativeAd.Instance.setListener(new GANativeListener(callback));
        _subscribed = true;
#endif
    }
}
