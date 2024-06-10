using System;
using System.Collections.Generic;
using _Game._Scripts.UIs.Component;
using _Game.Data;
using _Game.GameGrid;
using _Game.Managers;
using _Game.UIs.Screen;
using UnityEngine;
using UnityEngine.UI;
using VinhLB;

namespace _Game.UIs.Popup
{
    public class DailyChallengePopup : UICanvas
    {
        private const float PANEL_INIT_HEIGHT = 662f;
        [SerializeField] private RectTransform panel;
        [SerializeField] private Slider dailyChallengeSliderProgress;
        [SerializeField] private List<DailyChallengeRewardButton> dailyChallengeRewardButtons;
        [SerializeField] List<DailyChallengeButton> dailyChallengeButtons;
        [SerializeField] private List<GameObject> dailyChallengeButtonAnchors;
        private DailyChallengeButton _currentBtnClick;

        public DailyChallengeRewardButton LastClickRewardButton { get; set; }

        // interact Btn
        [SerializeField] private GameObject notYetBtn;
        [SerializeField] private HButton payToPlayBtn;
        [SerializeField] private HButton adsToPlayBtn;
        [SerializeField] private HButton playBtn;
        
        private void Awake()
        {
            // Set panel height by adding 123f for each row, each row has 7 buttons
            panel.sizeDelta = new Vector2(panel.sizeDelta.x, PANEL_INIT_HEIGHT + (130f * Mathf.CeilToInt(DataManager.Ins.DaysInMonth / 7f)));
            // Set listener for interact btn
            payToPlayBtn.onClick.AddListener(OnClickPayToPlayButton);
            adsToPlayBtn.onClick.AddListener(OnClickAdToPlayButton);
            playBtn.onClick.AddListener(OnClickPlayButton);
        }

        private void OnDestroy()
        {
            // Remove listener for interact btn
            payToPlayBtn.onClick.RemoveAllListeners();
            adsToPlayBtn.onClick.RemoveAllListeners();
            playBtn.onClick.RemoveAllListeners();
        }

        private void OnUpdateProgress()
        {
            List<DailyChallengeRewardMilestone> dcm = DataManager.Ins.ConfigData.dailyChallengeRewardMilestones;
            int count = DataManager.Ins.GameData.user.dailyLevelIndexComplete.Count;
            if (count == 0)
            {
                dailyChallengeSliderProgress.value = 0;
                return;
            }
            float progress = 0;
            int mileStoneNums = dcm.Count;
            float mileStoneProgress = 1/(float)mileStoneNums;
            for (int i = 0; i < mileStoneNums; i++)
            {
                if (count >= dcm[i].clearLevelNeed)
                {
                    progress += mileStoneProgress;       
                }
                else
                {
                    if (i == 0) break;
                    progress += (float) (count - dcm[i-1].clearLevelNeed)/(dcm[i].clearLevelNeed - dcm[i-1].clearLevelNeed) * mileStoneProgress;
                    break;  
                }
            }
            dailyChallengeSliderProgress.value = progress;
        }

        public override void Setup(object param = null)
        {
            base.Setup(param);
            int firstDayOfWeekInMonth = (int) new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1).DayOfWeek;
            for (int i = 0; i < dailyChallengeButtonAnchors.Count; i++)
            {
                // Set true if i less than day of week, else set false
                dailyChallengeButtonAnchors[i].SetActive(i < firstDayOfWeekInMonth);
            }
            
            // Set up buttons
            for (int index = 0; index < dailyChallengeButtons.Count; index++)
            { 
                // if index is greater than days in month, disable button
                if (index >= DataManager.Ins.DaysInMonth)
                {
                    dailyChallengeButtons[index].gameObject.SetActive(false);
                    continue;
                }
                DailyChallengeButton dailyChallengeButton = dailyChallengeButtons[index];
                dailyChallengeButton.SetIndex(index, DataManager.Ins.CurrentDay);
                dailyChallengeButton.onClick.AddListener(delegate
                {
                    OnClickDailyChallengeButton(
                        dailyChallengeButton.Index);
                });
                dailyChallengeButtons[index].gameObject.SetActive(true);
            }
            for (int index = 0; index < dailyChallengeRewardButtons.Count; index++)
            {
                dailyChallengeRewardButtons[index].OnInit(this, DataManager.Ins.ConfigData.dailyChallengeRewardMilestones[index]);
            }
            
            OnUpdateProgress();
            // click today button
            OnClickDailyChallengeButton(DataManager.Ins.CurrentDay - 1);
        }
        
        public override void Close()
        {
            // Remove listener for buttons
            for (int index = 0; index < dailyChallengeButtons.Count; index++)
            {
                dailyChallengeButtons[index].onClick.RemoveAllListeners();
            }
            _currentBtnClick.OnUnHover();
            _currentBtnClick = null;
            if (LastClickRewardButton)
            {
                LastClickRewardButton.OnRelease();
                LastClickRewardButton = null;
            }
            base.Close();
        }

        private void OnClickDailyChallengeButton(int index)
        {
            if (dailyChallengeButtons[index] == _currentBtnClick) return;
            if (_currentBtnClick != null)
                _currentBtnClick.OnUnHover();
            _currentBtnClick = dailyChallengeButtons[index];
            _currentBtnClick.OnHover();
            OnHandleCurrentButton();
        }
        
        private void OnClickPlayButton()
        {
            DataManager.Ins.ChangeDailyChallengeFreePlay(false);
            OnPlay();
        }

        private void OnClickAdToPlayButton()
        {
            AdsManager.Ins.RewardedAds.Show(OnPlay);
        }
        
        private void OnClickPayToPlayButton()
        {
            if (GameManager.Ins.TrySpendHeart(1))
            {
                // Temporary solution: just play
                OnPlay();
            }
        }

        private void OnPlay()
        {
            LevelManager.Ins.dailyLevelClickedDay = _currentBtnClick.Index;
            UIManager.Ins.CloseAll();
            TransitionScreen ui = UIManager.Ins.OpenUI<TransitionScreen>();
            ui.OnOpenCallback += () =>
            {
                LevelManager.Ins.OnGoLevel(LevelType.DailyChallenge, DataManager.Ins.GetDailyChallengeDay(LevelManager.Ins.dailyLevelClickedDay));
                UIManager.Ins.OpenUI<InGameScreen>();
            };
        }
        
        private void OnHandleCurrentButton() 
        {
            // hide all interact btn
            notYetBtn.SetActive(false);
            payToPlayBtn.gameObject.SetActive(false);
            playBtn.gameObject.SetActive(false);
            adsToPlayBtn.gameObject.SetActive(false);
            switch (_currentBtnClick.State)
            {
                // if current button is not yet, show not yet btn
                case DailyChallengeButtonState.NotYet:
                    notYetBtn.SetActive(true);
                    break;
                // if current button is un clear, show pay to play btn
                case DailyChallengeButtonState.UnClear:
                    adsToPlayBtn.gameObject.SetActive(true);
                    break;
                // if current button is clear, show replay btn
                case DailyChallengeButtonState.Clear:
                    // replayBtn.gameObject.SetActive(true); // TEMPORARY REMOVE
                    notYetBtn.SetActive(true);
                    break;
                // if current button is today, show play btn
                case DailyChallengeButtonState.Today:
                    if (DataManager.Ins.IsDailyChallengeFreePlay())
                    {
                        playBtn.gameObject.SetActive(true);
                    }
                    else
                    {
                       adsToPlayBtn.gameObject.SetActive(true);
                    }
                    break;
            }
        }


    }
}
