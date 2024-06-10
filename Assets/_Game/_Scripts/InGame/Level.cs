using System;
using System.Collections.Generic;
using _Game.Data;
using _Game.DesignPattern;
using _Game.GameGrid;
using _Game.GameGrid.GridSurface;
using _Game.GameGrid.Unit;
using _Game.GameGrid.Unit.DynamicUnit.Interface;
using _Game.GameGrid.Unit.DynamicUnit.Player;
using _Game.Managers;
using _Game.Utilities.Grid;
using DG.Tweening;
using GameGridEnum;
using MapEnum;
using UnityEngine;
using Object = UnityEngine.Object;

namespace _Game._Scripts.InGame
{
    public class Level
    {
        #region constructor
        public Level(LevelType type, int index, Transform parent = null, bool enableRootModel = false)
        {
            IsInit = false;
            // Set GridMap
            Index = index;
            // Get data
            // _textGridData = GameGridDataHandler.CreateGridData(index);
            _rawLevelData = JsonGridDataHandler.CreateLevelData(type, index);
            GridSizeX = _rawLevelData.s.x;
            gridSizeY = _rawLevelData.s.y;
            // Create Grid Map
            CreateGridMap();
            // Spawn Units (Not Init)
            OnSpawnUnits();
            // Spawn Grid Surface
            SpawnGridSurfaceToGrid(enableRootModel);
            // OnSpawnShadowUnit();
            // OnSetHintLine();
            // if isInit -> AddIsland & InitUnit
            if (parent is null) return;
            // Set parent
            SetParent(parent);
        }
        #endregion

        public IEnumerable<PlayerStep> GetPushHint()
        {
            if (_rawLevelData.pS is null)
            {
                return null;
            }
            // Convert the RawLevelData.pS to PlayerStep
            PlayerStep[] playerSteps = new PlayerStep[_rawLevelData.pS.Length];
            for (int i = 0; i < _rawLevelData.pS.Length; i++)
            {
                playerSteps[i] = new PlayerStep
                {
                    x = _rawLevelData.pS[i].x,
                    y = _rawLevelData.pS[i].y,
                    d = _rawLevelData.pS[i].d,
                    i = _rawLevelData.pS[i].i
                };
            }
            return playerSteps;
        }

        public void ResetNonIslandUnit()
        {
            for (int i = 0; i < nonIslandUnitLis.Count; i++)
            {
                nonIslandUnitLis[i].unit.OnDespawn();
                nonIslandUnitLis[i].unit.ResetData();
                // Spawn
                GridUnit unit =
                    (GridUnit)SimplePool.SpawnDirectFromPool(nonIslandUnitLis[i].unit, Vector3.zero,
                        Quaternion.identity);
                unit.OnInit(nonIslandUnitLis[i].mainCellIn, nonIslandUnitLis[i].startHeightIn, true,
                    nonIslandUnitLis[i].directionIn);
            }
        }

        public struct LevelUnitData
        {
            public GameGridCell mainCellIn;
            public HeightLevel startHeightIn;
            public Direction directionIn;
            public PoolType unitType;
            public GridUnit unit;
        }

        #region data
        // Init Data
        // private readonly TextGridData _textGridData;
        private readonly RawLevelData _rawLevelData;

        // Map

        private readonly int gridSizeY;

        // Surface & Unit

        // Island (Each island has some surfaces and units)
        public Dictionary<int, Island> Islands { get; } = new();

        // For handling when reset Island
        public HashSet<int> ResetIslandSet { get; } = new();

        // Some other data
        public GameGridCell FirstPlayerInitCell { get; private set; }

        private Direction firstPlayerDirection;

        // Get Data
        private int GridSizeX { get; }

        public int Index { get; }
        public Mesh CombineMesh { get; private set; }
        public readonly Mesh[] SurfaceCombineMesh = new Mesh[] {null, null, null };
        public readonly Mesh[] GrassCombineMesh = new Mesh[] {null, null, null };
        
        
        public bool IsInit { get; private set; }

        private GridSurface[,] GridSurfaceMap { get; set; }
        private bool[,] HasUnitInMap { get; set; }

        public List<LevelUnitData> UnitDataList { get; } = new(); // Not include ICharacter
        public List<LevelUnitData> CharacterDataList { get; } = new(); // Include ICharacter, but not include Player

        private readonly List<LevelUnitData> nonIslandUnitLis = new();

        public List<GridUnit> ShadowUnitList { get; } = new();

        public Grid<GameGridCell, GameGridCellData> GridMap { get; private set; }

        public LevelType LevelType => (LevelType)_rawLevelData.lt;

        public ThemeEnum Theme => (ThemeEnum)_rawLevelData.t;
        public LevelWinCondition LevelWinCondition => (LevelWinCondition)_rawLevelData.wc;
        public LevelNormalType LevelNormalType => (LevelNormalType)_rawLevelData.lnt;
        #endregion

        #region public function
        public void OnInitLevelSurfaceAndUnit()
        {
            AddIslandIdToSurface();
            for (int i = 0; i < UnitDataList.Count; i++) OnInitUnit(UnitDataList[i]);
            for (int i = 0; i < CharacterDataList.Count; i++) OnInitUnit(CharacterDataList[i]);
            IsInit = true;
        }

        public Island GetIsland(int islandID)
        {
            return Islands[islandID];
        }

        public Vector3 GetCenterPos()
        {
            float centerX = (GridMap.GetGridCell(0, 0).WorldX + GridMap.GetGridCell(GridSizeX - 1, 0).WorldX) / 2;
            float centerZ = (GridMap.GetGridCell(0, 0).WorldY + GridMap.GetGridCell(0, gridSizeY - 1).WorldY) / 2;
            return new Vector3(centerX, 0, centerZ);
        }

        public Vector3 GetBottomLeftPos()
        {
            GameGridCell cell = GridMap.GetGridCell(0, 0);

            return new Vector3(cell.WorldX, 0f, cell.WorldY);
        }

        public Vector3 GetTopRightPos()
        {
            GameGridCell cell = GridMap.GetGridCell(GridSizeX - 1, gridSizeY - 1);

            return new Vector3(cell.WorldX, 0f, cell.WorldY);
        }

        public float GetMaxZPos()
        {
            return GridMap.GetGridCell(0, gridSizeY - 1).WorldY;
        }

        public float GetMinZPos()
        {
            return GridMap.GetGridCell(0, 0).WorldY;
        }

        public void AddNewUnitToIsland(GridUnit unit)
        {
            if (!Islands.ContainsKey(unit.islandID)) return;
            Islands[unit.islandID].AddNewUnitToIsland(unit);
        }

        public GameGridCell GetCellWorldPos(Vector3 position)
        {
            return GridMap.GetGridCell(position);
        }

        public GameGridCell GetCell(Vector2Int position)
        {
            return GridMap.GetGridCell(position.x, position.y);
        }

        public void SetFirstPlayerStepOnIsland(GameGridCell cell)
        {
            Islands[LevelManager.Ins.player.islandID].SetFirstPlayerStepCell(cell);
            CameraManager.Ins.ChangeCameraTargetPosition(Islands[LevelManager.Ins.player.islandID].centerIslandPos + LevelManager.Ins.CameraDownOffset);
        }

        public void ResetIslandPlayerOn()
        {
            if (!Islands.ContainsKey(LevelManager.Ins.player.islandID)) return;
            GridMap.Reset();
            LevelManager.Ins.IsConstructingLevel = true;
            Islands[LevelManager.Ins.player.islandID].ResetIsland();
            LevelManager.Ins.player.OnDespawn();
            LevelManager.Ins.player = SimplePool.Spawn<Player>(DataManager.Ins.GetGridUnit(PoolType.Player));
            LevelManager.Ins.player.OnInit(Islands[LevelManager.Ins.player.islandID].FirstPlayerStepCell);
            GridMap.CompleteObjectInit();
            LevelManager.Ins.IsConstructingLevel = false;
            LevelManager.Ins.ResetGameState();
        }

        public void ResetAllIsland()
        {
            for (int i = 0; i < Islands.Count; i++) Islands[i].ResetIsland();
        }

        public void OnDeSpawnLevel()
        {
            // Despawn all groundUnit
            for (int index0 = 0; index0 < GridSurfaceMap.GetLength(0); index0++)
                for (int index1 = 0; index1 < GridSurfaceMap.GetLength(1); index1++)
                {
                    GridSurface gridSurface = GridSurfaceMap[index0, index1];
                    if (gridSurface is not null) gridSurface.OnDespawn();
                }

            for (int i = 0; i < ShadowUnitList.Count; i++) Object.Destroy(ShadowUnitList[i].gameObject);

            if (IsInit)
            {
                // Despawn all unit in each island
                for (int i = 0; i < Islands.Count; i++)
                {
                    Island island = Islands[i];
                    island.ClearIsland();
                }

                for (int i = 0; i < nonIslandUnitLis.Count; i++) nonIslandUnitLis[i].unit.OnDespawn();
                nonIslandUnitLis.Clear();
                // Set player to null
                LevelManager.Ins.player.OnDespawn();
                LevelManager.Ins.player = null;
            }
            else
            {
                for (int i = 0; i < UnitDataList.Count; i++)
                {
                    LevelUnitData data = UnitDataList[i];
                    data.unit.OnDespawn();
                }
                for (int i = 0; i < CharacterDataList.Count; i++)
                {
                    LevelUnitData data = CharacterDataList[i];
                    data.unit.OnDespawn();
                }
            }

            FirstPlayerInitCell = null;
            // Clear all _islandDic data
            Islands.Clear();
            // Clear all _gridSurfaceMap data
            GridSurfaceMap = null;
            // Clear all _gridMap data
            GridMap = null;
            // Clear all _unitDataList data
            UnitDataList.Clear();
            // Clear all _characterDataList data
            CharacterDataList.Clear();
            // Clear all _shadowUnitList data
            ShadowUnitList.Clear();
            IsInit = false;
        }

        private Tween _tweenShadowUnitList;

        public void ChangeShadowUnitAlpha(bool isHide)
        {
            if (ShadowUnitList.Count == 0) return;
            // Kill the previous tween if they are running
            _tweenShadowUnitList?.Kill();
            float currentAlphaTransparency = ShadowUnitList[0].GetAlphaTransparency();

            if (isHide)
            {
                _tweenShadowUnitList = DOVirtual.Float(currentAlphaTransparency, 0, currentAlphaTransparency,
                        value => ShadowUnitList[0].SetAlphaTransparency(value))
                    .OnComplete(() =>
                    {
                        // Set active to false for all
                        for (int i = 0; i < ShadowUnitList.Count; i++) ShadowUnitList[i].gameObject.SetActive(false);
                    });
            }
            else
            {
                // Set active to true for all
                for (int i = 0; i < ShadowUnitList.Count; i++) ShadowUnitList[i].gameObject.SetActive(true);
                _tweenShadowUnitList = DOVirtual.Float(currentAlphaTransparency, 0.5f, 0.5f - currentAlphaTransparency,
                    value => ShadowUnitList[0].SetAlphaTransparency(value));
            }
        }

        public void ChangeShadowUnitAlpha(bool isHide, int index)
        {
            if (ShadowUnitList.Count <= index) return;
            // Kill the previous tween if they are running
            _tweenShadowUnitList?.Kill();
            float currentAlphaTransparency = ShadowUnitList[index].GetAlphaTransparency();
            if (isHide)
            {
                Debug.Log("Hide");
                _tweenShadowUnitList = DOVirtual.Float(currentAlphaTransparency, 0, currentAlphaTransparency,
                        value => ShadowUnitList[index].SetAlphaTransparency(value))
                    .OnComplete(() => ShadowUnitList[index].gameObject.SetActive(false));
            }
            else
            {
                ShadowUnitList[index].gameObject.SetActive(true);
                Debug.Log("Show shadow unit is active: " + ShadowUnitList[index].gameObject.activeSelf);
                _tweenShadowUnitList = DOVirtual.Float(currentAlphaTransparency, 0.5f, 0.5f - currentAlphaTransparency,
                    value => { ShadowUnitList[index].SetAlphaTransparency(value); }).OnComplete(() =>
                    Debug.Log("Show shadow unit is active 2: " + ShadowUnitList[index].gameObject.activeSelf));
                Debug.Log("Show shadow unit is active 3: " + ShadowUnitList[index].gameObject.activeSelf);
            }
        }

        public bool HasUnitOnCell(int x, int y)
        {
            return HasUnitInMap[x, y];
        }
        #endregion

        #region private function
        private void SetParent(Transform parent)
        {
            // set all gridSurface tp parent
            for (int index0 = 0; index0 < GridSurfaceMap.GetLength(0); index0++)
                for (int index1 = 0; index1 < GridSurfaceMap.GetLength(1); index1++)
                {
                    GridSurface gridSurface = GridSurfaceMap[index0, index1];
                    if (gridSurface is null) continue;
                    gridSurface.Tf.SetParent(parent);
                }

            // set all gridUnit to parent
            for (int i = 0; i < UnitDataList.Count; i++)
            {
                LevelUnitData data = UnitDataList[i];
                data.unit.Tf.SetParent(parent);
            }

            // set all character to parent
            for (int i = 0; i < CharacterDataList.Count; i++)
            {
                LevelUnitData data = CharacterDataList[i];
                data.unit.Tf.SetParent(parent);
            }
        }

        private void CreateGridMap()
        {
            GridMap = new Grid<GameGridCell, GameGridCellData>(GridSizeX, gridSizeY, Constants.CELL_SIZE,
                default, () => new GameGridCell(), GridPlane.XZ);
            GridSurfaceMap = new GridSurface[GridSizeX, gridSizeY];
            HasUnitInMap = new bool[GridSizeX, gridSizeY];
        }

        private void SpawnGridSurfaceToGrid(bool enableRootModel = false)
        {
            List<MeshFilter> groundFilters = null;
            List<MeshFilter>[] surfaceFilters = null;
            List<MeshFilter>[] grassFilters = null;
            if (CombineMesh is null)
            {
                groundFilters = new List<MeshFilter>();
                surfaceFilters = new List<MeshFilter>[] { new(), new(), new() };            
                grassFilters = new List<MeshFilter>[] { new(), new(), new() };
            }
            // Loop through all sfD (surface data) in _rawLevelData
            for (int i = 0; i < _rawLevelData.sfD.Length; i++)
            {
                RawLevelData.GridSurfaceData surfaceData = _rawLevelData.sfD[i];
                // Get surface from data manager
                GridSurface gridSurface = DataManager.Ins.GetGridSurface((PoolType)surfaceData.t);
                // If surface is null -> continue
                if (gridSurface is null) continue;
                // Spawn surface
                GameGridCell gridCell = GridMap.GetGridCell(surfaceData.p.x, surfaceData.p.y);

                GridSurface surfaceClone = SimplePool.Spawn<GridSurface>(gridSurface,
                    new Vector3(gridCell.WorldX, 0, gridCell.WorldY), Quaternion.identity);
                // Set surface to grid cell               
                gridCell.SetSurface(surfaceClone);
                // Set surface to GridSurfaceMap
                GridSurfaceMap[surfaceData.p.x, surfaceData.p.y] = surfaceClone;
                // Init surface
                surfaceClone.OnInit(Index, gridCell.GetCellPosition(), new Vector2Int(GridSizeX, gridSizeY),
                    (Direction)surfaceData.d, (MaterialEnum)surfaceData.m, (ThemeEnum)_rawLevelData.t,
                    HasUnitInMap[gridCell.X, gridCell.Y]);

                if (CombineMesh is null && surfaceClone is GroundSurface clone)
                {
                    groundFilters?.AddRange(clone.CombineMeshs(enableRootModel));
                    surfaceFilters?[(int)clone.groundMaterialEnum]?.Add(clone.GroundMeshFilter);
                    grassFilters?[(int)clone.groundMaterialEnum]?.Add(clone.GrassMeshFilter);
                    clone.ActiveMesh(enableRootModel, (ThemeEnum) _rawLevelData.t); 
                }
            }
            if (CombineMesh is null)
            {
                CombineMesh = Optimize.CombineMeshes(groundFilters);
                for(int i = 0; i < SurfaceCombineMesh.Length; i++)
                {
                    SurfaceCombineMesh[i] = Optimize.CombineMeshes(surfaceFilters?[i]);
                }
                for (int i = 0; i < GrassCombineMesh.Length; i++)
                {
                    GrassCombineMesh[i] = Optimize.CombineMeshes(grassFilters?[i]);
                }
            }
        }

        private void AddIslandIdToSurface()
        {
            int currentIslandID = 0;
            for (int y = 0; y < GridSurfaceMap.GetLength(1); y++)
                for (int x = 0; x < GridSurfaceMap.GetLength(0); x++)
                    if (IsGridSurfaceHadIsland(x, y, out GridSurface gridSurface))
                    {
                        FloodFillIslandID(gridSurface, x, y, currentIslandID);
                        Islands[currentIslandID].SetIslandPos();
                        currentIslandID++;
                    }

            return;

            void FloodFillIslandID(GridSurface gridSurface, int x, int y, int islandID)
            {
                gridSurface.IslandID = islandID;
                Islands.TryAdd(islandID, new Island(islandID, this));
                Islands[islandID].AddGridCell(GridMap.GetGridCell(x, y));
                if (IsGridSurfaceHadIsland(x - 1, y, out GridSurface leftGridSurface))
                    FloodFillIslandID(leftGridSurface, x - 1, y, islandID);
                if (IsGridSurfaceHadIsland(x + 1, y, out GridSurface rightGridSurface))
                    FloodFillIslandID(rightGridSurface, x + 1, y, islandID);
                if (IsGridSurfaceHadIsland(x, y - 1, out GridSurface downGridSurface))
                    FloodFillIslandID(downGridSurface, x, y - 1, islandID);
                if (IsGridSurfaceHadIsland(x, y + 1, out GridSurface upGridSurface))
                    FloodFillIslandID(upGridSurface, x, y + 1, islandID);
            }

            bool IsGridSurfaceHadIsland(int x, int y, out GridSurface gridSurface)
            {
                gridSurface = null;
                int rows = GridSurfaceMap.GetLength(0);
                int cols = GridSurfaceMap.GetLength(1);
                if (x < 0 || x >= rows || y < 0 || y >= cols) return false;
                gridSurface = GridSurfaceMap[x, y];
                if (gridSurface is null) return false;
                if (gridSurface.SurfaceType == GridSurfaceType.Water) return false;
                return gridSurface.IslandID < 0;
            }
        }

        private void OnSpawnUnits()
        {
            // Loop through all uD (unit data) in _rawLevelData
            for (int i = 0; i < _rawLevelData.uD.Length; i++)
            {
                RawLevelData.GridUnitData unitData = _rawLevelData.uD[i];
                // Get unit from data manager
                GridUnit gridUnit = DataManager.Ins.GetGridUnit((PoolType)unitData.t);
                // If unit is null -> continue
                if (gridUnit is null) continue;
                // Check if unit is player
                if ((PoolType)unitData.t is PoolType.Player)
                {
                    if (LevelManager.Ins.player != null) LevelManager.Ins.player.OnDespawn();
                    FirstPlayerInitCell = GridMap.GetGridCell(unitData.c.x, unitData.c.y);
                    firstPlayerDirection = (Direction)unitData.d;
                    LevelManager.Ins.player = (Player)SpawnUnit(unitData.c.x, unitData.c.y, (PoolType)unitData.t,
                        (Direction)unitData.d);
                }
                else
                {
                    SpawnUnit(unitData.c.x, unitData.c.y, (PoolType)unitData.t, (Direction)unitData.d);
                }
            }

            return;

            GridUnit SpawnUnit(int x, int y, PoolType type, Direction direction)
            {
                GameGridCell cell = GridMap.GetGridCell(x, y);
                GridUnit unit = SimplePool.Spawn<GridUnit>(DataManager.Ins.GetGridUnit(type));
                unit.UnitInitData = null; // null it to create new init data when init new level
                LevelUnitData levelUnitData = new()
                {
                    mainCellIn = cell,
                    startHeightIn = unit.overrideStartHeight ? unit.StartHeight : HeightLevel.One,
                    directionIn = direction,
                    unitType = type,
                    unit = unit
                };
                if (unit is ICharacter)
                {
                    if (unit is not Player) CharacterDataList.Add(levelUnitData);
                }
                else
                {
                    UnitDataList.Add(levelUnitData);
                }
                HasUnitInMap[x, y] = true;
                unit.OnSetPositionAndRotation(PredictUnitPos(levelUnitData.startHeightIn), direction);
                return unit;

                Vector3 PredictUnitPos(HeightLevel heightLevel)
                {
                    float offsetY = (float)heightLevel / 2 * Constants.CELL_SIZE;
                    if (unit.UnitTypeY == UnitTypeY.Down) offsetY -= unit.yOffsetOnDown;
                    return cell.WorldPos + Vector3.up * offsetY;
                }
            }
        }

        private void OnSpawnShadowUnit()
        {
            // Loop through all suD (shadow unit data) in _rawLevelData
            for (int i = 0; i < _rawLevelData.suD.Length; i++)
            {
                RawLevelData.ShadowUnitData shadowUnitData = _rawLevelData.suD[i];
                // Get unit from data manager
                GridUnit gridUnit = DataManager.Ins.GetGridUnit((PoolType)shadowUnitData.t);
                // If unit is null -> continue
                if (gridUnit is null) continue;
                // Spawn unit
                GridUnit unit = Object.Instantiate(gridUnit);
                unit.Tf.position = shadowUnitData.p;
                unit.Tf.eulerAngles = shadowUnitData.rA;
                unit.ChangeMaterial(DataManager.Ins.GetTransparentMaterial());
                unit.SetAlphaTransparency(0);
                unit.ChangeReceiveShadow(false);
                ShadowUnitList.Add(unit);
                unit.gameObject.SetActive(false);
            }
        }

        public void OnInitPlayerToLevel()
        {
            LevelManager.Ins.player.ResetData();
            LevelManager.Ins.player.OnInit(FirstPlayerInitCell, HeightLevel.One, false, firstPlayerDirection);
            Islands[FirstPlayerInitCell.Data.gridSurface.IslandID].AddInitUnitToIsland(
                LevelManager.Ins.player, LevelManager.Ins.player.UnitInitData, FirstPlayerInitCell);
            Islands[FirstPlayerInitCell.Data.gridSurface.IslandID].SetFirstPlayerStepCell(FirstPlayerInitCell);
        }

        private void OnInitUnit(LevelUnitData data)
        {
            data.unit.ResetData();
            data.unit.OnInit(data.mainCellIn, data.startHeightIn, false, data.directionIn, true);
            if (data.mainCellIn.Data.gridSurface == null)
            {
                nonIslandUnitLis.Add(data);
                return;
            }
            Islands[data.mainCellIn.Data.gridSurface.IslandID]
                .AddInitUnitToIsland(data.unit, data.unit.UnitInitData, data.mainCellIn);
        }
        #endregion
    }

    public enum LevelWinCondition
    {
        FindingFruit = 0,
        DefeatAllEnemy = 1,
        CollectAllChest = 2,
        FindingChest = 3,
        FindingChickenBbq = 4,
    }

    public enum LevelLoseCondition
    {
        Timeout = 0,
        Enemy = 1,
        Bee = 2,
    }

    [Serializable]
    public struct RawLevelData
    {
        public int t; // THEME
        public int lt; // LEVEL TYPE
        public int wc; // WIN CONDITION
        public int lnt; // LEVEL NORMAL TYPE 
        public Vector2Int s; // SIZE
        public GridSurfaceData[] sfD; // SURFACE DATA
        public GridUnitData[] uD; // UNIT DATA
        public ShadowUnitData[] suD; // SHADOW UNIT DATA
        public HintTrailData[] htD; // HINT TRAIL DATA
        public PlayerStep[] pS; // PLAYER STEP

        [Serializable]
        public struct GridSurfaceData
        {
            public Vector2Int p; // POSITION
            public int t; // TYPE
            public int d; // DIRECTION
            public int m; // MATERIAL
        }

        [Serializable]
        public struct GridUnitData
        {
            public Vector2Int c; // CELL
            public int t; // TYPE 
            public int d; // DIRECTION
        }

        [Serializable]
        public struct ShadowUnitData
        {
            public Vector3 p; // POSITION
            public Vector3 rA; // ROTATION ANGLE
            public int t; // TYPE
        }

        [Serializable]
        public struct HintTrailData
        {
            public Vector2 p; // POSITION XZ
        }


    }
}