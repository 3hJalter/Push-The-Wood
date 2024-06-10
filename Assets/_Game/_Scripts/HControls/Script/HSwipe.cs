using _Game.Camera;
using _Game.Managers;
using _Game.Utilities;
using GG.Infrastructure.Utils.Swipe;
using UnityEngine;
using UnityEngine.Events;

namespace HControls
{
    public class HSwipe : HMonoBehaviour
    {
        [SerializeField] private SwipeListener swipeListener;

        private void OnEnable()
        {
            swipeListener.onSwipe.AddListener(OnSwipe);
            swipeListener.onCancelSwipe.AddListener(OnCancelSwipe);
            swipeListener.onUnHold.AddListener(OnUnHold);
        }
        
        public void AddListener(UnityAction<string> action = null)
        {
            if (action == null)
            {
                swipeListener.onSwipe.AddListener(OnSwipe);
                return;
            }
            swipeListener.onSwipe.AddListener(action);
        }
        
        public void RemoveListener(UnityAction<string> action = null)
        {
            if (action == null)
            {
                swipeListener.onSwipe.RemoveListener(OnSwipe);
                return;
            }
            swipeListener.onSwipe.RemoveListener(action);
        }

        private static void OnCancelSwipe()
        {
            //DevLog.Log(DevId.Hoang, "Cancel swipe");
            HInputManager.SetDirectionInput(Direction.None);
        }
        
        private static void OnUnHold()
        {
            if (CameraManager.Ins.IsCurrentCameraIs(ECameraType.ZoomOutCamera))
                CameraManager.Ins.ChangeCamera(ECameraType.InGameCamera, Constants.ZOOM_OUT_TIME);
        }
        
        private static void OnSwipe(string direction)
        {
            switch (direction)
            {
                case DirectionId.ID_LEFT:
                    HInputManager.SetDirectionInput(Direction.Left);
                    break;
                case DirectionId.ID_RIGHT:
                    HInputManager.SetDirectionInput(Direction.Right);
                    break;
                case DirectionId.ID_UP:
                    HInputManager.SetDirectionInput(Direction.Forward);
                    break;
                case DirectionId.ID_DOWN:
                    HInputManager.SetDirectionInput(Direction.Back);
                    break;
                case Constants.NONE:
                    HInputManager.SetDirectionInput(Direction.None);
                    // TODO: Zoom In function when swipe out
                    CameraManager.Ins.ChangeCamera(ECameraType.ZoomOutCamera,  Constants.ZOOM_OUT_TIME);
                    break;
            }
        }

        private void OnDisable()
        {
            swipeListener.onSwipe.RemoveListener(OnSwipe);
            swipeListener.onUnHold.RemoveListener(OnUnHold);
        }
    }
}
