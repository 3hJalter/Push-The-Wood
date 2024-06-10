using System;
using System.Collections.Generic;
using _Game.GameGrid;
using _Game.GameGrid.Unit;
using _Game.Managers;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Game._Scripts.Tutorial
{
    public interface ITutorialCondition
    {
        /* description: Force show tutorial at index 
         * params:
         * - index: index of tutorial in list
         * - isIncrement: if true, currentTutIndex will be increment by 1
         */
        public void OnForceShowTutorial(int index, bool isIncrement = true);
        
        
        /* description: Handle show tutorial when an unit move to a cell
         * params:
         * - cell: cell that unit move to
         * - triggerUnit: unit that move to cell
         */
        void HandleShowTutorial(GameGridCell cell, GridUnit triggerUnit);
        
        /* description: Handle show tutorial when an unit interact to another unit
         * params:
         * - triggerUnit: unit that interact
         * - targetUnit: unit that be interacted
         */
        void HandleShowTutorial(GridUnit triggerUnit, GridUnit targetUnit);
        
        /* description: Reset tutorial
         */
        void ResetTutorial();
    }
    
    public abstract class BaseTutorialData : SerializedScriptableObject
    {
        // ReSharper disable once Unity.RedundantSerializeFieldAttribute
        [SerializeField] protected List<GameTutorialScreen> tutorialScreens = new();
        protected int currentTutIndex;

        [ReadOnly]
        [SerializeField] protected UICanvas currentScreen;
        public UICanvas CurrentScreen => currentScreen;
        
        [ContextMenu("Reset Tutorial")]
        public void ResetTutorial()
        {
            currentTutIndex = 0;
        }

    }
}
