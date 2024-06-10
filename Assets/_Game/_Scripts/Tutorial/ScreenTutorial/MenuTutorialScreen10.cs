using System.Collections.Generic;
using _Game.Data;
using _Game.GameGrid;
using _Game.Managers;
using _Game.UIs.Popup;
using _Game.UIs.Screen;
using _Game.Utilities;
using _Game.Utilities.Timer;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using VinhLB;

namespace _Game._Scripts.Tutorial.ScreenTutorial
{
    public class MenuTutorialScreen10 :  MenuTutorialScreen
    {
        [SerializeField] private Image panel;
        [SerializeField] private HButton dailyChallengeButton;
        [SerializeField] private List<Image> dailyChallengeImage;
        [SerializeField] private Transform lockImage;
        [SerializeField] private Image unlockImage;
        [SerializeField] private GameObject focusObject;
        
        public override void Setup(object param = null)
        {
            base.Setup(param);
            panel.color = new Color(0, 0, 0, 0);
            dailyChallengeButton.interactable = false;
            focusObject.SetActive(false);
            unlockImage.gameObject.SetActive(false);
            lockImage.gameObject.SetActive(true);
            if (UIManager.Ins.IsLoaded<HomePage>())
            {
                UIManager.Ins.GetUI<HomePage>().ShowDailyChallengeButton(false);
            }
            TimerManager.Ins.WaitForTime(1.5f, () =>
            {
                panel.color = new Color(0, 0, 0, 0.8f);
                OnUnlock();
            });
        }

        private void OnUnlock()
        {
            // TODO: Unlock Animation
            // 1. Set an animation which lock image is vibrate 3 times
            lockImage.DOShakePosition(0.5f, 10).SetLoops(2)
                .OnComplete(() =>
                {
                    // 2. Disable lock image, enable unlock image
                    lockImage.gameObject.SetActive(false);
                    unlockImage.gameObject.SetActive(true);
                    // 3. Fade out unlock image
                    unlockImage.DOFade(0, 0.3f).OnComplete(() =>
                    {
                        // 4. Make List<Image> dailyChallengeImage color to 0 0 0 0
                        for (int index = 0; index < dailyChallengeImage.Count; index++)
                        {
                            Image image = dailyChallengeImage[index];
                            image.color = new Color(1, 1, 1, 1);
                        }
                        // 5. Set dailyChallengeButton interactable
                        dailyChallengeButton.interactable = true;
                        // 6. Set focusObject active
                        focusObject.SetActive(true);
                    });
                });
        }

        public void OnClickDailyChallengeTutorial()
        {
            DataManager.Ins.GameData.user.completedOneTimeTutorial
                .Add(DataManager.Ins.ConfigData.unlockDailyChallengeAtLevelIndex);
            // 7. Show a fake DailyChallengePopup
            UIManager.Ins.GetUI<HomePage>().ShowDailyChallengeButton(true);
            UIManager.Ins.OpenUI<DailyChallengePopup>();
            CloseDirectly();
        }

        public void OnClickTutorialButton()
        {
            if (UIManager.Ins.IsLoaded<HomePage>())
            {
            }
            
            DevLog.Log(DevId.Hoang, "OnClick Tutorial Button");
            LevelManager.Ins.OnGoLevel(LevelType.DailyChallenge, 0);
            UIManager.Ins.CloseAll();
            UIManager.Ins.OpenUI<InGameScreen>();
        }
    }
}
