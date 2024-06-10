using System.Collections.Generic;
using _Game._Scripts.InGame.GameCondition.Data;
using _Game.DesignPattern.ConditionRule;
using _Game.DesignPattern.StateMachine;
using _Game.GameGrid.Unit.DynamicUnit.Chump.ChumpState;
using _Game.GameGrid.Unit.Interface;
using _Game.Managers;
using AudioEnum;
using GameGridEnum;
using UnityEngine;

namespace _Game.GameGrid.Unit.DynamicUnit.Chump
{
    public class Chump : GridUnitDynamic, IJumpTreeRootUnit, IBeInteractedUnit
    {
        [SerializeField] private StateEnum onBePushedStraightState;
        [SerializeField] private StateEnum onBePushedPerpendicularState;
        [SerializeField] private Raft.Raft raftPrefab;

        private StateMachine<Chump> stateMachine;
        public StateMachine<Chump> StateMachine => stateMachine;
        public override StateEnum CurrentStateId
        {
            get => stateMachine?.CurrentStateId ?? StateEnum.Idle;
            set => stateMachine.ChangeState(value);
        }

        private bool _isAddState;

        public Raft.Raft RaftPrefab => raftPrefab;

        public bool CanJumpOnTreeRoot(Direction direction = Direction.None)
        {
            if (gridUnitDynamicType is GridUnitDynamicType.ChumpHigh) return false;
            return direction switch
            {
                Direction.Forward or Direction.Back when UnitTypeXZ is UnitTypeXZ.None
                    or UnitTypeXZ.Vertical => true,
                Direction.Left or Direction.Right when UnitTypeXZ is UnitTypeXZ.None
                    or UnitTypeXZ.Horizontal => true,
                _ => false
            };
        }

        public override void OnInit(GameGridCell mainCellIn, HeightLevel startHeightIn = HeightLevel.One,
            bool isUseInitData = true, Direction skinDirection = Direction.None, bool hasSetPosAndRot = false)
        {
            unitTypeY = UnitTypeY.Up;
            unitTypeXZ = UnitTypeXZ.None;
            base.OnInit(mainCellIn, startHeightIn, isUseInitData, skinDirection, hasSetPosAndRot);
            if (!_isAddState)
            {
                _isAddState = true;
                stateMachine = new StateMachine<Chump>(this);
                //stateMachine.Debug = true;
                AddState();
            }
            stateMachine.ChangeState(StateEnum.Idle);
        }
        public override void OnDespawn()
        {
            base.OnDespawn();
            unitTypeY = UnitTypeY.Up;
            unitTypeXZ = UnitTypeXZ.None;
            stateMachine.ChangeState(StateEnum.Idle);
        }

        private void AddState()
        {
            stateMachine.AddState(StateEnum.Idle, new IdleChumpState());
            stateMachine.AddState(StateEnum.FormRaft, new FormRaftChumpState());
            stateMachine.AddState(StateEnum.Move, new MoveChumpState());
            stateMachine.AddState(StateEnum.Roll, new RollChumpState());
            stateMachine.AddState(StateEnum.TurnOver, new TurnOverChumpState());
            stateMachine.AddState(StateEnum.Fall, new FallChumpState());
            stateMachine.AddState(StateEnum.Block, new RollBlockChumpState());
            stateMachine.AddState(StateEnum.Emerge, new EmergeChumpState());
        }

        public override void OnPush(Direction direction, ConditionData conditionData = null)
        {
            List<GridUnitDynamic> blockUnits =
                conditionData is MovingData movingData ? movingData.blockDynamicUnits : new List<GridUnitDynamic>();
            for (int i = 0; i < blockUnits.Count; i++) blockUnits[i].OnBePushed(direction, this);
        }

        public override void OnBePushed(Direction direction, GridUnit pushUnit)
        {

            #region Check Condition

            BeInteractedData.SetData(direction, pushUnit);
            if (!conditionMergeOnBeInteracted.IsApplicable(BeInteractedData))
                // if (_currentState != states[StateEnum.Idle])
                //     ChangeState(StateEnum.Idle); 
                return;

            #endregion
            base.OnBePushed(direction, pushUnit);
            // Temporary fix for sound repeatedly when chump rolling (set pushUnit = this is rolling)
            if (pushUnit != this) AudioManager.Ins.PlaySfx(SfxType.PushChump); 
            #region Be push when below Box and in water

            if (pushUnit is Box.Box && IsInWater())
            {
                stateMachine.ChangeState(StateEnum.Move);
                return;
            }

            #endregion
            ChangeRollState(direction);
        }

        public void ChangeRollState(Direction direction)
        {
            if (unitTypeY is UnitTypeY.Up)
            {
                stateMachine.ChangeState(StateEnum.TurnOver);
            }
            else
            {
                if (IsPerpendicular(direction))
                {
                    stateMachine.ChangeState(onBePushedPerpendicularState);
                }
                else
                {
                    stateMachine.ChangeState(onBePushedStraightState);
                }
            }
        }

        public bool IsPerpendicular(Direction direction)
        {
            if (unitTypeXZ is UnitTypeXZ.Horizontal)
            {
                if (direction is Direction.Left or Direction.Right)
                    return false;
                else if (direction is Direction.Forward or Direction.Back)
                    return true;
            }
            else if (unitTypeXZ is UnitTypeXZ.Vertical)
            {
                if (direction is Direction.Left or Direction.Right)
                    return true;
                else if (direction is Direction.Forward or Direction.Back)
                    return false;
            }
            return false;
        }

        protected override void OnOutTriggerBelow(GridUnit triggerUnit)
        {
            base.OnOutTriggerBelow(triggerUnit);
            if (!LevelManager.Ins.IsConstructingLevel && IsCurrentStateIs(StateEnum.Idle))
                stateMachine.ChangeState(StateEnum.Fall);
        }

        protected override void OnEnterTriggerUpper(GridUnit triggerUnit)
        {
            switch (triggerUnit)
            {
                case Box.Box box:
                    OnBePushed(box.LastPushedDirection, box);
                    break;
                case Chump chump:
                    if (!chump.IsCurrentStateIs(StateEnum.FormRaft) && chump.IsOnWater())
                    {
                        foreach (GridUnit belowUnit in chump.belowUnits)
                        {
                            // if one of all below unit of TriggerChump is not Chump, return
                            if (belowUnit is not Chump) return;
                            // if the trigger chump is Up or in Fall state, not consider the below unit
                            if (chump.UnitTypeY is UnitTypeY.Up || chump.CurrentStateId is StateEnum.Fall || chump.CurrentStateId is StateEnum.TurnOver) continue;
                            // if the TriggerChump is Down and one of all below unit of TriggerChump is not same UnitTypeXZ, return
                            if (chump.UnitTypeXZ != belowUnit.UnitTypeXZ) return;
                        }
                        chump.StateMachine.ChangeState(StateEnum.FormRaft);
                    }
                    break;
            }
        }

        public MovingData MainMovingData => TurnOverData.UsingTurnId > MovingData.UsingTurnId ? TurnOverData : MovingData;

        public override bool IsCurrentStateIs(StateEnum stateEnum)
        {
            return stateMachine.CurrentState.Id == stateEnum;
        }

        public void SwitchType(Direction direction)
        {
            if (unitTypeY is UnitTypeY.Up)
            {
                unitTypeY = UnitTypeY.Down;
                unitTypeXZ = direction switch
                {
                    Direction.Back or Direction.Forward => UnitTypeXZ.Vertical,
                    Direction.Left or Direction.Right => UnitTypeXZ.Horizontal,
                    _ => unitTypeXZ
                };
            }
            else
            {
                unitTypeY = UnitTypeY.Up;
                unitTypeXZ = UnitTypeXZ.None;
            }
        }

        #region Rule

        [SerializeField] private ConditionMerge conditionMergeOnBeInteracted;
        [SerializeField] private ConditionMerge conditionMergeOnBePushed;

        public ConditionMerge ConditionMergeOnBeInteracted => conditionMergeOnBeInteracted;
        public ConditionMerge ConditionMergeOnBePushed => conditionMergeOnBePushed;

        private BeInteractedData beInteractedData;
        private TurnOverData turnOverData;
        private MovingData _movingData;

        public BeInteractedData BeInteractedData =>
            beInteractedData ??= new BeInteractedData(this);

        public TurnOverData TurnOverData => turnOverData ??= new TurnOverData(this);
        public MovingData MovingData => _movingData ??= new MovingData(this);
        #endregion

        #region SAVING DATA

        // Current Empty

        #endregion
    }
}
