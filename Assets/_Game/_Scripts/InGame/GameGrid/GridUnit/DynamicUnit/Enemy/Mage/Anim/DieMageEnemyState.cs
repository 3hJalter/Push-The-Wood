using _Game.DesignPattern.StateMachine;
using _Game.Utilities.Timer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace _Game.GameGrid.Unit.DynamicUnit.Enemy.EnemyStates
{
    public class DieMageEnemyState : IState<MageEnemy>
    {
        private const float DIE_TIME = 1.08f;
        private STimer timer;
        public StateEnum Id => StateEnum.Die;

        public void OnEnter(MageEnemy t)
        {
            timer = TimerManager.Ins.PopSTimer();
            t.ChangeAnim(Constants.DIE_ANIM);
            timer.Start(DIE_TIME, () =>
            {
                t.RemoveFromLevelManager();
                t.OnDespawn();
            });
        }

        public void OnExecute(MageEnemy t)
        {
            
        }

        public void OnExit(MageEnemy t)
        {
            timer.Stop();
            TimerManager.Ins.PushSTimer(timer);
        }
    }
}