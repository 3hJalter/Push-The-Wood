using System;
using _Game._Scripts.Managers;
using _Game.DesignPattern;
using _Game.GameGrid.Unit.Interface;
using _Game.GameGrid.Unit.StaticUnit.Chest;
using _Game.Utilities;
using GameGridEnum;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using VinhLB;

namespace _Game.GameGrid.Unit.StaticUnit
{
    /* Description:
     * This class is used for listen from button unit
     * The chest will be opened when all button units are entered
     */
    public class ChestButtonUnit : BChest, IFinalPoint
    {
        [SerializeField] private GameObject lockedChestModel;
        [SerializeField] private ParticleSystem chestUnlockParticle;
        
        [Title("Canvas")] 
        [SerializeField] private GameObject canvas;
        [SerializeField] private TextMeshProUGUI textMeshPro;
        
        [ReadOnly]
        [SerializeField] private bool isLocked;
        private int numberOfButtonEntered;
        private int numberOfButtonInLevel;

        public bool IsActive => gameObject.activeSelf;
        
        public override void OnOpenChestComplete()
        {
            base.OnOpenChestComplete();
            if (LevelManager.Ins.CollectedChests.Add(this))
            {
                CollectingResourceManager.Ins.SpawnCollectingRewardKey(1, LevelManager.Ins.player.transform);
                LevelManager.Ins.KeyRewardCount += 1;
                LevelManager.Ins.goldCount += 40;
            }
            OnRemoveFromLevelManager();
        }

        public override void OnInit(GameGridCell mainCellIn, HeightLevel startHeightIn = HeightLevel.One,
            bool isUseInitData = true,
            Direction skinDirection = Direction.None, bool hasSetPosAndRot = false)
        {
            base.OnInit(mainCellIn, startHeightIn, isUseInitData, skinDirection, hasSetPosAndRot);
            SetUpButtonUnit();
            OnLockChest();
            OnAddToLevelManager();

        }

        public override void OnDespawn()
        {
            DeSetUpButtonUnit();
            OnRemoveFromLevelManager(false);
            base.OnDespawn();
        }

        protected override void OnEnterTriggerNeighbor(GridUnit triggerUnit)
        {
            if (isLocked) return;
            base.OnEnterTriggerNeighbor(triggerUnit);
        }

        private Action<bool> OnButtonUnitEnter()
        {
            // if dispatch true, button unit is entered
            return isEntered =>
            {
                if (isEntered)
                {
                    // Not add more button if chest is unlocked or number of button entered is greater than number of button in level
                    if (!isLocked || numberOfButtonEntered >= numberOfButtonInLevel) return;
                    numberOfButtonEntered++;
                }
                else
                {
                    // Not reduce more button if chest is unlocked or number of button entered is less than 0
                    if (!isLocked || numberOfButtonEntered <= 0) return;
                    numberOfButtonEntered--;
                }

                SetText();
                if (numberOfButtonEntered == numberOfButtonInLevel && isLocked) OnUnlockChest();
            };
        }

        private void OnLockChest()
        {
            isLocked = true;
            chestAnimator.gameObject.SetActive(false);
            chestModel.SetActive(false);
            lockedChestModel.SetActive(true);
            canvas.SetActive(true);
            SetText();
        }

        private void OnUnlockChest()
        {
            isLocked = false;
            chestAnimator.gameObject.SetActive(true);
            chestModel.SetActive(true);
            lockedChestModel.SetActive(false);
            canvas.SetActive(false);
            chestUnlockParticle.Play();
            if (neighborUnits.Contains(LevelManager.Ins.player))
            {
                OnEnterTriggerNeighbor(LevelManager.Ins.player);
            }
        }

        private void SetText()
        {
            textMeshPro.text = $"{numberOfButtonEntered}/{numberOfButtonInLevel}";
        }

        // public override void OnBePushed(Direction direction = Direction.None, GridUnit pushUnit = null)
        // {
        //     if (isInteracted || isLocked) return;
        //     if (pushUnit is not Player) return;
        //     isInteracted = true;
        //     base.OnBePushed(direction, pushUnit);
        //     ShowAnim(true);
        //     DOVirtual.DelayedCall(Constants.CHEST_OPEN_TIME, OnOpenChestComplete);
        // }
        
        

        private void SetUpButtonUnit()
        {
            // Get number of button in level
            numberOfButtonInLevel = 0;
            numberOfButtonEntered = 0;
            LevelManager.Ins.CurrentLevel.UnitDataList.ForEach(unitData =>
            {
                if (unitData.unit.PoolType == PoolType.ButtonUnit) numberOfButtonInLevel++;
            });
            // Add listener
            EventGlobalManager.Ins.OnButtonUnitEnter.AddListener(OnButtonUnitEnter(), true);
        }

        private void DeSetUpButtonUnit()
        {
            EventGlobalManager.Ins.OnButtonUnitEnter.RemoveListener(OnButtonUnitEnter());
            numberOfButtonEntered = 0;
            numberOfButtonInLevel = 0;
        }
        
        private void OnAddToLevelManager()
        {
            LevelManager.Ins.OnAddFinalPoint(this);
        }
        
        private void OnRemoveFromLevelManager(bool checkWin = true)
        {
            LevelManager.Ins.OnRemoveFinalPoint(this, checkWin);
        }
    }
}
