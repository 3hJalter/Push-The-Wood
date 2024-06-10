using _Game.Data;
using _Game.DesignPattern;
using GameGridEnum;
using UnityEngine;

namespace _Game.GameGrid.GridSurface
{
    public abstract class GridSurface : GameUnit
    {
        [SerializeField]
        protected GridSurfaceType surfaceType;
        [SerializeField]
        private int islandID = -1;


        public int IslandID
        {
            get => islandID;
            set
            {
                if (value < 0 || islandID >= 0) return;
                if (surfaceType != GridSurfaceType.Water) islandID = value;
            }
        }

        public GridSurfaceType SurfaceType => surfaceType;

        public virtual void OnInit(int levelIndex, Vector2Int gridCellPos, Vector2Int gridSize,
            Direction rotateDirection = Direction.Forward, MaterialEnum materialEnum = MaterialEnum.None,
            ThemeEnum themeEnum = ThemeEnum.Default, bool hasUnitInMap = false)
        {
        }

        public virtual void OnDespawn()
        {
            islandID = -1;
            this.Despawn();
        }
    }
}