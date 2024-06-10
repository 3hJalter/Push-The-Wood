using _Game.DesignPattern;
using _Game.DesignPattern.StateMachine;
using _Game.GameGrid.Unit.StaticUnit;
using _Game.Managers;
using _Game.Utilities.Timer;
using GameGridEnum;
using System;
using System.Collections.Generic;
using _Game._Scripts.InGame;
using _Game._Scripts.Managers;
using _Game._Scripts.Utilities;
using AudioEnum;
using MoreMountains.NiceVibrations;
using UnityEngine;

namespace _Game.GameGrid.Unit.DynamicUnit.Player.PlayerState
{
    public class CutTreePlayerState : AbstractPlayerState
    {
        private bool _isExecuted;
        float originAnimSpeed;
        const float STATE_TIME = 0.4f;
        List<Action> actions = new List<Action>();
        List<float> times = new List<float>() { Constants.CUT_TREE_TIME, STATE_TIME };

        public override StateEnum Id => StateEnum.CutTree;

        public override void OnEnter(Player t)
        {
            originAnimSpeed = t.AnimSpeed;
            t.SetAnimSpeed(originAnimSpeed * Constants.CUT_TREE_ANIM_TIME / Constants.CUT_TREE_TIME);
            t.ChangeAnim(Constants.CUT_TREE_ANIM);
            t.LookDirection(t.CutTreeData.inputDirection);
            CalculateActionAndTime();
            TimerManager.Ins.WaitForTime(times, actions);


            void CalculateActionAndTime()
            {
                actions.Clear();
                actions.Add(CutTreeAction);
                actions.Add(ChangeState);
            }

            void CutTreeAction()
            {
                CutTree(t);
            }

            void ChangeState()
            {
                t.StateMachine.ChangeState(StateEnum.Idle);
            }
        }

        public override void OnExecute(Player t)
        {
            SaveCommand(t);
        }

        public override void OnExit(Player t)
        {
            _isExecuted = false;
            t.SetAnimSpeed(originAnimSpeed);
        }

        private void CutTree(Player t)
        {
            if (!_isExecuted)
            {
                HVibrate.Haptic(HapticTypes.MediumImpact);
                AudioManager.Ins.PlaySfx(SfxType.PushTree);
                t.InvokeActWithUnit(t.CutTreeData.tree);
                _isExecuted = true;
                LevelManager.Ins.SaveGameState(true);
                // Make the tree OnBePushed with direction calculated by position of the Player and the Tree
                // Spawn a TreeRoot
                t.MainCell.ValueChange();
                t.CutTreeData.tree.MainCell.ValueChange();
                LevelManager.Ins.SaveGameState(false);
                TreeRoot treeRoot = SimplePool.Spawn<TreeRoot>(DataManager.Ins.GetGridUnit(PoolType.TreeRoot));
                treeRoot.OnInit(t.CutTreeData.tree.MainCell, t.CutTreeData.tree.StartHeight);
                LevelManager.Ins.CurrentLevel.AddNewUnitToIsland(treeRoot);
                // Spawn a Chump
                Chump.Chump chump = SimplePool.Spawn<Chump.Chump>(t.CutTreeData.tree.chumpPrefab);
                chump.OnInit(t.CutTreeData.tree.MainCell, t.CutTreeData.tree.StartHeight + 1);
                LevelManager.Ins.CurrentLevel.AddNewUnitToIsland(chump);
                chump.OnBePushed(t.CutTreeData.inputDirection, t);

                //DEV: Save state for new object
                // Despawn the Tree
                t.CutTreeData.tree.OnDespawn();
                LevelManager.Ins.SaveGameState(true); // BUG FROM HERE
                chump.MainCell.ValueChange();
                // Push the Chump with the direction
                AudioManager.Ins.PlaySfx(SfxType.PushTree);
                ParticlePool.Play(DataManager.Ins.VFXData.GetParticleSystem(VFXType.LeafExplosion),
                    t.CutTreeData.tree.Tf.position + Vector3.up * 2f);
            }
        }
    }
}