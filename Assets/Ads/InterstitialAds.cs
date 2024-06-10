using _Game.Managers;
using _Game.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InterstitialAds : MonoBehaviour
{
    string adUnitId = "c8ea8f6273eef263";
    int retryAttempt;
    bool showImmediate = false;
    bool isShowingAds = false;
    Action onAdsClose = null;
    public bool IsShowingAds => isShowingAds;

    // Start is called before the first frame update
    void Start()
    {
        // Attach callback
        MaxSdkCallbacks.Interstitial.OnAdLoadedEvent += OnInterstitialLoadedEvent;
        MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent += OnInterstitialLoadFailedEvent;
        MaxSdkCallbacks.Interstitial.OnAdDisplayedEvent += OnInterstitialDisplayedEvent;
        MaxSdkCallbacks.Interstitial.OnAdClickedEvent += OnInterstitialClickedEvent;
        MaxSdkCallbacks.Interstitial.OnAdHiddenEvent += OnInterstitialHiddenEvent;
        MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent += OnInterstitialAdFailedToDisplayEvent;
        isShowingAds = false;
        // Load the first interstitial
    }

    public void Load(bool show = false)
    {
        MaxSdk.LoadInterstitial(adUnitId);
        showImmediate = show;
    }

    public void Show(Action onAdsClose = null)
    {
        if (MaxSdk.IsInterstitialReady(adUnitId))
        {
            isShowingAds = true;
            MaxSdk.ShowInterstitial(adUnitId);
            AnalysticManager.Ins.InterAdsShow(_Game.Ads.Placement.In_Game);
            this.onAdsClose = onAdsClose;
            DevLog.Log(DevId.Hung, "ADS: SHOWING INTER");
        }
        else
        {
            Debug.LogWarning("Interstitial ad is not ready. Make sure to load it before showing.");
            Load(true); // Load a new interstitial ad
        }
    }

    private void OnInterstitialLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        // Interstitial ad is ready for you to show. MaxSdk.IsInterstitialReady(adUnitId) now returns 'true'
        AnalysticManager.Ins.AppsFlyerTrackEvent("af_inters_successfullyloaded");
        if (showImmediate)
            MaxSdk.ShowInterstitial(adUnitId);
        // Reset retry attempt
        retryAttempt = 0;
    }

    private void OnInterstitialLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
    {
        // Interstitial ad failed to load 
        // AppLovin recommends that you retry with exponentially higher delays, up to a maximum delay (in this case 64 seconds)

        retryAttempt++;
        double retryDelay = Math.Pow(2, Math.Min(6, retryAttempt));

        Invoke("LoadInterstitial", (float)retryDelay);
    }

    private void OnInterstitialDisplayedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) 
    {
        AnalysticManager.Ins.AppsFlyerTrackEvent("af_inters_displayed");
    }

    private void OnInterstitialAdFailedToDisplayEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
    {
        // Interstitial ad failed to display. AppLovin recommends that you load the next ad.
        Load();
    }

    private void OnInterstitialClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) { }

    private void OnInterstitialHiddenEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        // Interstitial ad is hidden. Pre-load the next ad.
        isShowingAds = false;
        this.onAdsClose?.Invoke();
        DevLog.Log(DevId.Hung, "ADS: HIDE INTER");
        Load();
    }

    // Update is called once per frame
    void Update()
    {

    }
}
