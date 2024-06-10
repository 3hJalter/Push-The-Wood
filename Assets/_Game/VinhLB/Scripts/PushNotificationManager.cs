using System;
using _Game.DesignPattern;
using _Game.Utilities;
using UnityEngine;

namespace VinhLB
{
    public class PushNotificationManager : Singleton<PushNotificationManager>
    {
        private void Awake()
        {
            DontDestroyOnLoad(this);
            
            GleyNotifications.Initialize();
        }

        private void OnApplicationQuit()
        {
            CheckDailyReward();
        }

        private void OnApplicationFocus(bool focus)
        {
            // DevLog.Log(DevId.Vinh, $"OnApplicationFocus: {focus}");
            if (focus == false)
            {
                // If user left your app schedule all your notifications
                // CheckDailyReward();
            }
            else
            {
                // Call initialize when user returns to your app to cancel all pending notifications
                GleyNotifications.Initialize();
            }
        }

        private void CheckDailyReward()
        {
            if (!DailyRewardManager.Ins.IsTodayRewardObtained)
            {
                TimeSpan timeSpan = new TimeSpan(0, 5, 0);
                GleyNotifications.SendNotification(Constants.GAME_TITLE, "Come back to receive daily gift today!", timeSpan);
            }
            else
            {
                TimeSpan timeSpan = DateTime.UtcNow.AddHours(24).Date - DateTime.UtcNow;
                GleyNotifications.SendNotification(Constants.GAME_TITLE, "It's a new day! Come back to receive daily gift!", timeSpan);
            }
        }
    }
}