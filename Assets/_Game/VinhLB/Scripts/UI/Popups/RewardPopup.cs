using System;
using System.Collections.Generic;
using _Game.Managers;
using _Game.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace VinhLB
{
    public class RewardPopup : UICanvas, IClickable
    {
        public event Action OnClicked;
        
        [SerializeField]
        private RewardItem _rewardItemPrefab;
        [SerializeField]
        private ScrollRect _rewardScrollRect;
        [SerializeField]
        private RectTransform _rewardContentRectTF;
        [SerializeField]
        private GameObject _leftArrowGO;
        [SerializeField]
        private GameObject _rightArrowGO;
        
        private List<RewardItem> _rewardItemList = new List<RewardItem>();
        
        private void Awake()
        {
            _rewardScrollRect.onValueChanged.AddListener(OnRewardScrollRectValueChanged);
        }

        private void OnDestroy()
        {
            _rewardScrollRect.onValueChanged.RemoveListener(OnRewardScrollRectValueChanged);
        }

        public override void Setup(object param = null)
        {
            base.Setup(param);

            if (param is not Reward[] rewards)
            {
                return;
            }
            
            if (rewards.Length > 0)
            {
                // Adjust rewards parent
                if (rewards.Length > 3)
                {
                    _rewardScrollRect.enabled = true;
                    _rewardContentRectTF.anchorMin = new Vector2(0f, _rewardContentRectTF.anchorMin.y);
                    _rewardContentRectTF.anchorMax = new Vector2(0f, _rewardContentRectTF.anchorMax.y);
                    _rewardContentRectTF.pivot = new Vector2(0f, 0.5f);
                    _rewardContentRectTF.anchoredPosition = Vector2.zero;
                }
                else
                {
                    _rewardScrollRect.enabled = false;
                    _rewardContentRectTF.anchorMin = new Vector2(0.5f, _rewardContentRectTF.anchorMin.y);
                    _rewardContentRectTF.anchorMax = new Vector2(0.5f, _rewardContentRectTF.anchorMax.y);
                    _rewardContentRectTF.pivot = new Vector2(0.5f, 0.5f);
                    _rewardContentRectTF.anchoredPosition = Vector2.zero;
                    
                    _leftArrowGO.SetActive(false);
                    _rightArrowGO.SetActive(false);
                }
                
                // Adjust _rewardItemList size
                int differentInSize = rewards.Length - _rewardItemList.Count;
                if (differentInSize > 0)
                {
                    for (int i = 0; i < differentInSize; i++)
                    {
                        RewardItem rewardItem = Instantiate(_rewardItemPrefab, _rewardContentRectTF);
                        
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
        }
        public override void Open(object param = null)
        {
            base.Open(param);
            AudioManager.Ins.PlaySfx(AudioEnum.SfxType.CollectReward);
        }
        public override void Close()
        {
            base.Close();
            
            DevLog.Log(DevId.Vinh, "Collect rewards");
            for (int i = 0; i < _rewardItemList.Count; i++)
            {
                _rewardItemList[i].Reward.Obtain(_rewardItemList[i].IconImagePosition);
            }
                
            OnClicked?.Invoke();
            OnClicked = null;
        }

        private void OnRewardScrollRectValueChanged(Vector2 value)
        {
            if (_rewardScrollRect.horizontalNormalizedPosition < 0.05f)
            {
                _leftArrowGO.SetActive(false);
            }
            else
            {
                _leftArrowGO.SetActive(true);
            }

            if (_rewardScrollRect.horizontalNormalizedPosition > 0.95f)
            {
                _rightArrowGO.SetActive(false);
            }
            else
            {
                _rightArrowGO.SetActive(true);
            }
        }
    }
}