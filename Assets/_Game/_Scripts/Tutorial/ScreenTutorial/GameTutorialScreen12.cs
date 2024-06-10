using _Game._Scripts.Managers;
using _Game.Managers;
using _Game.Utilities;
using GG.Infrastructure.Utils.Swipe;
using HControls;
using UnityEngine;
using UnityEngine.Events;
using VinhLB;

namespace _Game._Scripts.Tutorial.ScreenTutorial
{
    public class GameTutorialScreen12 : GameTutorialScreen
    {
        private UnityAction<string> _swipeEvent;
        private UnityAction<string> _testSwipeEvent;
        
        // TEMPORARY
        private Direction _saveDirection;
        
        public override void Setup(object param = null)
        {
            base.Setup(param);

            MoveInputManager.Ins.ShowContainer(true);
            // Add Close this screen when swipe
            MoveInputManager.Ins.Dpad.SetButtonAlpha(Direction.Forward,1f);
            MoveInputManager.Ins.Dpad.LockInput(Direction.Back, true);
            MoveInputManager.Ins.Dpad.LockInput(Direction.Left, true);
            MoveInputManager.Ins.Dpad.LockInput(Direction.Right, true);
            
            
            MoveInputManager.Ins.Dpad.OnPointerDown += () =>
            {
                _saveDirection = HInputManager.GetDirectionInput();
                MoveInputManager.Ins.Dpad.LockInput(Direction.Back, false);
                MoveInputManager.Ins.Dpad.LockInput(Direction.Left, false);
                MoveInputManager.Ins.Dpad.LockInput(Direction.Right, false);
                MoveInputManager.Ins.Dpad.SetButtonAlpha(Direction.Forward, 0.3f);
                MoveInputManager.Ins.Dpad.ClearAction();
                CloseDirectly(false);
                MoveInputManager.Ins.Dpad.OnButtonPointerDown((int) _saveDirection);
            };
            
            // _swipeEvent = direction =>
            // {
            //     DevLog.Log(DevId.Hoang, $"Swipe - {direction}");
            //     if (direction != DirectionId.ID_UP) return;
            //     HInputManager.SetDirectionInput(Direction.Forward);
            //     CloseDirectly(false);
            // };
            // // Remove the default swipe event
            // MoveInputManager.Ins.HSwipe.RemoveListener();
            // MoveInputManager.Ins.HSwipe.AddListener(_swipeEvent);
            // MoveInputManager.Ins.HSwipe.AddListener(_testSwipeEvent);
            // // OpenMask();
        }
        
        // public override void CloseDirectly(object param = null)
        // {
        //     // Remove Close this screen when swipe 
        //     MoveInputManager.Ins.HSwipe.RemoveListener(_swipeEvent);
        //     MoveInputManager.Ins.HSwipe.RemoveListener(_testSwipeEvent);
        //     // Add the default swipe event
        //     MoveInputManager.Ins.HSwipe.AddListener();
        //     base.CloseDirectly(param);
        // }
    }
}
