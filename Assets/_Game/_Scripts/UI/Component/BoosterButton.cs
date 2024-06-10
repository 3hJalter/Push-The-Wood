using System;
using _Game.Resource;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Game._Scripts.UIs.Component
{
    [RequireComponent(typeof(HButton))]
    public class BoosterButton : HMonoBehaviour
    {
        [SerializeField] private BoosterType type;

        public BoosterType Type => type;

        private bool _isLock;
        private bool _isInteractable;
        private bool _isFocus;
        private bool _isShowAmount = true;
        private bool _isShowAds;
        
        public bool IsLock
        {
            get => _isLock;
            set
            {
                _isLock = value;
                button.interactable = !value;
                if (_isLock)
                {
                    adsRequireImage.gameObject.SetActive(false);
                    amountFrame.gameObject.SetActive(false);
                }
                if (!HasAlternativeImage)
                {
                    lockImage.gameObject.SetActive(value);
                }
                else alternativeLockImage.gameObject.SetActive(value);
            }
        }
        
        public bool IsInteractable
        {
            get => _isInteractable;
            set
            {
                if (_isLock) value = false;
                _isInteractable = value;
                button.interactable = value;
                if (!_isInteractable) adsRequireImage.gameObject.SetActive(false);
                if (!HasAlternativeImage) unInteractableImage.gameObject.SetActive(!value);
                else alternativeUnInteractableImage.gameObject.SetActive(!value);
            }
        }
        
        public bool IsFocus
        {
            get => _isFocus;
            set
            {
                _isFocus = value;
                 if (focusImage) focusImage.SetActive(value);
            }
        }
        
        public bool IsShowAmount
        {
            get => _isShowAmount;
            set
            {
                _isShowAmount = value;
                amountFrame.gameObject.SetActive(value && !_isLock);
            }
        }
        
        public bool IsShowAds
        {
            get => _isShowAds;
            set
            {
                _isShowAds = value;
                adsRequireImage.gameObject.SetActive(value);
            }
        }
        
        public bool HasAlternativeImage
        {
            get => hasAlternativeImage;
            set
            {
                hasAlternativeImage = value;
                image.gameObject.SetActive(!value);
                lockImage.gameObject.SetActive(!value && _isLock);
                unInteractableImage.gameObject.SetActive(!value && !_isInteractable);
                alternativeImage.gameObject.SetActive(value);
                alternativeLockImage.gameObject.SetActive(value && _isLock);
                alternativeUnInteractableImage.gameObject.SetActive(value && !_isInteractable);
            }
        }
        
        [SerializeField] private HButton button;
        
        public HButton Button => button;
        
        [Title("Focus")]
        [SerializeField] private GameObject focusImage;

        [Title("Set Lock & Interactable")] 
        [SerializeField] private Image image;
        [SerializeField] private Image lockImage;
        [SerializeField] private Image unInteractableImage;
        [SerializeField] 
        private TMP_Text _unlockLevelText;
        
        [SerializeField] private bool hasAlternativeImage;
        [ShowIf("hasAlternativeImage")]
        [SerializeField] private Image alternativeImage;
        [ShowIf("hasAlternativeImage")]
        [SerializeField] private Image alternativeLockImage;
        [ShowIf("hasAlternativeImage")]
        [SerializeField] private Image alternativeUnInteractableImage;
        [ShowIf("hasAlternativeImage")]
        [SerializeField] 
        private TMP_Text _alternativeUnlockLevelText;
        
        [Title("Monetize")]
        [SerializeField] private RectTransform amountFrame;
        [SerializeField] private TMP_Text amountText;

        public TMP_Text AmountText => amountText;

        [SerializeField] private Image adsRequireImage;
        
        private event Action OnClick;

        private void Awake()
        {
            button = GetComponent<HButton>();
            button.onClick.AddListener(OnClickEvent);
        }
        
        public void AddEvent(Action action)
        {
            OnClick += action;
        }
        
        public void RemoveEvent(Action action)
        {
            OnClick -= action;
        }
        
        private void OnClickEvent()
        {
            if (_isLock || !_isInteractable) return;
            OnClick?.Invoke();
        }
        
        public void SetActive(bool value)
        {
            gameObject.SetActive(value);
        }
        
        public void SetAmount(int amount)
        {
            amountText.text = amount.ToString();
            if (amount <= 0)
            {
                amountFrame.gameObject.SetActive(false);
                adsRequireImage.gameObject.SetActive(_isInteractable);
            }
            else
            {
                amountFrame.gameObject.SetActive(_isShowAmount && !_isLock);
                adsRequireImage.gameObject.SetActive(false);
            }
        }

        public void SetUnlockLevel(int unlockLevelIndex)
        {
            if (_unlockLevelText != null)
            {
                _unlockLevelText.text = $"Lv.{unlockLevelIndex + 1}";
            }
            if (_alternativeUnlockLevelText != null)
            {
                _alternativeUnlockLevelText.text = $"Lv.{unlockLevelIndex + 1}";
            }
        }
    }
}
