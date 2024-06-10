using System;
using _Game.Utilities;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace VinhLB
{
    public abstract class TabPage : UICanvas
    {
        [SerializeField]
        private CanvasGroup _canvasGroup;
        [SerializeField]
        private Image _blockPanel;

        public override void Setup(object param = null)
        {
            base.Setup(param);
            
            if (param is true)
            {
                _canvasGroup.alpha = 1f;
                _blockPanel.gameObject.SetActive(false);
            }
            else
            {
                _canvasGroup.alpha = 0f;
                _blockPanel.gameObject.SetActive(true);
            }
        }

        public override void Open(object param = null)
        {
            base.Open(param);
            
            if (param is true)
            {
                
            }
            else
            {
                _blockPanel.gameObject.SetActive(true);
                
                DOVirtual.Float(0f, 1f, 1f, value => _canvasGroup.alpha = value)
                    .OnComplete(() => _blockPanel.gameObject.SetActive(false));
            }
        }
    }
}