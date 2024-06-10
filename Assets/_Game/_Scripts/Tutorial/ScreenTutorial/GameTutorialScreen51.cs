using _Game.Camera;
using _Game.Managers;
using _Game.UIs.Screen;
using _Game.Utilities.Timer;
using HControls;
using UnityEngine;
using UnityEngine.UI;

namespace _Game._Scripts.Tutorial.ScreenTutorial
{
    public class GameTutorialScreen51 : GameTutorialScreen
    {
        [SerializeField] private Image panel;
        [SerializeField] private HHoldingButton holdingButton;
        [SerializeField] private GameObject deco;
        
        public override void Setup(object param = null)
        {
            base.Setup(param);
            HInputManager.LockInput();
            SetupHoldAction();
            panel.raycastTarget = false;
            TimerManager.Ins.WaitForFrame(1, () =>
            {
                if (GameManager.Ins.IsState(GameState.InGame)) GameplayManager.Ins.OnPauseGame();
                if (!UIManager.Ins.IsOpened<InGameScreen>()) return;
                UIManager.Ins.CloseUI<InGameScreen>();
                // Stop timer
            });
            TimerManager.Ins.WaitForTime(1f, () =>
            {
                panel.raycastTarget = true;
                panel.color = new Color(0,0,0,0.75f);
                deco.SetActive(true);
            });
        }

        private void SetupHoldAction()
        {
            holdingButton.OnInit(Constants.HOLD_TOUCH_TIME, null, PointerUpAction, HoldAction);
            return;

            void PointerUpAction()
            {
                if (CameraManager.Ins.IsCurrentCameraIs(ECameraType.ZoomOutCamera)) CameraManager.Ins.ChangeCamera(ECameraType.InGameCamera, Constants.ZOOM_OUT_TIME);
                HInputManager.LockInput(false);
                CloseDirectly();
            }

            // Set up holding button
            void HoldAction()
            {
                if (!holdingButton.IsHolding) return;
                CameraManager.Ins.ChangeCamera(ECameraType.ZoomOutCamera, Constants.ZOOM_OUT_TIME);
                panel.color = new Color(1, 1, 1, 0);
                deco.SetActive(false);
            }
        }
    }
}
