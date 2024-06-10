using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using _Game.DesignPattern;
using Firebase.Analytics;
using _Game.Data;
using _Game.Resource;
using _Game.Ads;
using _Game.Utilities.Timer;
using System;
using System.Globalization;
using _Game._Scripts.InGame;
using AppsFlyerSDK;
using Firebase.Extensions;

namespace _Game.Managers
{
    public class AnalysticManager : Singleton<AnalysticManager>
    {
        bool _firstAdsSession = false;
        STimer _firstAdsSessionTimer;
        int _firstAdsSessionTime = 0;
        bool isFirebaseInit = false;
        private void Awake()
        {
            DontDestroyOnLoad(this);

            if(!Application.isEditor)
                Init();

            _firstAdsSessionTime = 0;
            _firstAdsSessionTimer = TimerManager.Ins.PopSTimer();
            _firstAdsSessionTimer.Start(1, () =>
            {
                _firstAdsSessionTime += 1;
            }, true);
        }

        private void Init()
        {
            Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
                var dependencyStatus = task.Result;
                if (dependencyStatus == Firebase.DependencyStatus.Available)
                {
                    // Create and hold a reference to your FirebaseApp,
                    // where app is a Firebase.FirebaseApp property of your application class.

                    // Set a flag here to indicate whether Firebase is ready to use by your app.
                    isFirebaseInit = true;
                }
                else
                {
                    UnityEngine.Debug.LogError(System.String.Format(
                      "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
                    // Firebase Unity SDK is not safe to use here.
                    isFirebaseInit = false;
                }
            });
        }
        public void LevelStart(Level level)
        {
            LevelType type = level.LevelType;
            int index = level.Index;
            if (!isFirebaseInit) return;
            switch (type)
            {
                case LevelType.Normal:
                    FirebaseAnalytics.LogEvent("level_start", "level", index);
                    break;
                case LevelType.DailyChallenge:
                    FirebaseAnalytics.LogEvent("level_start", new Parameter[] 
                    { 
                        new Parameter("level_dailyChallenge", index),
                        new Parameter("day", DateTime.UtcNow.Date.ToString(CultureInfo.InvariantCulture))
                    });
                    break;
                case LevelType.Secret:
                    FirebaseAnalytics.LogEvent("level_start", "level_expedition", index);
                    break;
            }
        }

        public void LevelComplete(Level level)
        {
            if (!isFirebaseInit) return;
            Parameter[] param = null;
            LevelType type = level.LevelType;
            int index = level.Index;
            switch (type)
            {
                case LevelType.Normal:
                    param = new Parameter[3];
                    param[0] = new Parameter("level", index);
                    param[1] = new Parameter("retry", DataManager.Ins.GameData.user.retryTime);
                    param[2] = new Parameter("duration", GameplayManager.Ins.GameDuration);
                    break;
                case LevelType.DailyChallenge:
                    param = new Parameter[2];
                    param[0] = new Parameter("level_dailyChallenge", index);
                    param[1] = new Parameter("duration", GameplayManager.Ins.GameDuration);
                    break;
                case LevelType.Secret:
                    param = new Parameter[2];
                    param[0] = new Parameter("level_expedition", index);
                    param[1] = new Parameter("duration", GameplayManager.Ins.GameDuration);
                    break;
            }

            FirebaseAnalytics.LogEvent("level_complete", param);
        }

        public void LevelFail(Level level, LevelLoseCondition loseCondition)
        {
            if (!isFirebaseInit) return;

            Parameter[] param = new Parameter[3];
            LevelType type = level.LevelType;
            int index = level.Index;
            
            switch (type)
            {
                case LevelType.Normal:
                    param[0] = new Parameter("level", index);
                    param[1] = new Parameter("duration", GameplayManager.Ins.GameDuration);
                    param[2] = new Parameter("reason", loseCondition.ToString());
                    break;
                case LevelType.DailyChallenge:
                    param[0] = new Parameter("level_dailyChallenge", index);
                    param[1] = new Parameter("duration", GameplayManager.Ins.GameDuration);
                    param[2] = new Parameter("reason", loseCondition.ToString());
                    break;
                case LevelType.Secret:
                    param[0] = new Parameter("level_expedition", index);
                    param[1] = new Parameter("duration", GameplayManager.Ins.GameDuration);
                    param[2] = new Parameter("reason", loseCondition.ToString());
                    break;
            }

            FirebaseAnalytics.LogEvent("level_fail", param);
        }

        public void BoosterSpend(BoosterType type)
        {
            if (!isFirebaseInit) return;

            FirebaseAnalytics.LogEvent("booster_spend", "name", type.ToString());
        }

        public void RewardAdsComplete(Ads.Placement place)
        {
            if (!isFirebaseInit) return;

            FirebaseAnalytics.LogEvent("ads_reward_complete", "placement", place.ToString());
            CheckFirstAdsSession(place);           
        }

        public void InterAdsShow(Ads.Placement place)
        {
            if (!isFirebaseInit) return;

            FirebaseAnalytics.LogEvent("ad_inter_show", "placement", place.ToString());
            CheckFirstAdsSession(place);
        }

        private void CheckFirstAdsSession(Ads.Placement place)
        {
            if (!isFirebaseInit) return;

            if (!_firstAdsSession)
            {
                _firstAdsSession = true;
                FirebaseAnalytics.LogEvent("first_ads_session", new Parameter[]
                {
                    new Parameter("placement", place.ToString()),
                    new Parameter("duration", _firstAdsSessionTime)
                });
                _firstAdsSessionTimer.Stop();
            }
        }
        public void ResourceEarn(BoosterType type,Resource.Placement place, int amount)
        {
            if (!isFirebaseInit) return;

            FirebaseAnalytics.LogEvent("resource_earn", new Parameter[]
            {
                new Parameter("name", type.ToString()),
                new Parameter("placement", place.ToString()),
                new Parameter("value", amount)
            });
        }

        public void ResourceEarn(CurrencyType type, Resource.Placement place, int amount)
        {
            if (!isFirebaseInit) return;

            FirebaseAnalytics.LogEvent("resource_earn", new Parameter[]
            {
                new Parameter("name", type.ToString()),
                new Parameter("placement", place.ToString()),
                new Parameter("value", amount)
            });
        }

        //public void ResourceSpend(BoosterType type, Resource.Placement place, int amount)
        //{
        //    if (!isFirebaseInit) return;

        //    FirebaseAnalytics.LogEvent("resource_spend", new Parameter[]
        //    {
        //        new Parameter("name", type.ToString()),
        //        new Parameter("placement", place.ToString()),
        //        new Parameter("value", amount)
        //    });
        //}

        public void ResourceSpend(CurrencyType type, Resource.Placement place, int amount)
        {
            if (!isFirebaseInit) return;

            FirebaseAnalytics.LogEvent("resource_spend", new Parameter[]
            {
                new Parameter("name", type.ToString()),
                new Parameter("placement", place.ToString()),
                new Parameter("value", amount)
            });
        }

        public void FireUserProps()
        {
            if (!isFirebaseInit) return;

            FirebaseAnalytics.LogEvent("UserProps","Level",DataManager.Ins.GameData.user.normalLevelIndex);
        }

        public void Day()
        {
            if (!isFirebaseInit) return;

            FirebaseAnalytics.LogEvent("Day", "day", DataManager.Ins.GameData.user.playedDay);
        }
        public void AppsFlyerTrackEvent(string name)
        {
            //Dictionary<string, string> eventValue = new Dictionary<string, string>();
            //eventValue.Add("af_quantity", "1");
            AppsFlyer.sendEvent(name, null);
        }

        public void AppsFlyerTrackParamEvent(string name, Dictionary<string, string> param)
        {
            AppsFlyer.sendEvent(name, param);
        }
    }
}