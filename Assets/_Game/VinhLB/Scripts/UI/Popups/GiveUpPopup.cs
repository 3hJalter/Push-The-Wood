using System;
using _Game.Managers;
using UnityEngine;

namespace VinhLB
{
    public class GiveUpPopup : UICanvas
    {
        [SerializeField]
        private HButton _giveUpButton;

        private Action _onGiveUpClick;

        private void Awake()
        {
            _giveUpButton.onClick.AddListener(OnGiveUpClick);
        }

        public override void Open(object param = null)
        {
            base.Open(param);
            
            // UIManager.Ins.OpenUI<StatusBarScreen>(true);

            _onGiveUpClick = null;
            if (param is Action action)
            {
                _onGiveUpClick = action;
            }
        }

        public override void Close()
        {
            // UIManager.Ins.CloseUI<StatusBarScreen>();
            
            base.Close();
        }

        private void OnGiveUpClick()
        {
            GameManager.Ins.GainHeart(-1);
            
            _onGiveUpClick.Invoke();

            Close();
        }
    }
}