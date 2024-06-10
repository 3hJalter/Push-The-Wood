using System;
using System.Collections.Generic;
using _Game.Data;
using _Game.DesignPattern;
using _Game.Managers;
using _Game.Resource;
using _Game.UIs.Popup;
using _Game.Utilities;
using UnityEngine;
using VinhLB;

namespace _Game._Scripts.UIs.Component
{
    [RequireComponent(typeof(HButton))]
    public class DailyChallengeRewardButton : MonoBehaviour
    {
        [SerializeField] private HButton hButton;
        [SerializeField] private GameObject lockIcon;
        [SerializeField] private GameObject unlockIcon;
        [SerializeField] private GameObject claimedIcon;

        private DailyChallengePopup _popup;
        private DailyChallengeRewardMilestone _milestone;
        private DcRewardState _state;

        
        [SerializeField] private RewardItem rewardItemPrefab;
        [SerializeField] private Transform rewardItemContainer;
        
        private MiniPool<RewardItem> rewardItemPool;

        private RectTransform _containerRect;
        private float _initializeContainerWidth;

        private const float ITEM_WIDTH = 42.5f;
        
        public DcRewardState State
        {
            get => _state;
            set
            {
                _state = value;
                OnChangeState();
            }
        }

        private void Awake()
        {
            _containerRect = rewardItemContainer.GetComponent<RectTransform>();
            _initializeContainerWidth = _containerRect.sizeDelta.x;
            hButton.onClick.AddListener(OnClick);
            rewardItemContainer.gameObject.SetActive(false);
            rewardItemPool = new MiniPool<RewardItem>();
            rewardItemPool.OnInit(rewardItemPrefab, 3, rewardItemContainer);
        }
        
        private void OnDestroy()
        {
            hButton.onClick.RemoveAllListeners();
        }

        public void OnInit(DailyChallengePopup popup, DailyChallengeRewardMilestone milestone)
        {
            _popup = popup;
            _milestone = milestone;
            // Verify state
            int clearCount = DataManager.Ins.GameData.user.dailyLevelIndexComplete.Count;
            if (clearCount >= _milestone.clearLevelNeed)
            {
                State = DataManager.Ins.GameData.user.dailyChallengeRewardCollected.Contains(_milestone.clearLevelNeed) ? DcRewardState.Claimed : DcRewardState.Unlock;
            }
            else
            {
                State = DcRewardState.Lock;
            } 
        }

        public void OnRelease()
        {
            rewardItemContainer.gameObject.SetActive(false);
            rewardItemPool.Collect();
        }
        
        private void OnClick()
        {
            if (_popup.LastClickRewardButton)
            {
                _popup.LastClickRewardButton.OnRelease();
            }
            if (State is DcRewardState.Lock)
            {
                _popup.LastClickRewardButton = this;
                rewardItemContainer.gameObject.SetActive(true);
                Reward[] rewards = IsCanCollectFirstReward ? _milestone.firstRewards : _milestone.rewards;
                for (int i = 0; i < rewards.Length; i++)
                {
                    rewardItemPool.Spawn().Initialize(rewards[i]);
                }
                // add more width for container
                _containerRect.sizeDelta = new Vector2(_initializeContainerWidth + (rewards.Length - 1) * ITEM_WIDTH, _containerRect.sizeDelta.y);
            }
            else if (State is DcRewardState.Unlock)
            {
                State = DcRewardState.Claimed;
                UIManager.Ins.OpenUI<RewardPopup>(GetReward()).OnClicked += () =>
                {
                    DataManager.Ins.GameData.user.dailyChallengeRewardCollected.Add(_milestone.clearLevelNeed);
                    DataManager.Ins.Save();
                    GameManager.Ins.PostEvent(EventID.OnUpdateUI);
                };
            }
        }

        private void OnChangeState()
        {
            lockIcon.SetActive(State == DcRewardState.Lock);
            unlockIcon.SetActive(State == DcRewardState.Unlock);
            claimedIcon.SetActive(State == DcRewardState.Claimed);
            hButton.interactable = State is DcRewardState.Unlock or DcRewardState.Lock;
        }
        
        private bool IsCanCollectFirstReward => _milestone.hasFirstReward &&
                   !DataManager.Ins.GameData.user.isCollectDailyChallengeRewardOneTime.Contains(_milestone.clearLevelNeed);
        
        private Reward[] GetReward()
        {
            if (!_milestone.hasFirstReward ||
                DataManager.Ins.GameData.user.isCollectDailyChallengeRewardOneTime.Contains(_milestone.clearLevelNeed))
                return _milestone.rewards;
            DataManager.Ins.GameData.user.isCollectDailyChallengeRewardOneTime.Add(_milestone.clearLevelNeed);
            return _milestone.firstRewards;
        }
    }

    public enum DcRewardState
    {
        Lock = 0,
        Unlock = 1,
        Claimed = 2,
    }
}
