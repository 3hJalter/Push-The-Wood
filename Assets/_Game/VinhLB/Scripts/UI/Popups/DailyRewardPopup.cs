using System;
using System.Collections;
using System.Collections.Generic;
using _Game.DesignPattern;
using _Game.Managers;
using _Game.UIs.Popup;
using _Game.Utilities;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace VinhLB
{
    public class DailyRewardPopup : UICanvas
    {
        [SerializeField]
        private Button _claimButton;
        [SerializeField]
        private Button _fakeClaimButton;
        [SerializeField]
        private List<DailyRewardItem> _dailyRewardItemList;

        private void Awake()
        {
            _claimButton.onClick.AddListener(() =>
            {
                if (DailyRewardManager.Ins.IsTodayRewardObtained)
                {
                    DevLog.Log(DevId.Vinh, "You have claimed today reward");
                    UIManager.Ins.OpenUI<NotificationPopup>("You have claimed today reward");
                }
                else
                {
                    DailyRewardManager.Ins.ObtainTodayReward();
                    Close();
                }
            });

            DailyRewardManager.Ins.OnDailyRewardParamsChanged += DailyRewardManager_OnDailyRewardParamsChanged;
        }

        public override void Setup(object param = null)
        {
            base.Setup(param);

            UpdateVisual();
        }

        private void UpdateVisual()
        {
            SetupDailyRewards();

            bool claimable = !DailyRewardManager.Ins.IsTodayRewardObtained;
            _claimButton.gameObject.SetActive(claimable);
            _fakeClaimButton.gameObject.SetActive(!claimable);
        }

        private void SetupDailyRewards()
        {
            for (int i = 0; i < DailyRewardManager.Ins.DailyRewardSettings.CycleDays; i++)
            {
                if (_dailyRewardItemList[i].Rewards != DailyRewardManager.Ins.Rewards[i])
                {
                    _dailyRewardItemList[i].Initialize(i, DailyRewardManager.Ins.Rewards[i]);
                }

                _dailyRewardItemList[i].UpdateVisual();
            }
        }

        private void DailyRewardManager_OnDailyRewardParamsChanged()
        {
            UpdateVisual();
        }
    }
}