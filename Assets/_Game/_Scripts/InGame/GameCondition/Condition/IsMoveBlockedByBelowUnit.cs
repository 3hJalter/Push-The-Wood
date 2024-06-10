using _Game._Scripts.InGame.GameCondition.Data;
using _Game.DesignPattern.ConditionRule;
using _Game.GameGrid;
using _Game.GameGrid.Unit;
using GameGridEnum;
using UnityEngine;

namespace _Game._Scripts.InGame.GameCondition.Condition
{
    [CreateAssetMenu(fileName = "IsMoveBlockedByBelowUnit", menuName = "ConditionSO/Moving/IsMoveBlockedByBelowUnit",
        order = 1)]
    public class IsMoveBlockedByBelowUnit : ScriptableObject, ICondition
    {
        public bool IsApplicable(ConditionData dataIn)
        {
            if (dataIn is not MovingData data) return false;
            if (data.owner is not GridUnitDynamic player) return false;
            dataIn.Condition = CONDITION.NONE;
            // Get Below Player Information
            GameGridCell currentCell = player.MainCell;
            bool canMoveDirectly = currentCell.Data.canMovingDirectly;
            HeightLevel belowSurfaceStartHeight = Constants.DirFirstHeightOfSurface[currentCell.SurfaceType];
            // Condition
            if (canMoveDirectly) return true;
            if (player.StartHeight <= belowSurfaceStartHeight) return false;
            GridUnit belowUnit = player.GetBelowUnitAtMainCell();
            if (!belowUnit) return false;
            // Only Consider the first unit (MainCell)
            bool returnVal = true;
            switch (belowUnit.UnitTypeXZ)
            {
                case UnitTypeXZ.None:
                    return false;
                case UnitTypeXZ.Both:
                    return true;
                case UnitTypeXZ.Vertical:                  
                    returnVal = data.inputDirection is Direction.Forward or Direction.Back;
                    if(!returnVal)
                        dataIn.Condition = CONDITION.RUN_ABOVE_CHUMP;
                    break;
                case UnitTypeXZ.Horizontal:
                    returnVal = data.inputDirection is Direction.Left or Direction.Right;
                    if (!returnVal)
                        dataIn.Condition = CONDITION.RUN_ABOVE_CHUMP;
                    break;

            }
            return returnVal;
        }
    }
}
