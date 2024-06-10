using System;
using System.Collections.Generic;
using _Game.Utilities;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Game.Data
{
    [Serializable]
    public class GameData
    {
        public SettingData setting = new();
        public UserData user = new();

        [Serializable]
        public class UserData
        {
            public const int PLAYER_SKIN_COUNT = 6;
            // Level Progress Data
            public int normalLevelIndex;
           
            
            public List<int> completedOneTimeTutorial = new();
            
            // daily challenge
            public int currentDay;
            public int daysInMonth;
            public int[] dailyLevelIndex = {
                1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12,
                13, 14, 15, 16, 17, 18, 19, 20, 21, 22,
                23, 24, 25, 26, 27, 28, 29, 30, 31
            };
            public List<int> dailyLevelIndexComplete = new();
            public List<int> dailyChallengeRewardCollected = new(); 
            public List<int> isCollectDailyChallengeRewardOneTime = new();
            public bool isFreeDailyChallengeFirstTime = true;
            public bool isOpenInGameDailyChallengeTut;
            
            // secret level
            public int secretLevelUnlock;
            public List<int> secretLevelIndexComplete = new();
            
            // daily reward            
            public int dailyRewardClaimedCount;
            public DateTime startDailyRewardClaimDate = DateTime.UtcNow.Date;
            public DateTime lastDailyRewardClaimDate = DateTime.UtcNow.Date.AddHours(-24);
            
            // Income Progress Data
            public int gold;
            public int heart = 5;
            public int heartRemaningTime;
            public int heartStopCountingSecond;
            public int rewardChestKeys;
            public int levelChestProgress;
            public int currentRewardChestIndex = 0;
            public int currentLevelChestIndex = 0;
            public int rewardChestUnlock = 0;
            public int levelChestUnlock = 0;
            public int secretMapPieces = 7;

            // Booster Data
            public int undoCount = 10;
            public int resetIslandCount;
            public int growTreeCount = 3;
            public int pushHintCount = 1;
            
            // Purchase & First rate Data
            public bool purchasedNoAds;
            public bool rated;

            // TODO: Shop data
            public int currentPlayerSkinIndex = 0;
            public int currentUnlockPlayerSkinIndex = 0;
            public int[] playerSkinState = new int[PLAYER_SKIN_COUNT];
            public int[] playerRentSkinState = new int[PLAYER_SKIN_COUNT] {-1, -1, -1, -1, -1, -1};
            
            //Other Data
            public int playedDay = 0;
            public int sessionPlayed;
            public int retryTime;
            
            public DateTime lastTimeLogOut = DateTime.MinValue;
            
            public bool isFirstDayOfWeekCheck;

            //Ads Data
            public int hintAdsCount;
            public int interAdsStepCount = 0;
        }

        [Serializable]
        public class SettingData
        {
            public bool enablePn;
            public bool requestedPn;
            public bool haptic = true;
            
            public bool isBgmMute;
            public bool isSfxMute;
            public bool isEnvSoundMute;
            
            public int highPerformance = 1;
            public bool iOsTrackingRequested;
            public int moveChoice = 0;
        }
    }

    public static class Database
    {
        private const string DATA_KEY = "GameData";

        public static void SaveData(GameData data)
        {
            string dataString = JsonConvert.SerializeObject(data);
            PlayerPrefs.SetString(DATA_KEY, dataString);
            PlayerPrefs.Save();
        }

        public static GameData LoadData()
        {

            if (PlayerPrefs.HasKey(DATA_KEY))
            {
                return JsonConvert.DeserializeObject<GameData>(PlayerPrefs.GetString(DATA_KEY));
            }
            // If no game data can be loaded, create new one
            DevLog.Log(DevId.Hoang, "No game data can be loaded, create new one");
            GameData gameData = new();
            SaveData(gameData);
            return gameData;
        }
    }
}
