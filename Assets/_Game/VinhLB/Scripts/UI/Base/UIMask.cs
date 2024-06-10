using System;
using Coffee.UIExtensions;
using UnityEngine;
using UnityEngine.UI;

namespace VinhLB
{
    public class UIMask : HMonoBehaviour
    {
        [SerializeField]
        private RectTransform _rectTransform;
        [SerializeField]
        private Image _image;
        [SerializeField]
        private Unmask _unmask;

        private IClickable _clickableItem;
        private Action _onClicked;
        private UnmaskRaycastFilter _unmaskRaycastFilter;

        public UnmaskRaycastFilter UnmaskRaycastFilter => _unmaskRaycastFilter;

        public void Initialize(Vector3 position, Vector2 size, Sprite sprite, IClickable clickableItem,
            Action onClicked, RectTransform targetRectTF, UnmaskRaycastFilter unmaskRaycastFilter)
        {
            _rectTransform.position = position;
            _rectTransform.sizeDelta = size;
            _image.sprite = sprite;

            _clickableItem = clickableItem;
            _onClicked = onClicked;
            if (_clickableItem != null)
            {
                _clickableItem.OnClicked += _onClicked;
            }

            if (targetRectTF != null)
            {
                _unmask.fitTarget = targetRectTF;
            }

            if (unmaskRaycastFilter != null && unmaskRaycastFilter != _unmaskRaycastFilter)
            {
                _unmaskRaycastFilter = unmaskRaycastFilter;
                _unmaskRaycastFilter.targetUnmask = _unmask;
                _unmaskRaycastFilter.enabled = true;
            }
        }

        public void Open()
        {
            gameObject.SetActive(true);
        }

        public void Close()
        {
            if (_clickableItem != null)
            {
                _clickableItem.OnClicked -= _onClicked;
            }

            if (_unmaskRaycastFilter != null)
            {
                _unmaskRaycastFilter.enabled = false;
            }

            gameObject.SetActive(false);
        }
    }
}