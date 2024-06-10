using _Game.DesignPattern.StateMachine;
using DG.Tweening;
using UnityEngine;

namespace _Game.GameGrid.Unit.DynamicUnit.ButtonUnitState
{
    public class IdleButtonState : IState<ButtonUnit>
    {
        private bool isFirstEnterDone;
        private float _enterTime;
        private float _exitTime;
        private const float HALF_BUTTON_ENTER_TIME = 0.15f;
        public StateEnum Id => StateEnum.Idle;
        public void OnEnter(ButtonUnit t)
        {
            if (!isFirstEnterDone)
            {
                t.ChangeButton(false);
                isFirstEnterDone = true;
            }
            else  if (t.StateMachine.PreviousState is StateEnum.Enter)
            {
                _enterTime = Time.time;
                // Check if the time between the last exit and the current enter is less than 0.4f
                float interval = _enterTime - _exitTime;
                if (interval < HALF_BUTTON_ENTER_TIME)
                {
                    // Do a tween that changes the size y from current y to 30 in 0.15 - (_enterTime - _exitTime) seconds
                    t.animTween?.Kill();
                    t.ChangeButton(true);
                    t.animTween = t.BtnModelTransform.DOScaleY(30, HALF_BUTTON_ENTER_TIME - interval).SetEase(Ease.OutBack)
                        .OnComplete(() =>
                        {
                            t.ChangeButton(false);
                            t.animTween = t.BtnModelTransform.DOScaleY(100, HALF_BUTTON_ENTER_TIME).SetEase(Ease.InBack);
                        });
                }
                else
                {
                    t.ChangeButton(false);
                }
            }
        }

        public void OnExecute(ButtonUnit t)
        {
        }

        public void OnExit(ButtonUnit t)
        {
            _exitTime = Time.time;
        }
    }
}
