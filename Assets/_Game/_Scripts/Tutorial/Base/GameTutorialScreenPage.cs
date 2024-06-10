using System;
using System.Collections.Generic;
using _Game._Scripts.UIs.Component;
using _Game.Managers;
using _Game.UIs.Screen;
using _Game.Utilities.Timer;
using HControls;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace _Game._Scripts.Tutorial
{
    public class GameTutorialScreenPage : GameTutorialScreen
    {
         [SerializeField] private HButton closeButton;
         [SerializeField] private GameObject videoContainer;
         [SerializeField] private List<VideoTutorial> videoTutorials;
         [SerializeField] private Image panel;
         [SerializeField] private EventTrigger panelTrigger;

        [SerializeField] private int loopVideoPerPage = 1;

        private bool _firstLoopAllPagesDone;
        private int _currentVideoIndex;

        public event Action OnCloseCallback;
        
        public override void Setup(object param = null)
        {
            base.Setup(param);
            panel.color = new Color(0,0,0,0);
            HInputManager.LockInput();
            videoContainer.SetActive(false);
            closeButton.gameObject.SetActive(false);
            panelTrigger.enabled = false;
            // Wait to zoom in camera
            TimerManager.Ins.WaitForFrame(1, () =>
            {
                if (GameManager.Ins.IsState(GameState.InGame)) GameplayManager.Ins.OnPauseGame();
                if (!UIManager.Ins.IsOpened<InGameScreen>()) return;
                UIManager.Ins.CloseUI<InGameScreen>();
                // Stop timer
            });
            TimerManager.Ins.WaitForTime(1f, HandleShowVideo);
        }

        private void HandleShowVideo()
        {
            _currentVideoIndex = 0;
            videoTutorials[_currentVideoIndex].isFadeUIOnAppear = true;
            videoTutorials[_currentVideoIndex].OnPrepared += () =>
            {
                panel.color = new Color(0,0,0,0.65f);
                videoContainer.SetActive(true);
            };
            videoTutorials[_currentVideoIndex].Play(loopVideoPerPage, GoNextVideo);
        }
        
        private void GoNextVideo()
        {
            _currentVideoIndex++;
            if (_currentVideoIndex < videoTutorials.Count)
            {
                videoTutorials[_currentVideoIndex].Play(loopVideoPerPage, GoNextVideo);
            }
            else
            {
                if (!_firstLoopAllPagesDone)
                {
                    _firstLoopAllPagesDone = true;
                    closeButton.gameObject.SetActive(true);
                    panelTrigger.enabled = true;
                }
                _currentVideoIndex = 0;
                videoTutorials[_currentVideoIndex].Play(loopVideoPerPage, GoNextVideo);
            }
        }

        public void OnClose()
        {
            OnCloseCallback?.Invoke();
            CloseDirectly();
        }
        
        public override void CloseDirectly(object param = null)
        {
            videoTutorials[_currentVideoIndex].Stop();
            HInputManager.LockInput(false);
            base.CloseDirectly(param);
        }
    }
}
