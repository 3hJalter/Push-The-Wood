using System;
using System.Collections;
using System.Collections.Generic;
using _Game._Scripts.InGame;
using _Game.Data;
using _Game.DesignPattern;
using _Game.GameGrid;
using _Game.Managers;
using _Game.Resource;
using _Game.UIs.Popup;
using AudioEnum;
using DG.Tweening;
using GameGridEnum;
using UnityEngine;
using UnityEngine.UI;
using VinhLB;
using UnityRandom = UnityEngine.Random;

namespace _Game.UIs.Screen
{
    public class WinScreen : UICanvas
    {
        [SerializeField]
        private GameObject _container;
        [SerializeField]
        private CanvasGroup _canvasGroup;
        [SerializeField]
        private Image _blockPanel;
        [SerializeField]
        private Image _contentImage;
        [SerializeField]
        private RectTransform _rewardParentRectTF;
        [SerializeField]
        private RewardItem _rewardItemPrefab;
        [SerializeField]
        private HButton _claimX2Button;
        [SerializeField]
        private HButton _fakeClaimX2Button;
        [SerializeField]
        private HButton _continueButton;

        private Action _onContinueClick;
        private List<RewardItem> _rewardItemList = new List<RewardItem>();
        private Coroutine _spawnEffectsCoroutine;
        private List<ParticleSystem> _spawnedEffects = new List<ParticleSystem>();

        private void Awake()
        {
            _claimX2Button.onClick.AddListener(OnClaimX2Click);

            // GameManager.Ins.RegisterListenerEvent(EventID.OnChangeLayoutForBanner, ChangeLayoutForBanner);
            // ChangeLayoutForBanner(AdsManager.Ins.IsBannerOpen);
        }

        private void OnDestroy()
        {
            // GameManager.Ins.UnregisterListenerEvent(EventID.OnChangeLayoutForBanner, ChangeLayoutForBanner);
        }

        public override void Setup(object param = null)
        {
            base.Setup(param);

            if (param is true)
            {
                _canvasGroup.alpha = 1f;
                _blockPanel.gameObject.SetActive(false);
            }
            else
            {
                _canvasGroup.alpha = 0f;
                _blockPanel.gameObject.SetActive(true);
            }

            UpdateVisual((Reward[])param);
            DefineActionOnContinueButton();
        }

        public override void Open(object param = null)
        {
            base.Open(param);

            _claimX2Button.gameObject.SetActive(true);
            _fakeClaimX2Button.gameObject.SetActive(false);

            _container.SetActive(false);
            for (int i = 0; i < _rewardItemList.Count; i++)
            {
                _rewardItemList[i].Reward.Obtain(_rewardItemList[i].IconImagePosition);
            }
            DOVirtual.DelayedCall(1f, () =>
            {
                _container.SetActive(true);
                AudioManager.Ins.PlaySfx(SfxType.Win);

                if (param is not true)
                {
                    DOVirtual.Float(0f, 1f, 1f, value => _canvasGroup.alpha = value)
                        .OnComplete(() =>
                        {
                            _blockPanel.gameObject.SetActive(false);

                            if (_spawnEffectsCoroutine == null)
                            {
                                _spawnEffectsCoroutine = StartCoroutine(SpawnEffectsCoroutine());
                            }
                        });
                }
            });
        }

        public override void Close()
        {
            base.Close();

            if (_spawnEffectsCoroutine != null)
            {
                StopCoroutine(_spawnEffectsCoroutine);
                _spawnEffectsCoroutine = null;

                for (int i = 0; i < _spawnedEffects.Count; i++)
                {
                    _spawnedEffects[i].Stop();
                    _spawnedEffects[i].Clear();
                }
                _spawnedEffects.Clear();
            }
        }

        private void DefineActionOnContinueButton()
        {
            _continueButton.onClick.RemoveAllListeners();
            // Hide the next level button if the current level is not Normal level
            Level level = LevelManager.Ins.CurrentLevel;

            if (level.LevelType is LevelType.Normal &&
                level.Index < DataManager.Ins.ConfigData.unlockBonusChestAtLevelIndex - 1)
            {
                _onContinueClick = OnGoNextLevel;
            }
            else
            {
                if (AdsManager.Ins.IsCanShowInter)
                {
                    _onContinueClick = OnGoMenu;
                }
                else
                {
                    _onContinueClick = null;
                    _continueButton.onClick.AddListener(OnGoMenu);
                }
            }
            _continueButton.onClick.AddListener(() => OnContinueClick(_onContinueClick));
        }
        
        private void UpdateVisual(Reward[] rewards)
        {
            _contentImage.sprite = DataManager.Ins.UIResourceDatabase.WinScreenResourceConfigDict[
                LevelManager.Ins.CurrentLevel.LevelWinCondition].MainIconSprite;

            if (rewards != null && rewards.Length > 0)
            {
                _rewardParentRectTF.gameObject.SetActive(true);
                // Adjust _rewardItemList size
                int differentInSize = rewards.Length - _rewardItemList.Count;
                if (differentInSize > 0)
                {
                    for (int i = 0; i < differentInSize; i++)
                    {
                        RewardItem rewardItem = Instantiate(_rewardItemPrefab, _rewardParentRectTF);

                        _rewardItemList.Add(rewardItem);
                    }
                }
                else if (differentInSize < 0)
                {
                    int startIndex = _rewardItemList.Count - 1;
                    for (int i = startIndex; i > startIndex + differentInSize; i--)
                    {
                        Destroy(_rewardItemList[i].gameObject);

                        _rewardItemList.RemoveAt(i);
                    }
                }

                // Initialize reward item
                for (int i = 0; i < rewards.Length; i++)
                {
                    _rewardItemList[i].Initialize(rewards[i]);
                }
            }
            else
            {
                _rewardParentRectTF.gameObject.SetActive(false);
            }
        }

        private void ChangeLayoutForBanner(object isBannerActive)
        {
            int sizeAnchor = (bool)isBannerActive ? DataManager.Ins.ConfigData.bannerHeight : 0;
            MRectTransform.offsetMin = new Vector2(MRectTransform.offsetMin.x, sizeAnchor);
        }

        private void OnClaimX2Click()
        {
            if (!_claimX2Button.gameObject.activeSelf)
            {
                return;
            }

            AdsManager.Ins.RewardedAds.Show(UpdateX2Reward, Ads.Placement.Win_Popup);

            void UpdateX2Reward()
            {
                UpdateVisual(GameplayManager.Ins.GetWinGameRewards(true));
                
                _claimX2Button.gameObject.SetActive(false);
                _fakeClaimX2Button.gameObject.SetActive(true);
                
                for (int i = 0; i < _rewardItemList.Count; i++)
                {
                    if (_rewardItemList[i].Reward.RewardType == RewardType.Currency &&
                        _rewardItemList[i].Reward.CurrencyType == CurrencyType.Gold)
                    {
                        // We need double gold visual but we receive first half gold amount
                        // when the win screen open so we just receive other half gold amount
                        _rewardItemList[i].Reward.Amount /= 2;
                        _rewardItemList[i].Reward.Obtain(_rewardItemList[i].IconImagePosition);
                    }
                }
            }
        }

        private void OnGoNextLevel()
        {
            LevelManager.Ins.OnNextLevel(LevelType.Normal);
            Close();
            // GameManager.Ins.PostEvent(DesignPattern.EventID.OnCheckShowInterAds, _onContinueClick);   
        }

        private void OnContinueClick(Action action)
        {
            GameManager.Ins.PostEvent(EventID.OnCheckShowInterAds, action);
            _continueButton.onClick.RemoveAllListeners();
        }

        private void OnGoMenu()
        {
            LevelType type = LevelManager.Ins.CurrentLevel.LevelType;
            UIManager.Ins.CloseAll();
            TransitionScreen ui = UIManager.Ins.OpenUI<TransitionScreen>();
            ui.OnOpenCallback += () =>
            {
                LevelManager.Ins.OnGoLevel(LevelType.Normal, LevelManager.Ins.NormalLevelIndex, false);
                UIManager.Ins.OpenUI<MainMenuScreen>();
                if (type is LevelType.DailyChallenge && !DataManager.Ins.IsCollectedAllDailyChallengeReward())
                {
                    UIManager.Ins.OpenUI<DailyChallengePopup>();
                }
            };
        }

        private IEnumerator SpawnEffectsCoroutine()
        {
            while (true)
            {
                Vector3 randomViewportPoint = new Vector3(
                    UnityRandom.Range(0.15f, 0.85f),
                    UnityRandom.Range(0.75f, 0.95f));
                Vector3 spawnPosition = CameraManager.Ins.ViewportToWorldPoint(randomViewportPoint);
                // Debug.Log($"{randomViewportPoint} | {spawnPosition}");
                ParticleSystem spawnedEffect = ParticlePool.Play(
                    DataManager.Ins.VFXData.GetParticleSystem(VFXType.Confetti),
                    spawnPosition, Quaternion.identity);
                if (!_spawnedEffects.Contains(spawnedEffect))
                {
                    _spawnedEffects.Add(spawnedEffect);
                }

                int confettiType = (int)SfxType.Confetti1;
                AudioManager.Ins.PlaySfx((SfxType)(confettiType + UnityRandom.Range(0, 3)));

                yield return new WaitForSeconds(UnityRandom.Range(1.5f, 2.5f));
            }
        }
    }
}