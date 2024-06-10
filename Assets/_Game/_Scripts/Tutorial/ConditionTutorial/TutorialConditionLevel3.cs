using System;
using _Game.GameGrid;
using _Game.GameGrid.Unit;
using _Game.GameGrid.Unit.DynamicUnit.Player;
using _Game.Managers;
using UnityEngine;

namespace _Game._Scripts.Tutorial.ConditionTutorial
{
    [CreateAssetMenu(fileName = "TutorialLevel3", menuName = "ScriptableObjects/TutorialData/Lvl3", order = 1)]
    public class TutorialConditionLevel3 : BaseTutorialData, ITutorialCondition
    {
        public void HandleShowTutorial(GameGridCell cell, GridUnit triggerUnit)
        {
            if (currentTutIndex == 0)
            {
                if (triggerUnit is not Player) return;
                GameplayManager.Ins.OnFreePushHint(false, true);
                currentTutIndex++;
            }
        }
        
        public void OnForceShowTutorial(int index, bool isIncrement = true)
        { }

        public void HandleShowTutorial(GridUnit triggerUnit, GridUnit targetUnit)
        {
            
        }
    }
}
