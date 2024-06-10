using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MEC;
using UnityEngine;

namespace VinhLB
{
    public class TransitionScreen : UICanvas
    {
        [SerializeField]
        private CanvasGroup _canvasGroup;
        [SerializeField]
        private RectTransform _backgroundRectTF;

        private bool _isFirstTime = true;
        private bool _isWidthFitting = false;
        private Coroutine _delayActionsCoroutine;

        public event Action OnOpenCallback;
        public event Action OnCloseCallback;

        private void Awake()
        {
            _canvasGroup.alpha = 0;
        }

        public override void Setup(object param = null)
        {
            base.Setup(param);
            
            UpdateBackground();
            
            _canvasGroup.alpha = 0;
        }

        public override void Open(object param = null)
        {
            base.Open(param);

            // Stop the previous coroutine if it's still running
            if (_delayActionsCoroutine != null)
            {
                StopCoroutine(_delayActionsCoroutine);
            }
            
            float delayTime = param != null ? (float)param : 1f;
            _delayActionsCoroutine = StartCoroutine(DelayActionsCoroutine(delayTime));
        }

        public override void Close()
        {
            OnCloseCallback?.Invoke();
            OnCloseCallback = null;
            
            base.Close();
        }

        private void UpdateBackground()
        {
            float currentScreenRatio = (float)Screen.width / Screen.height;
            // DevLog.Log(DevId.Vinh, $"{Constants.REF_SCREEN_RATIO} | {currentScreenRatio}");
            if (currentScreenRatio >= Constants.REF_SCREEN_RATIO && (_isFirstTime || !_isWidthFitting))
            {
                // Fit width
                _backgroundRectTF.anchorMin = new Vector2(0, 0.5f);
                _backgroundRectTF.anchorMax = new Vector2(1f, 0.5f);
                _backgroundRectTF.SetPadding(0, 0);
                _backgroundRectTF.SetSizeDeltaHeight(2500f);
                _isFirstTime = false;
                _isWidthFitting = true;
            }
            else if (currentScreenRatio < Constants.REF_SCREEN_RATIO && (_isFirstTime || _isWidthFitting))
            {
                // Fit height
                _backgroundRectTF.anchorMin = new Vector2(0.5f, 0);
                _backgroundRectTF.anchorMax = new Vector2(0.5f, 1f);
                _backgroundRectTF.SetPadding(0, 0);
                _backgroundRectTF.SetSizeDeltaWidth(1500f);
                _isFirstTime = false;
                _isWidthFitting = false;
            }
        }

        private IEnumerator DelayActionsCoroutine(float delayTime)
        {
            Tween openTween = 
                DOVirtual.Float(0, 1f, 0.125f, value => _canvasGroup.alpha = value);
            
            yield return openTween.WaitForCompletion();
            
            // Wait a frame to make sure the OnOpenCallback is set
            // yield return new WaitForEndOfFrame();
            
            OnOpenCallback?.Invoke();
            OnOpenCallback = null;
            
            yield return new WaitForSeconds(delayTime);

            Tween closeTween =
                DOVirtual.Float(1f, 0, 0.175f, value => _canvasGroup.alpha = value);
                    
            yield return closeTween.WaitForCompletion();

            Close();

            _delayActionsCoroutine = null;
        }
    }
}