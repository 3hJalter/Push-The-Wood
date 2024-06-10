using System;
using System.Collections.Generic;
using _Game.Camera;
using _Game.Managers;
using _Game.Utilities;
using _Game.Utilities.Timer;

namespace HControls
{
    public class HHoldingButton : HMonoBehaviour
    {
        private bool isHolding;
        public bool IsHolding => isHolding;

        private readonly List<float> timerList = new();
        private readonly List<Action> actions = new();
        private STimer timer;
        
        private Action onPointerDownAction;
        private Action onPointerUpAction;
        
        private void Awake()
        {
            // timerList.Add(Constants.HOLD_TOUCH_TIME);
            // actions.Add(() =>
            // {
            //     if (isHolding)
            //     {
            //         CameraManager.Ins.ChangeCamera(ECameraType.ZoomOutCamera, Constants.ZOOM_OUT_TIME);
            //     }
            // });
        }

        public void OnInit(float time, Action pointerDownAction, Action pointerUpAction, Action holdAction)
        {
            timerList.Add(time);
            actions.Add(holdAction);
            onPointerDownAction = pointerDownAction;
            onPointerUpAction = pointerUpAction;
        }
        
        public void OnButtonPointerDown()
        {
            isHolding = true;
            timer = TimerManager.Ins.WaitForTime(timerList, actions);
            onPointerDownAction?.Invoke();
        }

        public void OnButtonPointerUp()
        {
            isHolding = false;
            if (timer.TimeRemaining <= 0)
            {
                onPointerUpAction?.Invoke();
            }
            else timer.Stop();
        }
    }
}
