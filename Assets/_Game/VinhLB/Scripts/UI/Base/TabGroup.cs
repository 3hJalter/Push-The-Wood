using System;
using System.Collections;
using System.Collections.Generic;
using _Game.Managers;
using Sirenix.OdinInspector;
using UnityEngine;

namespace VinhLB
{
    public class TabGroup : HMonoBehaviour
    {
        [SerializeField]
        private Dictionary<TabButton, TabPage> _tabGroupDict = new();
        [SerializeField]
        private TabButton _activeTabButton;
        [SerializeField]
        private float _inactiveTabButtonWidth = 300f;
        [SerializeField]
        private bool _useCustomSprite;
        [ShowIf(nameof(_useCustomSprite), false)]
        [SerializeField]
        private Sprite _tabIdleSprite;
        [ShowIf(nameof(_useCustomSprite), false)]
        [SerializeField]
        private Sprite _tabHoverSprite;
        [ShowIf(nameof(_useCustomSprite), false)]
        [SerializeField]
        private Sprite _tabActiveSprite;

        private TabButton _selectedTabButton;

        private void Awake()
        {
            if (_tabGroupDict.Keys.Count > 0)
            {
                foreach (TabButton tabButton in _tabGroupDict.Keys)
                {
                    tabButton.SetPreferredWidth(_inactiveTabButtonWidth);
                }
            }
        }

        public void ResetSelectedTab(bool pageAnimated)
        {
            if (_tabGroupDict.Keys.Count > 0)
            {
                OnTabSelected(_activeTabButton, false, pageAnimated);
            }
        }

        public void ClearSelectedTab()
        {
            if (_selectedTabButton == null)
            {
                return;
            }

            if (_tabGroupDict.Keys.Count > 0)
            {
                OnTabSelected(null, false, false);
            }
        }

        public void OnTabEnter(TabButton button)
        {
            // ResetTabs();
            //
            // if (_selectedTabButton == null || button != _selectedTabButton)
            // {
            //     if (!_useCustomSprite)
            //     {
            //         button.SetBackgroundAlpha(0.025f);
            //     }
            //     else
            //     {
            //         button.ChangeBackgroundSprite(_tabHoverSprite);
            //     }
            // }
        }

        public void OnTabExit(TabButton button)
        {
            // ResetTabs();
        }

        public void OnTabSelected(TabButton button, bool buttonAnimated, bool pageAnimated)
        {
            if (button == _selectedTabButton)
            {
                button.PlayIconAnim();

                return;
            }

            _selectedTabButton = button;

            ResetTabs();

            if (_selectedTabButton != null)
            {
                if (!_useCustomSprite)
                {
                    _selectedTabButton.SetActiveState(true, buttonAnimated);
                }
                else
                {
                    _selectedTabButton.SetBackgroundSprite(_tabActiveSprite);
                }

                _tabGroupDict[_selectedTabButton] = GetTabPage(_selectedTabButton);
            }

            foreach (KeyValuePair<TabButton, TabPage> element in _tabGroupDict)
            {
                if (element.Value == null)
                {
                    continue;
                }

                if (element.Key == _selectedTabButton)
                {
                    element.Value.Setup(!pageAnimated);
                    element.Value.Open(!pageAnimated);
                }
                else
                {
                    element.Value.Close();
                }
            }
        }

        protected virtual TabPage GetTabPage(TabButton tabButton)
        {
            return _tabGroupDict[tabButton];
        }

        private void ResetTabs()
        {
            foreach (TabButton tabButton in _tabGroupDict.Keys)
            {
                if (tabButton == _selectedTabButton)
                {
                    continue;
                }

                if (!_useCustomSprite)
                {
                    if (tabButton.Interactable)
                    {
                        tabButton.SetActiveState(false, false);
                    }
                }
                else
                {
                    tabButton.SetBackgroundSprite(_tabIdleSprite);
                }
            }
        }
    }
}