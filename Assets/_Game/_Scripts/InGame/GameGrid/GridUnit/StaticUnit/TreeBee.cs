using System.Collections.Generic;
using _Game._Scripts.InGame;
using _Game._Scripts.Managers;
using _Game.DesignPattern;
using _Game.GameGrid.Unit.DynamicUnit.Player;
using _Game.GameGrid.Unit.Interface;
using _Game.Managers;
using DG.Tweening;
using GameGridEnum;
using MEC;
using UnityEngine;

namespace _Game.GameGrid.Unit.StaticUnit
{
    public class TreeBee : Tree
    {
        [SerializeField] private Tree treePrefab;
        [SerializeField] private Transform hive;
        private bool _isSetHiveLocalPos;
        private Vector3 _hiveInitLocalPos;
        private Vector3 _hiveFallPos;

        public override void OnInit(GameGridCell mainCellIn, HeightLevel startHeightIn = HeightLevel.One, bool isUseInitData = true,
            Direction skinDirection = Direction.None, bool hasSetPosAndRot = false)
        {
            base.OnInit(mainCellIn, startHeightIn, isUseInitData, skinDirection, hasSetPosAndRot);
            hive.parent ??= skin;
            hive.gameObject.SetActive(true);
            if (_isSetHiveLocalPos) hive.localPosition = _hiveInitLocalPos;
            _hiveFallPos = hive.position + new Vector3(0, -0.5f, 0);
        }

        public override void OnCutTree(GridUnit cutter)
        {
            base.OnCutTree(cutter);
            if (cutter is not Player player) return;
            
            // TEST: Make the hive fall down, then the player will be stunned -> End Game
            // TEMPORARY: Make the hive out of the tree
            hive.parent = null;
            DOVirtual.DelayedCall(Constants.CUT_TREE_TIME, () =>
            {
                player.IsStun = true;
                hive.DOMove(_hiveFallPos, 0.2f).OnComplete(() =>
                {
                    Timing.RunCoroutine(DelayInActiveHive());
                    player.IsDead = true;
                });
            });
        }

        public override void OnBePushed(Direction direction, GridUnit pushUnit)
        {
            base.OnBePushed(direction, pushUnit);
            if (pushedUnit is IBeInteractedUnit biu && biu.BeInteractedData.pushUnit is Player)
            {
                if (LevelManager.Ins.IsSavePlayerPushStep)
                {
                    GameplayManager.Ins.SavePushHint.SaveStep(
                        biu.BeInteractedData.pushUnitMainCell.X,
                        biu.BeInteractedData.pushUnitMainCell.Y,
                        (int) biu.BeInteractedData.inputDirection,
                        biu.BeInteractedData.pushUnit.islandID);        
                }
                EventGlobalManager.Ins.OnPlayerPushStep?.Dispatch(new PlayerStep
                {
                    x = biu.BeInteractedData.pushUnitMainCell.X,
                    y = biu.BeInteractedData.pushUnitMainCell.Y,
                    d = (int) biu.BeInteractedData.inputDirection,
                    i = biu.BeInteractedData.pushUnit.islandID
                });
            }
        }

        protected override void OnPushedComplete()
        {
            if (!gameObject.activeSelf) return;
            
            #region Special Case for handle PushHint

            if (pushedUnit is IBeInteractedUnit biu && biu.BeInteractedData.pushUnit is Player)
            {
                // Spawn the Base tree in this cell + Save the state
                LevelManager.Ins.SaveGameState(true);
                pushedUnit.MainCell.ValueChange();
                mainCell.ValueChange();
                LevelManager.Ins.SaveGameState(false);
            }
            // Spawn tree
            Tree tree = SimplePool.Spawn<Tree>(treePrefab);
            tree.OnInit(mainCell, startHeight);
            LevelManager.Ins.CurrentLevel.AddNewUnitToIsland(tree);
            // Then despawn it
            #endregion
            
            OnDespawn();
            LevelManager.Ins.SaveGameState(true);

        }

        protected override void OnAwakeCall()
        {
            _hiveInitLocalPos = hive.localPosition;
            _isSetHiveLocalPos = true;
        }

        protected override void OnDestroyCall()
        {
            _isSetHiveLocalPos = false;
        }
        
        private IEnumerator<float> DelayInActiveHive()
        {
            yield return Timing.WaitForSeconds(1f);
            hive.gameObject.SetActive(false);
        }
    }
}
