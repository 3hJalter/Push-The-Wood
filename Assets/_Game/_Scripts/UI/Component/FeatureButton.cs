using System;
using TMPro;
using UnityEngine;
using VinhLB;

namespace _Game._Scripts.UIs.Component
{
    [RequireComponent(typeof(HButton))]
    public class FeatureButton : HMonoBehaviour, IClickable
    {
        public event Action OnClicked;
        
        [SerializeField]
        private HButton _button;
        [SerializeField]
        private GameObject _lockedGO;
        [SerializeField]
        private GameObject _unlockGO;
        [SerializeField]
        private TMP_Text _unlockedLevelText;
        [SerializeField]
        private GameObject _notificationGO;
        [SerializeField]
        private TMP_Text _notificationText;
        
        private bool _isUnlocked;
        private int _unlockedLevelIndex = -1;

        public void AddListener(UnityEngine.Events.UnityAction action)
        {
            _button.onClick.AddListener(action);
        }

        public void RemoveListener(UnityEngine.Events.UnityAction action)
        {
            _button.onClick.RemoveListener(action);
        }

        public bool IsUnlocked
        {
            get => _isUnlocked;
            set
            {
                _isUnlocked = value;
                if (_isUnlocked)
                {
                    _button.interactable = true;
                    _unlockGO.SetActive(true);
                    _lockedGO.SetActive(false);
                }
                else
                {
                    _button.interactable = false;
                    _unlockGO.SetActive(false);
                    _lockedGO.SetActive(true);
                }
            }
        }
        
        public void SetUnlockLevelIndex(int levelIndex)
        {
            if (levelIndex < 0 && levelIndex == _unlockedLevelIndex)
            {
                return;
            }
            
            _unlockedLevelIndex = levelIndex;
            
            _unlockedLevelText.text = $"Lv.{_unlockedLevelIndex + 1}";
        }

        public void SetNotificationAmount(int value)
        {
            if (value <= 0) // Turn off if value <= 0
            {
                _notificationGO.SetActive(false);
            }
            else // Turn on if value > 0
            {
                if (!_notificationGO.activeSelf)
                {
                    _notificationGO.SetActive(true);
                }

                _notificationText.text = value <= 9 ? $"{value}" : "9+";
            }
        }
    }
}