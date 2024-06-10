using System;
using System.Collections;
using System.Collections.Generic;
using _Game.Managers;
using _Game.UIs.Popup;
using AudioEnum;
using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace VinhLB
{
    public class TabButton : HMonoBehaviour, IClickable, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [Serializable]
        private enum AnimType
        {
            GoUp = 0,
            Enlarge = 1
        }

        public event Action OnClicked;

        [Header("General")]
        [SerializeField]
        private TabGroup _tabGroup;
        [SerializeField]
        private bool _interactable = true;
        [SerializeField]
        private SfxType _buttonSound = SfxType.ClickOpen;

        [Header("Animation")]
        [SerializeField]
        private bool _changeSizeWhenActive = true;
        [ShowIf(nameof(_changeSizeWhenActive), false)]
        [SerializeField]
        private LayoutElement _layoutElement;
        [SerializeField]
        private Image _background;
        [SerializeField]
        private RectTransform _activeRectTF;
        [SerializeField]
        private Image _lockedIcon;
        [SerializeField]
        private bool _showIcon = true;
        [ShowIf(nameof(_showIcon), false)]
        [SerializeField]
        private Image _activeIcon;
        [ShowIf(nameof(_showIcon), false)]
        [SerializeField]
        private Image _inactiveIcon;
        [SerializeField]
        private TMP_Text _nameText;
        [ShowIf("@this._nameText != null", false)]
        [SerializeField]
        private bool _hideNameTextWhenInactive = true;
        [ShowIf("@this._nameText != null", false)]
        [SerializeField]
        private bool _changeNameTextColor = false;
        [ShowIf("@this._nameText != null && _changeNameTextColor", false)]
        [SerializeField]
        private Color _activeNameTextColor = Color.white;
        [ShowIf("@this._nameText != null && _changeNameTextColor", false)]
        [SerializeField]
        private Color _inactiveNameTextColor = Color.white;
        [SerializeField]
        private AnimType _animType;
        [ShowIf(nameof(_animType), AnimType.GoUp, false)]
        [SerializeField]
        private float _goUpDuration = 0.2f;
        [ShowIf(nameof(_animType), AnimType.GoUp, false)]
        [SerializeField]
        private float _goUpOffset = 10f;
        [ShowIf(nameof(_animType), AnimType.Enlarge, false)]
        [SerializeField]
        private Transform _contentRectTF;
        [ShowIf("@this._animType == AnimType.GoUp || this._animType == AnimType.Enlarge", false)]
        [SerializeField]
        private float _sizeIncrease = 1.25f;

        private bool _active;
        private Tween _activeIconAnimTween;
        private Vector3 _activeIconBasePosition;

        public bool Interactable => _interactable;

        private void Awake()
        {
            _activeIconBasePosition = _activeIcon.rectTransform.anchoredPosition;
        }

        private void Start()
        {
            SetInteractable(_interactable);
        }

        public void SetInteractable(bool value)
        {
            _interactable = value;

            if (_lockedIcon == null)
            {
                return;
            }

            if (_interactable)
            {
                _lockedIcon.gameObject.SetActive(false);

                SetActiveState(_active, false);
            }
            else
            {
                SetActiveState(false, false);

                _lockedIcon.gameObject.SetActive(true);
            }
        }

        public void SetActiveState(bool value, bool animated)
        {
            _active = value;

            _activeRectTF.DOKill();
            _activeIcon.rectTransform.DOKill();
            _activeIconAnimTween = null;

            if (_active)
            {
                if (_changeSizeWhenActive)
                {
                    _layoutElement.flexibleWidth = 1f;
                }

                _activeRectTF.gameObject.SetActive(true);

                if (_showIcon)
                {
                    _inactiveIcon.gameObject.SetActive(false);
                    _activeIcon.gameObject.SetActive(true);
                }

                if (_hideNameTextWhenInactive)
                {
                    _nameText.gameObject.SetActive(true);
                }
                else
                {
                    if (_changeNameTextColor)
                    {
                        _nameText.color = _activeNameTextColor;
                    }
                }

                if (_animType == AnimType.GoUp)
                {
                    Vector3 activeIconTargetPosition = Vector3.up * (_activeIconBasePosition.y + _goUpOffset);
                    if (animated)
                    {
                        DOVirtual.Float(0f, -_goUpOffset, _goUpDuration,
                            (floatValue) => { _activeRectTF.SetPaddingTop(floatValue); }).SetEase(Ease.OutExpo);
                        
                        _nameText.color = new Color(1f, 1f, 1f, 0f); 
                        _nameText.DOColor(_activeNameTextColor, _goUpDuration).SetEase(Ease.OutExpo);

                        if (_showIcon)
                        {
                            Sequence iconSequence = DOTween.Sequence();
                            iconSequence.AppendCallback(PlayIconAnim)
                                .Join(_activeIcon.rectTransform.DOAnchorPos3D(activeIconTargetPosition, _goUpDuration))
                                .Join(_activeIcon.rectTransform.DOScale(1.25f, _goUpDuration))
                                .SetEase(Ease.OutExpo);
                        }
                    }
                    else
                    {
                        if (_showIcon)
                        {
                            _activeIcon.rectTransform.anchoredPosition = activeIconTargetPosition;
                            _activeIcon.rectTransform.localRotation = Quaternion.identity;
                            _activeIcon.rectTransform.localScale = Vector3.one * _sizeIncrease;

                            _nameText.color = _activeNameTextColor;
                        }
                    }
                }
                else if (_animType == AnimType.Enlarge)
                {
                    _contentRectTF.localScale = Vector3.one * _sizeIncrease;
                }
            }
            else
            {
                if (_changeSizeWhenActive)
                {
                    _layoutElement.flexibleWidth = 0f;
                }

                _activeRectTF.gameObject.SetActive(false);

                if (_showIcon)
                {
                    _activeIcon.gameObject.SetActive(false);
                    _inactiveIcon.gameObject.SetActive(true);
                }

                if (_hideNameTextWhenInactive)
                {
                    _nameText.gameObject.SetActive(false);
                }
                else
                {
                    if (_changeNameTextColor)
                    {
                        _nameText.color = _inactiveNameTextColor;
                    }
                }

                if (_animType == AnimType.GoUp)
                {
                    if (_showIcon)
                    {
                        _activeIcon.rectTransform.localPosition = _activeIconBasePosition;
                        _activeIcon.rectTransform.localRotation = Quaternion.identity;
                        _activeIcon.rectTransform.localScale = Vector3.one;
                    }
                }
                else if (_animType == AnimType.Enlarge)
                {
                    _contentRectTF.localScale = Vector3.one;
                }
            }
        }

        public void SetBackgroundAlpha(float value)
        {
            Color color = _background.color;
            color.a = Mathf.Clamp01(value);
            _background.color = color;
        }

        public void SetBackgroundSprite(Sprite sprite)
        {
            _background.sprite = sprite;
        }

        public void SetPreferredWidth(float value)
        {
            _layoutElement.preferredWidth = value;
        }

        public void PlayIconAnim()
        {
            if (!_showIcon)
            {
                return;
            }

            if (_activeIconAnimTween != null)
            {
                return;
            }

            _activeIconAnimTween = _activeIcon.rectTransform.DOPunchRotation(Vector3.one * 10f, 0.4f, 8, 1f)
                .OnComplete(() => { _activeIconAnimTween = null; });
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_interactable)
            {
                _tabGroup.OnTabSelected(this, true, false);

                OnClicked?.Invoke();
            }
            else
            {
                UIManager.Ins.OpenUI<NotificationPopup>(Constants.FEATURE_COMING_SOON);
            }

            AudioManager.Ins.PlaySfx(_buttonSound);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_interactable)
            {
                _tabGroup.OnTabEnter(this);
            }
            else
            {
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_interactable)
            {
                _tabGroup.OnTabExit(this);
            }
            else
            {
            }
        }
    }
}