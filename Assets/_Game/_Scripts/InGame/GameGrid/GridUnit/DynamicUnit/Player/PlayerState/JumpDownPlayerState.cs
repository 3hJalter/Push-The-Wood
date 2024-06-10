using _Game.DesignPattern;
using _Game.DesignPattern.StateMachine;
using _Game.Managers;
using DG.Tweening;
using GameGridEnum;
using UnityEngine;

namespace _Game.GameGrid.Unit.DynamicUnit.Player.PlayerState
{
    public class JumpDownPlayerState : AbstractPlayerState
    {
        private bool _isExecuted;
        private bool _firstTime;
        public override StateEnum Id => StateEnum.JumpDown;

        public override void OnEnter(Player t)
        {
            t.ChangeAnim(Constants.MOVE_ANIM);
            AudioManager.Ins.PlaySfx(AudioEnum.SfxType.Walk);
            _firstTime = true;
        }

        public override void OnExecute(Player t)
        {
            SaveCommand(t);
            //if (!_firstTime && t.InputDetection.InputAction == InputAction.ButtonDown)
            //{
            //    UpdateDirection(t);
            //}
            if (_isExecuted) return;
            _isExecuted = true;
            _firstTime = false;
            t.Tf.DOMove(t.EnterPosData.initialPos, Constants.MOVING_TIME / 2)
                .SetEase(Ease.Linear).SetUpdate(UpdateType.Fixed).OnComplete(() =>
                {
                    t.Tf.DOMove(t.EnterPosData.finalPos, Constants.MOVING_TIME / 2).SetEase(Ease.Linear)
                        .SetUpdate(UpdateType.Fixed).OnComplete(() =>
                        {
                            ParticlePool.Play(DataManager.Ins.VFXData.GetParticleSystem(VFXType.Dust),
                                t.transform.position - Vector3.up * 0.5f);
                            t.OnEnterTrigger(t);
                            t.StateMachine.ChangeState(StateEnum.Idle);
                        });
                });
        }

        public override void OnExit(Player t)
        {
            _isExecuted = false;
            t.OnCharacterChangePosition();
        }
    }
}