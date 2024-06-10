using System;
using _Game._Scripts.InGame;
using _Game.DesignPattern.StateMachine;
using _Game.GameGrid.Unit.StaticUnit;
using _Game.Managers;
using _Game.Utilities.Timer;

namespace _Game.GameGrid.Unit.DynamicUnit.Player.PlayerState
{
    public class DiePlayerState : AbstractPlayerState
    {
        private const float DIE_TIME = 1.08f;
        public override StateEnum Id => StateEnum.Die;

        public override void OnEnter(Player t)
        {
            GameplayManager.Ins.IsCanUndo = false;
            GameplayManager.Ins.IsCanResetIsland = false;
            AudioManager.Ins.PlaySfx(AudioEnum.SfxType.PlayerPassout);
            t.ChangeAnim(Constants.DIE_ANIM);
            TimerManager.Ins.WaitForTime(DIE_TIME, OnLoseGame);
            void OnLoseGame()
            {
                LevelLoseCondition loseCondition = LevelLoseCondition.Enemy;
                if (t.CutTreeData.tree is TreeBee) // TEMPORARY because currently only have two type of die
                {
                    loseCondition = LevelLoseCondition.Bee;
                }
                GameManager.Ins.PostEvent(DesignPattern.EventID.LoseGame, loseCondition);
            }
        }

        public override void OnExecute(Player t)
        {
            
        }

        public override void OnExit(Player t)
        {
            
        }
    }
}
