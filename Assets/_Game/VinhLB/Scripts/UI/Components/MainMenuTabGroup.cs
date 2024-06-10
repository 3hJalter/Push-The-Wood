using System.Collections.Generic;
using _Game.Managers;
using UnityEngine;

namespace VinhLB
{
    public class MainMenuTabGroup : TabGroup
    {
        [System.Serializable]
        private enum MainMenuTabPageType
        {
            None = -1,
            Home = 0,
            Shop = 1,
            Collection = 2
        }
        
        [SerializeField]
        private Dictionary<TabButton, MainMenuTabPageType> _mainMenuTabGroupDict = new();
        
        protected override TabPage GetTabPage(TabButton tabButton)
        {
            MainMenuTabPageType type = _mainMenuTabGroupDict[tabButton];
            switch (type)
            {
                case MainMenuTabPageType.Shop:
                    return UIManager.Ins.GetUI<ShopPage>();
                case MainMenuTabPageType.Home:
                    return UIManager.Ins.GetUI<HomePage>();
                case MainMenuTabPageType.Collection:
                    return UIManager.Ins.GetUI<CollectionPage>();
            }
            
            return base.GetTabPage(tabButton);
        }
    }
}