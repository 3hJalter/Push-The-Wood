using _Game.DesignPattern.StateMachine;
using _Game.Managers;
using AudioEnum;
using DG.Tweening;
using UnityEngine;

namespace _Game.GameGrid.Unit.DynamicUnit.Player.PlayerState
{
    public class MovePlayerState : AbstractPlayerState
    {
        private bool firstTime;
        private float initAnimSpeed;
        private Tween moveTween;
        private float originMovingDistance;

        public override StateEnum Id => StateEnum.Move;

        public override void OnEnter(Player t)
        {
            t.ChangeAnim(Constants.MOVE_ANIM);
            initAnimSpeed = t.AnimSpeed;
            t.SetAnimSpeed(initAnimSpeed * (Constants.MOVING_ANIM_TIME / Constants.MOVING_TIME));
            firstTime = true;
            AudioManager.Ins.PlaySfx(SfxType.Walk);
        }
        public override void OnExecute(Player t)
        {
            //NOTE: Cache input and speed up animation
            SaveCommand(t);
            if (!firstTime && t.InputDetection.InputAction == InputAction.ButtonDown)
            {
                // TEMPORARY:
                t.SetAnimSpeed(initAnimSpeed / Constants.MOVING_TIME_FAST_RATE * (Constants.MOVING_ANIM_TIME / Constants.MOVING_TIME));
                moveTween.Kill();
                moveTween = t.Tf.DOMove(t.EnterPosData.finalPos, CalculateMoveRemainingTime())
                    .SetEase(Ease.Linear).SetUpdate(UpdateType.Fixed).OnComplete(() =>
                    {
                        t.LookDirection(t.Direction);
                        t.SetAnimSpeed(initAnimSpeed);

                        t.OnEnterTrigger(t);
                        t.StateMachine.ChangeState(StateEnum.Idle);
                    });
            }

            if (moveTween != null) return;
            firstTime = false;
            originMovingDistance = (t.EnterPosData.finalPos - t.Tf.position).sqrMagnitude;
            moveTween = t.Tf.DOMove(t.EnterPosData.finalPos, Constants.MOVING_TIME)
                .SetEase(Ease.Linear).SetUpdate(UpdateType.Fixed).OnComplete(() =>
                {
                    t.OnEnterTrigger(t);
                    t.StateMachine.ChangeState(StateEnum.Idle);
                });


            float CalculateMoveRemainingTime()
            {
                float remainingDistance = (t.EnterPosData.finalPos - t.Tf.position).sqrMagnitude;
                return Constants.MOVING_TIME_FAST_RATE * Constants.MOVING_TIME *
                       Mathf.Sqrt(remainingDistance / originMovingDistance);
            }
        }
        public override void OnExit(Player t)
        {
            moveTween.Kill();
            moveTween = null;
            t.OnCharacterChangePosition();
            t.SetAnimSpeed(initAnimSpeed);
        }
    }
}
