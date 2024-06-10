using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VinhLB
{
    public class DailyRewardItem : HMonoBehaviour
    {
        [SerializeField]
        private Button _button;
        [SerializeField]
        private TMP_Text _dayText;
        [SerializeField]
        private RewardItem _rewardItemPrefab;
        [SerializeField]
        private Transform _rewardItemParentTF;
        [SerializeField]
        private RectTransform _activeRectTF;
        [SerializeField]
        private RectTransform _inactiveRectTF;
        [SerializeField]
        private RectTransform _checkedRectTF;

        private int _day = -1;
        private Reward[] _rewards;
        private List<RewardItem> _rewardItemList = new();

        public Reward[] Rewards => _rewards;

        public void Initialize(int day, Reward[] rewards)
        {
            _day = day;
            _rewards = rewards;

            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(() =>
            {
                DailyRewardManager.Ins.ObtainTodayReward();
            });
        }

        public void UpdateVisual()
        {
            if (_day < 0 || _rewards is null)
            {
                return;
            }

            _dayText.text = $"Day {_day + 1}";

            // Adjust _rewardItemList size
            int differentInSize = _rewards.Length - _rewardItemList.Count;
            if (differentInSize > 0)
            {
                for (int i = 0; i < differentInSize; i++)
                {
                    RewardItem rewardItem = Instantiate(_rewardItemPrefab, _rewardItemParentTF);

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
            
            for (int i = 0; i < _rewards.Length; i++)
            {
                _rewardItemList[i].Initialize(_rewards[i]);
            }

            if (_day <= DailyRewardManager.Ins.CycleDay)
            {
                if (_day == DailyRewardManager.Ins.CycleDay && !DailyRewardManager.Ins.IsTodayRewardObtained)
                {
                    _button.interactable = true;
                    _activeRectTF.gameObject.SetActive(true);
                    _inactiveRectTF.gameObject.SetActive(false);
                    _checkedRectTF.gameObject.SetActive(false);
                }
                else
                {
                    _button.interactable = false;
                    _activeRectTF.gameObject.SetActive(false);
                    _inactiveRectTF.gameObject.SetActive(true);
                    _checkedRectTF.gameObject.SetActive(true);
                }
            }
            else
            {
                _button.interactable = false;
                _activeRectTF.gameObject.SetActive(false);
                _inactiveRectTF.gameObject.SetActive(false);
                _checkedRectTF.gameObject.SetActive(false);
            }
        }
    }
}