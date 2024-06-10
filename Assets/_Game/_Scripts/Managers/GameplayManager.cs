using System.Collections.Generic;
using _Game._Scripts.InGame;
using _Game._Scripts.Managers;
using _Game._Scripts.UIs.Component;
using _Game.Data;
using _Game.DesignPattern;
using _Game.GameGrid;
using _Game.Resource;
using _Game.UIs.Screen;
using _Game.Utilities.Timer;
using AudioEnum;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Playables;
using VinhLB;

namespace _Game.Managers
{
    [DefaultExecutionOrder(-90)]
    public class GameplayManager : Singleton<GameplayManager>
    {
        [SerializeField]
        private PushHintObject pushHintObject;

        private readonly Dictionary<int, bool> isBoughtPushHintInIsland = new();
        private readonly Dictionary<int, bool> isBoughtGrowTreeInIsland = new();
        private readonly Dictionary<int, bool> isCanGrowTreeInIsland = new();

        private bool IsBoughtGrowTreeInIsland(int islandID)
        {
            return isBoughtGrowTreeInIsland.ContainsKey(islandID) && isBoughtGrowTreeInIsland[islandID];
        }

        private bool IsCanGrowTreeInIsland(int islandID)
        {
            return isCanGrowTreeInIsland.ContainsKey(islandID) && isCanGrowTreeInIsland[islandID];
        }

        private void SetBoughtTreeInIsland(int islandID, bool value)
        {
            isBoughtGrowTreeInIsland.TryAdd(islandID, false);
            isBoughtGrowTreeInIsland[islandID] = value;
            screen.OnBoughtGrowTree(value);
        }

        public void SetGrowTreeInIsland(int islandID, bool value)
        {
            isCanGrowTreeInIsland.TryAdd(islandID, false);
            isCanGrowTreeInIsland[islandID] = value;
            screen.SetActiveGrowTree(value);
            isBoughtGrowTreeInIsland.TryAdd(islandID, false);
            screen.growTreeButton.SetAmount(DataManager.Ins.GameData.user.growTreeCount);
            if (isCanGrowTreeInIsland[islandID] && isBoughtGrowTreeInIsland[islandID])
            {
                screen.growTreeButton.IsShowAds = false;
            }
        }

        private PushHint _pushHint;
        private bool isCanResetIsland = true;
        private bool isCanUndo = true;

        private InGameScreen screen;

        public InGameScreen Screen => screen;
        public PushHint PushHint => _pushHint;

        [ReadOnly]
        [SerializeField]
        private int time;
        private int _gameDuration;
        private bool _isShowSaveMeScreen;
        private STimer timer;

        public PushHintObject PushHintObject => pushHintObject;

        public SavePushHint SavePushHint { get; private set; }

        public bool IsCanUndo
        {
            get => isCanUndo;
            set
            {
                isCanUndo = value;
                screen.SetActiveUndo(value);
                if (isCanUndo)
                {
                    screen.undoButton.IsInteractable = true;
                    screen.undoButton.SetAmount(DataManager.Ins.GameData.user.undoCount);
                }
                else
                {
                    screen.undoButton.IsInteractable = false;
                }
            }
        }

        public bool IsCanResetIsland
        {
            get => isCanResetIsland;
            set
            {
                isCanResetIsland = value;
                screen.SetActiveResetIsland(value);
            }
        }
        public int GameDuration => _gameDuration;

        private void Awake()
        {
            screen = UIManager.Ins.GetUI<InGameScreen>();
            screen.Close();
            timer = TimerManager.Ins.PopSTimer();
            GameManager.Ins.RegisterListenerEvent(EventID.StartGame, OnStartGame);
            GameManager.Ins.RegisterListenerEvent(EventID.MoreTimeGame, OnMoreTimeGame);
            GameManager.Ins.RegisterListenerEvent(EventID.WinGame, OnWinGame);
            GameManager.Ins.RegisterListenerEvent(EventID.LoseGame, OnLoseGame);
            GameManager.Ins.RegisterListenerEvent(EventID.Pause, OnPauseGame);
            GameManager.Ins.RegisterListenerEvent(EventID.UnPause, OnUnPauseGame);
            // TODO: Refactor Booster later to avoid duplicate code

            EventGlobalManager.Ins.OnUsingBooster.AddListener(OnUsingBooster);
            screen.undoButton.SetAmount(DataManager.Ins.GameData.user.undoCount);
            screen.pushHintButton.SetAmount(DataManager.Ins.GameData.user.pushHintCount);
            screen.growTreeButton.SetAmount(DataManager.Ins.GameData.user.growTreeCount);
            // screen.resetIslandButton.SetAmount(DataManager.Ins.GameData.user.resetIslandCount);
        }

        private void Start()
        {
            EventGlobalManager.Ins.OnChangeBoosterAmount.AddListener(OnChangeBoosterAmount);
            EventGlobalManager.Ins.OnPlayerChangeIsland.AddListener(OnPlayerChangeIsland);
        }

        private void OnDestroy()
        {
            GameManager.Ins.UnregisterListenerEvent(EventID.StartGame, OnStartGame);
            GameManager.Ins.UnregisterListenerEvent(EventID.MoreTimeGame, OnMoreTimeGame);
            GameManager.Ins.UnregisterListenerEvent(EventID.WinGame, OnWinGame);
            GameManager.Ins.UnregisterListenerEvent(EventID.LoseGame, OnLoseGame);
            GameManager.Ins.UnregisterListenerEvent(EventID.Pause, OnPauseGame);
            GameManager.Ins.UnregisterListenerEvent(EventID.UnPause, OnUnPauseGame);
            EventGlobalManager.Ins.OnChangeBoosterAmount?.RemoveListener(OnChangeBoosterAmount);
            EventGlobalManager.Ins.OnUsingBooster?.RemoveListener(OnUsingBooster);
            EventGlobalManager.Ins.OnPlayerChangeIsland?.RemoveListener(OnPlayerChangeIsland);
            TimerManager.Ins.PushSTimer(timer);
        }

        private bool IsBoughtPushHintInIsland(int islandID)
        {
            return isBoughtPushHintInIsland[islandID];
        }

        private void SetBoughtPushHintInIsland(int islandID, bool value)
        {
            isBoughtPushHintInIsland[islandID] = value;
        }

        private void OnChangeBoosterAmount(BoosterType boosterType, int amount)
        {
            switch (boosterType)
            {
                case BoosterType.Undo:
                    UpdateBoosterCount(ref DataManager.Ins.GameData.user.undoCount, screen.undoButton, amount);
                    break;
                case BoosterType.GrowTree:
                    UpdateBoosterCount(ref DataManager.Ins.GameData.user.growTreeCount, screen.growTreeButton,
                        amount);
                    break;
                case BoosterType.PushHint:
                    UpdateBoosterCount(ref DataManager.Ins.GameData.user.pushHintCount, screen.pushHintButton,
                        amount);
                    break;
                    // case BoosterType.ResetIsland:
                    //     UpdateBoosterCount(ref DataManager.Ins.GameData.user.resetIslandCount, screen.resetIslandButton,
                    //         amount);
                    //     break;
            }

            #region ANALYSTIC
            if (amount < 0)
            {
                AnalysticManager.Ins.BoosterSpend(boosterType);
            }
            else if (amount > 0)
            {
                AnalysticManager.Ins.ResourceEarn(boosterType, Placement.None, amount);
            }
            #endregion
        }

        private void UpdateBoosterCount(ref int boosterCount, BoosterButton button, int amount)
        {
            boosterCount += amount;
            button.SetAmount(boosterCount);
        }

        public void ClearBoosterPurchase()
        {
            isBoughtPushHintInIsland.Clear();
            isBoughtGrowTreeInIsland.Clear();
            isCanGrowTreeInIsland.Clear();
        }

        public void OnPauseGame()
        {
            timer.Stop();
        }

        public void OnUnPause()
        {
            timer.Start(1f, CountTime, true);
        }

        private void OnUnPauseGame(object o)
        {
            // Cast o to GameState
            GameState nextState = (GameState)o;
            // More check for some level that not need timer
            if (nextState is GameState.InGame) timer.Start(1f, CountTime, true);
        }

        private void OnResetTime()
        {
            LevelType levelType = LevelManager.Ins.CurrentLevel.LevelType;
            time = levelType is LevelType.Normal
                ? DataManager.Ins.GetLevelTime(levelType, LevelManager.Ins.CurrentLevel.LevelNormalType)
                : DataManager.Ins.GetLevelTime(LevelManager.Ins.CurrentLevel.LevelType);
            // if first level of normal level, or daily time is MaxValue because it is tutorial level
            if (LevelManager.Ins.IsFirstTutorialLevel) time = int.MaxValue;
            _gameDuration = 0;
            screen.Time = time;
            timer.Start(1f, CountTime, true);
        }

        private void OnMoreTimeGame()
        {
            time = DataManager.Ins.ConfigData.MoreTimeAdded;
            screen.Time = time;
            timer.Start(1f, CountTime, true);
            IsCanResetIsland = true;
            IsCanUndo = true;
            GameManager.Ins.ChangeState(GameState.InGame);
        }

        private void OnStartGame()
        {
            if (LevelManager.Ins.IsSavePlayerPushStep) SavePushHint = new SavePushHint();
            _lastPlayerIslandId = -1;
            _isShowSaveMeScreen = false;
            PushHintObject.SetActive(false);
            OnResetTime();
            screen.OnHandleTutorial();
            screen.OnCheckBoosterLock();
            IsCanResetIsland = true;
            IsCanUndo = false;
            isBoughtPushHintInIsland.Clear();
            _pushHint?.OnStopHint(); // Clear old hint
            _pushHint = new PushHint(LevelManager.Ins.CurrentLevel.GetPushHint());
            if (LevelManager.Ins.CurrentLevel.IsInit)
            {
                #region Handling Tree seed when level is play again without init
                List<int> keysToModify = new();

                foreach (KeyValuePair<int, bool> isBought in isBoughtGrowTreeInIsland)
                {
                    if (isBought.Value)
                    {
                        keysToModify.Add(isBought.Key);
                    }
                }

                foreach (int key in keysToModify)
                {
                    isBoughtGrowTreeInIsland[key] = false;
                    if (key == LevelManager.Ins.player.islandID)
                    {
                        screen.OnBoughtGrowTree(false);
                    }
                }

                keysToModify.Clear();

                foreach (KeyValuePair<int, bool> canGrow in isCanGrowTreeInIsland)
                {
                    if (!canGrow.Value)
                    {
                        keysToModify.Add(canGrow.Key);
                    }
                }

                foreach (int key in keysToModify)
                {
                    isCanGrowTreeInIsland[key] = true;
                }
                #endregion

                OnPlayerChangeIsland(true);
            }
            else
            {
                isBoughtGrowTreeInIsland.Clear();
                isCanGrowTreeInIsland.Clear();
            }
            ChangePlayerSkin();
            screen.OnSetBoosterAmount();
            screen.OnShowFocusBooster(false);
            GameManager.Ins.ChangeState(GameState.InGame);
            // If hard level, show a notification -> If it None -> not show

            #region BANNER
            AdsManager.Ins.ShowBannerAds(BannerAds.TYPE.ADSMOB);

            // if(AdsManager.Ins.IsBannerOpen)
            //     GameManager.Ins.PostEvent(EventID.OnChangeLayoutForBanner, true);
            #endregion

            #region ANALYSTIC
            AnalysticManager.Ins.LevelStart(LevelManager.Ins.CurrentLevel);
            #endregion
        }

        private void OnWinGame()
        {
            if (LevelManager.Ins.IsSavePlayerPushStep) SavePushHint.Save();
            _pushHint.OnStopHint();
            timer.Stop();
            UIManager.Ins.OpenUI<WinScreen>(GetWinGameRewards());
            GameManager.Ins.ChangeState(GameState.WinGame);

            #region ANALYSTIC
            AnalysticManager.Ins.LevelComplete(LevelManager.Ins.CurrentLevel);
            switch (LevelManager.Ins.CurrentLevel.LevelType)
            {
                case LevelType.Normal:
                    DataManager.Ins.GameData.user.retryTime = 0;
                    AnalysticManager.Ins.AppsFlyerTrackParamEvent("af_level_achieved", new Dictionary<string, string>
                    {
                        { "level", DataManager.Ins.GameData.user.normalLevelIndex.ToString() }
                    });
                    break;
            }
            #endregion
        }

        public Reward[] GetWinGameRewards(bool doubleGold = false)
        {
            int goldAmount = LevelManager.Ins.goldCount;
            if (doubleGold)
            {
                goldAmount *= 2;
            }
            int keyAmount = LevelManager.Ins.KeyRewardCount;
            int secretMapPieceAmount = LevelManager.Ins.SecretMapPieceCount;
            List<Reward> rewards = new();
            if (goldAmount > 0)
            {
                Reward goldReward = new()
                {
                    RewardType = RewardType.Currency,
                    CurrencyType = CurrencyType.Gold,
                    Amount = goldAmount
                };
                rewards.Add(goldReward);
            }

            if (keyAmount > 0)
            {
                Reward keyReward = new()
                {
                    RewardType = RewardType.Currency,
                    CurrencyType = CurrencyType.RewardKey,
                    Amount = keyAmount
                };
                rewards.Add(keyReward);
            }
            if (secretMapPieceAmount > 0)
            {
                Reward secretMapPieceReward = new()
                {
                    RewardType = RewardType.Currency,
                    CurrencyType = CurrencyType.SecretMapPiece,
                    Amount = secretMapPieceAmount
                };
                rewards.Add(secretMapPieceReward);
            }
            foreach (KeyValuePair<BoosterType, int> boosterReward in LevelManager.Ins.boosterRewardCount)
            {
                if (boosterReward.Value <= 0) continue;
                Reward booster = new()
                {
                    RewardType = RewardType.Booster,
                    BoosterType = boosterReward.Key,
                    Amount = boosterReward.Value
                };
                rewards.Add(booster);
            }
            return rewards.ToArray();
        }

        public void OnLoseGame(object o)
        {
            _pushHint.OnStopHint();
            timer.Stop();
            // Show Different Lose based on reason (Ex: Lose by Die will not show More time booster, instead show Revive) -> Check by the time remaining
            switch (LevelManager.Ins.CurrentLevel.LevelType)
            {
                case LevelType.Normal:
                case LevelType.Secret:
                    GameManager.Ins.TrySpendHeart(1);
                    break;
            }
            UIManager.Ins.OpenUI<LoseScreen>(o);
            GameManager.Ins.ChangeState(GameState.LoseGame);

            #region ANALYSTIC
            AnalysticManager.Ins.LevelFail(LevelManager.Ins.CurrentLevel, (LevelLoseCondition)o);
            switch (LevelManager.Ins.CurrentLevel.LevelType)
            {
                case LevelType.Normal:
                    DataManager.Ins.GameData.user.retryTime += 1;
                    break;
            }
            #endregion
        }

        private void CountTime()
        {
            if (time < 0) return;
            time -= 1;
            _gameDuration += 1;
            screen.Time = time;
            if (time <= 0)
            {
                if (!_isShowSaveMeScreen)
                {
                    _isShowSaveMeScreen = true;
                    UIManager.Ins.OpenUI<SaveMeScreen>();
                }
                else
                {
                    OnLoseGame(LevelLoseCondition.Timeout);
                }
            }
        }

        #region Booster

        private void OnUsingBooster(BoosterType boosterType)
        {
            if (LevelManager.Ins.player.IsStun || LevelManager.Ins.player.IsDead) return;
            switch (boosterType)
            {
                case BoosterType.Undo:
                    OnUndo();
                    break;
                case BoosterType.GrowTree:
                    OnGrowTree();
                    break;
                case BoosterType.PushHint:
                    OnPushHint();
                    break;
                case BoosterType.ResetIsland:
                    OnResetIsland();
                    break;
            }
        }
        #region Undo
        public void OnFreeUndo()
        {
            if (!isCanUndo) return;
            if (!LevelManager.Ins.OnUndo()) return;
            if (!_pushHint.IsStartHint)
            {
                if (_pushHint.IsPlayerMakeHintWrong) _pushHint.OnStopHint();
            }
            else
            {
                if (_pushHint.IsPlayerMakeHintWrong) _pushHint.OnContinueHint();
                else
                {
                    _pushHint.OnRevertHint();
                }
            }
        }

        private void OnUndo()
        {
            if (!isCanUndo) return;
            if (!LevelManager.Ins.OnUndo()) return;
            // Send an undo Signal
            EventGlobalManager.Ins.OnUndoBoosterCall.Dispatch(LevelManager.Ins.player.islandID);
            OnChangeBoosterAmount(BoosterType.Undo, -1);
            // TEMPORARY
            if (!_pushHint.IsStartHint)
            {
                if (_pushHint.IsPlayerMakeHintWrong) _pushHint.OnStopHint();
            }
            else
            {
                if (_pushHint.IsPlayerMakeHintWrong) _pushHint.OnContinueHint();
                else
                {
                    _pushHint.OnRevertHint();
                }
            }

            #region Old Buy Undo

            // if (DataManager.Ins.GameData.user.undoCount <= 0)
            // {
            //     BoosterConfig boosterConfig = DataManager.Ins.ConfigData.boosterConfigList[(int)BoosterType.Undo];
            //     boosterConfig.UIResourceConfig = DataManager.Ins.GetBoosterUIResourceConfig(boosterConfig.Type);
            //     UIManager.Ins.OpenUI<BoosterWatchVideoPopup>(boosterConfig);
            // }
            // else
            // {
            //     if (!LevelManager.Ins.OnUndo()) return;
            //     // Send an undo Signal
            //     EventGlobalManager.Ins.OnUndoBoosterCall.Dispatch(LevelManager.Ins.player.islandID);
            //     OnChangeBoosterAmount(BoosterType.Undo, -1);
            //     // TEMPORARY
            //     if (!_pushHint.IsStartHint)
            //     {
            //         if (_pushHint.IsPlayerMakeHintWrong) _pushHint.OnStopHint();
            //     }
            //     else
            //     {
            //         if (_pushHint.IsPlayerMakeHintWrong) _pushHint.OnContinueHint();
            //         else
            //         {
            //             _pushHint.OnRevertHint();
            //         }
            //     }
            // }

            #endregion
        }
        #endregion

        #region Reset Island
        public void OnFreeResetIsland(bool isShowHint = true)
        {
            LevelManager.Ins.ResetLevelIsland();
            if (!_pushHint.IsStartHint) return;
            _pushHint.OnStopHint();
            _pushHint.OnStartHint(LevelManager.Ins.player.islandID, isShowHint);
        }

        private void OnResetIsland()
        {
            if (DataManager.Ins.GameData.user.resetIslandCount <= 0)
            {
                BoosterConfig boosterConfig =
                    DataManager.Ins.ConfigData.boosterConfigList[(int)BoosterType.ResetIsland];
                boosterConfig.UIResourceConfig = DataManager.Ins.GetBoosterUIResourceConfig(boosterConfig.Type);
                UIManager.Ins.OpenUI<BoosterWatchVideoPopup>(boosterConfig);
            }
            else
            {
                OnChangeBoosterAmount(BoosterType.ResetIsland, -1);
                LevelManager.Ins.ResetLevelIsland();
                int pIslandID = LevelManager.Ins.player.islandID;
                if (!isBoughtPushHintInIsland.TryGetValue(pIslandID, out bool value)) return;
                if (value) _pushHint.OnStartHint(pIslandID);
            }
        }
        #endregion

        #region Grow Tree
        public void OnFreeGrowTree()
        {
            int pIslandID = LevelManager.Ins.player.islandID;
            SetBoughtTreeInIsland(pIslandID, true);
            EventGlobalManager.Ins.OnGrowTree.Dispatch(pIslandID);
        }

        private void OnGrowTree()
        {
            int pIslandID = LevelManager.Ins.player.islandID;
            if (IsBoughtGrowTreeInIsland(pIslandID))
            {
                if (EventGlobalManager.Ins.OnGrowTree.listenerCount <= 0) return;
                AudioManager.Ins.PlaySfx(SfxType.GrowTree);
                EventGlobalManager.Ins.OnGrowTree.Dispatch(pIslandID);
                if (_pushHint.IsStartHint) _pushHint.OnStopHint();
                return;
            }
            if (DataManager.Ins.GameData.user.growTreeCount <= 0)
            {
                BoosterConfig boosterConfig = DataManager.Ins.ConfigData.boosterConfigList[(int)BoosterType.GrowTree];
                boosterConfig.UIResourceConfig = DataManager.Ins.GetBoosterUIResourceConfig(boosterConfig.Type);
                UIManager.Ins.OpenUI<BoosterWatchVideoPopup>(boosterConfig);
            }
            else
            {
                if (EventGlobalManager.Ins.OnGrowTree.listenerCount <= 0) return;
                AudioManager.Ins.PlaySfx(SfxType.GrowTree);
                if (!IsBoughtGrowTreeInIsland(pIslandID))
                {
                    OnChangeBoosterAmount(BoosterType.GrowTree, -1);
                    SetBoughtTreeInIsland(pIslandID, true);
                    screen.growTreeButton.IsShowAds = false; // TEMPORARY
                }
                EventGlobalManager.Ins.OnGrowTree.Dispatch(pIslandID);
                if (_pushHint.IsStartHint)
                {
                    screen.ActivePushHintIsland(true);
                    _pushHint.OnStopHint();
                }
            }
        }
        #endregion

        # region PUSH HINT
        public void OnFreePushHint(bool isReset, bool isShowHint)
        {
            int playerIslandID = LevelManager.Ins.player.islandID;
            SetBoughtPushHintInIsland(playerIslandID, true);
            if (_pushHint.IsStartHint) return;
            if (isReset) LevelManager.Ins.ResetLevelIsland();
            OnStartHintOnPlayerIsland(playerIslandID, isShowHint);
        }

        private void OnPushHint()
        {
            int playerIslandID = LevelManager.Ins.player.islandID;
            if (!isBoughtPushHintInIsland.ContainsKey(playerIslandID)) return;
            if (DataManager.Ins.GameData.user.pushHintCount <= 0 && !IsBoughtPushHintInIsland(playerIslandID))
            {
                BoosterConfig boosterConfig = DataManager.Ins.ConfigData.boosterConfigList[(int)BoosterType.PushHint];
                boosterConfig.UIResourceConfig = DataManager.Ins.GetBoosterUIResourceConfig(boosterConfig.Type);
                UIManager.Ins.OpenUI<BoosterWatchVideoPopup>(boosterConfig);
            }
            else
            {
                AudioManager.Ins.PlaySfx(SfxType.Hint);
                // If this is first player step cell in this island, return
                if (!IsBoughtPushHintInIsland(playerIslandID))
                {
                    OnChangeBoosterAmount(BoosterType.PushHint, -1);
                    SetBoughtPushHintInIsland(playerIslandID, true);
                }
                LevelManager.Ins.ResetLevelIsland();
                OnStartHintOnPlayerIsland(playerIslandID);
            }
        }

        private void OnStartHintOnPlayerIsland(int islandID, bool isShowHint = true)
        {
            // check if _pushHint is run
            if (_pushHint.IsStartHint && _pushHint.LastHintIslandId != islandID)
            {
                _pushHint.OnStopHint();
            }
            screen.ActivePushHintIsland(false);
            _pushHint.OnStartHint(islandID, isShowHint);
        }

        private int _lastPlayerIslandId = -1;

        private void OnPlayerChangeIsland(bool isInit)
        {
            int islandId = LevelManager.Ins.player.islandID;
            if (_lastPlayerIslandId == islandId) return;
            _lastPlayerIslandId = islandId;
            screen.ActivePushHintIsland(_pushHint.ContainIsland(islandId) && !_pushHint.IsCompleted(islandId) &&
                                        !_pushHint.isHintActiveOnIsland(islandId));
            screen.ActiveGrowTreeIsland(isBoughtGrowTreeInIsland.ContainsKey(islandId));
            // Check if contain
            if (!isBoughtPushHintInIsland.ContainsKey(islandId))
            {
                SetBoughtPushHintInIsland(islandId, false);
            }
            bool isBoughtPushHint = IsBoughtPushHintInIsland(islandId);
            screen.OnBoughtPushHintOnIsland(islandId, isBoughtPushHint, isInit);

            // Tree seed
            if (isBoughtGrowTreeInIsland.ContainsKey(islandId))
            {
                bool isCanGrowTree = IsCanGrowTreeInIsland(islandId);
                screen.ActiveGrowTreeIsland(isCanGrowTree);
                bool isBoughtTree = IsBoughtGrowTreeInIsland(islandId);
                screen.OnBoughtGrowTreeOnIsland(islandId, isBoughtTree);
            }
        }

        public void OnShowFocusBooster(bool show)
        {
            screen.OnShowFocusBooster(show);
        }
        #endregion
        #endregion

        #region Skin
        private void ChangePlayerSkin()
        {
            DataManager.Ins.CheckingRentPlayerSkinCount();
            LevelManager.Ins.player.ChangeSkin(DataManager.Ins.CurrentPlayerSkinIndex);
        }

        #endregion
    }
}