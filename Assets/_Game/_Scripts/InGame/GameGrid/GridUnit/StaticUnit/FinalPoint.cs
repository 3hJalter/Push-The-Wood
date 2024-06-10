using _Game._Scripts.Managers;
using _Game.Data;
using _Game.GameGrid.Unit.DynamicUnit.Player;
using _Game.GameGrid.Unit.Interface;
using _Game.Managers;
using _Game.Resource;
using DG.Tweening;
using GameGridEnum;
using UnityEngine;
using VinhLB;

namespace _Game.GameGrid.Unit.StaticUnit
{
    public class FinalPoint : GridUnitStatic, IFinalPoint, IResourceHolder
    {
        [SerializeField] protected Target indicatorTarget;
        [SerializeField] protected bool isInteracted;
        [SerializeField] protected ParticleSystem winParticle;

        public bool IsActive => gameObject.activeSelf;
        
        public override void OnInit(GameGridCell mainCellIn, HeightLevel startHeightIn = HeightLevel.One, bool isUseInitData = true,
            Direction skinDirection = Direction.None, bool hasSetPosAndRot = false)
        {
            base.OnInit(mainCellIn, startHeightIn, isUseInitData, skinDirection, hasSetPosAndRot);
            if (!isInteracted) indicatorTarget.enabled = true;
            OnAddToLevelManager();
        }
        
        public override void OnDespawn()
        {
            isInteracted = false;
            winParticle.Stop();
            OnRemoveFromLevelManager(false);
            base.OnDespawn();
        }
        
        private void OnReachPoint()
        {
            indicatorTarget.enabled = false;
            winParticle.Play();
            OnTakeResource(); // TEMPORARY
            OnRemoveFromLevelManager();
        }
        
        protected override void OnEnterTriggerNeighbor(GridUnit triggerUnit)
        {
            if (isInteracted) return;
            if (triggerUnit is not Player) return;
            isInteracted = true;
            OnReachPoint();
        }

        private void OnAddToLevelManager()
        {
            LevelManager.Ins.OnAddFinalPoint(this);
        }
        
        private void OnRemoveFromLevelManager(bool checkWin = true)
        {
            LevelManager.Ins.OnRemoveFinalPoint(this, checkWin);
        }

        public void OnTakeResource()
        {
            #region Get Resource
            
            if (LevelManager.Ins.CurrentLevel.LevelType == LevelType.Normal)
            {
                int levelIndex = LevelManager.Ins.CurrentLevel.Index;

                if (levelIndex == 6)
                {
                    CollectingResourceManager.Ins.SpawnCollectingRewardKey(3, LevelManager.Ins.player.Tf);
                    LevelManager.Ins.KeyRewardCount += 3;
                }
                else if (levelIndex >= DataManager.Ins.ConfigData.unlockSecretLevelAtLevelIndex)
                {
                    int getCompass = levelIndex -
                                     DataManager.Ins.ConfigData.unlockSecretLevelAtLevelIndex;
                    if (getCompass > 0 && getCompass % 4 == 0)
                    {
                        CollectingResourceManager.Ins.SpawnCollectingCompass(1, LevelManager.Ins.player.Tf);
                        LevelManager.Ins.SecretMapPieceCount += 1;
                    }
                }
            }
            #endregion
        }
    }
}
