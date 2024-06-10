using System;
using System.Collections.Generic;
using _Game._Scripts.InGame;
using _Game.DesignPattern;
using _Game.GameGrid;
using _Game.Managers;
using HControls;
using MEC;
using UnityEngine;
using UnityEngine.UI;
using VinhLB;

namespace _Game.UIs.Screen
{
    public class SaveMeScreen : UICanvas
    {
        private const float TIMER = 3f;

        [SerializeField]
        private HButton _moreTimeButton;
        [SerializeField]
        private HButton _closeButton;
        [SerializeField]
        private Image _clockFillImage;

        private float _progress; // DO THIS FOR FILL AMOUNT

        private void Awake()
        {
            _closeButton.onClick.AddListener(OnGoLose);
            _moreTimeButton.onClick.AddListener(OnMoreTimeTicket);
        }

        private void OnDestroy()
        {
            _closeButton.onClick.RemoveAllListeners();
            _moreTimeButton.onClick.RemoveAllListeners();
        }

        public override void Setup(object param = null)
        {
            base.Setup(param);
            GameManager.Ins.ChangeState(GameState.Pause);
            HInputManager.LockInput();
            // _closeButton.gameObject.SetActive(false);
            _progress = 1f;
            // _clockFillImage.fillAmount = _progress;
            Timing.RunCoroutine(CountDown());
        }

        public override void Open(object param = null)
        {
            base.Open(param);
            
            AudioManager.Ins.PlaySfx(AudioEnum.SfxType.PopupOpen);
        }
        
        public override void Close()
        {
            HInputManager.LockInput(false);
            
            base.Close();
        }

        private void OnGoLose()
        {
            switch (LevelManager.Ins.CurrentLevel.LevelType)
            {
                case Data.LevelType.Normal:
                case Data.LevelType.Secret:
                    UIManager.Ins.OpenUI<GiveUpPopup>((Action)ActualGoLose);
                    break;
                default:
                    ActualGoLose();
                    break;
            }

            void ActualGoLose()
            {
                GameplayManager.Ins.OnLoseGame(LevelLoseCondition.Timeout);
                Close();
            }
        }

        private void OnMoreTimeTicket()
        {
            AdsManager.Ins.RewardedAds.Show(() =>
            {
                GameManager.Ins.PostEvent(EventID.MoreTimeGame);
                Close();
            });
        }

        private IEnumerator<float> CountDown()
        {
            float time = TIMER;
            while (time > 0)
            {
                time -= Time.deltaTime;
                
                _progress = Mathf.Clamp(time / TIMER, 0f, TIMER);
                // _clockFillImage.fillAmount = _progress;
                
                yield return Timing.WaitForOneFrame;
            }
            // _closeButton.gameObject.SetActive(true);
        }
    }
}