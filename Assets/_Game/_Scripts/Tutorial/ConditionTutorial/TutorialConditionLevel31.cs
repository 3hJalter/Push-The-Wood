using System;
using _Game.GameGrid;
using _Game.GameGrid.Unit;
using _Game.GameGrid.Unit.DynamicUnit.Player;
using _Game.Managers;
using UnityEngine;

namespace _Game._Scripts.Tutorial.ConditionTutorial
{
    [CreateAssetMenu(fileName = "TutorialLevel31", menuName = "ScriptableObjects/TutorialData/Lvl31", order = 1)]
    public class TutorialConditionLevel31 : BaseTutorialData, ITutorialCondition
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
                currentTutIndex++;
            }
        }

        public void HandleShowTutorial(GridUnit triggerUnit, GridUnit targetUnit)
        {
            
        }
    }
}
