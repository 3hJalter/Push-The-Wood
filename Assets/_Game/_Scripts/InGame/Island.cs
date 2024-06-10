using System.Collections.Generic;
using System.Linq;
using _Game.DesignPattern;
using _Game.GameGrid;
using _Game.GameGrid.Unit;
using _Game.Managers;
using _Game.Utilities;
using DG.Tweening;
using UnityEngine;

namespace _Game._Scripts.InGame
{
    public class Island
    {
        private Level _thisLevel;
        private readonly HashSet<GridUnit> _gridUnits = new();
        private readonly Dictionary<GameGridCell, UnitInitData> _initGridUnitDic = new();
        private readonly int _islandID;
        public HashSet<GridUnit> GridUnits => _gridUnits;

        public Island(int islandID, Level level)
        {
            _thisLevel = level;
            _islandID = islandID;
        }

        public List<GameGridCell> GridCells { get; } = new();

        public GameGridCell FirstPlayerStepCell { get; private set; }

        public Vector3 centerIslandPos;
        public Vector3 minXIslandPos;
        public Vector3 maxXIslandPos;
        public Vector3 minZIslandPos;
        public Vector3 maxZIslandPos;

        public Vector2Int islandSize;
        public bool isSmallIsland;

        public void SetIslandPos()
        {
            centerIslandPos = Vector3.zero;
            minXIslandPos = maxXIslandPos = minZIslandPos = maxZIslandPos = GridCells[0].WorldPos;
            for (int i = 0; i < GridCells.Count; i++)
            {
                centerIslandPos += GridCells[i].WorldPos;
                if (GridCells[i].WorldPos.x < minXIslandPos.x)
                    minXIslandPos = GridCells[i].WorldPos;
                if (GridCells[i].WorldPos.x > maxXIslandPos.x)
                    maxXIslandPos = GridCells[i].WorldPos;
                if (GridCells[i].WorldPos.z < minZIslandPos.z)
                    minZIslandPos = GridCells[i].WorldPos;
                if (GridCells[i].WorldPos.z > maxZIslandPos.z)
                    maxZIslandPos = GridCells[i].WorldPos;
            }
            centerIslandPos /= GridCells.Count;
            // Island size x  = (maxX - minX) / Constants.CELL_SIZE + 1
            // Island size z  = (maxZ - minZ) / Constants.CELL_SIZE + 1
            islandSize = new Vector2Int(
                (int) ((maxXIslandPos.x - minXIslandPos.x) / Constants.CELL_SIZE + 1),
                (int) ((maxZIslandPos.z - minZIslandPos.z) / Constants.CELL_SIZE + 1));
            isSmallIsland = islandSize.x < Constants.MAX_SMALL_ISLAND_SIZE;
        }
        
        
            
        public void SetFirstPlayerStepCell(GameGridCell cell)
        {
            FirstPlayerStepCell ??= cell;
        }

        public void AddGridCell(GameGridCell cell)
        {
            GridCells.Add(cell);
        }

        public void AddInitUnitToIsland(GridUnit unit, UnitInitData data, GameGridCell cell)
        {
            _gridUnits.Add(unit);
            _initGridUnitDic.Add(cell, data);
        }

        public void AddNewUnitToIsland(GridUnit unit)
        {
            _gridUnits.Add(unit);
        }

        public void ClearIsland(bool clearIslandSet = false)
        {
            if (clearIslandSet) _thisLevel.ResetIslandSet.Clear();
            HashSet<int> islandIDSet = new();
            for (int i = 0; i < GridCells.Count; i++)
            {
                GameGridCell cell = GridCells[i];
                cell.ClearGridUnit(_islandID, ref islandIDSet);
            }

            foreach (GridUnit unit in _gridUnits.Where(unit => unit.gameObject.activeSelf))
            {
                if (unit.islandID != _islandID) continue;
                unit.OnDespawn();
            }

            _gridUnits.Clear();

            if (islandIDSet.Count <= 0) return;
            foreach (int id in islandIDSet.Where(id => _thisLevel.ResetIslandSet.Add(id)))
            {
                _thisLevel.Islands[id].ResetIsland(false);
            }
        }

        public void ResetIsland(bool clearIslandSet = true)
        {
            DOTween.KillAll();
            ClearIsland(clearIslandSet);
            foreach (KeyValuePair<GameGridCell, UnitInitData> pair in _initGridUnitDic)
            {
                if (pair.Value.Type == PoolType.Player) continue;
                GridUnit unit =
                    SimplePool.Spawn<GridUnit>(DataManager.Ins.GetGridUnit(pair.Value.Type));
                unit.ResetData();
                unit.OnInit(pair.Key, pair.Value.StartHeight, true, pair.Value.SkinDirection);
                AddNewUnitToIsland(unit);
            }
        }
    }
}
