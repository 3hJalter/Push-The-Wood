using System;
using System.Collections.Generic;
using _Game.Camera;
using _Game.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace HControls
{
    public class HDpad : HMonoBehaviour
    {
        [Tooltip("0 is Left, 1 is Right, 2 is Up, 3 is Down")]
        [SerializeField]
        private bool highlightButton = true;

        [SerializeField] private HHoldingButton holdingButton;
        
        
        [SerializeField] private HDpadButton[] dpadButtons;

        [SerializeField] private List<Image> dpadImages;

        public event Action OnPointerDown;

        public void ClearAction()
        {
            OnPointerDown = null;
        }
        
        private void Awake()
        {
            holdingButton.OnInit(Constants.HOLD_TOUCH_TIME, null, PointerUpAction, HoldAction);
        }
        
        void PointerUpAction()
        {
            if (CameraManager.Ins.IsCurrentCameraIs(ECameraType.ZoomOutCamera)) CameraManager.Ins.ChangeCamera(ECameraType.InGameCamera, Constants.ZOOM_OUT_TIME);
            HInputManager.LockInput(false);
        }

        // Set up holding button
        void HoldAction()
        {
            if (!holdingButton.IsHolding) return;
            HInputManager.LockInput();
            CameraManager.Ins.ChangeCamera(ECameraType.ZoomOutCamera, Constants.ZOOM_OUT_TIME);
        }

        private void OnDisable()
        {
            for (int i = 0; i < dpadButtons.Length; i++) dpadButtons[i].PointerDownImg.SetActive(false);
            HInputManager.SetDirectionInput(Direction.None);
        }

        public void SetAlpha(float alpha)
        {
            for (int i = 0; i < dpadImages.Count; i++)
            {
                dpadImages[i].color = new Color(dpadImages[i].color.r, dpadImages[i].color.g, dpadImages[i].color.b, alpha);
            }
        }
        
        public void SetButtonAlpha(Direction direction, float alpha)
        {
            dpadImages[(int) direction].color = new Color(dpadImages[(int) direction].color.r, dpadImages[(int) direction].color.g, dpadImages[(int) direction].color.b, alpha);
        }
        
        public void SetHoldingButton(bool isHolding)
        {
            holdingButton.enabled = isHolding;
        }
        
        public void OnButtonPointerDown(int index)
        {
            if (highlightButton) dpadButtons[index].PointerDownImg.SetActive(true);
            HInputManager.SetDirectionInput(dpadButtons[index].Direction);
            OnPointerDown?.Invoke();
        }

        public void OnButtonPointerUp(int index)
        {
            dpadButtons[index].PointerDownImg.SetActive(false);
            HInputManager.SetDirectionInput(Direction.None);
        }

        public void LockInput(Direction back, bool isLock)
        {
            dpadButtons[(int) back].LockInput(isLock);
        }
    }
}
