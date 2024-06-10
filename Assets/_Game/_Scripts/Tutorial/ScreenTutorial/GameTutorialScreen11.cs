using _Game._Scripts.Managers;
using _Game.Managers;
using _Game.UIs.Screen;
using HControls;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace _Game._Scripts.Tutorial.ScreenTutorial
{
    public class GameTutorialScreen11 : GameTutorialScreen
    {
        private UnityAction<string> _swipeEvent;
        [SerializeField] private float moveTime = 1f;
        [SerializeField] private RectTransform _imageRectContainer;
        [SerializeField] private Image panel;
        
        // TEMPORARY
        private Direction _saveDirection;
        
        public override void Setup(object param = null)
        {
            base.Setup(param);
            MoveInputManager.Ins.ShowContainer(true);
            
            MoveInputManager.Ins.Dpad.SetAlpha(1f);
            
            MoveInputManager.Ins.Dpad.OnPointerDown += () =>
            {
                _saveDirection = HInputManager.GetDirectionInput();
                MoveInputManager.Ins.Dpad.SetAlpha(0.3f);
                _imageRectContainer.gameObject.SetActive(false);
                panel.color = new Color(1, 1, 1, 0);
                TutorialManager.Ins.currentGameTutorialScreenScreen = null;
                UIManager.Ins.OpenUI<InGameScreen>(false);
                GameplayManager.Ins.OnFreePushHint(false, true);
                MoveInputManager.Ins.Dpad.ClearAction();
                MoveInputManager.Ins.Dpad.OnButtonPointerDown((int) _saveDirection);
                
            };
            
            // // Add Close this screen when swipe
            // _swipeEvent = _ =>
            // {
            //     // FXManager.Ins.TrailHint.OnPlay(new List<Vector3>()
            //     // {
            //     //     new(7,3,7),
            //     //     new(7,3,13),
            //     // }, 8f, true);
            //     AfterSwipe();
            // };
            // MoveInputManager.Ins.HSwipe.AddListener(_swipeEvent);
        }

        // private void AfterSwipe()
        // {
        //     // Set panel alpha to 0
        //     panel.color = new Color(1, 1, 1, 0);
        //     // Tween to move _imageRectContainer to _finalImageRectPosition
        //     TutorialManager.Ins.currentGameTutorialScreenScreen = null;
        //     UIManager.Ins.OpenUI<InGameScreen>(false);
        //     GameplayManager.Ins.OnFreePushHint(false, true);
        //     MoveInputManager.Ins.HSwipe.RemoveListener(_swipeEvent);
        // }
    }
}
