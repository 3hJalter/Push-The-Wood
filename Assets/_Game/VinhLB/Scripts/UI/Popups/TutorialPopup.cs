using System;
using GameGridEnum;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VinhLB
{
    public class TutorialPopup : UICanvas
    {
        [SerializeField]
        private TutorialUIData _tutorialUIData;

        [SerializeField]
        private Image _tutorialImage;
        [SerializeField]
        private TMP_Text _descriptionText;
        [SerializeField]
        private Button _leftButton;
        [SerializeField]
        private Button _rightButton;

        private TutorialUI[] _tutorialUIs;
        private int _currentTutorialUIIndex;

        private void Awake()
        {
            _leftButton.onClick.AddListener(() =>
            {
                _currentTutorialUIIndex = Mathf.Clamp(_currentTutorialUIIndex - 1, 0, _tutorialUIs.Length);
                
                UpdateTutorialUI();
            });
            _rightButton.onClick.AddListener(() =>
            {
                _currentTutorialUIIndex = Mathf.Clamp(_currentTutorialUIIndex + 1, 0, _tutorialUIs.Length);
                
                UpdateTutorialUI();
            });
        }

        public override void Setup(object param = null)
        {
            base.Setup(param);

            if (param is TutorialType tutorialType)
            {
                SetupTutorialUI(tutorialType);
            }
            else
            {
                SetupTutorialUI((TutorialType)0);
            }
        }

        private void SetupTutorialUI(TutorialType tutorialType)
        {
            _tutorialUIs = _tutorialUIData.TutorialUIDict[tutorialType];

            _currentTutorialUIIndex = 0;
            
            UpdateTutorialUI();
        }

        private void UpdateTutorialUI()
        {
            _tutorialImage.sprite = _tutorialUIs[_currentTutorialUIIndex].TutorialSprite;
            _descriptionText.text = _tutorialUIs[_currentTutorialUIIndex].Description;

            if (_currentTutorialUIIndex == 0)
            {
                _leftButton.gameObject.SetActive(false);
            }
            else if (_currentTutorialUIIndex == _tutorialUIs.Length - 1)
            {
                _rightButton.gameObject.SetActive(false);
            }
            else
            {
                _leftButton.gameObject.SetActive(true);
                _rightButton.gameObject.SetActive(true);
            }
        }
    }
}