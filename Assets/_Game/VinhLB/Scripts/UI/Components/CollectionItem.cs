using System;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VinhLB
{
    public class CollectionItem : HMonoBehaviour
    {
        public event Action<int, int> OnClick;

        [SerializeField]
        private RectTransform _rectTransform;
        [SerializeField]
        private HButton _button;
        [SerializeField]
        private TMP_Text _nameText;
        [SerializeField]
        private Image _contentImage;
        [SerializeField]
        private TMP_Text _priceText;
        [SerializeField]
        private GameObject _priceGO;
        [SerializeField]
        private GameObject[] _selectedGOs;
        [SerializeField]
        private GameObject _choosenGO;
        [SerializeField]
        private GameObject _lockedFrameGO;
        [SerializeField]
        private GameObject _lockedIconGO;
        [SerializeField]
        private GameObject _unlockedFrameGO;

        private bool _isLocked;
        private int _id;
        private int _data;

        public RectTransform RectTransform => _rectTransform;
        public bool IsLocked => _isLocked;
        public string UnlockInfo { get; private set; }
        public int Id => _id;
        public int Data => _data;

        private void Awake()
        {
            _button.onClick.AddListener(OnCollectionItemClick);
        }

        public void Initialize(int id, int data, string text, Sprite sprite, int price)
        {
            _id = id;
            _data = data;
            _nameText.text = text;
            _contentImage.sprite = sprite;
            _priceText.text = price.ToString(Constants.VALUE_FORMAT, CultureInfo.InvariantCulture);
        }

        private void OnCollectionItemClick()
        {
            OnClick?.Invoke(_id, _data);
        }

        public void SetLocked(bool value, string unlockInfo = null)
        {
            _isLocked = value;
            UnlockInfo = unlockInfo;
            
            if (_isLocked)
            {
                _lockedFrameGO.SetActive(true);
                _lockedIconGO.SetActive(true);
                _unlockedFrameGO.SetActive(false);
            }
            else
            {
                _lockedFrameGO.SetActive(false);
                _lockedIconGO.SetActive(false);
                _unlockedFrameGO.SetActive(true);
            }
        }

        public void SetSelected(bool value)
        {
            for (int i = 0; i < _selectedGOs.Length; i++)
            {
                _selectedGOs[i].SetActive(value);
            }

            if (!_isLocked)
            {
                _unlockedFrameGO.SetActive(!value);
            }
        }

        public void SetChosen(bool value)
        {
            _choosenGO.SetActive(value);
        }
        
        public void ShowPrice(bool value)
        {
            _priceGO.SetActive(value);
        }
    }
}