using System.Collections.Generic;
using _Game.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Game._Scripts.UIs.Component
{
    public class DailyChallengeButton : HButton
    {
        [SerializeField] private Image btnImage;
        [SerializeField] private Image iconComplete;
        public DailyChallengeButtonState State { get; private set; }
        public int Index { get; private set; }

        private TextMeshProUGUI _dayText;
        
        public List<Sprite> btnSprites;
        public Image hoverImg;

        private TextMeshProUGUI DayText => _dayText? _dayText : _dayText = GetComponentInChildren<TextMeshProUGUI>();

        public void OnHover()
        {
            // Change button sprite
            hoverImg.enabled = true;
        }
        
        public void OnUnHover()
        {
            // Change button sprite
            hoverImg.enabled = false;
        }
        
        public void SetIndex(int index, int currentDay)
        {
            Index = index;
            DayText.text = (index + 1).ToString();
            if (index > currentDay - 1) // Index can be 0, but day start from 1
            {
                State = DailyChallengeButtonState.NotYet;
            } else if (DataManager.Ins.IsClearDailyChallenge(index))
            {
                State = DailyChallengeButtonState.Clear;
            } else if (index == currentDay - 1)
            {
                State = DailyChallengeButtonState.Today;
            } else
            {
                State = DailyChallengeButtonState.UnClear;
            }
            OnChangeState();
        }

        private void OnChangeState()
        {
            // Change button sprite
            btnImage.sprite = btnSprites[(int) State];
            iconComplete.enabled = State == DailyChallengeButtonState.Clear;
        }
    }
    
    public enum DailyChallengeButtonState
    {
        NotYet = 0,
        UnClear = 1,
        Clear = 2,
        Today = 3,
    }
}
