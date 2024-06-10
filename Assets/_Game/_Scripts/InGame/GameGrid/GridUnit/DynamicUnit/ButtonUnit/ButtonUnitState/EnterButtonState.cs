using _Game._Scripts.Managers;
using _Game.DesignPattern.StateMachine;
using DG.Tweening;

namespace _Game.GameGrid.Unit.DynamicUnit.ButtonUnitState
{
    public class EnterButtonState : IState<ButtonUnit>
    {
        private const float BUTTON_ENTER_TIME = 0.3f;
        public StateEnum Id => StateEnum.Enter;
        private bool _isComplete;
        public void OnEnter(ButtonUnit t)
        {
            // Tween to change the size y from 100 to 20 in BUTTON_ENTER_TIME
            _isComplete = false;
            t.animTween?.Kill();
            t.ChangeButton(true);
            t.animTween = t.BtnModelTransform.DOScaleY(20, BUTTON_ENTER_TIME).SetEase(Ease.OutBack).OnComplete(() =>
            {
                _isComplete = true;
                EventGlobalManager.Ins.OnButtonUnitEnter?.Dispatch(true);
            });
        }

        public void OnExecute(ButtonUnit t)
        {
        }

        public void OnExit(ButtonUnit t)
        {
            t.animTween?.Kill();
            t.ChangeButton(false);
            // Tween to change the size y from 20 to 100 in BUTTON_ENTER_TIME
            t.animTween = t.BtnModelTransform.DOScaleY(100, BUTTON_ENTER_TIME).SetEase(Ease.InBack);
            // Check if complete tween
            if (!_isComplete) return;
            _isComplete = false;
            EventGlobalManager.Ins.OnButtonUnitEnter?.Dispatch(false);
            // DUST Effect
        }
    }
}
