using _Game._Scripts.InGame;
using _Game.DesignPattern.StateMachine;
using _Game.GameGrid.Unit.StaticUnit;
using _Game.Managers;
using _Game.Utilities.Timer;
using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Game.GameGrid.Unit.DynamicUnit.Enemy.EnemyStates
{
    public class DieArcherEnemyState : IState<ArcherEnemy>
    {
        private const float DIE_TIME = 1.08f;
        private const float THROW_TIME = 0.4f;
        private STimer timer;
        private List<float> times;
        private List<Action> actions;
        GameGridCell behindCell;
        public StateEnum Id => StateEnum.Die;

        public void OnEnter(ArcherEnemy t)
        {
            if(times == null)
            {
                times = new List<float>() {THROW_TIME + 0.05f , DIE_TIME};
                actions = new List<Action>() {DropIntoWater, () =>
                    {
                        t.RemoveFromLevelManager();
                        t.OnDespawn();
                    }
                };

            }
            if(timer == null)
                timer = TimerManager.Ins.PopSTimer();
            t.ChangeAnim(Constants.DIE_ANIM);
            AudioManager.Ins.PlaySfx(AudioEnum.SfxType.EnemyDie);
            behindCell = t.MainCell.GetNeighborCell(t.Direction);
            t.Tf.DOMove(t.Tf.position + Constants.DirVector3F[t.Direction] * 0.4f, THROW_TIME);
            t.OnOutCells();
            timer.Start(times, actions);

            void DropIntoWater()
            {
                switch (behindCell.Data.gridSurfaceType)
                {
                    case GameGridEnum.GridSurfaceType.Water:
                        t.Tf.DOMoveY(0, Constants.MOVING_TIME * 3);
                        break;
                }
            }
        }

        public void OnExecute(ArcherEnemy t)
        {

        }

        public void OnExit(ArcherEnemy t)
        {
            timer.Stop();
            TimerManager.Ins.PushSTimer(timer);
        }
    }
}
