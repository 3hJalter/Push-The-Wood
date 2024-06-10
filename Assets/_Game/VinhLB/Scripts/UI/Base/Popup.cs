using DG.Tweening;
using UnityEngine;

namespace VinhLB
{
    public class Popup : UICanvas
    {
        [SerializeField]
        private RectTransform _panelRectTF;
        
        public override void Open(object param = null)
        {
            base.Open(param);

            _panelRectTF.DOScale(1f, 0.2f).From(0f).SetEase(Ease.OutBack);
        }
    }
}