using System;
using _Game._Scripts.Managers;
using _Game.GameGrid;
using _Game.GameGrid.Unit;
using _Game.GameGrid.Unit.DynamicUnit.Chump;
using _Game.GameGrid.Unit.DynamicUnit.Player;
using _Game.Managers;
using _Game.Resource;
using _Game.Utilities;
using UnityEngine;
using VinhLB;
using Tree = _Game.GameGrid.Unit.StaticUnit.Tree;

namespace _Game._Scripts.Tutorial.ConditionTutorial
{
    [CreateAssetMenu(fileName = "TutorialLevel4", menuName = "ScriptableObjects/TutorialData/Lvl4", order = 1)]
    public class TutorialConditionLevel4 : BaseTutorialData, ITutorialCondition
    {
        public void HandleShowTutorial(GameGridCell cell, GridUnit triggerUnit)
        {
            if (currentTutIndex == 0)
            {
                if (triggerUnit is not Player) return;
                int boosterUnlockIndex = DataManager.Ins.ConfigData.boosterConfigList[(int) BoosterType.Undo].UnlockAtLevel;
                if (!TutorialManager.Ins.IsOneTimeTutCompleted(boosterUnlockIndex))
                {
                    GameTutorialScreenPage ui = (GameTutorialScreenPage) UIManager.Ins.OpenUIDirectly(tutorialScreens[0]);
                    ui.OnCloseCallback += () =>
                    {
                        UIManager.Ins.OpenUI<OverlayScreen>().BoosterUnlockEffectUI.PlayUnlockEffect(BoosterType.Undo);
                        DataManager.Ins.GameData.user.completedOneTimeTutorial
                            .Add(boosterUnlockIndex); // UNDO index = 0
                    };
                }
                currentTutIndex++;
            }
            
        }

        public void OnForceShowTutorial(int index, bool isIncrement = true)
        { }

        public void HandleShowTutorial(GridUnit triggerUnit, GridUnit targetUnit)
        { }
    }
}
