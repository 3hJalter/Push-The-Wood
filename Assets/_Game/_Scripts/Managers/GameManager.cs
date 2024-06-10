using System;
using _Game._Scripts.Utilities;
using _Game.Data;
using _Game.DesignPattern;
using _Game.Resource;
using _Game.Utilities;
using _Game.Utilities.Timer;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Playables;
using VinhLB;

namespace _Game.Managers
{
    public enum GameState
    {
        MainMenu,
        InGame,
        WorldMap,
        Pause,
        Transition,
        WinGame,
        LoseGame
    }

    [DefaultExecutionOrder(-95)]
    public class GameManager : Dispatcher<GameManager>
    {
        [SerializeField] private GameState gameState;

        [SerializeField] private bool reduceScreenResolution;

        private GameData _gameData;
        private STimer heartTimer;
        private int currentRegenHeartTime;
        public int CurrentRegenHeartTime => currentRegenHeartTime;
        public float ReduceRatio { get; private set; }
        public bool IsReduce { get; private set; }

        private void Awake()
        {
            Input.multiTouchEnabled = false;
            Application.targetFrameRate = 60;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            DontDestroyOnLoad(gameObject);

            if (reduceScreenResolution)
            {
                const int maxScreenHeight = 1280;
                float ratio = Screen.currentResolution.width / (float)Screen.currentResolution.height;
                if (Screen.currentResolution.height > maxScreenHeight)
                {
                    IsReduce = true;
                    int newScreenWidth = Mathf.RoundToInt(ratio * maxScreenHeight);
                    ReduceRatio = Screen.currentResolution.width / (float)newScreenWidth;
                    Screen.SetResolution(newScreenWidth, maxScreenHeight, true);
                }
            }
            heartTimer = TimerManager.Ins.PopSTimer();
            VerifyGameData();
        }

        private void VerifyGameData()
        {
            _gameData = DataManager.Ins.GameData;
            if (_gameData.user.normalLevelIndex == 0) //NOTE: First time run game
            {
                DataManager.Ins.SetUnlockCharacterSkin(0, true);
                DataManager.Ins.SetCharacterSkinIndex(0);
            }

            #region Handle modified Daily Challenge

            if (_gameData.user.dailyLevelIndex.Length < 31) // MAXIMUM number of days in month{
            {
                _gameData.user.dailyLevelIndex = new[]
                {
                    1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12,
                    13, 14, 15, 16, 17, 18, 19, 20, 21, 22,
                    23, 24, 25, 26, 27, 28, 29, 30, 31
                };
                Database.SaveData(_gameData);
            }
            #endregion

            #region Handle day online         
            _gameData.user.currentDay = DateTime.UtcNow.Day;
            _gameData.user.daysInMonth = DateTime.DaysInMonth(DateTime.UtcNow.Year, DateTime.UtcNow.Month);

            // Get now day & last login day
            int day = (int)(DateTime.UtcNow.Date - _gameData.user.lastTimeLogOut.ToUniversalTime().Date)
                .TotalDays;
            // get how many month pass since last login
            int month = (DateTime.UtcNow.Year - _gameData.user.lastTimeLogOut.Year) * 12 +
                        DateTime.UtcNow.Month - _gameData.user.lastTimeLogOut.Month;

            #region Handle heart time
            InitHeart();          
            #endregion

            if (day > 0)
            {
                _gameData.user.playedDay += 1;
                _gameData.user.isFreeDailyChallengeFirstTime = true;
                AnalysticManager.Ins.Day();
            }

            if (month > 0)
            {
                _gameData.user.dailyLevelIndexComplete.Clear();
                _gameData.user.dailyChallengeRewardCollected.Clear();
                _gameData.user.dailyLevelIndex.Shuffle();
                Database.SaveData(_gameData);
            }

            #endregion

            SmoothHeart = Heart;
            SmoothGold = Gold;
            SmoothRewardKeys = CanClaimRewardChest ? DataManager.Ins.ConfigData.requireRewardKey : RewardKeys;
            SmoothLevelProgress = CanClaimLevelChest ? DataManager.Ins.ConfigData.requireLevelProgress : LevelProgress;
        }

        #region Game State Handling

        public void ChangeState(GameState gameStateI)
        {
            if (gameStateI == GameState.Pause)
            {
                DOTween.PauseAll();
                PostEvent(EventID.Pause);
            }

            if (gameState == GameState.Pause && gameStateI != GameState.Pause)
            {
                DOTween.PlayAll();
                PostEvent(EventID.UnPause);
                PostEvent(EventID.UnPause, gameStateI);
            }
            gameState = gameStateI;
            PostEvent(EventID.OnChangeGameState, gameStateI);
        }

        public bool IsState(GameState gameStateI)
        {
            return gameState == gameStateI;
        }

        #endregion

        #region Income Data function

        public int SecretLevelUnlock => _gameData.user.secretLevelUnlock;
        public int SecretMapPieces => _gameData.user.secretMapPieces;
        public int Gold => _gameData.user.gold;
        public int Heart => _gameData.user.heart;
        public int RewardKeys => _gameData.user.rewardChestKeys;
        public int LevelProgress => _gameData.user.levelChestProgress;
        public float SmoothGold { get; set; }
        public float SmoothHeart { get; set; }
        public float SmoothRewardKeys { get; set; }
        public float SmoothLevelProgress { get; set; }
        public bool CanClaimRewardChest => _gameData.user.currentRewardChestIndex < _gameData.user.rewardChestUnlock;
        public bool CanClaimLevelChest => _gameData.user.currentLevelChestIndex < _gameData.user.levelChestUnlock;

        public void GainBooster(BoosterType type, int amount)
        {
            switch (type)
            {
                case BoosterType.Undo:
                    _gameData.user.undoCount += amount;
                    break;
                case BoosterType.PushHint:
                    _gameData.user.pushHintCount += amount;
                    break;
                case BoosterType.GrowTree:
                    _gameData.user.growTreeCount += amount;
                    break;
                case BoosterType.ResetIsland:
                    _gameData.user.resetIslandCount += amount;
                    break;
            }

            PostEvent(EventID.OnUpdateUI);
            Database.SaveData(_gameData);
        }

        public void GainGold(int value, object source = null)
        {
            TryModifyGold(value, source);
        }
        private void InitHeart()
        {
            int day = (int)(DateTime.UtcNow.Date - _gameData.user.lastTimeLogOut.ToUniversalTime().Date)
                .TotalDays;
            if (_gameData.user.heartRemaningTime < 0)
                _gameData.user.heartRemaningTime = 0;
            int offlineSecond = (int)DateTime.UtcNow.TimeOfDay.TotalSeconds + _gameData.user.heartRemaningTime;
            if (day > 0)
            {
                offlineSecond += (day - 1) * 3600 * 24 + 86400 - _gameData.user.heartStopCountingSecond;
            }
            else
            {
                offlineSecond -= _gameData.user.heartStopCountingSecond;
            }
            int regenHeart = offlineSecond / DataManager.Ins.ConfigData.regenHeartTime;
            GainHeart(regenHeart);
            if(Heart >= DataManager.Ins.ConfigData.maxHeart)
            {
                currentRegenHeartTime = 0;
            }
            else 
            {
                currentRegenHeartTime = offlineSecond % DataManager.Ins.ConfigData.regenHeartTime;
            }
        }
        public void GainHeart(int value, object source = null)
        {
            TryModifyHeart(value, source);
            CheckRegenHeartTimer();
        }
        private void CheckRegenHeartTimer()
        {
            if(Heart >= DataManager.Ins.ConfigData.maxHeart)
            {
                heartTimer.Stop();
                currentRegenHeartTime = 0;
                _gameData.user.heartRemaningTime = 0;
                PostEvent(EventID.OnHeartTimeChange, -1);
            }
            else
            {               
                if (!heartTimer.IsStart)
                    heartTimer.Start(1, UpdateHeartCount, true);
            }
            
            void UpdateHeartCount()
            {
                if (currentRegenHeartTime < 0)
                    currentRegenHeartTime = 0;

                currentRegenHeartTime += 1;
                PostEvent(EventID.OnHeartTimeChange, currentRegenHeartTime);
                DevLog.Log(DevId.Hung,$"Update Timer: {currentRegenHeartTime}" );

                if (currentRegenHeartTime >= DataManager.Ins.ConfigData.regenHeartTime)
                {
                    currentRegenHeartTime -= DataManager.Ins.ConfigData.regenHeartTime;
                    GainHeart(1);
                }
            }
        }
        
        public void GainRewardKeys(int amount, object source = null)
        {
            // _gameData.user.rewardChestKeys += amount;
            // if(_gameData.user.rewardChestKeys >= DataManager.Ins.ConfigData.requireRewardKey)
            // {
            //     _gameData.user.rewardChestUnlock += _gameData.user.rewardChestKeys / DataManager.Ins.ConfigData.requireRewardKey;
            //     _gameData.user.rewardChestKeys = _gameData.user.rewardChestKeys % DataManager.Ins.ConfigData.requireRewardKey;
            //     //PostEvent(EventID.OnUnlockRewardChest, _gameData.user.rewardChestUnlock);
            // }
            // //PostEvent(EventID.OnRewardChestKeyChange, _gameData.user.rewardChestKeys);
            // PostEvent(EventID.OnUpdateUIs);
            // Database.SaveData(_gameData);

            TryModifyRewardKeys(amount, source);
        }

        public void GainLevelProgress(int amount, object source = null)
        {
            // _gameData.user.levelChestProgress += amount;
            // if (_gameData.user.levelChestProgress >= DataManager.Ins.ConfigData.requireLevelProgress)
            // {
            //     _gameData.user.levelChestUnlock += _gameData.user.levelChestProgress /
            //                                        DataManager.Ins.ConfigData.requireLevelProgress;
            //     _gameData.user.levelChestProgress = _gameData.user.levelChestProgress %
            //                                         DataManager.Ins.ConfigData.requireLevelProgress;
            // }
            // PostEvent(EventID.OnUpdateUIs);
            // Database.SaveData(_gameData);

            TryModifyLevelProgress(amount, source);
        }

        public void GainSecretMapPiece(int amount)
        {
            _gameData.user.secretMapPieces += amount;
            if (_gameData.user.secretMapPieces >= DataManager.Ins.ConfigData.requireSecretMapPiece)
            {
                _gameData.user.secretLevelUnlock +=
                    _gameData.user.secretMapPieces / DataManager.Ins.ConfigData.requireSecretMapPiece;
                _gameData.user.secretMapPieces %= DataManager.Ins.ConfigData.requireSecretMapPiece;
                //PostEvent(EventID.OnUnlockSecretMap, _gameData.user.secretLevelUnlock);
            }

            //PostEvent(EventID.OnSecretMapPieceChange, _gameData.user.secretMapPieces);
            PostEvent(EventID.OnUpdateUI);
            Database.SaveData(_gameData);
        }

        public bool TrySpendGold(int amount, object source = null)
        {
            return TryModifyGold(-amount, source);
        }

        public bool TrySpendHeart(int amount, object source = null)
        {
            return TryModifyHeart(-amount, source);
        }

        public void ResetUserData()
        {
            TrySpendGold(_gameData.user.gold);
            TrySpendHeart(_gameData.user.heart);

            _gameData.user.secretLevelUnlock = 0;
        }

        private bool TryModifyGold(int amount, object source)
        {
            if (_gameData.user.gold + amount < 0) return false;

            #region ANALYSTIC

            if (amount > 0)
                AnalysticManager.Ins.ResourceEarn(CurrencyType.Gold, Placement.None, amount);
            else if (amount < 0) AnalysticManager.Ins.ResourceSpend(CurrencyType.Gold, Placement.None, amount);

            #endregion

            ResourceChangeData data = new()
            {
                ChangedAmount = amount,
                OldValue = _gameData.user.gold,
                NewValue = _gameData.user.gold + amount,
                Source = source
            };

            _gameData.user.gold += amount;
            PostEvent(EventID.OnChangeGold, data);
            Database.SaveData(_gameData);

            if (amount < 0) SmoothGold = _gameData.user.gold;

            return true;
        }

        private bool TryModifyHeart(int amount, object source)
        {
            int newValue = _gameData.user.heart + amount;

            if (newValue < 0) return false;

            if(newValue > DataManager.Ins.ConfigData.maxHeart)
            {
                newValue = DataManager.Ins.ConfigData.maxHeart;
            }

            ResourceChangeData data = new()
            {
                ChangedAmount = newValue - _gameData.user.heart,
                OldValue = _gameData.user.heart,
                NewValue = newValue,
                Source = source
            };

            _gameData.user.heart = newValue;
            PostEvent(EventID.OnChangeHeart, data);
            Database.SaveData(_gameData);

            if (amount < 0) SmoothHeart = _gameData.user.heart;

            return true;
        }

        private bool TryModifyRewardKeys(int amount, object source)
        {
            if (_gameData.user.rewardChestKeys + amount < 0) return false;

            ResourceChangeData data = new()
            {
                ChangedAmount = amount,
                OldValue = _gameData.user.rewardChestKeys,
                NewValue = _gameData.user.rewardChestKeys + amount,
                Source = source
            };

            _gameData.user.rewardChestKeys += amount;
            if (_gameData.user.rewardChestKeys >= DataManager.Ins.ConfigData.requireRewardKey)
            {
                _gameData.user.rewardChestUnlock +=
                    _gameData.user.rewardChestKeys / DataManager.Ins.ConfigData.requireRewardKey;
                _gameData.user.rewardChestKeys =
                    _gameData.user.rewardChestKeys % DataManager.Ins.ConfigData.requireRewardKey;
            }

            PostEvent(EventID.OnChangeRewardKeys, data);
            Database.SaveData(_gameData);

            if (amount < 0) SmoothRewardKeys = _gameData.user.rewardChestKeys;

            return true;
        }

        private bool TryModifyLevelProgress(int amount, object source)
        {
            if (_gameData.user.levelChestProgress + amount < 0) return false;

            ResourceChangeData data = new()
            {
                ChangedAmount = amount,
                OldValue = _gameData.user.levelChestProgress,
                NewValue = _gameData.user.levelChestProgress + amount,
                Source = source
            };

            _gameData.user.levelChestProgress += amount;
            if (_gameData.user.levelChestProgress >= DataManager.Ins.ConfigData.requireLevelProgress)
            {
                _gameData.user.levelChestUnlock += _gameData.user.levelChestProgress /
                                                   DataManager.Ins.ConfigData.requireLevelProgress;
                _gameData.user.levelChestProgress = _gameData.user.levelChestProgress %
                                                    DataManager.Ins.ConfigData.requireLevelProgress;
            }

            PostEvent(EventID.OnChangeLevelProgress, data);
            Database.SaveData(_gameData);

            if (amount < 0) SmoothLevelProgress = _gameData.user.levelChestProgress;

            return true;
        }

        #endregion

        #region OnApplication

        private void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus) return;
            DataManager.Ins.Save();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                _gameData.user.lastTimeLogOut = DateTime.Now;
                _gameData.user.heartRemaningTime = currentRegenHeartTime;
                _gameData.user.heartStopCountingSecond = (int)DateTime.UtcNow.TimeOfDay.TotalSeconds;
                DataManager.Ins.Save();
            }
            else
            {
                InitHeart();
            }
        }

        private void OnApplicationQuit()
        {
            _gameData.user.lastTimeLogOut = DateTime.Now;
            _gameData.user.heartRemaningTime = currentRegenHeartTime;
            _gameData.user.heartStopCountingSecond = (int)DateTime.UtcNow.TimeOfDay.TotalSeconds;
            DataManager.Ins.Save();
        }

        #endregion
    }
}
