using _Game.DesignPattern.StateMachine;
using _Game.GameGrid.Unit.DynamicUnit.Enemy.EnemyStates;
using _Game.GameGrid.Unit.DynamicUnit.Interface;
using _Game.Managers;
using GameGridEnum;

namespace _Game.GameGrid.Unit.DynamicUnit.Enemy
{
    public class ArcherEnemy : GridUnitCharacter, IEnemy
    {
        private StateMachine<ArcherEnemy> stateMachine;
        public StateMachine<ArcherEnemy> StateMachine => stateMachine;
        public override StateEnum CurrentStateId
        {
            get => StateEnum.Idle;
            set => stateMachine.ChangeState(value);
        }
        
        private void FixedUpdate()
        {
            if (!GameManager.Ins.IsState(GameState.InGame)) return;
            stateMachine?.UpdateState();
        }
        public override void OnInit(GameGridCell mainCellIn, HeightLevel startHeightIn = HeightLevel.One,
            bool isUseInitData = true, Direction skinDirection = Direction.None, bool hasSetPosAndRot = false)
        {
            base.OnInit(mainCellIn, startHeightIn, isUseInitData, skinDirection, hasSetPosAndRot);

            SaveInitData(LevelManager.Ins.CurrentLevel.Index);
            if (!_isAddState)
            {
                _isAddState = true;
                stateMachine = new StateMachine<ArcherEnemy>(this);
                AddState();
            }
            IsDead = false;
            Direction = Direction.None;
            AddToLevelManager();
            stateMachine.ChangeState(StateEnum.Idle);
            GameManager.Ins.RegisterListenerEvent(DesignPattern.EventID.OnChangeGameState, OnGameStateChange);
        }

        public override void OnDespawn()
        {
            base.OnDespawn();
            GameManager.Ins.UnregisterListenerEvent(DesignPattern.EventID.OnChangeGameState, OnGameStateChange);
        }
        public override void OnBePushed(Direction direction, GridUnit pushUnit)
        {
            AudioManager.Ins.PlaySfx(AudioEnum.SfxType.PushEnemy);           

            if (pushUnit is Player.Player player)
            {
                LookDirection(Constants.InvDirection[direction]);
                Direction = direction;
                stateMachine.ChangeState(StateEnum.Idle);
                player.CheckingStunState();
            }
        }
        protected override void AddState()
        {
            stateMachine.AddState(StateEnum.Idle, new IdleArcherEnemyState());
            stateMachine.AddState(StateEnum.Attack, new AttackArcherEnemyState());
            stateMachine.AddState(StateEnum.Die, new DieArcherEnemyState());
        }   
        public override void OnCharacterDie()
        {
            IsDead = true;
            stateMachine.ChangeState(StateEnum.Die);
        }

        public void AddToLevelManager()
        {
            LevelManager.Ins.OnAddEnemy(this);
        }

        public void RemoveFromLevelManager()
        {
            LevelManager.Ins.OnRemoveEnemy(this);
        }

        public bool IsActive => gameObject.activeSelf;
        
        private void OnGameStateChange(object param)
        {
            GameState state = (GameState)param;
            switch (state)
            {
                case GameState.InGame:
                case GameState.MainMenu:
                case GameState.WinGame:
                case GameState.LoseGame:
                    if (stateMachine.CurrentState.Id == StateEnum.Idle)
                        stateMachine.ChangeState(StateEnum.Idle);
                    break;
            }
        }

        // protected override void OnMementoRestoreData()
        // {
        //     AddToLevelManager();
        // }
    }
}