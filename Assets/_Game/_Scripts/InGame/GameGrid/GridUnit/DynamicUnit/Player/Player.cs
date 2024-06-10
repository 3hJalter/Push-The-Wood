using System;
using System.Collections.Generic;
using _Game._Scripts.InGame;
using _Game._Scripts.InGame.GameCondition.Data;
using _Game._Scripts.Managers;
using _Game._Scripts.Utilities;
using _Game.AI;
using _Game.DesignPattern.ConditionRule;
using _Game.DesignPattern.StateMachine;
using _Game.GameGrid.Unit.DynamicUnit.Interface;
using _Game.GameGrid.Unit.DynamicUnit.Player.PlayerState;
using _Game.GameGrid.Unit.StaticUnit;
using _Game.Managers;
using _Game.Utilities;
using GameGridEnum;
using HControls;
using MoreMountains.NiceVibrations;
using UnityEngine;

namespace _Game.GameGrid.Unit.DynamicUnit.Player
{
    public class Player : GridUnitCharacter, IJumpTreeRootUnit, ICharacter
    {
        #region MODULE
        [SerializeField]
        AutoMoveAgent agent;
        #endregion
        #region PROPERTYS
        [HideInInspector]
        public bool isRideVehicle;
        [HideInInspector]
        public bool IsStun = false;
        public Transform[] VFXPositions;
        #endregion
        #region INPUT CACHE
        public readonly Queue<Direction> CommandCache = new();
        public bool IsInputCache { get; private set; } // DEV: Can put this in separate component
        #endregion

        public Direction InputDirection;
        private StateMachine<Player> stateMachine;
        public StateMachine<Player> StateMachine => stateMachine;
        public override StateEnum CurrentStateId
        {
            get => StateEnum.Idle;
            set => stateMachine.ChangeState(value);
        }
        
        private IVehicle _vehicle;
        public InputDetection InputDetection { get; private set; } = new InputDetection();
        public override GameGridCell MainCell
        {
            get => mainCell;
            protected set
            {
                mainCell = value;
                CheckingStunState();
            }
        }

        #region Trigger Event

        public event Action<GameGridCell, GridUnit> OnMoveToCell;
        public event Action<GridUnit, GridUnit> OnActWithUnit;

        public void SetMoveToCellEvent(Action<GameGridCell, GridUnit> action)
        {
            OnMoveToCell = action;
        }
        
        public void SetActWithUnitEvent(Action<GridUnit, GridUnit> action)
        {
            OnActWithUnit = action;
        }
        
        public void InvokeActWithUnit(GridUnit targetUnit)
        {
            OnActWithUnit?.Invoke(this, targetUnit);
        }
        
        #endregion
        
        private void Awake()
        {
            agent.enabled = false;
            agent.Init(this);
            GameManager.Ins.RegisterListenerEvent(DesignPattern.EventID.WinGame, OnWin);
        }
        private void FixedUpdate()
        {
            //if (!(GameManager.Ins.IsState(GameState.InGame) || agent.isActiveAndEnabled)) return;

            //NOTE: 
            if (!agent.isActiveAndEnabled)
            {
                InputDirection = HInputManager.GetDirectionInput();
            }
            // TEST: Reset the Input if Direction is not none and Move is Swipe (Swipe only take one input per swipe)
            // if (Direction != Direction.None && MoveInputManager.Ins.CurrentChoice is MoveInputManager.MoveChoice.Swipe) HInputManager.SetDefault();
            //stateMachine.Debug = true;
            if (InputDirection is not Direction.None) lastPushedDirection = InputDirection;
            InputDetection.GetInput(InputDirection);
            stateMachine?.UpdateState();

        }

        public bool CanJumpOnTreeRoot(Direction direction = Direction.None)
        {
            return true;
        }

        public override void OnInit(GameGridCell mainCellIn, HeightLevel startHeightIn = HeightLevel.One,
            bool isUseInitData = true, Direction skinDirection = Direction.None, bool hasSetPosAndRot = false)
        {
            base.OnInit(mainCellIn, startHeightIn, isUseInitData, skinDirection, hasSetPosAndRot);
            ChangeSkin(DataManager.Ins.CurrentUIPlayerSkinIndex);
            if (!_isAddState)
            {
                _isAddState = true;
                stateMachine = new StateMachine<Player>(this);
                AddState();
            }
            //stateMachine.Debug = false;          
            skin.localPosition = Vector3.zero;
            skin.localRotation = Quaternion.identity;
            Direction = Direction.None;
            IsDead = false;
            IsStun = false;
            stateMachine.ChangeState(StateEnum.Idle);
            if(GameManager.Ins.IsState(GameState.InGame))
                EventGlobalManager.Ins.OnPlayerChangeIsland?.Dispatch(true);
        }

        public override void OnDespawn()
        {
            _vehicle = null;
            if (_isAddState) stateMachine.OverrideState = StateEnum.None;
            CommandCache.Clear();
            base.OnDespawn();
        }

        protected override void AddState()
        {
            stateMachine.AddState(StateEnum.Idle, new IdlePlayerState());
            stateMachine.AddState(StateEnum.Move, new MovePlayerState());
            stateMachine.AddState(StateEnum.Interact, new InteractPlayerState());
            stateMachine.AddState(StateEnum.Push, new PushPlayerState());
            stateMachine.AddState(StateEnum.JumpUp, new JumpUpPlayerState());
            stateMachine.AddState(StateEnum.JumpDown, new JumpDownPlayerState());
            stateMachine.AddState(StateEnum.CutTree, new CutTreePlayerState());
            stateMachine.AddState(StateEnum.Die, new DiePlayerState());
            stateMachine.AddState(StateEnum.Happy, new HappyPlayerState());
            stateMachine.AddState(StateEnum.Stun, new StunPlayerState());
            stateMachine.AddState(StateEnum.Sleep, new SleepPlayerState());
            stateMachine.AddState(StateEnum.SitDown, new SitDownPlayerState());
            stateMachine.AddState(StateEnum.RunAboveChump, new RunAboveChumpPlayerState());
        }

        public override void OnPush(Direction direction, ConditionData conditionData = null)
        {
            // Only push the highest unit, which is last index of the list
            if (MovingData.blockDynamicUnits.Count > 0)
            {
                #region Save
                LevelManager.Ins.SaveGameState(true);
                mainCell.ValueChange();
                MovingData.blockDynamicUnits[^1].MainCell.ValueChange();
                LevelManager.Ins.SaveGameState(false);
                #endregion
                MovingData.blockDynamicUnits[^1].OnBePushed(direction, this);
                // TutorialManager.Ins.OnUnitActWithOther(this, MovingData.blockDynamicUnits[^1]);
                InvokeActWithUnit(MovingData.blockDynamicUnits[^1]);
                HVibrate.Haptic(HapticTypes.MediumImpact);
                return;
            }
            if (MovingData.blockStaticUnits.Count > 0)
            {
                MovingData.blockStaticUnits[^1].OnBePushed(direction, this);
                // TutorialManager.Ins.OnUnitActWithOther(this, MovingData.blockStaticUnits[^1]);
                InvokeActWithUnit(MovingData.blockStaticUnits[^1]);
                HVibrate.Haptic(HapticTypes.LightImpact);
            }
        }

        public override void OnEnterCells(GameGridCell enterMainCell, List<GameGridCell> enterNextCells = null)
        {
            base.OnEnterCells(enterMainCell, enterNextCells);
            OnMoveToCell?.Invoke(enterMainCell, this);
        }

        public override bool IsCurrentStateIs(StateEnum stateEnum)
        {
            return stateMachine.CurrentState.Id == stateEnum;
        }

        public bool HasVehicle()
        {
            return _vehicle is not null;
        }

        public void SetIslandId(GameGridCell nextMainCell, out bool isChangeIsland)
        {
            isChangeIsland = false;
            if (islandID == nextMainCell.IslandID || nextMainCell.IslandID == -1) return;
            islandID = nextMainCell.IslandID;
            isChangeIsland = true;
            EventGlobalManager.Ins.OnPlayerChangeIsland?.Dispatch(false);
            LevelManager.Ins.CurrentLevel.SetFirstPlayerStepOnIsland(nextMainCell);
        }

        public void SetVehicle(IVehicle vehicle)
        {
            _vehicle = vehicle;
        }

        public void OnRideVehicle(Direction tDirection)
        {
            #region Save
            LevelManager.Ins.SaveGameState(true);
            MainCell.ValueChange();
            ((GridUnit)_vehicle).MainCell.ValueChange();
            LevelManager.Ins.SaveGameState(false);
            #endregion
            isRideVehicle = true;
            GameplayManager.Ins.IsCanUndo = false;
            _vehicle.Ride(tDirection, this);
        }

        public bool CanRideVehicle(Direction tDirection)
        {
            // Temporary for only raft
            if (_vehicle is not Raft.Raft r) return false;
            // Check if there is a unit in the direction
            GameGridCell targetCell = mainCell.GetNeighborCell(tDirection);
            if (targetCell == null) return false;
            // Get all unit in the target cell, from Player StartHeight to Player EndHeight
            List<GridUnit> units = targetCell.GetGridUnits(StartHeight, EndHeight);
            // if no unit, mean that player can move out the raft to the target cell
            if (units == null || units.Count == 0) return false;
            // if there is a unit, check if it is a tree root
            if (units.Count == 1 && units[0] is TreeRoot) return false;
            // Set the unit to the vehicle
            r.blockDirectionUnits = units;
            return true;
        }
        public void CheckingStunState()
        {
            if (mainCell != null && !IsStun && mainCell.Data.IsDanger)
            {
                GameManager.Ins.PostEvent(DesignPattern.EventID.ObjectInOutDangerCell, mainCell);
                IsStun = true;
            }
        }

        public (GameGridCell, GameGridCell) SetActiveAgent(bool value)
        {
            GameGridCell waterCell = null, playerCell = null;
            if (value)
            {
                agent.enabled = true;
                (waterCell, playerCell) = agent.GetPathToSitDown();
                InputDirection = agent.NextDirection;
            }
            else
            {
                agent.enabled = false;
                InputDirection = Direction.None;
                CommandCache.Clear();
            }
            return (waterCell, playerCell);
        }
        public void OnCharacterChangePosition()
        {
            _OnCharacterChangePosition?.Invoke();
            InputDirection = agent.NextDirection;
        }
        private void OnWin()
        {
            StateMachine.OverrideState = StateEnum.Happy;
            StateMachine.ChangeState(StateEnum.Happy);
        }
        #region Camera Setup for Player

        private const int OUT_OF_ISLAND_CELL_BEFORE_TARGET_CAM_TO_PLAYER = 0;
        private const float OFFSET = 3f;
        private const float CAMERA_DOWN_OFFSET = Constants.CELL_SIZE * Constants.DOWN_CAMERA_CELL_OFFSET;
        private const float MOVE_TIME = 0.25f;
        private static readonly Vector3 CameraDownOffsetVector = new(0, 0, CAMERA_DOWN_OFFSET);

        private bool _isOutOfIsland;
        
        public void SetUpCamera(Island island, GameGridCell cell)
        {
            // TRICK: Take offset = 3 to make camera not really see the edge of the island

            float x = cell.WorldPos.x;
            float z = cell.WorldPos.z;
            
            // If the player is out of the island 1 cell, the camera will change target to Player
            const float OUT_OF_ISLAND_OFFSET = Constants.CELL_SIZE * OUT_OF_ISLAND_CELL_BEFORE_TARGET_CAM_TO_PLAYER;
            if (island.minXIslandPos.x - x > OUT_OF_ISLAND_OFFSET || x - island.maxXIslandPos.x > OUT_OF_ISLAND_OFFSET)
                                                                  // || island.minZIslandPos.z - z > OUT_OF_ISLAND_OFFSET || z - island.maxZIslandPos.z > OUT_OF_ISLAND_OFFSET)
            {
                _isOutOfIsland = true;
                CameraManager.Ins.ChangeCameraTargetPosition(cell.WorldPos + CameraDownOffsetVector, MOVE_TIME);
                return;
            }
            
            // NOTE: If the island is small (Size.x < 7), the camera will not change target pos
            if (island.isSmallIsland && !_isOutOfIsland) return;
            _isOutOfIsland = false;
            if (Mathf.Abs(x - island.minXIslandPos.x) < 0.1f) // if minX
            {
                CameraManager.Ins.ChangeCameraTargetPosition(new Vector3(island.minXIslandPos.x + OFFSET, 0, island.centerIslandPos.z + CAMERA_DOWN_OFFSET), MOVE_TIME);
            }
            else if (Mathf.Abs(x - island.maxXIslandPos.x) < 0.1f) // if maxX
            {
                CameraManager.Ins.ChangeCameraTargetPosition(new Vector3(island.maxXIslandPos.x - OFFSET, 0, island.centerIslandPos.z + CAMERA_DOWN_OFFSET), MOVE_TIME);
            }
            else
            {
                // Change the Camera to center if the player is come from bank and the distance is 1 Cell
                if (Mathf.Abs(x - island.centerIslandPos.x) < Constants.CELL_SIZE)
                {
                    CameraManager.Ins.ChangeCameraTargetPosition(island.centerIslandPos + CameraDownOffsetVector, MOVE_TIME);
                }
                else
                {
                    // Change the camera to center island if it is the first step on the island
                    if (island.FirstPlayerStepCell == cell)
                    {
                        CameraManager.Ins.ChangeCameraTargetPosition(island.centerIslandPos + CameraDownOffsetVector,
                            MOVE_TIME);
                    }
                }
            }
        }

        #endregion

        #region Rule

        public ConditionMerge conditionMergeOnMoving;
        private MovingData _movingData;
        private CutTreeData _cutTreeData;

        public MovingData MovingData => _movingData ??= new MovingData(this);
        public CutTreeData CutTreeData => _cutTreeData ??= new CutTreeData(this);

        #endregion

        #region SAVING DATA

        protected override void OnMementoRestoreData()
        {
            LevelManager.Ins.SetCameraToPlayerIsland();
            EventGlobalManager.Ins.OnPlayerChangeIsland?.Dispatch(false);
        }

        public override IMemento RawSave()
        {
            return new PlayerMemento(this, Tf.parent, isRideVehicle, _vehicle, CurrentStateId, isSpawn, Tf.position, skin.rotation, startHeight, endHeight
                , unitTypeY, unitTypeXZ, belowUnits, neighborUnits, upperUnits, mainCell, cellInUnits, islandID, lastPushedDirection);
        }

        public class PlayerMemento : DynamicUnitMemento<Player>
        {
            protected IVehicle vehicle = null;
            protected bool isRideVehicle;
            protected Transform parentTf;
            public PlayerMemento(GridUnitDynamic main, Transform parentTf, bool isRideVehicle, IVehicle vehicle, StateEnum currentState, params object[] data) : base(main, currentState, data)
            {
                this.vehicle = vehicle;
                this.isRideVehicle = isRideVehicle;
                this.parentTf = parentTf;
            }

            public override void Restore()
            {
                base.Restore();
                main._vehicle = vehicle;
                main.isRideVehicle = isRideVehicle;
                if (main.Tf.parent != parentTf)
                    main.Tf.SetParent(parentTf);
            }
        }
        #endregion

        public override void OnCharacterDie()
        {
            stateMachine.OverrideState = StateEnum.Die;
            IsDead = true;
            stateMachine.ChangeState(StateEnum.Die);
        }

        private void OnDestroy()
        {
            GameManager.Ins.UnregisterListenerEvent(DesignPattern.EventID.WinGame, OnWin);
        }
    }
}
