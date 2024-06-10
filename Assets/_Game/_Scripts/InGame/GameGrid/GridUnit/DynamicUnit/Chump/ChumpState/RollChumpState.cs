using _Game._Scripts.InGame;
using _Game._Scripts.Managers;
using _Game.DesignPattern.StateMachine;
using _Game.Managers;
using _Game.Utilities;
using DG.Tweening;
using UnityEngine;

namespace _Game.GameGrid.Unit.DynamicUnit.Chump.ChumpState
{
    public class RollChumpState : IState<Chump>
    {
        private bool _isRoll;
        Tween moveTween;
        public StateEnum Id => StateEnum.Roll;

        public void OnEnter(Chump t)
        {
            t.MovingData.SetData(t.BeInteractedData.inputDirection);
            _isRoll = t.ConditionMergeOnBePushed.IsApplicable(t.MovingData);
            OnExecute(t);
        }

        public void OnExecute(Chump t)
        {
            if (!_isRoll)
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
                // TODO: rotate in place animation for skin
                moveTween = t.Tf.DOMove(t.EnterPosData.initialPos,
                        t.EnterPosData.isFalling ? Constants.MOVING_TIME * 0.8f : Constants.MOVING_TIME)
                    .SetEase(Ease.Linear).SetUpdate(UpdateType.Fixed).OnComplete(() =>
                    {
                        if (t.EnterPosData.isFalling)
                        {
                            //Falling to water
                            t.StateMachine.ChangeState(StateEnum.Fall);
                        }
                        else
                        {
                            t.OnEnterTrigger(t);
                            t.StateMachine.ChangeState(StateEnum.Idle);
                            if (t.gameObject.activeSelf)
                                t.OnBePushed(t.BeInteractedData.inputDirection, t);
                        }
                    });
            }
        }

        public void OnExit(Chump t)
        {
            moveTween.Kill();
        }

        private Vector3 GetRotateAxis(GridUnit t, Direction direction)
        {
            Vector3 axis = t.skin.localRotation.eulerAngles;
            if (direction is Direction.Back or Direction.Forward) axis.x += 359;
            else if (direction is Direction.Left or Direction.Right) axis.z += 359;
            return axis;
        }
    }
}
