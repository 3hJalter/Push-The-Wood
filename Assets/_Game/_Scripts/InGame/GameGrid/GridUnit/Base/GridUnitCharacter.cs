using _Game.DesignPattern;
using _Game.DesignPattern.StateMachine;
using _Game.GameGrid.Unit.DynamicUnit.Enemy;
using _Game.GameGrid.Unit.DynamicUnit.Interface;
using _Game.Utilities;
using _Game.Utilities.Grid;
using DG.Tweening;
using GameGridEnum;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Game.GameGrid.Unit
{
    public abstract class GridUnitCharacter : GridUnitDynamic, ICharacter, IDamageable
    {
        public Action _OnCharacterChangePosition;
        [SerializeField]
        protected Animator animator;
        [HideInInspector]
        public bool IsDead { get; set; }
        protected string _currentAnim = Constants.INIT_ANIM;
        protected bool _isAddState;
        [HideInInspector]
        public readonly List<GameGridCell> AttackRange = new List<GameGridCell>();
        public readonly List<DangerIndicator> AttackRangeVFX = new List<DangerIndicator>();
        public readonly List<Vector3> AttackRangePos = new List<Vector3>();
        
        public Direction Direction = Direction.None;
        public float AnimSpeed => animator.speed;
        public void ChangeAnim(string animName, bool forceAnim = false)
        {
            if (!forceAnim)
                if (_currentAnim.Equals(animName))
                    return;
            animator.ResetTrigger(_currentAnim);
            _currentAnim = animName;
            animator.SetTrigger(_currentAnim);
        }
        
        public void ChangeAnimNoStore(string animName, bool forceAnim = false)
        {
            if (!forceAnim)
                if (_currentAnim.Equals(animName))
                    return;
            animator.ResetTrigger(_currentAnim);
            animator.SetTrigger(animName);
        }
        
        public void SetAnimSpeed(float speed)
        {
            animator.speed = speed;
        }
        public bool IsCurrentAnimDone()
        {
            return animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1;
        }

        public void LookDirection(Direction directionIn)
        {
            if (directionIn is Direction.None) return;
            skinRotationDirection = directionIn;
            skin.DOLookAt(Tf.position + Constants.DirVector3[directionIn], 0.2f, AxisConstraint.Y, Vector3.up);
        }
        protected abstract void AddState();
        public abstract void OnCharacterDie();
        public override void OnDespawn()
        {
            base.OnDespawn();
            foreach (GameGridCell cell in AttackRange)
            {
                cell.Data.IsBlockDanger = false;
                cell.Data.SetDanger(false, GetHashCode());
            }
            AttackRange.Clear();
            foreach (DangerIndicator indicator in AttackRangeVFX)
            {
                indicator.Despawn();
            }
            AttackRangeVFX.Clear();
        }
        public override void ChangeSkin(int index)
        {
            animator = skinController.ChangeSkin(index);
            ChangeAnim(_currentAnim, true);
        }
        #region SAVING DATA
        public override IMemento RawSave()
        {
            return new CharacterUnitMemento<GridUnitCharacter>(this, IsDead, _currentAnim, CurrentStateId, isSpawn, Tf.position, skin.rotation, startHeight, endHeight
                , unitTypeY, unitTypeXZ, belowUnits, neighborUnits, upperUnits, mainCell, cellInUnits, islandID, lastPushedDirection);
        }

        

        public class CharacterUnitMemento<T> : DynamicUnitMemento<T> where T : GridUnitCharacter
        {
            protected bool isDead;
            protected string currentAnim;
            public CharacterUnitMemento(GridUnitCharacter main, bool isDead, string currentAnim, StateEnum currentState, params object[] data) : base((T)main, currentState, data)
            {
                this.isDead = isDead;
                this.currentAnim = currentAnim;
            }

            public override void Restore()
            {
                base.Restore();
                if (isDead == false && main.IsDead && main is IEnemy mainEnemy)
                {
                    mainEnemy.AddToLevelManager();
                }
                main.IsDead = isDead;
                main.ChangeAnim(currentAnim, true);
                
            }
        }
        #endregion
    }
}