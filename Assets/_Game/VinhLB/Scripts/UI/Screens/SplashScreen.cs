using System;
using _Game.Utilities.Timer;
using DG.Tweening;
using UnityEngine;

namespace VinhLB
{
    public class SplashScreen : UICanvas
    {
        [SerializeField]
        private CanvasGroup _contentCanvasGroup;

        private event Action OnOpenCallback;

        private void Awake()
        {
            _contentCanvasGroup.alpha = 0;
        }

        public override void Open(object param = null)
        {
            base.Open(param);
            if (param is Action onOpenCallback)
            {
                OnOpenCallback = onOpenCallback;
            }
            
            _contentCanvasGroup.alpha = 0;
            _contentCanvasGroup.DOFade(1f, 0.2f).SetEase(Ease.Linear);

            TimerManager.Ins.WaitForTime(0.8f, CallBack);
            void CallBack()
            {
                OnOpenCallback?.Invoke();
                OnOpenCallback = null;
            }

        }
    }
}