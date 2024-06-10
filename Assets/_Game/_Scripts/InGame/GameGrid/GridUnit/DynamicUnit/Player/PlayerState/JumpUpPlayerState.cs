using _Game.DesignPattern;
using _Game.DesignPattern.StateMachine;
using _Game.Managers;
using DG.Tweening;
using GameGridEnum;
using UnityEngine;

namespace _Game.GameGrid.Unit.DynamicUnit.Player.PlayerState
{
    public class JumpUpPlayerState : AbstractPlayerState
    {
        private bool _isExecuted;
        private bool _firstTime;

        public override StateEnum Id => StateEnum.JumpUp;

        public override void OnEnter(Player t)
        {
            t.ChangeAnim(Constants.JUMP_UP_ANIM);
            _firstTime = true;
            AudioManager.Ins.PlaySfx(AudioEnum.SfxType.Walk);
            ParticlePool.Play(DataManager.Ins.VFXData.GetParticleSystem(VFXType.Dust),
                t.transform.position - Vector3.up * 0.5f);
        }

        public override void OnExecute(Player t)
        {
            //if (!_firstTime && t.InputDetection.InputAction == InputAction.ButtonDown)
            //{
            //    UpdateDirection(t);
            //}
            SaveCommand(t);
            if (_isExecuted) return;
            _isExecuted = true;
            _firstTime = false;
            t.Tf.DOMove(t.EnterPosData.finalPos, Constants.MOVING_TIME)
                .SetEase(Ease.Linear).SetUpdate(UpdateType.Fixed).OnComplete(() =>
                {
                    t.OnEnterTrigger(t);
                    t.StateMachine.ChangeState(StateEnum.Idle);
                });
        }

        public override void OnExit(Player t)
        {
            _isExecuted = false;
            t.OnCharacterChangePosition();
        }
    }
}