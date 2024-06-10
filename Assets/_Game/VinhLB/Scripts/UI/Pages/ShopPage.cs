using UnityEngine;
using UnityEngine.UI;

namespace VinhLB
{
    public class ShopPage : TabPage
    {
        [SerializeField]
        private ScrollRect _scrollRect;

        public override void Open(object param = null)
        {
            base.Open(param);

            _scrollRect.verticalNormalizedPosition = 1f;
        }
    }
}