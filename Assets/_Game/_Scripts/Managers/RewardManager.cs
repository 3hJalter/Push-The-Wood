using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Game.Managers
{
    using _Game._Scripts.Utilities;
    using _Game.Data;
    using _Game.DesignPattern;
    using _Game.Resource;
    using System;
    using VinhLB;

    public class RewardManager : Singleton<RewardManager>
    {
        [SerializeField]
        HomeReward homeReward;
        public HomeReward HomeReward => homeReward;
        private void Awake()
        {
            homeReward = new HomeReward(DataManager.Ins.GameData);
            DontDestroyOnLoad(gameObject);
        }
    }

    [Serializable]
    public class HomeReward
    {
        private GameData gameData;
        
        public HomeReward(GameData gameData)
        {
            this.gameData = gameData;
        }
        
        public bool IsCanClaimRC => gameData.user.currentRewardChestIndex < gameData.user.rewardChestUnlock;
        public bool IsCanClaimLC => gameData.user.currentLevelChestIndex < gameData.user.levelChestUnlock;
        
        public bool TryClaimRewardChest()
        {
            if (IsCanClaimRC)
            {
                gameData.user.currentRewardChestIndex += 1;
                UIManager.Ins.OpenUI<RewardPopup>(GetRCReward());
                GameManager.Ins.PostEvent(EventID.OnClaimRewardChest, gameData.user.currentRewardChestIndex - 1);
                GameManager.Ins.PostEvent(EventID.OnUpdateUI);

                return true;
            }
            
            return false;
        }

        public bool TryClaimLevelChest()
        {
            if (IsCanClaimLC)
            {
                gameData.user.currentLevelChestIndex += 1;
                UIManager.Ins.OpenUI<RewardPopup>(GetLCReward());
                GameManager.Ins.PostEvent(EventID.OnClaimLevelChest, gameData.user.currentLevelChestIndex - 1);
                GameManager.Ins.PostEvent(EventID.OnUpdateUI);
                
                return true;
            }
            
            return false;
        }

        private Reward[] GetRCReward()
        {
            Reward[] rewards = new Reward[] { new Reward() };
            int index = HUtilities.WheelRandom(DataManager.Ins.ConfigData.RCRates);
            BoosterType type = DataManager.Ins.ConfigData.RCRewards[index];
            rewards[0].BoosterType = type;
            rewards[0].RewardType = RewardType.Booster;
            rewards[0].Amount = DataManager.Ins.ConfigData.RCQuantitys[index];
            return rewards;
        }

        private Reward[] GetLCReward()
        {
            Reward[] rewards = new Reward[] { new Reward() };
            int index = HUtilities.WheelRandom(DataManager.Ins.ConfigData.LCRates);
            CurrencyType type = DataManager.Ins.ConfigData.LCRewards[index];
            rewards[0].CurrencyType = type;
            rewards[0].RewardType = RewardType.Currency;
            switch (type)
            {
                case CurrencyType.Gold:
                    rewards[0].Amount = UnityEngine.Random.Range(100, 201);
                    break;
                case CurrencyType.Heart:
                    rewards[0].Amount = DataManager.Ins.ConfigData.LCQuantitys[index];
                    break;
            }
            return rewards;
        }
    }
}