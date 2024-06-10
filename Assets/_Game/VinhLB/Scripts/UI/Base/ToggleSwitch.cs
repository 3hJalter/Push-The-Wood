using System;
using _Game.Utilities;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace VinhLB
{
    public class ToggleSwitch : ToggleButton
    {
        [Header("Setup")]
        [SerializeField]
        private Slider _slider;
        [SerializeField]
        [Range(0f, 1f)]
        private float _sliderValue;
        [SerializeField]
        private bool _showTexts = true;
        [ShowIf(nameof(_showTexts), false)]
        [SerializeField]
        private CanvasGroup _onTextCanvasGroup;
        [ShowIf(nameof(_showTexts), false)]
        [SerializeField]
        private CanvasGroup _offTextCanvasGroup;

        [Header("Animation")]
        [SerializeField]
        private bool _animated = true;
        [ShowIf(nameof(_animated), false)]
        [SerializeField]
        private float _animationDuration = 0.5f;
        [ShowIf(nameof(_animated), false)]
        [SerializeField]
        private Ease _slideEase = Ease.Linear;

        private Tween _animateSliderTween;

        public bool Animated
        {
            get => _animated;
            set => _animated = value;
        }
        
        private void OnValidate()
        {
            SetupSliderComponent();

            _slider.value = _sliderValue;

            if (_showTexts)
            {
                _onTextCanvasGroup.gameObject.SetActive(true);
                _offTextCanvasGroup.gameObject.SetActive(true);

                UpdateTexts(_sliderValue);
            }
            else
            {
                _onTextCanvasGroup.gameObject.SetActive(false);
                _offTextCanvasGroup.gameObject.SetActive(false);
            }
        }

        private void Awake()
        {
            SetupSliderComponent();
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            base.OnPointerClick(eventData);
            
            float endValue = _isOn ? 1f : 0f;
            if (_animated)
            {
                if (_animateSliderTween != null)
                {
                    DOTween.Kill(_animateSliderTween);
                }

                float startValue = _slider.value;
                _animateSliderTween = DOVirtual.Float(startValue, endValue, _animationDuration, (value) =>
                {
                    _slider.value = value;

                    if (_showTexts)
                    {
                        UpdateTexts(value);
                    }
                }).SetEase(_slideEase).OnComplete(() => { _slider.value = endValue; });
            }
            else
            {
                _slider.value = endValue;

                if (_showTexts)
                {
                    UpdateTexts(endValue);
                }
            }
        }

        private void SetupSliderComponent()
        {
            if (_slider == null)
            {
                DevLog.Log(DevId.Vinh, "No slider component found!");
                return;
            }

            _slider.interactable = false;
            ColorBlock sliderColors = _slider.colors;
            sliderColors.disabledColor = Color.white;
            _slider.colors = sliderColors;
            _slider.transition = Selectable.Transition.None;
        }

        private void UpdateTexts(float value)
        {
            if (_onTextCanvasGroup is null || _offTextCanvasGroup is null)
            {
                DevLog.Log(DevId.Vinh, "No on/off text canvas group component found!");
                return;
            }

            float normalizedValue = (value - 0.5f) * 2f;
            if (normalizedValue > 0f)
            {
                _offTextCanvasGroup.alpha = 0f;
                _onTextCanvasGroup.alpha = normalizedValue;
            }
            else
            {
                _onTextCanvasGroup.alpha = 0f;
                _offTextCanvasGroup.alpha = -normalizedValue;
            }
        }
    }
}