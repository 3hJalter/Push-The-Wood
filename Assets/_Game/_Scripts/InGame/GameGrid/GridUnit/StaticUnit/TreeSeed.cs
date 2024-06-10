using System.Collections.Generic;
using System.Linq;
using _Game._Scripts.Managers;
using _Game.DesignPattern;
using _Game.Managers;
using DG.Tweening;
using GameGridEnum;
using HControls;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Game.GameGrid.Unit.StaticUnit
{
    public class TreeSeed : GridUnitStatic
    {
        private bool _isGrown;

        [ReadOnly]
        [SerializeField] private Direction fallTreeDirection = Direction.None;

        [SerializeField] private Animator animator;
        private string _currentAnim = Constants.INIT_ANIM;
        private Tween _currentFallTween;
        
        
        private readonly Dictionary<Direction, Vector3> _fallDirectionLocalSkinRot = new()
        {
            { Direction.None, Vector3.zero},
            {Direction.Forward, new Vector3(40,0,40)},
            {Direction.Back, new Vector3(-40,0,-40)},
            {Direction.Left, new Vector3(0,0,50)},
            {Direction.Right, new Vector3(0,0,-50)},
        };
        
        public override void OnInit(GameGridCell mainCellIn, HeightLevel startHeightIn = HeightLevel.One, bool isUseInitData = true,
            Direction skinDirection = Direction.None, bool hasSetPosAndRot = false)
        {
            base.OnInit(mainCellIn, startHeightIn, isUseInitData, skinDirection, hasSetPosAndRot);
            EventGlobalManager.Ins.OnGrowTree.AddListener(OnGrow);
            EventGlobalManager.Ins.OnUndoBoosterCall.AddListener(OnUndoBoosterCall);
            ChangeAnim(Constants.IDLE_ANIM);
            GameplayManager.Ins.SetGrowTreeInIsland(islandID, upperUnits.Count == 0);
            _isGrown = false;
        }

        protected override void OnMementoRestoreSpawn()
        {
            EventGlobalManager.Ins.OnGrowTree.AddListener(OnGrow);
            EventGlobalManager.Ins.OnUndoBoosterCall.AddListener(OnUndoBoosterCall);
            GameplayManager.Ins.SetGrowTreeInIsland(islandID, upperUnits.Count == 0);
            ChangeAnim(Constants.IDLE_ANIM);
            _isGrown = false;
        }

        public override void OnDespawn()
        {
            EventGlobalManager.Ins.OnGrowTree.RemoveListener(OnGrow);
            EventGlobalManager.Ins.OnUndoBoosterCall.AddListener(OnUndoBoosterCall);
            GameplayManager.Ins.SetGrowTreeInIsland(islandID, false);
            base.OnDespawn();
        }

        protected override void OnEnterTriggerUpper(GridUnit triggerUnit)
        {
            GameplayManager.Ins.SetGrowTreeInIsland(islandID, false);
            fallTreeDirection = triggerUnit.LastPushedDirection;
            // fall the skin to the direction if it not none
            if (fallTreeDirection == Direction.None) return;
            _currentFallTween?.Kill();
            _currentFallTween = skin.DOLocalRotate(_fallDirectionLocalSkinRot[fallTreeDirection], 0.15f).OnKill(() => _currentFallTween = null);
        }

        protected override void OnOutTriggerUpper(GridUnit triggerUnit)
        {
            base.OnOutTriggerUpper(triggerUnit);
            GameplayManager.Ins.SetGrowTreeInIsland(islandID, true);
            // If fall direction is not none, then reset it
            if (fallTreeDirection == Direction.None) return;
            if (_currentFallTween != null && _currentFallTween.IsPlaying())
            {
                _currentFallTween.OnComplete(() =>
                {
                    _currentFallTween = skin.DOLocalRotate(_fallDirectionLocalSkinRot[Direction.None], 0.15f).OnKill(() => _currentFallTween = null);
                });
            }
            else
            {
                _currentFallTween = skin.DOLocalRotate(_fallDirectionLocalSkinRot[Direction.None], 0.15f).OnKill(() => _currentFallTween = null);
            }
            fallTreeDirection = Direction.None;
        }


        private void OnUndoBoosterCall(int pId)
        {
            if (pId != islandID) return;
            _currentFallTween?.Kill();
            if (upperUnits.Count == 0)
            { 
                fallTreeDirection = Direction.None;
                GameplayManager.Ins.SetGrowTreeInIsland(islandID, true);
            }
            else
            {
                fallTreeDirection = upperUnits.First().LastPushedDirection;
                GameplayManager.Ins.SetGrowTreeInIsland(islandID, false);
            }
            skin.localRotation = Quaternion.Euler(_fallDirectionLocalSkinRot[fallTreeDirection]); 
        }
        
        [ContextMenu("Grow Tree")] // TEST
        public void OnGrow(int inputIslandID)
        {
            if (inputIslandID != islandID) return;
            if (_isGrown) return;
            // If has unit on top, return
            if (upperUnits.Count > 0)
                return;
            _isGrown = true;
           // TODO: Some animation
           ChangeAnim(Constants.TREE_GROW_ANIM);
           // On Complete Animation, Spawn the tree, Despawn the seed
           // Cancel player movement
           HInputManager.LockInput();
           
           DOVirtual.DelayedCall(Constants.GROW_TREE_ANIM_TIME, () =>
           {
               LevelManager.Ins.SaveGameState(true);
               mainCell.ValueChange();
               LevelManager.Ins.SaveGameState(false);

               Tree tree = SimplePool.Spawn<Tree>(DataManager.Ins.GetGridUnit(PoolType.TreeShort));
               tree.OnInit(mainCell, startHeight);
               LevelManager.Ins.CurrentLevel.AddNewUnitToIsland(tree);

               // Despawn
               OnDespawn();
               LevelManager.Ins.SaveGameState(true);
           }).OnKill(() => HInputManager.LockInput(false));
        }
        
        private void ChangeAnim(string animName, bool forceAnim = false)
        {
            if (!forceAnim)
                if (_currentAnim.Equals(animName))
                    return;
            animator.ResetTrigger(_currentAnim);
            _currentAnim = animName;
            animator.SetTrigger(_currentAnim);
        }
    }
}
