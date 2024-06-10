using System;
using System.Collections.Generic;
using _Game.Managers;
using _Game.Resource;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using VinhLB;

namespace _Game.Data
{
    [CreateAssetMenu(fileName = "ConfigData", menuName = "ScriptableObjects/ConfigData", order = 1)]
    public class ConfigData : SerializedScriptableObject
    {
        #region Feature Unlock
        [FoldoutGroup("Feature Unlock")]
        public readonly int unlockGoMainMenuOnLoseAtLevelIndex = 5;
        [FoldoutGroup("Feature Unlock")]
        public readonly int unlockBonusChestAtLevelIndex = 5;
        [FoldoutGroup("Feature Unlock")]
        public readonly int unlockDailyChallengeAtLevelIndex = 9;
        [FoldoutGroup("Feature Unlock")]
        public readonly int unlockSecretLevelAtLevelIndex = 19;
        #endregion

        #region In Game
        [FoldoutGroup("In Game")]
        // Time per level
        [FoldoutGroup("In Game")]
        public int maxHeart = 5;
        [FoldoutGroup("In Game")]
        public int regenHeartTime = 900;
        [FoldoutGroup("In Game")]
        public int goldToBuyHeart = 600;

        [FoldoutGroup("In Game/Time Per Level/Normal")]
        [InfoBox("Do not change the order of the list: Easy -> Medium -> Hard")]
        public List<TimePerNormalLevel> timePerNormalLevel = new()
        {
            new TimePerNormalLevel() { levelNormalType = LevelNormalType.Easy, time = 300 },
            new TimePerNormalLevel() { levelNormalType = LevelNormalType.Medium, time = 600 },
            new TimePerNormalLevel() { levelNormalType = LevelNormalType.Hard, time = 900 },
        };
        [FoldoutGroup("In Game/Time Per Level")]
        public readonly int timePerDailyChallengeLevel = 900;
        [FoldoutGroup("In Game/Time Per Level")]
        public readonly int timePerSecretLevel = 900;
        [FoldoutGroup("In Game/Reward Chest")]
        public readonly int requireRewardKey = 3;
        [FoldoutGroup("In Game/Reward Chest")]
        public readonly BoosterType[] RCRewards;
        [FoldoutGroup("In Game/Reward Chest")]
        public readonly int[] RCQuantitys;
        [FoldoutGroup("In Game/Reward Chest")]
        public readonly float[] RCRates;

        [FoldoutGroup("In Game/Level Chest")]
        public readonly int requireLevelProgress = 3;
        [FoldoutGroup("In Game/Level Chest")]
        public readonly CurrencyType[] LCRewards;
        [FoldoutGroup("In Game/Level Chest")]
        public readonly int[] LCQuantitys;
        [FoldoutGroup("In Game/Level Chest")]
        public readonly float[] LCRates;
        [FoldoutGroup("In Game/Secret Level")]
        public readonly int requireSecretMapPiece = 8;
        [FoldoutGroup("In Game/Characters")]
        public readonly int[] CharacterCosts;
        [FoldoutGroup("In Game/Time")]
        public readonly int DangerTime = 30;
        [FoldoutGroup("In Game/MoreTimeAdded")]
        public readonly int MoreTimeAdded = 60;

        // [FoldoutGroup("In Game/Reward")] public readonly int finalPointGoldReward = 20;
        [FoldoutGroup("In Game/Reward")] public readonly int bonusChestGoldReward = 40;
        // [FoldoutGroup("In Game/Reward")] public readonly int finalEnemyGoldReward = 40;
        
        [FoldoutGroup("In Game/Reward")] public readonly int easyLevelGoldReward = 20;
        [FoldoutGroup("In Game/Reward")] public readonly int mediumLevelGoldReward = 40;
        [FoldoutGroup("In Game/Reward")] public readonly int hardLevelGoldReward = 80;
        [FoldoutGroup("In Game/Reward")] public readonly int dailyChallengeGoldReward = 40;
        [FoldoutGroup("In Game/Reward")] public readonly int secretLevelGoldReward = 200;
        #endregion

        #region Daily Challenge Reward
        [FoldoutGroup("Daily Challenge Reward")]
        // Note: Add a theme resource when have, and make player take it the first time player clear full 7 days
        public List<DailyChallengeRewardMilestone> dailyChallengeRewardMilestones = new();
        #endregion

        #region Booster Purchase
        // Ticket purchase
        [FoldoutGroup("Booster Purchase")]
        [InfoBox("Do not change the order of the list, check order in BoosterType enum")]
        public List<BoosterConfig> boosterConfigList = new();
        #endregion

        #region Ads
        [FoldoutGroup("Ads")]
        public readonly int stepInterAdsCountMax = 3;
        [FoldoutGroup("Ads")]
        public readonly int interAdsCooldownTime = 45;
        [FoldoutGroup("Ads")]
        public readonly int startInterAdsLevel = 8;
        [FoldoutGroup("Ads")]
        public readonly int startBannerAds = 8;
        [FoldoutGroup("Ads")]
        public readonly int winLevelCountInterAds = 3;
        [FoldoutGroup("Ads")]
        public int bannerHeight = 100;
        [FoldoutGroup("Ads")]
        public int reloadBannerTime = 120;
        [FoldoutGroup("Ads")]
        public int interAdsCappingTime = 150;
        #endregion

        #region Collection
        [FoldoutGroup("Collection")]
        public readonly int maxRentCount = 3;

        #endregion

        // [BoxGroup("Monetize")]

        public BoosterConfig GetBoosterConfig(BoosterType boosterType)
        {
            return boosterConfigList.Find(x => x.Type == boosterType);
        }
    }

    [Serializable]
    public struct DailyChallengeRewardMilestone
    {
        public int clearLevelNeed;
        public bool hasFirstReward;
        [ShowIf(nameof(hasFirstReward))]
        public Reward[] firstRewards;
        public Reward[] rewards;
    }

    [Serializable]
    public struct DailyChallengeReward
    {
        public CurrencyType currencyType;
        public int quantity;

        public void GetReward(object param)
        {
            switch (currencyType)
            {
                case CurrencyType.Gold:
                    GameManager.Ins.GainGold(quantity, param);
                    break;
                case CurrencyType.Heart:
                    GameManager.Ins.GainHeart(quantity, param);
                    break;
                case CurrencyType.RandomBooster:
                    break;
                case CurrencyType.None:
                case CurrencyType.SecretMapPiece:
                default:
                    break;
            }
        }
    }

    [Serializable]
    public struct BoosterConfig
    {
        [SerializeField]
        private BoosterType type;
        [SerializeField]
        private int unlockAtLevel;
        [SerializeField]
        private GoldPerBuyRatio goldPerBuyRatio;

        [HideInInspector]
        public UIResourceConfig UIResourceConfig;

        public BoosterType Type => type;
        public int UnlockAtLevel => unlockAtLevel;
        public string Name => UIResourceConfig.Name;
        public Sprite MainIcon => UIResourceConfig.MainIconSprite;
        public GoldPerBuyRatio GoldPerBuyRatio => goldPerBuyRatio;
        // Do with goldPerBuyMore
    }

    [Serializable]
    public record TimePerNormalLevel
    {
        public LevelNormalType levelNormalType;
        public int time;
    }

    [Serializable]
    public struct GoldPerBuyRatio
    {
        public GoldPerBuyRatio(int first, int second)
        {
            goldNeed = first;
            itemsPerBuy = second;
        }

        public int goldNeed;
        public int itemsPerBuy;
    }
}