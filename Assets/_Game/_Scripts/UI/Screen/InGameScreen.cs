using System;
using _Game._Scripts.InGame;
using _Game._Scripts.Managers;
using _Game._Scripts.UIs.Component;
using _Game.Camera;
using _Game.Data;
using _Game.GameGrid;
using _Game.Managers;
using _Game.UIs.Popup;
using _Game.Utilities;
using _Game.Utilities.Timer;
using AudioEnum;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VinhLB;

namespace _Game.UIs.Screen
{
    public class InGameScreen : UICanvas
    {
        private const float UNDO_CD_TIME = 0.3f;
        private const float TIME_TXT_SCALE_UP = 1.1f;

        [SerializeField]
        private CanvasGroup canvasGroup;
        [SerializeField]
        private HButton settingButton;
        [SerializeField]
        private GameObject timerContainer;

        // [SerializeField]
        // private Image blockPanel;
        // Booster Button
        public BoosterButton undoButton;
        public BoosterButton pushHintButton;
        public BoosterButton growTreeButton;
        // public BoosterButton resetIslandButton;
        // Time & Level Text 
        [SerializeField]
        private Image timeImage;
        [SerializeField]
        private TMP_Text timeText;
        [SerializeField]
        private TMP_Text _levelText;
        [SerializeField]
        private Color dangerTimeColor;

        [SerializeField]
        private Image objectiveImage;
        [SerializeField]
        private TMP_Text objectiveCounterText;
        
        bool isTimeNormal = false;
        private STimer resetIslandTimer;

        private int time;

        private STimer undoTimer;

        private Tween _fadeTween;

        public int Time
        {
            get => time;
            set
            {
                time = value;
                int second = time % 60;
                int minute = time / 60;

                #region TIME ANIM
                if (time <= DataManager.Ins.ConfigData.DangerTime)
                {
                    timeImage.color = dangerTimeColor;
                    timeText.color = dangerTimeColor;
                    timeImage.transform.DOShakePosition(0.3f, 6f, 40, 90, false, true, ShakeRandomnessMode.Harmonic);
                    timeText.transform.DOScale(TIME_TXT_SCALE_UP, 0.15f).SetLoops(2, LoopType.Yoyo);
                    AudioManager.Ins.PlaySfx(SfxType.ClockTick);
                    isTimeNormal = false;
                }
                else
                {
                    if (!isTimeNormal)
                    {
                        timeText.transform.DOKill();
                        timeImage.color = Color.white;
                        timeText.transform.localScale = Vector3.one;
                        timeText.color = Color.white;
                        isTimeNormal = true;
                    }
                }
                #endregion

                timeText.text = $"{minute:00}:{second:00}";
            }
        }

        private void Awake()
        {
            LevelManager.Ins._OnNextLevel += LevelManager_OnLevelNext;
            LevelManager.Ins.OnObjectiveChange += UpdateObjectiveText;
            undoTimer = TimerManager.Ins.PopSTimer();
            resetIslandTimer = TimerManager.Ins.PopSTimer();
            undoButton.AddEvent(OnClickUndo);
            pushHintButton.AddEvent(OnClickPushHint);
            growTreeButton.AddEvent(OnClickGrowTree);
            // resetIslandButton.AddEvent(OnClickResetIsland);
            // GameManager.Ins.RegisterListenerEvent(DesignPattern.EventID.OnChangeLayoutForBanner, ChangeLayoutForBanner);
        }

        private void OnDestroy()
        {
            LevelManager.Ins._OnNextLevel -= LevelManager_OnLevelNext;
            TimerManager.Ins.PushSTimer(undoTimer);
            TimerManager.Ins.PushSTimer(resetIslandTimer);
            undoButton.RemoveEvent(OnClickUndo);
            pushHintButton.RemoveEvent(OnClickPushHint);
            growTreeButton.RemoveEvent(OnClickGrowTree);
            // resetIslandButton.RemoveEvent(OnClickResetIsland);
            // GameManager.Ins.UnregisterListenerEvent(DesignPattern.EventID.OnChangeLayoutForBanner, ChangeLayoutForBanner);
        }

        
        public override void Setup(object param = null)
        {
            base.Setup(param);
            GameManager.Ins.ChangeState(GameState.InGame);
            OnShowFocusBooster(false);
            UpdateLevelText();
            // Log param
            if (param is null) MoveInputManager.Ins.ShowContainer(true);
            else MoveInputManager.Ins.ShowContainer(true, (bool)param);
            if (CameraManager.Ins.IsCurrentCameraIs(ECameraType.InGameCamera)) return;
            canvasGroup.alpha = 0f;
            // blockPanel.enabled = true;
            UpdateObjectiveText();
            isTimeNormal = false;
            CameraManager.Ins.ChangeCamera(ECameraType.ZoomOutCamera);
            TimerManager.Ins.WaitForTime(0.1f, () => { CameraManager.Ins.ChangeCamera(ECameraType.InGameCamera, 1f); });
        }

        public override void Open(object param = null)
        {
            base.Open(param);
            AudioManager.Ins.PlayBgm(BgmType.InGame, 1f);
            AudioManager.Ins.PlayEnvironment(EnvironmentType.Ocean, 1f);
            DebugManager.Ins?.OpenDebugCanvas(UI_POSITION.IN_GAME);
            if (TutorialManager.Ins.currentGameTutorialScreenScreen)
            {
                Close();
                return;
            }
            _fadeTween = DOVirtual.Float(0f, 1f, 0.1f, value => canvasGroup.alpha = value);
                // .OnKill(() => { blockPanel.enabled = false; });
        }

        public override void Close()
        {
            _fadeTween?.Kill();
            OnShowFocusBooster(false);
            MoveInputManager.Ins.ShowContainer(false);
            base.Close();
        }


        public override void Show()
        {
            if (TutorialManager.Ins.currentGameTutorialScreenScreen) return;
            base.Show();
        }

        public void OnClickSettingButton()
        {
            UIManager.Ins.OpenUI<SettingsPopup>();
        }

        public void OnClickToggleBuildingMode()
        {
            Close();
            UIManager.Ins.OpenUI<BuildingScreen>();
            GridBuildingManager.Ins.ToggleBuildMode();
        }

        public void SetActiveUndo(bool active)
        {
            if (active)
                undoTimer.Stop();
            undoButton.IsInteractable = active;
        }

        public void SetActiveResetIsland(bool active)
        {
            resetIslandTimer.Stop();
            // resetIslandButton.IsInteractable = active;
        }

        public void OnBoughtGrowTree(bool active)
        {
            DevLog.Log(DevId.Hoang, "On Bought Grow Tree: " + active);
            growTreeButton.IsShowAmount = !active;
        }

        public void SetActiveGrowTree(bool active)
        {
            growTreeButton.IsInteractable = active;
        }

        public void ActivePushHintIsland(bool active)
        {
            pushHintButton.IsInteractable = active;
        }

        public void ActiveGrowTreeIsland(bool active)
        {
            growTreeButton.IsInteractable = active;
        }

        public void OnBoughtPushHintOnIsland(int islandID, bool active, bool isInit)
        {
            // Check if the current island is the island that the player is on
            if (LevelManager.Ins.player.islandID != islandID) return;
            if (isInit)
            {
                pushHintButton.HasAlternativeImage = false;
                pushHintButton.IsFocus = false;
            }
            pushHintButton.SetAmount(DataManager.Ins.GameData.user.pushHintCount);
            if (active) pushHintButton.IsShowAds = false;
        }

        public void OnBoughtGrowTreeOnIsland(int islandID, bool active)
        {
            // Check if the current island is the island that the player is on
            if (LevelManager.Ins.player.islandID != islandID) return;
            growTreeButton.SetAmount(DataManager.Ins.GameData.user.growTreeCount);
            if (active) growTreeButton.IsShowAds = false;
        }

        private void ChangeLayoutForBanner(object isBannerActive)
        {
            int sizeAnchor = (bool)isBannerActive
                ? UIManager.Ins.ConvertPixelToUnitHeight(DataManager.Ins.ConfigData.bannerHeight)
                : 0;
            MRectTransform.offsetMin = new Vector2(MRectTransform.offsetMin.x, sizeAnchor);
        }

        private void UpdateLevelText()
        {
            int levelIndex = 1;
            switch (LevelManager.Ins.CurrentLevel.LevelType)
            {
                case LevelType.Normal:
                    levelIndex += LevelManager.Ins.NormalLevelIndex;
                    _levelText.text = $"Level {levelIndex}";
                    break;
                case LevelType.Secret:
                    levelIndex += LevelManager.Ins.SecretLevelIndex;
                    _levelText.text = $"EXPEDITION";
                    break;
                case LevelType.DailyChallenge:
                    if (LevelManager.Ins.DailyLevelIndex == 0)
                    {
                        _levelText.text = "Tutorial";
                        break;
                    }
                    levelIndex += LevelManager.Ins.dailyLevelClickedDay;
                    _levelText.text = $"Day {levelIndex}";
                    break;
            }
        }

        private void UpdateObjectiveText()
        {
            objectiveImage.sprite = DataManager.Ins.UIResourceDatabase.objectiveIconDict[LevelManager.Ins.CurrentLevel.LevelWinCondition].MainIconSprite;
            objectiveCounterText.text = $"{LevelManager.Ins.ObjectiveCounterLeft()}/{LevelManager.Ins.ObjectiveTotal}";
        }

        private void LevelManager_OnLevelNext()
        {
            UpdateLevelText();
            UpdateObjectiveText();
        }

        public void OnCheckBoosterLock()
        {
            LevelType type = LevelManager.Ins.CurrentLevel.LevelType;
            if (type is LevelType.Normal)
            {
                int currentLevelIndex = LevelManager.Ins.CurrentLevel.Index;
                
                int unlockLevelIndex = DataManager.Ins.ConfigData.boosterConfigList[(int)undoButton.Type].UnlockAtLevel;
                undoButton.SetUnlockLevel(unlockLevelIndex);
                undoButton.IsLock = currentLevelIndex < unlockLevelIndex;
                
                unlockLevelIndex = DataManager.Ins.ConfigData.boosterConfigList[(int)pushHintButton.Type].UnlockAtLevel;
                pushHintButton.SetUnlockLevel(unlockLevelIndex);
                pushHintButton.IsLock = currentLevelIndex < unlockLevelIndex;

                unlockLevelIndex = DataManager.Ins.ConfigData.boosterConfigList[(int)growTreeButton.Type].UnlockAtLevel;
                growTreeButton.SetUnlockLevel(unlockLevelIndex);
                growTreeButton.IsLock = currentLevelIndex < unlockLevelIndex;
                
                // isLock = currentLevel < DataManager.Ins.ConfigData.boosterConfigList[(int)resetIslandButton.Type].UnlockAtLevel;
                // resetIslandButton.IsLock = true;
            }
            else
            {
                undoButton.IsLock = false;
                pushHintButton.IsLock = false;
                growTreeButton.IsLock = false;
                // resetIslandButton.IsLock = false;
            }
        }

        public void OnSetBoosterAmount()
        {
            undoButton.IsShowAmount = true;
            undoButton.SetAmount(DataManager.Ins.GameData.user.undoCount);
            pushHintButton.IsShowAmount = true;
            pushHintButton.SetAmount(DataManager.Ins.GameData.user.pushHintCount);
            growTreeButton.IsShowAmount = true;
            growTreeButton.SetAmount(DataManager.Ins.GameData.user.growTreeCount);
            // resetIslandButton.SetAmount(DataManager.Ins.GameData.user.resetIslandCount);
            // Get the unlock level of the booster
        }

        #region Booster
        private void OnClickUndo()
        {
            // Check number of ticket to use
            EventGlobalManager.Ins.OnUsingBooster.Dispatch(Resource.BoosterType.Undo);
            undoButton.Button.interactable = false;
            AudioManager.Ins.PlaySfx(SfxType.Undo);
            undoTimer.Start(UNDO_CD_TIME, () => undoButton.Button.interactable = true);

            if (undoButton.IsFocus) undoButton.IsFocus = false;
            // if (resetIslandButton.IsFocus) resetIslandButton.IsFocus = false;
        }

        private void OnClickGrowTree()
        {
            EventGlobalManager.Ins.OnUsingBooster.Dispatch(Resource.BoosterType.GrowTree);
            if (undoButton.IsFocus) undoButton.IsFocus = false;
            // if (resetIslandButton.IsFocus) resetIslandButton.IsFocus = false;
        }

        private void OnClickResetIsland()
        {
            EventGlobalManager.Ins.OnUsingBooster.Dispatch(Resource.BoosterType.ResetIsland);
            if (undoButton.IsFocus) undoButton.IsFocus = false;
            // if (resetIslandButton.IsFocus) resetIslandButton.IsFocus = false;
        }

        private void OnClickPushHint()
        {
            EventGlobalManager.Ins.OnUsingBooster.Dispatch(Resource.BoosterType.PushHint);
            if (undoButton.IsFocus) undoButton.IsFocus = false;
            // if (resetIslandButton.IsFocus) resetIslandButton.IsFocus = false;
        }
        #endregion


        private Tween _tryAgainImageTween;

        public void OnShowFocusBooster(bool show)
        {
            undoButton.IsFocus = show;
            // resetIslandButton.IsFocus = show;
        }

        public void OnHandleTutorial()
        {
            bool isTutorial = LevelManager.Ins.IsFirstTutorialLevel;
            LevelType type = LevelManager.Ins.CurrentLevel.LevelType;
            // Handle Showing time & Setting button
            timerContainer.SetActive(!isTutorial);
            settingButton.gameObject.SetActive(!isTutorial);
            // Hide Showing Booster
            if (type is LevelType.Normal && LevelManager.Ins.CurrentLevel.Index < 1)
            {
                undoButton.gameObject.SetActive(false);
                growTreeButton.gameObject.SetActive(false);
                pushHintButton.gameObject.SetActive(false);
                // resetIslandButton.gameObject.SetActive(false);
            }
            else
            {
                undoButton.gameObject.SetActive(true);
                growTreeButton.gameObject.SetActive(true);
                pushHintButton.gameObject.SetActive(true);
                // resetIslandButton.gameObject.SetActive(true);
            }
        }

        #region Unlimited Booster
        public void OnClickUnlimitedUndo()
        {
            GameplayManager.Ins.OnFreeUndo();
        }

        public void OnClickUnlimitedResetIsland()
        {
            GameplayManager.Ins.OnFreeResetIsland();
        }
        #endregion
    }
}