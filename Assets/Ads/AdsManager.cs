using _Game.DesignPattern;
using _Game.GameGrid;
using _Game.Utilities.Timer;
using AppsFlyerSDK;
using GoogleMobileAds.Api;
using NSubstitute;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VinhLB;
using static UnityEngine.Rendering.DebugUI;

namespace _Game.Managers
{
    [DefaultExecutionOrder(-100)]
    public class AdsManager : Singleton<AdsManager>
    {
        [SerializeField]
        BannerAds Banner;
        [SerializeField]
        RewardedAds Reward;
        [SerializeField]
        InterstitialAds Interstitial;
        public RewardedAds RewardedAds => Reward;

        STimer cooldownTimer;
        STimer interTimer;
        STimer bannerTimer;
        Action interCallBack;

        int intAdsStepCount = 0;
        public bool IsBannerOpen => Banner.IsBannerOpen;
        public bool IsCanShowInter
        {
            get
            {
                if (cooldownTimer.IsStart || (DebugManager.Ins && !DebugManager.Ins.IsShowAds))
                {
                    return false;
                }
                switch (LevelManager.Ins.CurrentLevel.LevelType)
                {
                    case Data.LevelType.Normal:
                        int levelIndex = DataManager.Ins.GameData.user.normalLevelIndex;
                        if (levelIndex < DataManager.Ins.ConfigData.startInterAdsLevel || interTimer.IsStart)
                        {
                            return false;
                        }
                        return true;
                    case Data.LevelType.Secret:
                        return true;
                    case Data.LevelType.DailyChallenge:
                        return true;
                }
                return false;
            }
        }

        protected void Awake()
        {
            DontDestroyOnLoad(gameObject);
            cooldownTimer = TimerManager.Ins.PopSTimer();
            interTimer = TimerManager.Ins.PopSTimer();
            bannerTimer = TimerManager.Ins.PopSTimer();

            MaxSdkCallbacks.OnSdkInitializedEvent += (MaxSdkBase.SdkConfiguration sdkConfiguration) =>
            {
                // AppLovin SDK is initialized, start loading ads
                Reward.Load();
                Interstitial.Load();
            };
            //GameManager.Ins.RegisterListenerEvent(EventID.OnInterAdsStepCount, OnInterAdsStepCount);
            MaxSdk.SetSdkKey("ZoNyqu_piUmpl33-qkoIfRp6MTZGW9M5xk1mb1ZIWK6FN9EBu0TXSHeprC3LMPQI7S3kTc1-x7DJGSV8S-gvFJ");
            MaxSdk.InitializeSdk();

            GameManager.Ins.RegisterListenerEvent(EventID.OnCheckShowInterAds, CheckShowInterAds);           
            MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent += OnRewardedAdRevenuePaidEvent;

            interTimer.Start((float)DataManager.Ins.ConfigData.interAdsCooldownTime);
            cooldownTimer.Start((float)DataManager.Ins.ConfigData.interAdsCooldownTime);
        }

        public void ShowBannerAds(BannerAds.TYPE type)
        {
            int levelIndex = DataManager.Ins.GameData.user.normalLevelIndex;
            if (levelIndex < DataManager.Ins.ConfigData.startBannerAds || (DebugManager.Ins && !DebugManager.Ins.IsShowAds))
                return;
            switch (type)
            {
                case BannerAds.TYPE.ADSMOB:
                    Banner.Show(type);
                    bannerTimer.Start(DataManager.Ins.ConfigData.reloadBannerTime, () => Banner.Show(type), true);
                    break;
            }

        }

        public void HideBannerAds()
        {
            if (DebugManager.Ins && !DebugManager.Ins.IsShowAds)
                return;
            if (IsBannerOpen)
            {
                bannerTimer.Stop();
                Banner.Hide();
            }
        }

        private void CheckShowInterAds(object callBack = null)
        {
            interCallBack = (Action)callBack;
            if (!IsCanShowInter)
            {
                interCallBack?.Invoke();
                interCallBack = null;
                return;
            }
            else
            {
                switch (LevelManager.Ins.CurrentLevel.LevelType)
                {
                    case Data.LevelType.Normal:
                        interTimer.Start((float)DataManager.Ins.ConfigData.interAdsCappingTime);
                        break;
                }
                ShowInterAdsWithSplashScreen();
            }
            AnalysticManager.Ins.AppsFlyerTrackEvent("af_inters_logicgame");
        }

        private void ShowInterAds()
        {
            Interstitial.Show(OnInterAdsDone);
        }

        private void ShowInterAdsWithSplashScreen()
        {
            UIManager.Ins.OpenUI<SplashScreen>((Action)(() => Interstitial.Show(() =>
            {
                OnInterAdsDone();
                UIManager.Ins.CloseUI<SplashScreen>();
            })));
        }

        private void OnInterAdsStepCount(object value)
        {
            int levelIndex = DataManager.Ins.GameData.user.normalLevelIndex;
            if (levelIndex < DataManager.Ins.ConfigData.startInterAdsLevel || cooldownTimer.IsStart
                || (DebugManager.Ins && !DebugManager.Ins.IsShowAds)) return;

            AnalysticManager.Ins.AppsFlyerTrackEvent("af_inters_logicgame");
            DataManager.Ins.AddInterAdsStepCount((int)value);
            if (DataManager.Ins.InterAdsStepCount >= DataManager.Ins.ConfigData.stepInterAdsCountMax)
            {
                ShowInterAdsWithSplashScreen();
            }
        }

        private void OnInterAdsDone()
        {
            DataManager.Ins.AddInterAdsStepCount(-DataManager.Ins.ConfigData.stepInterAdsCountMax);
            cooldownTimer.Start((float)DataManager.Ins.ConfigData.interAdsCooldownTime);
            interCallBack?.Invoke();
            interCallBack = null;
        }

        private void OnRewardedAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            //NOTE: Ad revenue paid. Use this callback to track user revenue.
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("ad_platform", "appLovin");
            parameters.Add("ad_source", adInfo.NetworkName);
            parameters.Add("ad_unit_name", adInfo.AdUnitIdentifier);
            parameters.Add("ad_format", adInfo.AdFormat);
            parameters.Add("placement", adInfo.Placement);
            parameters.Add("value", adInfo.Revenue.ToString());
            parameters.Add("currency", "USD");

            AppsFlyerAdRevenue.logAdRevenue(adInfo.NetworkName,
                AppsFlyerAdRevenueMediationNetworkType.AppsFlyerAdRevenueMediationNetworkTypeApplovinMax,
                adInfo.Revenue,
                "USD",
                parameters);
        }
        private void OnDestroy()
        {
            //GameManager.Ins.UnregisterListenerEvent(EventID.OnInterAdsStepCount, OnInterAdsStepCount);
            GameManager.Ins.UnregisterListenerEvent(EventID.OnCheckShowInterAds, CheckShowInterAds);
        }
    }
}