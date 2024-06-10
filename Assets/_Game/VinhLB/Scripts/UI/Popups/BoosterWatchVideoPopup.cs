using System;
using _Game._Scripts.Managers;
using _Game.Data;
using _Game.Managers;
using _Game.Utilities;
using System.Collections.Generic;
using _Game.Resource;
using _Game.UIs.Popup;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace VinhLB
{
    public class BoosterWatchVideoPopup : UICanvas
    {
        [SerializeField]
        private Image _boosterIcon;
        [FormerlySerializedAs("_boosterText")]
        [SerializeField]
        private TMP_Text _boosterNameText;
        [SerializeField]
        private TMP_Text _boosterAmountText;
        [SerializeField]
        private Image _currencyIcon;
        [SerializeField]
        private TMP_Text _currencyAmountText;
        [SerializeField]
        private TMP_Text _videoAmountText;
        [SerializeField]
        private HButton _buyButton;

        private BoosterConfig _boosterConfig;
        
        public override void Setup(object param = null)
        {
            base.Setup(param);
            // Cast param to BoosterConfig
            if (param != null)
            {
                _boosterConfig = (BoosterConfig)param;
                
                // _currencyAmountTextContentSizeFitter.enabled = false;
            }
            GameManager.Ins.ChangeState(GameState.Pause);
            
            //_buyButton.gameObject.SetActive(GameManager.Ins.AdTickets >= _boosterConfig.TicketPerBuyRatio.ticketNeed);
        }

        public override void Open(object param = null)
        {
            base.Open(param);

            UIManager.Ins.OpenUI<StatusBarScreen>(true);
        }
        
        public override void Close()
        {
            UIManager.Ins.CloseUI<StatusBarScreen>();
            GameManager.Ins.ChangeState(GameState.InGame);
            base.Close();
        }

        public override void UpdateUI()
        {
            base.UpdateUI();
            // Change the _boosterIcon to boosterConfig.icon & _boosterText to boosterConfig.name
            _boosterIcon.sprite = _boosterConfig.MainIcon;
            _boosterNameText.text = _boosterConfig.Name;
            _boosterAmountText.text = $"x{_boosterConfig.GoldPerBuyRatio.itemsPerBuy}";
            _currencyAmountText.text = _boosterConfig.GoldPerBuyRatio.goldNeed.ToString(("#,#"));
            _videoAmountText.text = "CLAIM";

            //switch (_boosterConfig.Type)
            //{
            //    case BoosterType.PushHint:
            //        _videoAmountText.text = $"CLAIM({DataManager.Ins.HintAdsCount}/{_boosterConfig.TicketPerBuyRatio.ticketNeed})";
            //        break;
            //    default:
            //        _videoAmountText.text = "CLAIM";
            //        break;
            //}
        }
        
        public void OnClickBuyButton()
        {
            if (GameManager.Ins.TrySpendGold(_boosterConfig.GoldPerBuyRatio.goldNeed))
            {
                EventGlobalManager.Ins.OnChangeBoosterAmount.Dispatch(_boosterConfig.Type, _boosterConfig.GoldPerBuyRatio.itemsPerBuy);
                Close();
            }
            else
            {
                UIManager.Ins.OpenUI<NotificationPopup>("You do not have enough Gold!");
            }
        }

        public void OnClickClaimButton()
        {
            AdsManager.Ins.RewardedAds.Show(null, null, new List<BoosterType> { _boosterConfig.Type },
                new List<int> { _boosterConfig.GoldPerBuyRatio.itemsPerBuy});
            Close();          
        }
    }
}