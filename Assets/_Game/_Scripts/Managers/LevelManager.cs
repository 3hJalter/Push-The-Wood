using System;
using System.Collections.Generic;
using System.Linq;
using _Game._Scripts.InGame;
using _Game._Scripts.Managers;
using _Game._Scripts.Tutorial;
using _Game._Scripts.Utilities;
using _Game.Camera;
using _Game.Data;
using _Game.DesignPattern;
using _Game.GameGrid.Unit;
using _Game.GameGrid.Unit.DynamicUnit.Interface;
using _Game.GameGrid.Unit.DynamicUnit.Player;
using _Game.GameGrid.Unit.Interface;
using _Game.GameGrid.Unit.StaticUnit.Chest;
using _Game.Managers;
using _Game.Resource;
using _Game.UIs.Screen;
using _Game.Utilities;
using _Game.Utilities.Timer;
using DG.Tweening;
using MoreMountains.NiceVibrations;
using Sirenix.OdinInspector;
using UnityEngine;
using VinhLB;
using static _Game.Utilities.Grid.Grid<_Game.GameGrid.GameGridCell, _Game.GameGrid.GameGridCellData>;

namespace _Game.GameGrid
{
    public class LevelManager : Singleton<LevelManager>
    {
        // private readonly Dictionary<int, Level> _preLoadLevels = new();

        // public Dictionary<int, Level> PreLoadLevels => _preLoadLevels;

        public event Action OnLevelGenerated;
        public event Action OnLevelRestarted;
        public event Action OnLevelIslandReset;
        public event Action OnCheckWinCondition;
        public event Action _OnNextLevel;
        public event Action OnObjectiveChange;

        [SerializeField]
        [ReadOnly]
        private int normalLevelIndex;
        [SerializeField]
        [ReadOnly]
        private int dailyLevelIndex;
        [SerializeField]
        [ReadOnly]
        private int secretLevelIndex;
        [SerializeField]
        MeshFilter groundCombineMeshFilter;
        [SerializeField]
        MeshFilter[] surfaceCombineMeshFilters;
        [SerializeField]
        MeshFilter[] grassCombineMeshFilters;
        // TEMPORARY
        public int dailyLevelClickedDay;
        
        private Level _currentLevel;
        private bool _isRestarting;
        private bool _isResetting;
        private ThemeEnum _currentTheme = ThemeEnum.None;
        

        public readonly HashSet<GameUnit> SaveChangeUnits = new HashSet<GameUnit>();
        public int KeyRewardCount = 0;
        public int SecretMapPieceCount = 0;
        public int goldCount;
        public Dictionary<BoosterType, int> boosterRewardCount = new();
        public int NormalLevelIndex => normalLevelIndex;
        public int DailyLevelIndex => dailyLevelIndex;
        public int SecretLevelIndex => secretLevelIndex;
        public Level CurrentLevel => _currentLevel;
        public bool IsConstructingLevel;

        public bool IsFirstLevel => normalLevelIndex == 0;
        
        public bool IsHardLevel => CurrentLevel.LevelType == LevelType.Normal && CurrentLevel.LevelNormalType == LevelNormalType.Hard;

        private CareTaker savingState;
        public bool IsCanUndo => savingState.IsCanUndo;

        private readonly Vector3 _cameraDownOffset = new(0, 0, Constants.DOWN_CAMERA_CELL_OFFSET * Constants.CELL_SIZE);

        public Vector3 CameraDownOffset => _cameraDownOffset;

        public bool IsFirstTutorialLevel => (CurrentLevel.LevelType == LevelType.Normal && NormalLevelIndex == 0)
                                       || (CurrentLevel.LevelType == LevelType.DailyChallenge && DailyLevelIndex == 0);
        
        [ReadOnly]
        public Player player;

        #region Objective Win Condition Number

        [ReadOnly] [SerializeField] private int objectiveTotal;
        private readonly HashSet<IEnemy> enemies = new();
        private readonly HashSet<IFinalPoint> finalPoints = new();
        public int ObjectiveTotal => objectiveTotal;
        private int EnemyNums => enemies.Count;
        private int FinalPointNums => finalPoints.Count;
        
        public void OnAddEnemy(IEnemy enemy)
        {
            bool isAdded = enemies.Add(enemy);
            if (isAdded && CurrentLevel.LevelWinCondition is LevelWinCondition.DefeatAllEnemy)
            {
                if (enemies.Count > objectiveTotal) objectiveTotal = enemies.Count;
                OnObjectiveChange?.Invoke();
            }
        }

        public void OnRemoveEnemy(IEnemy enemy)
        {
            bool isRemoved = enemies.Remove(enemy);
            if (isRemoved && CurrentLevel.LevelWinCondition is LevelWinCondition.DefeatAllEnemy)
            {
                OnObjectiveChange?.Invoke();
                EventGlobalManager.Ins.OnChangeLevelCollectingObjectNumber.Dispatch();
            }
        }
        
       public void OnAddFinalPoint(IFinalPoint finalPoint)
        {
            bool isAdded = finalPoints.Add(finalPoint);
            if (isAdded && CurrentLevel.LevelWinCondition is not LevelWinCondition.DefeatAllEnemy)
            {
                if (finalPoints.Count > objectiveTotal) objectiveTotal = finalPoints.Count;
                OnObjectiveChange?.Invoke();
            }
        }
       
       public void OnRemoveFinalPoint(IFinalPoint finalPoint, bool checkWin = true)
        {
            bool isRemoved = finalPoints.Remove(finalPoint);
            if (isRemoved && CurrentLevel.LevelWinCondition is not LevelWinCondition.DefeatAllEnemy)
            {
                OnObjectiveChange?.Invoke();
                if (checkWin) EventGlobalManager.Ins.OnChangeLevelCollectingObjectNumber.Dispatch();
                else objectiveTotal--; // No check win  -> The FinalPoint is DeSpawn by undo or reset the level -> decrease the objectiveTotal to add them again later
            }
        }
       
        public int ObjectiveCounterLeft()
        {
            LevelWinCondition condition = _currentLevel.LevelWinCondition;
            if (condition is LevelWinCondition.DefeatAllEnemy)
            {
                return objectiveTotal - EnemyNums;
            }
            return objectiveTotal - FinalPointNums;
        }
        
        #endregion

        public HashSet<BChest> CollectedChests { get; } = new();

        // DEBUG:
        [ReadOnly]
        [SerializeField] private LevelWinCondition levelWinCondition;

        [SerializeField] private bool isSavePlayerPushStep;
        public bool IsSavePlayerPushStep => isSavePlayerPushStep;
        
        private void Start()
        {
            if (DebugManager.Ins && DebugManager.Ins.Level >= 0)
            {
                DataManager.Ins.GameData.user.normalLevelIndex = DebugManager.Ins.Level;
            }
            normalLevelIndex = DataManager.Ins.GameData.user.normalLevelIndex;
            OnGenerateLevel(LevelType.Normal, normalLevelIndex, normalLevelIndex == 0);
            #region Handle If user passes first level

            if (DataManager.Ins.GameData.user.normalLevelIndex > 0) UIManager.Ins.OpenUI<MainMenuScreen>(true);

            #endregion
        }

        public void OnGenerateLevel(LevelType type, int index, bool needInit)
        {
            if (type == LevelType.Normal) OnCheckTutorial();
            GameplayManager.Ins.ClearBoosterPurchase();
            IsConstructingLevel = true;
            _currentLevel = new Level(type, index);
            if (_currentLevel.Theme != _currentTheme)
            {
                _currentTheme = _currentLevel.Theme;
                DataManager.Ins.OnChangeTheme(_currentTheme);
            }
            groundCombineMeshFilter.mesh = _currentLevel.CombineMesh;
            for(int i = 0; i < surfaceCombineMeshFilters.Length; i++)
            {
                surfaceCombineMeshFilters[i].mesh = _currentLevel.SurfaceCombineMesh[i];
            }
            for (int i = 0; i < grassCombineMeshFilters.Length; i++)
            {
                grassCombineMeshFilters[i].mesh = _currentLevel.GrassCombineMesh[i];
                grassCombineMeshFilters[i].gameObject.SetActive(_currentTheme is not ThemeEnum.Winter);
            }
            objectiveTotal = 0;
            if (needInit && !_currentLevel.IsInit)
            {
                InitLevel();
            }
            OnLevelGenerated?.Invoke();
        }

        public void InitLevel()
        {   
            GameManager.Ins.PostEvent(EventID.StartGame);          
            player.SetActiveAgent(false);
            ConstructingLevel();
            SetCameraToPlayerIsland(0f);
            OnObjectiveChange?.Invoke();
            //NOTE: Test
            DebugManager.Ins?.DebugGridData(_currentLevel.GridMap);
            // TEMPORARY: CUTSCENE, player will be show when cutscene end
            if (normalLevelIndex == 0) HidePlayer(true);
            KeyRewardCount = 0;
            goldCount = 0;
            boosterRewardCount.Clear();
            SecretMapPieceCount = 0;
        }

        private int _tutIndex;
        public int TutIndex => _tutIndex;
        
        private void OnAddEventToPlayer()
        {
            player.SetActWithUnitEvent(null);
            player.SetMoveToCellEvent(null);
            _tutIndex = CurrentLevel.Index;
            // Special case for tutorial in daily challenger
            if (CurrentLevel.LevelType == LevelType.DailyChallenge && !DataManager.Ins.IsOpenInGameDailyChallengeTut())
            {
                _tutIndex = -1;
                DataManager.Ins.ChangeOpenDailyChallengeTut(true);
            } else if (CurrentLevel.LevelType is not LevelType.Normal) return;
            if (TutorialManager.Ins.TutorialList.ContainsKey(_tutIndex))
            {
                player.SetActWithUnitEvent(TutorialManager.Ins.OnUnitActWithOther);
                player.SetMoveToCellEvent(TutorialManager.Ins.OnUnitMoveToCell);
            }
        }
        
        public void ConstructingLevel()
        {
            if (_currentLevel.IsInit) return;
            levelWinCondition = CurrentLevel.LevelWinCondition;
            OnAddWinCondition(CurrentLevel.LevelWinCondition);
            OnAddEventToPlayer();
            enemies.Clear();
            finalPoints.Clear();
            CollectedChests.Clear();
            _currentLevel.OnInitLevelSurfaceAndUnit();
            _currentLevel.OnInitPlayerToLevel();
            _currentLevel.GridMap.CompleteObjectInit();
            IsConstructingLevel = false;
            savingState = new CareTaker(this);
        }

        public void ResetLevelIsland()
        {
            _isResetting = true;
            _currentLevel.ResetIslandPlayerOn();
            SetCameraToPlayerIsland();
            OnLevelIslandReset?.Invoke();
            HandleObjectiveChangeOnReset();
            _isResetting = false;
        }

        private void HandleObjectiveChangeOnReset()
        {
            if (CurrentLevel.LevelWinCondition is LevelWinCondition.DefeatAllEnemy)
            {
                // check the enemy set, if it in-active -> remove it
                enemies.RemoveWhere(enemy => !enemy.IsActive);
                objectiveTotal = enemies.Count;
            }
            else
            {
                // check the final point set, if it in-active -> remove it
                finalPoints.RemoveWhere(finalPoint => !finalPoint.IsActive);
                objectiveTotal = finalPoints.Count;
            }
            OnObjectiveChange?.Invoke();
        }

        private void OnCheckTutorial()
        {
            if (TutorialManager.Ins.TutorialList.TryGetValue(normalLevelIndex, out ITutorialCondition tutorialData))
            {
                tutorialData.ResetTutorial();
            }
        }

        public void HidePlayer(bool isHide)
        {
            player.gameObject.SetActive(!isHide);
        }

        public void SetCameraToPlayerIsland(float moveTime = 1f)
        {
            if (player.islandID == -1) return;

            SetCameraToIsland(player.islandID, moveTime);
        }

        
        private void SetCameraToIsland(int index, float moveTime = 1f)
        {
            Vector3 position = CurrentLevel.GetIsland(index).centerIslandPos + _cameraDownOffset;
            CameraManager.Ins.ChangeCameraTargetPosition(position, moveTime);
        }

        private void SetCameraToPosition(Vector3 position)
        {
            CameraManager.Ins.ChangeCameraTargetPosition(position);
        }

        public void OnWin()
        {
            goldCount += GetEndGameGold();
            switch (_currentLevel.LevelType)
            {
                case LevelType.Normal:
                    // TEMPORARY: Add reward if the normal level is defeat enemy
                    if (CurrentLevel.LevelWinCondition == LevelWinCondition.DefeatAllEnemy)
                    {
                        CollectingResourceManager.Ins.SpawnCollectingRewardKey(1, player.Tf);
                        KeyRewardCount += 1;
                        if (CurrentLevel.Index >= DataManager.Ins.ConfigData.unlockSecretLevelAtLevelIndex)
                        {
                            int getCompass = CurrentLevel.Index - DataManager.Ins.ConfigData.unlockSecretLevelAtLevelIndex;
                            if (getCompass > 0 && getCompass % 4 == 0)
                            {
                                CollectingResourceManager.Ins.SpawnCollectingCompass(1, player.Tf);
                                SecretMapPieceCount += 1;
                            }
                        }
                    }
                    // +1 LevelIndex and save
                    normalLevelIndex++;
                    // Temporary handle when out of level
                    DataManager.Ins.GameData.user.normalLevelIndex = normalLevelIndex;
                    break;
                case LevelType.Secret:
                    if (DataManager.Ins.GameData.user.secretLevelIndexComplete.Contains(secretLevelIndex)) break;
                    DataManager.Ins.GameData.user.secretLevelIndexComplete.Add(secretLevelIndex);
                    break;
                case LevelType.DailyChallenge:
                    if (DataManager.Ins.GameData.user.dailyLevelIndexComplete.Contains(dailyLevelIndex)) break;
                    DataManager.Ins.GameData.user.dailyLevelIndexComplete.Add(dailyLevelIndex);
                    break;
                case LevelType.None:
                default:
                    break;
            }
            if(CurrentLevel.LevelType == LevelType.Normal)
                GameManager.Ins.GainLevelProgress(1);
            DataManager.Ins.Save();
            GameManager.Ins.PostEvent(EventID.WinGame);
            HVibrate.Haptic(HapticTypes.Success);
            // Future: Add reward collected in-game
        }

        private int GetEndGameGold()
        {
            switch (_currentLevel.LevelType)
            {
                case LevelType.Normal:
                    switch (_currentLevel.LevelNormalType)
                    {
                        case LevelNormalType.Easy:
                            return DataManager.Ins.ConfigData.easyLevelGoldReward;
                        case LevelNormalType.Medium:
                            return DataManager.Ins.ConfigData.mediumLevelGoldReward; 
                        case LevelNormalType.Hard:
                            return DataManager.Ins.ConfigData.hardLevelGoldReward;
                        case LevelNormalType.None:
                        default:
                            return DataManager.Ins.ConfigData.easyLevelGoldReward;
                    }
                    break;
                case LevelType.Secret:
                    return DataManager.Ins.ConfigData.secretLevelGoldReward;
                    case LevelType.DailyChallenge:
                    return DataManager.Ins.ConfigData.dailyChallengeGoldReward;
                case LevelType.None:
                default:
                    return 0;
            }
        }
        
        public void OnGoLevel(LevelType type, int index, bool needInit = true)
        {
            // Dev: Currently make it work only for daily challenge
            switch (type)
            {
                case LevelType.Normal: normalLevelIndex = index; break;
                case LevelType.Secret: secretLevelIndex = index; break;
                case LevelType.DailyChallenge: dailyLevelIndex = index; break;
                case LevelType.None:
                default: break;
            }
            TutorialManager.Ins.DiscardQueue();
            IsConstructingLevel = true;
            OnRemoveWinCondition();
            _currentLevel.OnDeSpawnLevel();
            OnGenerateLevel(type, index, needInit);
        }

        public void OnNextLevel(LevelType type, bool needInit = true)
        {
            // Load next level
            IsConstructingLevel = true;
            OnRemoveWinCondition();
            _currentLevel.OnDeSpawnLevel();
            OnGenerateLevel(type, normalLevelIndex, false);
            _OnNextLevel?.Invoke();
            if (!_currentLevel.IsInit)
            {
                InitLevel();
            }
            if (IsHardLevel)
            {
                UIManager.Ins.OpenUI<HardWarningScreen>();
                return;
            }
            // Hide the screen
            // SetCameraToPosition(CurrentLevel.GetCenterPos());
            UIManager.Ins.HideUI<InGameScreen>();
            SetCameraToPlayerIsland(0f);
            // Zoom out
            CameraManager.Ins.ChangeCamera(ECameraType.ZoomOutCamera, 0f);
            TimerManager.Ins.WaitForTime(0.1f, () =>
            {
                CameraManager.Ins.ChangeCamera(ECameraType.InGameCamera, 1f);
                TimerManager.Ins.WaitForTime(0.5f, () =>
                {
                    UIManager.Ins.ShowUI<InGameScreen>();
                });
            });
        }

        public void OnRestart()
        {
            if (!CurrentLevel.IsInit) return; // No Init -> Means No Restart
            _isRestarting = true;
            OnCheckTutorial();
            objectiveTotal = 0;
            enemies.Clear();
            finalPoints.Clear();
            CollectedChests.Clear();
            CurrentLevel.GridMap.Reset();
            IsConstructingLevel = true;
            player.OnDespawn();
            CurrentLevel.ResetAllIsland();
            CurrentLevel.ResetNonIslandUnit();
            player = SimplePool.Spawn<Player>(DataManager.Ins.GetGridUnit(PoolType.Player));
            player.OnInit(CurrentLevel.FirstPlayerInitCell);
            SetCameraToPlayerIsland();
            // FxManager.Ins.ResetTrackedTrampleObjectList();
            IsConstructingLevel = false;
            savingState.Reset();
            OnLevelRestarted?.Invoke();
            OnObjectiveChange?.Invoke();
            _isRestarting = false;
        }

        private void OnAddWinCondition(LevelWinCondition condition)
        {
            switch (condition)
            {
                case LevelWinCondition.DefeatAllEnemy:
                    OnCheckWinCondition += () =>
                    {
                        if (EnemyNums == 0 && GameManager.Ins.IsState(GameState.InGame) && !_isRestarting && !_isResetting)
                        {
                            OnWin();
                        }
                    };
                    break;
                case LevelWinCondition.CollectAllChest: 
                case LevelWinCondition.FindingFruit:
                case LevelWinCondition.FindingChest:
                case LevelWinCondition.FindingChickenBbq:
                    OnCheckWinCondition += () =>
                    {
                        if (FinalPointNums == 0 && GameManager.Ins.IsState(GameState.InGame) && !_isRestarting && !_isResetting)
                        {
                            OnWin();
                        }
                    };
                    break;
            }

            if (OnCheckWinCondition is not null)
            {
                EventGlobalManager.Ins.OnEnemyDie.AddListener(OnCheckWinCondition);
                EventGlobalManager.Ins.OnChangeLevelCollectingObjectNumber.AddListener(OnCheckWinCondition);
            }
            
        }

        private void OnRemoveWinCondition()
        {
            // Remove all win condition
            if (OnCheckWinCondition is not null)
            {
                EventGlobalManager.Ins.OnEnemyDie.RemoveListener(OnCheckWinCondition);
                EventGlobalManager.Ins.OnChangeLevelCollectingObjectNumber.RemoveListener(OnCheckWinCondition);
                OnCheckWinCondition = null;
            }
        }

        public void ResetGameState()
        {
            savingState.Reset();
        }

        public bool OnUndo()
        {
            bool success = savingState.Undo();
            player.SetUpCamera(CurrentLevel.Islands[player.islandID], player.MainCell);
            GameplayManager.Ins.IsCanUndo = IsCanUndo;
            return success;
        }
        
        public void SaveGameState(bool isMerge)
        {
            savingState.Save(isMerge);
            if (!isMerge)
                SaveChangeUnits.Clear();
            GameplayManager.Ins.IsCanUndo = IsCanUndo;
            
        }

        public void DiscardSaveState()
        {
            savingState.PopSave();
            GameplayManager.Ins.IsCanUndo = IsCanUndo;
            SaveChangeUnits.Clear();
        }
        public class CareTaker
        {
            LevelManager main;
            Stack<IMemento> dataHistorys = new Stack<IMemento>();
            Stack<List<IMemento>> objectHistorys = new Stack<List<IMemento>>();
            public bool IsCanUndo => dataHistorys.Count > 1;
            public CareTaker(LevelManager main)
            {
                this.main = main;

                #region Init Spawn State
                SavingObjects();
                HashSet<GridUnit> gridUnits = main._currentLevel.Islands[main.player.islandID].GridUnits;
                foreach (GridUnit gridUnit in gridUnits)
                {
                    gridUnit.MainCell.ValueChange();
                }
                SavingState();
                #endregion
            }

            public bool Undo()
            {
                if (main.CurrentLevel.GridMap.IsChange)
                {
                    // TEMPORARY
                    SavingState(true);
                    if (objectHistorys.Count < dataHistorys.Count)
                        objectHistorys.Push(objectHistorys.Peek());
                }
                if (dataHistorys.Count > 1)
                {
                    dataHistorys.Pop().Restore();
                    foreach (IMemento objectRevert in objectHistorys.Pop())
                    {
                        objectRevert.Restore();
                    }
                    if (objectHistorys.Count == 0)
                    {
                        SavingObjects();
                    }
                    return true;
                }
                return false;
            }

            public void Save(bool isMerge = false)
            {
                SavingState(isMerge);
                SavingObjects(isMerge);
            }

            public void PopSave()
            {
                dataHistorys.Pop();
                objectHistorys.Pop();
            }

            private void SavingState(bool isMerge = false)
            {
                if (!isMerge || dataHistorys.Count == 0)
                {
                    if (main.CurrentLevel.GridMap.IsChange)
                    {
                        IMemento states = main._currentLevel.GridMap.Save();
                        dataHistorys.Push(states);
                    }
                }
                else
                {
                    GridMemento stateData = (GridMemento)dataHistorys.Peek();
                    stateData.Merge((GridMemento)main._currentLevel.GridMap.Save());
                }
            }

            private void SavingObjects(bool isMerge = false)
            {
                List<IMemento> states;
                if (!isMerge || objectHistorys.Count == 0)
                {
                    states = new List<IMemento>() { main.player.Save() };
                    // TODO: (Later) Try to optimize this by not set the unit which are out of island (set unit island id to -1 before or check active...)
                    HashSet<GridUnit> gridUnits = main._currentLevel.Islands[main.player.islandID].GridUnits;
                    foreach (GridUnit gridUnit in gridUnits)
                    {
                        IMemento save = gridUnit.Save();
                        if (save != null && gridUnit.MainCell != null)
                            states.Add(save);
                    }
                    objectHistorys.Push(states);
                }
                else
                {
                    states = objectHistorys.Peek();
                    HashSet<GridUnit> gridUnits = main._currentLevel.Islands[main.player.islandID].GridUnits;
                    foreach (GridUnit gridUnit in gridUnits)
                    {
                        IMemento save = gridUnit.Save();
                        if (save != null && !states.Any(x => x.Id == gridUnit.GetHashCode()))
                            states.Add(save);
                    }
                }
            }

            public void Reset()
            {
                dataHistorys.Clear();
                objectHistorys.Clear();

                #region Init Spawn State
                SavingObjects();
                HashSet<GridUnit> gridUnits = main._currentLevel.Islands[main.player.islandID].GridUnits;
                foreach (GridUnit gridUnit in gridUnits)
                {
                    gridUnit.MainCell.ValueChange();
                }
                SavingState();
                #endregion
            }
        }
    }
}