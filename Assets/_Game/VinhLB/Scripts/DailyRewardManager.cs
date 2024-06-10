using System;
using System.Collections.Generic;
using _Game.DesignPattern;
using _Game.Managers;
using _Game.Utilities;
using UnityEngine;

namespace VinhLB
{
    public class DailyRewardManager : Singleton<DailyRewardManager>
    {
        public event Action OnDailyRewardParamsChanged;

        [SerializeField]
        private DailyRewardSettings _dailyRewardSettings;

        #region Properties
        public DailyRewardSettings DailyRewardSettings => _dailyRewardSettings;
        public bool IsTodayRewardObtained =>
            DateTime.UtcNow.Date <= DataManager.Ins.GameData.user.lastDailyRewardClaimDate;
        public int TotalDays => _dailyRewardSettings.MissRewardIfNotLogin
            ? (DateTime.UtcNow.Date - DataManager.Ins.GameData.user.startDailyRewardClaimDate.Date).Days
            : (DataManager.Ins.GameData.user.dailyRewardClaimedCount - (IsTodayRewardObtained ? 1 : 0));
        public int CycleDay => TotalDays % _dailyRewardSettings.CycleDays;
        public bool IsInFirstCycle => TotalDays / _dailyRewardSettings.CycleDays == 0;
        public List<Reward[]> Rewards => IsInFirstCycle && _dailyRewardSettings.DifferentFirstCycle
            ? _dailyRewardSettings.FirstCycleRewardList
            : _dailyRewardSettings.RewardsList;
        public Reward[] CurrentRewards => Rewards[CycleDay];
        #endregion

        private void Awake()
        {
            DontDestroyOnLoad(this);
        }

        public void ObtainTodayReward()
        {
            DevLog.Log(DevId.Vinh, $"Obtain day {CycleDay + 1} reward");
            UIManager.Ins.OpenUI<RewardPopup>(CurrentRewards);
            
            DataManager.Ins.GameData.user.dailyRewardClaimedCount += 1;
            DataManager.Ins.GameData.user.lastDailyRewardClaimDate = DateTime.UtcNow.Date;
            DataManager.Ins.Save();

            OnDailyRewardParamsChanged?.Invoke();
            
            GameManager.Ins.PostEvent(EventID.OnUpdateUI);
        }

        private void SetPlayerPrefsDateTime(string key, DateTime dateTime)
        {
            string dateTimeString = dateTime.ToShortDateString();
            PlayerPrefs.SetString(key, dateTimeString);
        }

        private DateTime GetPlayerPrefsDateTime(string key, DateTime defaultDateTime = default)
        {
            string dateTimeString = PlayerPrefs.GetString(key);
            if (!DateTime.TryParse(dateTimeString, out DateTime dateTime))
            {
                dateTime = defaultDateTime;
            }

            return dateTime;
        }

        #region Debug
// #if UNITY_EDITOR
        // [UnityEditor.MenuItem("Debug/Daily Reward/Print Parameters")]
        public static void PrintParameters()
        {
            DevLog.Log(DevId.Vinh,
                $"StartDailyRewardDateTime: {DataManager.Ins.GameData.user.startDailyRewardClaimDate}");
            DevLog.Log(DevId.Vinh,
                $"DailyRewardCount: {DataManager.Ins.GameData.user.dailyRewardClaimedCount}");
            DevLog.Log(DevId.Vinh,
                $"LastDailyRewardDateTime: {DataManager.Ins.GameData.user.lastDailyRewardClaimDate}");

            if (Application.isPlaying)
            {
                DevLog.Log(DevId.Vinh, $"IsInFirstCycle: {Ins.IsInFirstCycle}");
                DevLog.Log(DevId.Vinh, $"TotalDays: {Ins.TotalDays}");
                DevLog.Log(DevId.Vinh, $"CycleDay: {Ins.CycleDay}");
                DevLog.Log(DevId.Vinh, $"IsTodayRewardObtained: {Ins.IsTodayRewardObtained}");
            }
        }

        // [UnityEditor.MenuItem("Debug/Daily Reward/Set Can Collect Today")]
        public static void SetCanCollectToday()
        {
            DataManager.Ins.GameData.user.lastDailyRewardClaimDate = DateTime.UtcNow.AddHours(-24);
            DataManager.Ins.Save();

            if (Application.isPlaying)
            {
                Ins.OnDailyRewardParamsChanged?.Invoke();
            }
        }

        // [UnityEditor.MenuItem("Debug/Daily Reward/Increase 1 Daily Day")]
        public static void Increase1DailyDay()
        {
            if (Ins._dailyRewardSettings.MissRewardIfNotLogin)
            {
                DataManager.Ins.GameData.user.startDailyRewardClaimDate =
                    DataManager.Ins.GameData.user.startDailyRewardClaimDate.AddDays(-1);
                DataManager.Ins.Save();

                DevLog.Log(DevId.Vinh,
                    $"StartDailyRewardDateTime: {DataManager.Ins.GameData.user.startDailyRewardClaimDate}");
            }
            else
            {
                DataManager.Ins.GameData.user.dailyRewardClaimedCount += 1;
                DevLog.Log(DevId.Vinh, $"DailyRewardCount: {DataManager.Ins.GameData.user.dailyRewardClaimedCount}");
            }

            if (Application.isPlaying)
            {
                Ins.OnDailyRewardParamsChanged?.Invoke();
            }
        }

        // [UnityEditor.MenuItem("Debug/Daily Reward/Decrease 1 Daily Day")]
        public static void Decrease1DailyDay()
        {
            if (Ins._dailyRewardSettings.MissRewardIfNotLogin)
            {
                if (DataManager.Ins.GameData.user.startDailyRewardClaimDate.AddDays(1) <= DateTime.UtcNow.Date)
                {
                    // Make sure start date is not after today
                    DataManager.Ins.GameData.user.startDailyRewardClaimDate =
                        DataManager.Ins.GameData.user.startDailyRewardClaimDate.AddDays(1);
                    DataManager.Ins.Save();
                }

                DevLog.Log(DevId.Vinh,
                    $"StartDailyRewardDateTime: {DataManager.Ins.GameData.user.startDailyRewardClaimDate}");
            }
            else
            {
                DataManager.Ins.GameData.user.dailyRewardClaimedCount =
                    Mathf.Max(0, DataManager.Ins.GameData.user.dailyRewardClaimedCount - 1);
                DevLog.Log(DevId.Vinh, $"DailyRewardCount: {DataManager.Ins.GameData.user.dailyRewardClaimedCount}");
            }

            if (Application.isPlaying)
            {
                Ins.OnDailyRewardParamsChanged?.Invoke();
            }
        }

        // [UnityEditor.MenuItem("Debug/Daily Reward/Reset All")]
        public static void ResetAll()
        {
            DataManager.Ins.GameData.user.startDailyRewardClaimDate = DateTime.UtcNow.Date;
            DataManager.Ins.GameData.user.dailyRewardClaimedCount = 0;
            DataManager.Ins.GameData.user.lastDailyRewardClaimDate = DateTime.UtcNow.AddHours(-24);
            DataManager.Ins.Save();
            
            if (Application.isPlaying)
            {
                Ins.OnDailyRewardParamsChanged?.Invoke();
            }
        }
// #endif
        #endregion
    }
}