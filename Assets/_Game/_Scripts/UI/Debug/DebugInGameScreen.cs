using _Game._Scripts.Managers;
using _Game._Scripts.Tutorial;
using _Game.GameGrid;
using _Game.Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace _Game.UIs.Screen
{
    public class DebugInGameScreen : UICanvas
    {
        [SerializeField]
        Button onOffInGameUIBtn;
        [SerializeField]
        Button onOffTutorial;
        [SerializeField]
        Button onOffHint;
        [SerializeField]
        Button onOffButton;
        [SerializeField]
        GameObject container;

        private void Awake()
        {
            onOffInGameUIBtn.onClick.AddListener(OnOffInGameScreen);
            onOffTutorial.onClick.AddListener(OnOffTutorial);
            onOffButton.onClick.AddListener(OnOffUI);
            onOffHint.onClick.AddListener(OnOffHint);
        }
        private void OnDestroy()
        {
            onOffInGameUIBtn.onClick.RemoveAllListeners();
            onOffTutorial.onClick.RemoveAllListeners();
            onOffButton.onClick.RemoveAllListeners();
            onOffHint.onClick.RemoveAllListeners();
        }

        private void OnOffInGameScreen()
        {
            if (UIManager.Ins.IsOpened<InGameScreen>())
            {
                UIManager.Ins.CloseUI<InGameScreen>();
                MoveInputManager.Ins.ShowContainer(true);
            }               
            else
                UIManager.Ins.OpenUI<InGameScreen>();
        }

        private void OnOffUI()
        {
            container.SetActive(!container.activeInHierarchy);
        }

        private void OnOffTutorial()
        {
            BaseTutorialData tut = null;
            if (TutorialManager.Ins.TutorialList.ContainsKey(LevelManager.Ins.NormalLevelIndex))
                tut = TutorialManager.Ins.TutorialList[LevelManager.Ins.NormalLevelIndex] as BaseTutorialData;       
            if (tut == null || tut.CurrentScreen == null)
                return;

            tut.CurrentScreen.isDestroyOnClose = false;           
            if (!tut.CurrentScreen.gameObject.activeInHierarchy)
            {
                tut.CurrentScreen.Open();               
            }
            else
            {
                tut.CurrentScreen.Close();
                ;
            }
        }
        
        private void OnOffHint()
        {
            if (GameplayManager.Ins.PushHintObject.gameObject.activeInHierarchy)
            {
                GameplayManager.Ins.PushHintObject.SetActive(false);
            }
            else
            {
                GameplayManager.Ins.PushHint.OnStopHint();
                GameplayManager.Ins.OnFreePushHint(false, true);
            }
        }
    }
}