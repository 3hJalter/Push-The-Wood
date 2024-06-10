using _Game.DesignPattern;
using _Game.Managers;
using UnityEngine;
using VinhLB;

namespace _Game._Scripts.Tutorial
{
    public abstract class MenuTutorialScreen : UICanvas
    {
        public override void Setup(object param = null)
        {
            base.Setup(param);
            UIManager.Ins.CloseUI<DailyRewardPopup>();
        }

        private void Awake()
        {
            // GameManager.Ins.RegisterListenerEvent(EventID.OnChangeLayoutForBanner, ChangeLayoutForBanner);
            // ChangeLayoutForBanner(AdsManager.Ins.IsBannerOpen);
        }

        private void OnDestroy()
        {
            // GameManager.Ins.UnregisterListenerEvent(EventID.OnChangeLayoutForBanner, ChangeLayoutForBanner);
        }
        
        private void ChangeLayoutForBanner(object isBannerActive)
        {
            int sizeAnchor = (bool)isBannerActive ? DataManager.Ins.ConfigData.bannerHeight : 0;
            MRectTransform.offsetMin = new Vector2(MRectTransform.offsetMin.x, sizeAnchor);
        }
    }
}
