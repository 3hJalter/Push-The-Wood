using GameGridEnum;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Game.GameGrid.Unit
{
    public abstract class GridUnitStatic : GridUnit
    {
        [Title("Static Unit")]
        [SerializeField] protected GridUnitStaticType gridUnitStaticType;
        [SerializeField] protected Anchor anchor;
        
        public GridUnitStaticType GridUnitStaticType => gridUnitStaticType;

        public override void OnInit(GameGridCell mainCellIn, HeightLevel startHeightIn = HeightLevel.One,
            bool isUseInitData = true, Direction skinDirection = Direction.None, bool hasSetPosAndRot = false)
        {
            base.OnInit(mainCellIn, startHeightIn, isUseInitData, skinDirection, hasSetPosAndRot);
        }
        
        public virtual void OnInteract()
        { }
    }
}
