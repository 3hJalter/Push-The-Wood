using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using _Game._Scripts.UIs.Component;
using _Game.Camera;
using _Game.Data;
using _Game.DesignPattern;
using _Game.GameGrid;
using _Game.Managers;
using _Game.UIs.Popup;
using _Game.UIs.Screen;
using _Game.Utilities.Timer;
using Cinemachine;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VinhLB
{
    public class HomePage : TabPage
    {
        [SerializeField]
        private HButton _playButton;
        [SerializeField]
        private FeatureButton _dailyChallengeButton;
        [SerializeField]
        private FeatureButton _secretMapButton;
        [SerializeField]
        private FeatureButton _dailyRewardButton;
        [SerializeField]
        private FeatureButton _dailyMissionButton;

        [Space]
        [SerializeField]
        private FeatureButton _rewardChestButton;
        [SerializeField]
        private RectTransform _rewardChestIconRectTF;
        [SerializeField]
        private RectTransform _rewardChestCurrencyIconRectTF;
        [SerializeField]
        private GameObject _rewardKeyFrameGO;
        [SerializeField]
        private TMP_Text _rewardKeyText;
        [SerializeField]
        private GameObject _rewardChestEffectGO;

        [Space]
        [SerializeField]
        private FeatureButton _levelChestButton;
        [SerializeField]
        private RectTransform _levelChestIconRectTF;
        [SerializeField]
        private RectTransform _levelChestCurrencyIconRectTF;
        [SerializeField]
        private GameObject _levelStarFrameGO;
        [SerializeField]
        private TMP_Text _levelStarText;
        [SerializeField]
        private GameObject _levelChestEffectGO;

        [Space]
        [SerializeField]
        private TMP_Text _levelText;
        [SerializeField]
        private Image _levelNormalFrame;
        [SerializeField]
        private Color _easyLevelNormalColor;
        [SerializeField]
        private Color _mediumLevelNormalColor;
        [SerializeField]
        private Color _hardLevelNormalColor;
        [SerializeField]
        private Color _lastLevelNormalColor;
        [SerializeField]
        private GameObject _lastLevelLockGO;


        private STimer _shakeRewardTimer;
        private STimer _shakeLevelTimer;
        private Quaternion _startRewardChestIconQuaternion;
        private Quaternion _startLevelChestIconQuaternion;
        private bool _isTutorialRunning;
        private bool _isFirstShown;
        private Coroutine _delayOpenCoroutine;

        private event Func<Task> _delayCollectingRewardKeys;
        private event Func<Task> _delayCollectingLevelStars;

        private void Awake()
        {
            GameManager.Ins.RegisterListenerEvent(EventID.OnChangeRewardKeys,
                data => ChangeRewardKeyValue((ResourceChangeData)data));
            GameManager.Ins.RegisterListenerEvent(EventID.OnChangeLevelProgress,
                data => ChangeLevelProgressValue((ResourceChangeData)data));

            _playButton.onClick.AddListener(() =>
            {
                if (GameManager.Ins.Heart <= 0)
                {
                    UIManager.Ins.OpenUI<BuyMorePopup>();
                }
                else
                {
                    LevelManager.Ins.InitLevel();
                    UIManager.Ins.CloseAll();

                    TransitionScreen transitionScreen = UIManager.Ins.OpenUI<TransitionScreen>();
                    if (LevelManager.Ins.IsHardLevel)
                    {
                        transitionScreen.OnOpenCallback += OpenActions;
                        transitionScreen.OnCloseCallback += CloseActions;

                        void OpenActions()
                        {
                            transitionScreen.OnOpenCallback -= OpenActions;

                            CameraManager.Ins.ChangeCamera(ECameraType.ZoomOutCamera, 0f);
                        }

                        void CloseActions()
                        {
                            transitionScreen.OnCloseCallback -= CloseActions;

                            UIManager.Ins.OpenUI<HardWarningScreen>();
                        }
                    }
                    else
                    {
                        UIManager.Ins.OpenUI<InGameScreen>();
                    }
                }
            });
            _dailyChallengeButton.AddListener(() => { UIManager.Ins.OpenUI<DailyChallengePopup>(); });
            _dailyRewardButton.AddListener(() => { UIManager.Ins.OpenUI<DailyRewardPopup>(); });
            _dailyMissionButton.AddListener(() => { UIManager.Ins.OpenUI<DailyMissionPopup>(); });
            _secretMapButton.AddListener(() => { UIManager.Ins.OpenUI<SecretMapPopup>(); });
            _rewardChestButton.AddListener(() =>
            {
                if (!CanClaimRewardChest())
                {
                    return;
                }

                if (RewardManager.Ins.HomeReward.TryClaimRewardChest())
                {
                    int amount = GameManager.Ins.RewardKeys +
                                 (DataManager.Ins.GameData.user.rewardChestUnlock -
                                  DataManager.Ins.GameData.user.currentRewardChestIndex) *
                                 DataManager.Ins.ConfigData.requireRewardKey;
                    GameManager.Ins.SmoothRewardKeys = amount;

                    UpdateRewardChestUI();
                }
            });
            _levelChestButton.AddListener(() =>
            {
                if (!CanClaimLevelChest())
                {
                    return;
                }

                if (RewardManager.Ins.HomeReward.TryClaimLevelChest())
                {
                    int amount = GameManager.Ins.LevelProgress +
                                 (DataManager.Ins.GameData.user.levelChestUnlock -
                                  DataManager.Ins.GameData.user.currentLevelChestIndex) *
                                 DataManager.Ins.ConfigData.requireLevelProgress;
                    GameManager.Ins.SmoothLevelProgress = amount;

                    UpdateLevelChestUI();
                }
            });

            _shakeRewardTimer = TimerManager.Ins.PopSTimer();
            _shakeLevelTimer = TimerManager.Ins.PopSTimer();
            _startRewardChestIconQuaternion = _rewardChestIconRectTF.localRotation;
            _startLevelChestIconQuaternion = _levelChestIconRectTF.localRotation;
        }

        private void OnDestroy()
        {
            GameManager.Ins.UnregisterListenerEvent(EventID.OnChangeRewardKeys,
                data => ChangeRewardKeyValue((ResourceChangeData)data));
            GameManager.Ins.UnregisterListenerEvent(EventID.OnChangeLevelProgress,
                data => ChangeLevelProgressValue((ResourceChangeData)data));

            TimerManager.Ins.PushSTimer(_shakeRewardTimer);
        }

        public override void Setup(object param = null)
        {
            base.Setup(param);

            SetupHomeCamera();

            SetupFeature();

            if (_rewardChestButton.IsUnlocked)
            {
                UpdateRewardChestUI();
            }
            if (_levelChestButton.IsUnlocked)
            {
                UpdateLevelChestUI();
            }
        }

        public override void Open(object param = null)
        {
            base.Open(param);

            // SplashScreen splashScreen = UIManager.Ins.GetUI<SplashScreen>();
            // splashScreen.OnCloseCallback += OpenActions;
            //
            // async void OpenActions()
            // {
            //     splashScreen.OnCloseCallback -= OpenActions;
            //     
            //     List<Task> taskList = new List<Task>();
            //     if (_rewardChestButton.IsUnlocked && _delayCollectingRewardKeys != null)
            //     {
            //         Task task = _delayCollectingRewardKeys.Invoke();
            //         taskList.Add(task);
            //     }
            //     if (_levelChestButton.IsUnlocked && _delayCollectingLevelStars != null)
            //     {
            //         Task task = _delayCollectingLevelStars.Invoke();
            //         taskList.Add(task);
            //     }
            //
            //     if (taskList.Count > 0)
            //     {
            //         await Task.WhenAll(taskList);
            //     }
            //
            //     Debug.Log("done");
            //     _delayCollectingRewardKeys = null;
            //     _delayCollectingLevelStars = null;
            //
            //     OnShowMenuTutorial();
            //
            //     // if (!_isFirstShown)
            //     // {
            //     //     _isFirstShown = true;
            //     //     if (!DailyRewardManager.Ins.IsTodayRewardObtained)
            //     //     {
            //     //         UIManager.Ins.OpenUI<DailyRewardPopup>();
            //     //     }
            //     // }
            // }

            if (_delayOpenCoroutine != null)
            {
                StopCoroutine(_delayOpenCoroutine);
            }
            OnShowMenuTutorial();

            _delayOpenCoroutine = StartCoroutine(OpenActionsCoroutine());
        }

        public override void UpdateUI()
        {
            // If NormalLevelIndex is the last index of normal level in
            int levelIndex = LevelManager.Ins.NormalLevelIndex;
            _levelText.text = $"Level {levelIndex + 1}";

            #region Notif

            int dailyRewardNotificationAmount = DailyRewardManager.Ins.IsTodayRewardObtained ? 0 : 1;
            _dailyRewardButton.SetNotificationAmount(dailyRewardNotificationAmount);

            int dailyChallengeNotificationAmount = DataManager.Ins.IsClearAllDailyChallenge() &&
                                                   DataManager.Ins.IsCollectedAllDailyChallengeReward()
                ? 0
                : 1;
            _dailyChallengeButton.SetNotificationAmount(dailyChallengeNotificationAmount);

            int secretMapNotificationAmount = DataManager.Ins.IsClearAllSecretLevel() ? 0 : 1;
            _secretMapButton.SetNotificationAmount(secretMapNotificationAmount);
            
            #endregion
            
            if (levelIndex == DataManager.Ins.CountNormalLevel - 1)
            {
                _levelNormalFrame.color = _lastLevelNormalColor;
                _levelText.text = "Not Available";
                _lastLevelLockGO.SetActive(true);
                _playButton.interactable = false;

                return;
            }

            _playButton.interactable = true;
            _lastLevelLockGO.SetActive(false);

            LevelNormalType currentLevelLevelNormalType = LevelManager.Ins.CurrentLevel.LevelNormalType;
            switch (currentLevelLevelNormalType)
            {
                case LevelNormalType.Easy:
                    _levelNormalFrame.color = _easyLevelNormalColor;
                    break;
                case LevelNormalType.Medium:
                    _levelNormalFrame.color = _mediumLevelNormalColor;
                    break;
                case LevelNormalType.Hard:
                    _levelText.text += $" - {currentLevelLevelNormalType.ToString().ToUpper()}";
                    _levelNormalFrame.color = _hardLevelNormalColor;
                    break;
            }
        }

        public void ShowDailyChallengeButton(bool isShow)
        {
            _dailyChallengeButton.gameObject.SetActive(isShow);
        }

        public void ShowSecretMapButton(bool isShow)
        {
            _secretMapButton.gameObject.SetActive(isShow);
        }

        private void SetupHomeCamera()
        {
            LevelManager.Ins.ConstructingLevel();
            GameGridCell waterCell, playerCell;
            (waterCell, playerCell) = LevelManager.Ins.player.SetActiveAgent(true);
            Vector3 offset = waterCell.WorldPos - playerCell.WorldPos;
            offset.Set(Mathf.Abs(offset.x) < 0.001f ? Constants.CELL_SIZE : offset.x * 12, 3.5f,
                Mathf.Abs(offset.z) < 0.001f ? Constants.CELL_SIZE : offset.z * 12);

            CinemachineFramingTransposer transposerCam =
                CameraManager.Ins.GetCameraCinemachineComponent<CinemachineFramingTransposer>(_Game.Camera.ECameraType
                    .PerspectiveCamera);
            transposerCam.m_TrackedObjectOffset = offset;
            CameraManager.Ins.ChangeCameraTargetPosition(playerCell.WorldPos + Vector3.up * 2, 1f, Ease.OutQuad);
            CameraManager.Ins.ChangeCameraPosition(playerCell.WorldPos + offset);
        }

        private async void ChangeRewardKeyValue(ResourceChangeData data)
        {
            if (data.ChangedAmount > 0)
            {
                if (!gameObject.activeInHierarchy)
                {
                    _delayCollectingRewardKeys += SpawnCollectingUIRewardKeys;
                }
                else
                {
                    await SpawnCollectingUIRewardKeys();
                }
            }
            else
            {
                _rewardKeyText.text = $"{GameManager.Ins.RewardKeys}/{DataManager.Ins.ConfigData.requireRewardKey}";
            }

            async Task SpawnCollectingUIRewardKeys()
            {
                int collectingRewardKeys = Mathf.Min((int)data.ChangedAmount, Constants.MAX_UI_UNIT);
                Vector3 spawnPosition = data.Source as Vector3? ??
                                        CameraManager.Ins.ViewportToWorldPoint(new Vector3(0.5f, 0.5f));
                await CollectingResourceManager.Ins.SpawnCollectingUIRewardKeys(collectingRewardKeys, spawnPosition,
                    _rewardChestIconRectTF,
                    (progress) =>
                    {
                        GameManager.Ins.SmoothRewardKeys += data.ChangedAmount / collectingRewardKeys;

                        UpdateRewardChestUI();
                    });
            }
        }

        private async void ChangeLevelProgressValue(ResourceChangeData data)
        {
            if (data.ChangedAmount > 0)
            {
                if (!gameObject.activeInHierarchy)
                {
                    _delayCollectingLevelStars += SpawnCollectingUILevelStars;
                }
                else
                {
                    await SpawnCollectingUILevelStars();
                }
            }
            else
            {
                _rewardKeyText.text = $"{GameManager.Ins.LevelProgress}/{DataManager.Ins.ConfigData.requireRewardKey}";
            }

            async Task SpawnCollectingUILevelStars()
            {
                int collectingLevelStars = Mathf.Min((int)data.ChangedAmount, Constants.MAX_UI_UNIT);
                Vector3 spawnPosition = data.Source as Vector3? ??
                                        CameraManager.Ins.ViewportToWorldPoint(new Vector3(0.5f, 0.5f));
                await CollectingResourceManager.Ins.SpawnCollectingUILevelStars(collectingLevelStars, spawnPosition,
                    _levelChestIconRectTF,
                    (progress) =>
                    {
                        GameManager.Ins.SmoothLevelProgress += data.ChangedAmount / collectingLevelStars;

                        UpdateLevelChestUI();
                    });
            }
        }

        private void UpdateRewardChestUI()
        {
            if (CanClaimRewardChest())
            {
                if (!_shakeRewardTimer.IsStart)
                {
                    _shakeRewardTimer.Start(1f, () =>
                    {
                        _rewardChestIconRectTF.DOShakeRotation(0.5f, Vector3.forward * 10f, 10, 45f, true,
                            ShakeRandomnessMode.Harmonic);
                    }, true);
                    // _rewardKeyText.text = $"FULL";
                    // _rewardKeyText.color = Color.green;
                    // _rewardChestCurrencyIconRectTF.gameObject.SetActive(false);
                }
                if (_rewardKeyFrameGO.activeSelf)
                {
                    _rewardKeyFrameGO.SetActive(false);
                }
                if (!_rewardChestEffectGO.activeSelf)
                {
                    _rewardChestEffectGO.SetActive(true);
                }
            }
            else
            {
                if (_shakeRewardTimer.IsStart)
                {
                    _shakeRewardTimer.Stop();
                    // _rewardKeyText.color = Color.white;
                    // _rewardChestCurrencyIconRectTF.gameObject.SetActive(true);
                    _rewardChestIconRectTF.localRotation = _startRewardChestIconQuaternion;
                }
                if (_rewardChestEffectGO.activeSelf)
                {
                    _rewardChestEffectGO.SetActive(false);
                }
                if (!_rewardKeyFrameGO.activeSelf)
                {
                    _rewardKeyFrameGO.SetActive(true);
                }

                int normalizedValue = Mathf.RoundToInt(GameManager.Ins.SmoothRewardKeys);
                _rewardKeyText.text =
                    $"{normalizedValue}/{DataManager.Ins.ConfigData.requireRewardKey}";
            }
        }

        private void UpdateLevelChestUI()
        {
            if (CanClaimLevelChest())
            {
                if (!_shakeLevelTimer.IsStart)
                {
                    _shakeLevelTimer.Start(1f, () =>
                    {
                        _levelChestIconRectTF.DOShakeRotation(0.5f, Vector3.forward * 10f, 10, 45f, true,
                            ShakeRandomnessMode.Harmonic);
                    }, true);
                    // _levelStarText.text = $"FULL";
                    // _levelStarText.color = Color.green;
                    // _levelChestCurrencyIconRectTF.gameObject.SetActive(false);
                }
                if (_levelStarFrameGO.activeSelf)
                {
                    _levelStarFrameGO.SetActive(false);
                }
                if (!_levelChestEffectGO.activeSelf)
                {
                    _levelChestEffectGO.SetActive(true);
                }
            }
            else
            {
                if (_shakeLevelTimer.IsStart)
                {
                    _shakeLevelTimer.Stop();
                    // _levelStarText.color = Color.white;
                    // _levelChestCurrencyIconRectTF.gameObject.SetActive(true);
                    _levelChestIconRectTF.localRotation = _startLevelChestIconQuaternion;
                }
                if (!_levelStarFrameGO.activeSelf)
                {
                    _levelStarFrameGO.SetActive(true);
                }
                if (_levelChestEffectGO.activeSelf)
                {
                    _levelChestEffectGO.SetActive(false);
                }

                int normalizedValue = Mathf.RoundToInt(GameManager.Ins.SmoothLevelProgress);
                _levelStarText.text =
                    $"{normalizedValue}/{DataManager.Ins.ConfigData.requireLevelProgress}";
            }
        }

        private bool CanClaimRewardChest()
        {
            return GameManager.Ins.SmoothRewardKeys >= DataManager.Ins.ConfigData.requireRewardKey;
        }

        private bool CanClaimLevelChest()
        {
            return GameManager.Ins.SmoothLevelProgress >= DataManager.Ins.ConfigData.requireLevelProgress;
        }

        private void OpenMask()
        {
            RectTransform rectTransform = _dailyRewardButton.GetComponent<RectTransform>();
            MaskData maskData = new MaskData()
            {
                Position = rectTransform.position,
                Size = rectTransform.sizeDelta + Vector2.one * 40f,
                MaskType = MaskType.Eclipse,
                ClickableItem = _dailyRewardButton,
                OnClickedCallback = () => UIManager.Ins.CloseUI<MaskScreen>(),
                // If you want to make mask fit and follow target
                //TargetRectTF = rectTransform
            };
            UIManager.Ins.OpenUI<MaskScreen>(maskData);
        }

        private void SetupFeature()
        {
            // Get normal level index
            int normalLevelIndex = LevelManager.Ins.NormalLevelIndex;
            _dailyChallengeButton.SetUnlockLevelIndex(DataManager.Ins.ConfigData.unlockDailyChallengeAtLevelIndex);
            _dailyChallengeButton.IsUnlocked =
                normalLevelIndex >= DataManager.Ins.ConfigData.unlockDailyChallengeAtLevelIndex;

            _secretMapButton.SetUnlockLevelIndex(DataManager.Ins.ConfigData.unlockSecretLevelAtLevelIndex);
            _secretMapButton.IsUnlocked = normalLevelIndex >= DataManager.Ins.ConfigData.unlockSecretLevelAtLevelIndex;

            _levelChestButton.SetUnlockLevelIndex(DataManager.Ins.ConfigData.unlockBonusChestAtLevelIndex);
            _levelChestButton.IsUnlocked = normalLevelIndex >= DataManager.Ins.ConfigData.unlockBonusChestAtLevelIndex;

            _rewardChestButton.SetUnlockLevelIndex(DataManager.Ins.ConfigData.unlockBonusChestAtLevelIndex);
            _rewardChestButton.IsUnlocked = normalLevelIndex >= DataManager.Ins.ConfigData.unlockBonusChestAtLevelIndex;
        }

        private void OnShowMenuTutorial()
        {
            // Get normal level index
            int normalLevelIndex = LevelManager.Ins.NormalLevelIndex;

            GameManager.Ins.PostEvent(EventID.OnShowTutorialInMenu, normalLevelIndex);
        }

        private IEnumerator OpenActionsCoroutine()
        {
            while (UIManager.Ins.IsOpened<TransitionScreen>())
            {
                yield return null;
            }

            List<Task> taskList = new List<Task>();
            if (_rewardChestButton.IsUnlocked && _delayCollectingRewardKeys != null)
            {
                Task task = _delayCollectingRewardKeys.Invoke();
                taskList.Add(task);
            }
            if (_levelChestButton.IsUnlocked && _delayCollectingLevelStars != null)
            {
                Task task = _delayCollectingLevelStars.Invoke();
                taskList.Add(task);
            }

            if (taskList.Count > 0)
            {
                _delayCollectingRewardKeys = null;
                _delayCollectingLevelStars = null;

                yield return new WaitUntil(() => Task.WhenAll(taskList).IsCompleted);
            }

            // if (!_isFirstShown)
            // {
            //     _isFirstShown = true;
            //     if (!DailyRewardManager.Ins.IsTodayRewardObtained)
            //     {
            //         UIManager.Ins.OpenUI<DailyRewardPopup>();
            //     }
            // }

            _delayOpenCoroutine = null;
        }
    }
}