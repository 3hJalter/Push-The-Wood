using _Game._Scripts.InGame;
using _Game.DesignPattern.StateMachine;
using _Game.GameGrid.Unit.StaticUnit;
using _Game.Managers;
using _Game.Utilities.Timer;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Game.GameGrid.Unit.DynamicUnit.Enemy.EnemyStates
{
    public class AttackArcherEnemyState : IState<ArcherEnemy>
    {
        public StateEnum Id => StateEnum.Attack;
        private const float ATTACK_TIME = 0.4f;
        private const float ATTACK_ANIM_TIME = 1.15f;
        List<Action> actions = new List<Action>();
        List<float> times = new List<float>() { ATTACK_TIME, ATTACK_ANIM_TIME };

        public void OnEnter(ArcherEnemy t)
        {
            t.ChangeAnim(Constants.ATTACK_ANIM, true);
            actions.Clear();
            actions.Add(Attack);
            actions.Add(ChangeToIdle);
            TimerManager.Ins.WaitForTime(times, actions);

            void Attack()
            {
                LevelManager.Ins.player.IsDead = true;
                AudioManager.Ins.PlaySfx(AudioEnum.SfxType.ShotArrow);
            }
            void ChangeToIdle()
            {
                t.StateMachine.ChangeState(StateEnum.Idle);
            }
        }

        public void OnExecute(ArcherEnemy t)
        {

        }

        public void OnExit(ArcherEnemy t)
        {
        }
    }
}