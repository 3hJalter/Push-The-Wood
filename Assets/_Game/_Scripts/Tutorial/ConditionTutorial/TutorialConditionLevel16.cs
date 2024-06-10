using System;
using _Game.GameGrid;
using _Game.GameGrid.Unit;
using _Game.GameGrid.Unit.DynamicUnit.Player;
using _Game.Managers;
using UnityEngine;

namespace _Game._Scripts.Tutorial.ConditionTutorial
{
    [CreateAssetMenu(fileName = "TutorialLevel16", menuName = "ScriptableObjects/TutorialData/Lvl16", order = 1)]
    public class TutorialConditionLevel16 : BaseTutorialData, ITutorialCondition
    {
        public void OnForceShowTutorial(int index, bool isIncrement = true)
        {
            
        }

        public void HandleShowTutorial(GameGridCell cell, GridUnit triggerUnit)
        {
            if (currentTutIndex == 0)
            {
                if (triggerUnit is not Player) return;
                UIManager.Ins.OpenUIDirectly(tutorialScreens[currentTutIndex]);
                // GameplayManager.Ins.OnFreePushHint(false, true);
                currentTutIndex++;
            }
        }

        public void HandleShowTutorial(GridUnit triggerUnit, GridUnit targetUnit)
        {
            
        }
    }
}
