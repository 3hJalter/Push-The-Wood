using _Game.DesignPattern;
using _Game.DesignPattern.StateMachine;
using _Game.Utilities.Timer;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using _Game._Scripts.InGame;
using _Game._Scripts.InGame.GameCondition.Data;
using _Game._Scripts.Managers;
using UnityEngine;
using _Game.Utilities;
using _Game.Managers;
using System.Linq;

namespace _Game.GameGrid.Unit.DynamicUnit.Chump.ChumpState
{
    public class RollBlockChumpState : IState<Chump>
    {
        private int DEGREE = 60;
        Vector3 axis;
        float lastAngle = 0;
        Direction blockDirection;
        Tween moveTween;
        bool damage;

        List<GridUnit> blockObjects = new();

        public StateEnum Id => StateEnum.Block;

        public void OnEnter(Chump t)
        {
            blockObjects.Clear();
            damage = false;
            GameplayManager.Ins.IsCanUndo = false;
            OnExecute(t);
        }

        public void OnExecute(Chump t)
        {
            switch (t.UnitTypeY)
            {
                case UnitTypeY.Up:
                    GetBlockObjects(t.MainMovingData);
                    //NOTE: Blocking when chump is up
                    blockDirection = t.MainMovingData.inputDirection;
                    axis = Vector3.Cross(Vector3.up, Constants.DirVector3[blockDirection]);
                    lastAngle = 0;
                    DEGREE = 90 - (blockObjects[0].Size.y + 1) * 15;

                    moveTween = DOVirtual.Float(0, DEGREE * 2, Constants.MOVING_TIME * 1f, i =>
                    {
                        i = i <= DEGREE ? i : 2 * DEGREE - i;
                        t.skin.RotateAround(t.anchor.Tf.position, axis, i - lastAngle);
                        lastAngle = i;
                    }).SetUpdate(UpdateType.Fixed)
                    .SetEase(Ease.OutQuad)
                    .OnComplete(() => t.StateMachine.ChangeState(StateEnum.Idle));
                    damage = true;

                    if (blockObjects.Count > 0)
                        TimerManager.Ins.WaitForTime(Constants.MOVING_TIME * 0.5f, ObjectBlocking);
                    break;
                case UnitTypeY.Down:
                    //NOTE: Blocking when chump is down
                    GetBlockObjects(t.MainMovingData);
                    blockDirection = t.MainMovingData.inputDirection;
                    if (!IsSamePushDirection())
                    {
                        Vector3 originPos = t.Tf.position;
                        damage = true;
                        moveTween = t.Tf.DOMove(originPos + Constants.DirVector3F[blockDirection] * Constants.CELL_SIZE / 2f, Constants.MOVING_TIME * 0.5f).SetEase(Ease.InQuad)
                            .OnComplete(() => t.Tf.DOMove(originPos, Constants.MOVING_TIME * 0.5f).SetEase(Ease.OutQuad).OnComplete(ChangeToIdle));
                    }
                    else
                    {
                        Vector3 originPos = t.Tf.position;
                        moveTween = t.Tf.DOShakePosition(Constants.MOVING_TIME * 0.8f, 0.1f, 60, 0, false, true, ShakeRandomnessMode.Harmonic).SetEase(Ease.InQuad)
                            .OnComplete(ChangeToIdle);
                    }
                    if (blockObjects.Count > 0)
                        TimerManager.Ins.WaitForTime(Constants.MOVING_TIME * 0.5f, ObjectBlocking);

                    break;
            }

            return;

            void GetBlockObjects(MovingData data)
            {
                blockObjects.AddRange(data.blockStaticUnits);
                blockObjects.AddRange(data.blockDynamicUnits);
            }

            void ObjectBlocking()
            {
                string objectBlocking = "BLOCKING - ";
                for (int i = 0; i < blockObjects.Count; i++)
                {
                    blockObjects[i].OnBePushed(blockDirection, t);
                    //NOTE: Deal Damage
                    if (damage)
                    {
                        if (blockObjects[i] is IDamageable damageable)
                        {
                            damageable.IsDead = true;
                        }
                    }                   
                    objectBlocking += $"{blockObjects[i]} ";
                }
                //NOTE: Checking if push dynamic object does not create any change in grid -> discard the newest save.
                HashSet<GameUnit> changeUnits = LevelManager.Ins.SaveChangeUnits;
                if (!LevelManager.Ins.CurrentLevel.GridMap.IsChange)
                {
                    LevelManager.Ins.DiscardSaveState();
                }
                else
                {
                    if(changeUnits.Count == 1 && changeUnits.First().PoolType == PoolType.Player)
                    {
                        LevelManager.Ins.DiscardSaveState();
                        return;
                    }
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
                }
            }

            bool IsSamePushDirection()
            {
                if ((t.UnitTypeXZ == UnitTypeXZ.Horizontal && (t.MainMovingData.inputDirection == Direction.Left || t.MainMovingData.inputDirection == Direction.Right))
                    || (t.UnitTypeXZ == UnitTypeXZ.Vertical && (t.MainMovingData.inputDirection == Direction.Forward || t.MainMovingData.inputDirection == Direction.Back)))
                {
                    return true;
                }
                return false;
            }

            void ChangeToIdle()
            {
                t.StateMachine.ChangeState(StateEnum.Idle);
            }
        }

        public void OnExit(Chump t)
        {
            GameplayManager.Ins.IsCanUndo = true;
            moveTween.Kill();
        }

    }
}