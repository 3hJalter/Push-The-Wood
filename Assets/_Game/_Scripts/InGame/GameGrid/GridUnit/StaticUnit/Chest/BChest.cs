using System;
using _Game.GameGrid.Unit.DynamicUnit.Player;
using _Game.GameGrid.Unit.StaticUnit.Interface;
using _Game.Managers;
using AudioEnum;
using DG.Tweening;
using GameGridEnum;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Game.GameGrid.Unit.StaticUnit.Chest
{
    public abstract class BChest : GridUnitStatic, IChest
    {
        [Title("Chest")]
        [SerializeField] protected Animator chestAnimator;
        [SerializeField] protected GameObject chestModel;
        [SerializeField] protected Target indicatorTarget;
        
        [ReadOnly]
        [SerializeField] protected bool isInteracted;

        protected event Action OnChestOpen;
        
        public override void OnInit(GameGridCell mainCellIn, HeightLevel startHeightIn = HeightLevel.One, bool isUseInitData = true,
            Direction skinDirection = Direction.None, bool hasSetPosAndRot = false)
        {
            base.OnInit(mainCellIn, startHeightIn, isUseInitData, skinDirection, hasSetPosAndRot);
            if (!isInteracted) indicatorTarget.enabled = true;
        }

        public override void OnDespawn()
        {
            ShowAnim(false);
            isInteracted = false;
            base.OnDespawn();
        }

        public virtual void OnOpenChestComplete()
        {
            indicatorTarget.enabled = false;
        }
        
        private void ShowAnim(bool isShow)
        {
            chestAnimator.gameObject.SetActive(isShow);
            chestModel.SetActive(!isShow);
            if (!isShow) return;
            AudioManager.Ins.PlaySfx(SfxType.OpenChest);
            chestAnimator.SetTrigger(Constants.OPEN_ANIM);
        }

        protected override void OnEnterTriggerNeighbor(GridUnit triggerUnit)
        {
            if (isInteracted) return;
            if (triggerUnit is not Player) return;
            isInteracted = true;
            ShowAnim(true);
            OnChestOpen?.Invoke();
            DOVirtual.DelayedCall(Constants.CHEST_OPEN_TIME, OnOpenChestComplete);        
        }
    }
}
