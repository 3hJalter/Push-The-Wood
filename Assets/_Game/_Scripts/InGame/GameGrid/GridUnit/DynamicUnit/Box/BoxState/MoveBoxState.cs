using _Game._Scripts.InGame;
using _Game._Scripts.Managers;
using _Game.DesignPattern.StateMachine;
using _Game.Managers;
using DG.Tweening;

namespace _Game.GameGrid.Unit.DynamicUnit.Box.BoxState
{
    public class MoveBoxState : IState<Box>
    {
        private bool _isMove;
        Tween moveTween;
        public StateEnum Id => StateEnum.Move;
        
        public void OnEnter(Box t)
        {
            t.MovingData.SetData(t.BeInteractedData.inputDirection);
            _isMove = t.ConditionMergeOnBePushed.IsApplicable(t.MovingData);
            OnExecute(t);
        }

        public void OnExecute(Box t)
        {
            if (!_isMove)
            {
                t.StateMachine.ChangeState(StateEnum.Block);
            }
            else
            {
                #region Push Hint Step Handler

                if (t.BeInteractedData.pushUnit is Player.Player p)
                {
                    //NOTE: Saving when push dynamic object that make grid change
                    if (LevelManager.Ins.IsSavePlayerPushStep)
                    {
                        GameplayManager.Ins.SavePushHint.SaveStep(p.MainCell.X, p.MainCell.Y, (int) p.LastPushedDirection, p.islandID);        
                    }
                    EventGlobalManager.Ins.OnPlayerPushStep?.Dispatch(new PlayerStep
                    {
                        x = t.BeInteractedData.pushUnitMainCell.X,
                        y = t.BeInteractedData.pushUnitMainCell.Y,
                        d = (int) t.BeInteractedData.inputDirection,
                        i = t.BeInteractedData.pushUnit.islandID
                    });
                }

                #endregion
                t.SetEnterCellData(t.MovingData.inputDirection, t.MovingData.enterMainCell, t.UnitTypeY, false,
                    t.MovingData.enterCells);
                t.OnOutCells();
                t.OnEnterCells(t.MovingData.enterMainCell, t.MovingData.enterCells);
                // Animation and complete
                moveTween = t.Tf.DOMove(t.EnterPosData.initialPos,
                        t.EnterPosData.isFalling ? Constants.MOVING_TIME / 2 : Constants.MOVING_TIME)
                    .SetEase(Ease.Linear).SetUpdate(UpdateType.Fixed).OnComplete(() =>
                    {
                        if (t.EnterPosData.isFalling)
                        {
                            t.StateMachine.ChangeState(StateEnum.Fall);
                        }
                        else
                        {
                            t.OnEnterTrigger(t);
                            if (t.CurrentStateId == Id) t.StateMachine.ChangeState(StateEnum.Idle);
                        }
                    });
            }
        }

        public void OnExit(Box t)
        {
            moveTween.Kill();
        }
    }
}
