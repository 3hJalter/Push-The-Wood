using _Game._Scripts.Managers;
using _Game.Managers;
using _Game.Resource;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using _Game.Ads;

public class RewardedAds : MonoBehaviour
{
    string adUnitId = "e9cf2be5a77927b0";
    int retryAttempt;
    bool showImmediate = false;

    List<BoosterType> boosterTypes;
    List<CurrencyType> resourceTypes;

    List<int> boosterAmounts;
    List<int> resourceAmounts;
    _Game.Ads.Placement placement;

    private event Action OnCallBackAdReceivedRewardEvent;

    // Start is called before the first frame update
    void Start()
    {
        // Attach callback
        MaxSdkCallbacks.Rewarded.OnAdLoadedEvent += OnRewardedAdLoadedEvent;
        MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent += OnRewardedAdLoadFailedEvent;
        MaxSdkCallbacks.Rewarded.OnAdDisplayedEvent += OnRewardedAdDisplayedEvent;
        MaxSdkCallbacks.Rewarded.OnAdClickedEvent += OnRewardedAdClickedEvent;
        MaxSdkCallbacks.Rewarded.OnAdHiddenEvent += OnRewardedAdHiddenEvent;
        MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent += OnRewardedAdFailedToDisplayEvent;
        MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent += OnRewardedAdReceivedRewardEvent;
    }
    public void Load(bool show = false)
    {
        MaxSdk.LoadRewardedAd(adUnitId);
        showImmediate = show;
    }

    public void Show(Action callback, _Game.Ads.Placement placement = _Game.Ads.Placement.None)
    {
        OnClearReward();
        OnClearCallBack();
        OnCallBackAdReceivedRewardEvent = callback;
        this.placement = placement;
        Show();
    }

    public void Show(List<CurrencyType> resourceTypes = null, List<int> resourceAmount = null,
        List<BoosterType> boosterTypes = null, List<int> boosterAmount = null, _Game.Ads.Placement placement = _Game.Ads.Placement.None)
    {
        OnClearReward();
        OnClearCallBack();
        this.resourceTypes = resourceTypes;
        this.resourceAmounts = resourceAmount;
        this.boosterTypes = boosterTypes;
        this.boosterAmounts = boosterAmount;
        this.placement = placement;
        Show();
    }

    private void Show()
    {
        AnalysticManager.Ins.AppsFlyerTrackEvent("af_rewarded_logicgame");
        if (MaxSdk.IsRewardedAdReady(adUnitId))
        {
            if (DebugManager.Ins && !DebugManager.Ins.IsShowAds)
            {
                OnRewardedAdReceivedRewardEvent(default, default, default);
            }
            else
            {
                MaxSdk.ShowRewardedAd(adUnitId);
            }
        }
        else
        {
            Debug.LogWarning("Rewarded ad is not ready. Make sure to load it before showing.");
            Load(true); // Load a new rewarded ad
        }
    }

    private void OnRewardedAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        // Rewarded ad is ready for you to show. MaxSdk.IsRewardedAdReady(adUnitId) now returns 'true'.\
        AnalysticManager.Ins.AppsFlyerTrackEvent("af_rewarded_successfullyloaded");

        if (showImmediate)
            MaxSdk.ShowRewardedAd(adUnitId);
        // Reset retry attempt
        retryAttempt = 0;
    }

    private void OnRewardedAdLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
    {
        // Rewarded ad failed to load
        // AppLovin recommends that you retry with exponentially higher delays, up to a maximum delay (in this case 64 seconds).

        retryAttempt++;
        double retryDelay = Math.Pow(2, Math.Min(6, retryAttempt));

        Invoke("LoadRewardedAd", (float)retryDelay);
    }

    private void OnRewardedAdDisplayedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        AnalysticManager.Ins.AppsFlyerTrackEvent("af_rewarded_displayed");
    }

    private void OnRewardedAdFailedToDisplayEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
    {
        // Rewarded ad failed to display. AppLovin recommends that you load the next ad.
        Load();
    }

    private void OnRewardedAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) { }

    private void OnRewardedAdHiddenEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        // Rewarded ad is hidden. Pre-load the next ad
    }

    private void OnRewardedAdReceivedRewardEvent(string adUnitId, MaxSdk.Reward reward, MaxSdkBase.AdInfo adInfo)
    {
        if (resourceTypes != null)
        {
        }

        if (boosterTypes != null)
        {
            for (int i = 0; i < boosterTypes.Count; i++)
            {
                switch (boosterTypes[i])
                {
                    case BoosterType.PushHint:
                        EventGlobalManager.Ins.OnChangeBoosterAmount.Dispatch(boosterTypes[i], boosterAmounts[i]);
                        EventGlobalManager.Ins.OnUsingBooster.Dispatch(BoosterType.PushHint);
                        break;
                    default:
                        EventGlobalManager.Ins.OnChangeBoosterAmount.Dispatch(boosterTypes[i], boosterAmounts[i]);
                        break;
                }
            }
            AnalysticManager.Ins.RewardAdsComplete(placement);
            GameManager.Ins.PostEvent(_Game.DesignPattern.EventID.OnUpdateUI);
        }
        Load();

        OnCallBackAdReceivedRewardEvent?.Invoke();
    }

    private void OnClearReward()
    {
        resourceTypes = null;
        resourceAmounts = null;
        boosterTypes = null;
        boosterAmounts = null;
    }

    private void OnClearCallBack()
    {
        OnCallBackAdReceivedRewardEvent = null;
    }


    // Update is called once per frame
    void Update()
    {
        // Update logic here
    }
}
