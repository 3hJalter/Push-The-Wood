using _Game.DesignPattern;
using _Game.DesignPattern.StateMachine;
using _Game.Managers;
using DG.Tweening;
using GameGridEnum;
using UnityEngine;

namespace _Game.GameGrid.Unit.DynamicUnit.Player.PlayerState
{
    public class PushPlayerState : AbstractPlayerState
    {
        float originAnimSpeed;
        float _counterTime;
        private bool _isExecuted;
        private bool _firstTime;
        public override StateEnum Id => StateEnum.Push;
        
        public override void OnEnter(Player t)
        {
            originAnimSpeed = t.AnimSpeed;
            t.ChangeAnim(Constants.PUSH_ANIM, true);
            t.SetAnimSpeed(originAnimSpeed * Constants.PUSH_ANIM_TIME / Constants.PUSH_TIME);
            _counterTime = Constants.PUSH_TIME;
            _isExecuted = false;
            _firstTime = true;
        }

        public override void OnExecute(Player t)
        {
            // BUG: The time checks Idle anim instead of Push anim
            SaveCommand(t);
            if (_counterTime > 0)
            {
                _counterTime -= Time.fixedDeltaTime;
                if (_isExecuted || t.Direction == t.MovingData.inputDirection || t.Direction == Direction.None) return;
                t.StateMachine.ChangeState(StateEnum.Idle);
                return;
            }

            _firstTime = false;
            if (!_isExecuted)
            {
                _isExecuted = true;
                // Push the block Unit
                t.OnPush(t.MovingData.inputDirection);
                ParticlePool.Play(DataManager.Ins.VFXData.GetParticleSystem(VFXType.Dust),
                    t.transform.position + t.skin.transform.forward * (Constants.CELL_SIZE * 0.5f));
                DOVirtual.DelayedCall(Constants.PUSH_TIME / 2, OnCompletePush);

                void OnCompletePush()
                {
                    t.StateMachine.ChangeState(StateEnum.Idle);
                }
            }
        }

        public override void OnExit(Player t)
        {
            t.SetAnimSpeed(originAnimSpeed);
        }
    }
}