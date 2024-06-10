using System;
using _Game.DesignPattern;
using _Game.DesignPattern.StateMachine;
using _Game.GameGrid.Unit.DynamicUnit.Chump;
using _Game.GameGrid.Unit.DynamicUnit.Player;
using _Game.GameGrid.Unit.Interface;
using _Game.Managers;
using _Game.Utilities;
using AudioEnum;
using DG.Tweening;
using GameGridEnum;
using UnityEngine;

namespace _Game.GameGrid.Unit.StaticUnit
{
    public class Tree : GridUnitStatic, ITree
    {
        [SerializeField]
        public Chump chumpPrefab;
        private const int DEGREE = 6;
        private const float DECAY_VALUE = 0.95f;
        protected GridUnit pushedUnit;

        private Tween _pushTween;
        private TweenCallback _onPushedCompleteCallback;

        protected void Awake()
        {
            _onPushedCompleteCallback += OnPushedComplete;
            OnAwakeCall();
        }
        
        private void OnDestroy()
        {
            _onPushedCompleteCallback -= OnPushedComplete;
            OnDestroyCall();
        }

        public virtual void OnCutTree(GridUnit cutter)
        {
            if (cutter is not Player player) return;
            // Change it state to cut tree
            player.CutTreeData.SetData(GetDirectionFromPlayer(), this);
            player.StateMachine.ChangeState(StateEnum.CutTree);
            return;

            Direction GetDirectionFromPlayer()
            {
                Vector3 playerPos = player.MainCell.WorldPos;
                Vector3 treePos = mainCell.WorldPos;
                if (Math.Abs(playerPos.x - treePos.x) < 0.01f)
                    return playerPos.z > treePos.z ? Direction.Back : Direction.Forward;
                return playerPos.x > treePos.x ? Direction.Left : Direction.Right;
            }
        }

        public override void OnBePushed(Direction direction, GridUnit pushUnit)
        {
            //NOTE: Refactor
            //NOTE: Play Shake Animation
            AudioManager.Ins.PlaySfx(SfxType.BlockTree);
            pushedUnit = pushUnit;
            Vector3 axis = Vector3.Cross(Vector3.up, Constants.DirVector3[direction]);
            float lastAngle = 0;
            ParticlePool.Play(DataManager.Ins.VFXData.GetParticleSystem(VFXType.LeafExplosion),
                Tf.position + Vector3.up * 2f);
            _pushTween = DOVirtual.Float(0, DEGREE * 4 * DECAY_VALUE * DECAY_VALUE, Constants.MOVING_TIME * 0.5f, i =>
                {
                    float rotateAngle;
                    //NOTE: Calculate Angle
                    if (i <= DEGREE)
                    {
                        rotateAngle = i;
                    }
                    else if (i <= 3 * DEGREE * DECAY_VALUE)
                    {
                        rotateAngle = 2 * DEGREE - i;
                    }
                    else
                    {
                        rotateAngle = i - 4 * DEGREE * DECAY_VALUE * DECAY_VALUE;
                    }
                    transform.RotateAround(anchor.Tf.position, axis, rotateAngle - lastAngle);
                    lastAngle = rotateAngle;
                }).SetUpdate(UpdateType.Fixed)
                .SetEase(Ease.OutQuad).OnComplete(_onPushedCompleteCallback);
        }
        
        public override void OnDespawn()
        {
            base.OnDespawn();
            _pushTween?.Kill();
            pushedUnit = null;
        }
        
        protected virtual void OnPushedComplete()
        {
        }
        
        protected virtual void OnAwakeCall(){}
        protected virtual void OnDestroyCall(){}
    }
}