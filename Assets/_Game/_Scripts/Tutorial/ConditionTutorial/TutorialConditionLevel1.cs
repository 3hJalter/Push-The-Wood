using System;
using _Game._Scripts.Managers;
using _Game.GameGrid;
using _Game.GameGrid.Unit;
using _Game.GameGrid.Unit.DynamicUnit.Player;
using _Game.Managers;
using UnityEngine;

namespace _Game._Scripts.Tutorial.ConditionTutorial
{
    [CreateAssetMenu(fileName = "TutorialLevel1", menuName = "ScriptableObjects/TutorialData/Lvl1", order = 1)]
    public class TutorialConditionLevel1 : BaseTutorialData, ITutorialCondition
    {
        public void OnForceShowTutorial(int index, bool isIncrement = true)
        {
            if (index < 0 || index >= tutorialScreens.Count) return;
            currentTutIndex = index;
            currentScreen = UIManager.Ins.OpenUIDirectly(tutorialScreens[currentTutIndex]);
            if (isIncrement) currentTutIndex++;
        }
        
        public void HandleShowTutorial(GameGridCell cell, GridUnit triggerUnit)
        {
            if (triggerUnit is not Player) return;
            switch (currentTutIndex)
            {
                // Case 0 already handle when the cutscene is playing
                case 1:
                {
                    // if player at cell 7, 11
                    if (Math.Abs(cell.WorldX - 7) < Constants.TOLERANCE && Math.Abs(cell.WorldY - 11) < Constants.TOLERANCE) 
                    {
                        currentScreen.CloseDirectly();
                        MoveInputManager.Ins.OnForceResetMove();
                        currentScreen = UIManager.Ins.OpenUIDirectly(tutorialScreens[currentTutIndex]);
                        currentTutIndex++;
                        // FXManager.Ins.TrailHint.OnCancel();
                    }
                    break;
                }
                case 2:
                {
                    if (currentScreen is not null)
                    {
                        // currentScreen.CloseDirectly();
                        currentScreen = null;
                        currentTutIndex++;
                    }
                    break;
                }
            }
        }

        public void HandleShowTutorial(GridUnit triggerUnit, GridUnit targetUnit)
        {
            
        }
    }
}
