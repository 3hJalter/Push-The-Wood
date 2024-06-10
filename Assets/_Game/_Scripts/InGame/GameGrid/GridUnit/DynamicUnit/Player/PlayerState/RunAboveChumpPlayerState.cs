using _Game.DesignPattern;
using _Game.DesignPattern.StateMachine;
using _Game.Managers;
using _Game.Utilities.Timer;
using DG.Tweening;
using GameGridEnum;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace _Game.GameGrid.Unit.DynamicUnit.Player.PlayerState
{
    public class RunAboveChumpPlayerState : AbstractPlayerState
    {
        private const float ANIM_TIME = 1f;
        private readonly Vector3 UNIT_VECTOR = new Vector3(1, 0, 1);
        private readonly Vector3 WATER_SPLASH_OFFSET = Vector3.up * 0.18f;

        private readonly Quaternion FORWARD_ROTATION = Quaternion.Euler(0, 0, 0);
        private readonly Quaternion BACK_ROTATION = Quaternion.Euler(0, 180, 0);
        private readonly Quaternion RIGHT_ROTATION = Quaternion.Euler(0, 90, 0);
        private readonly Quaternion LEFT_ROTATION = Quaternion.Euler(0, 270, 0);



        public override StateEnum Id => StateEnum.RunAboveChump;
        STimer timer;
        Direction oldDirection;
        //bool isRunAboveChump;
        GridUnit chump;

        public override void OnEnter(Player t)
        {
            if (timer == null)
                timer = TimerManager.Ins.PopSTimer();
            chump = t.MainCell.GetGridUnitAtHeight(Constants.DirFirstHeightOfSurface[GridSurfaceType.Water] + 1);
            oldDirection = t.Direction;
            if (chump is Chump.Chump)
            {
                chump.skin.DOLocalRotate(Vector3.Cross(Constants.DirVector3F[oldDirection], UNIT_VECTOR) * 720f, ANIM_TIME, RotateMode.LocalAxisAdd);
                PlayParticle();
            }
            else 
                ChangeIdleState();

            t.ChangeAnim(Constants.RUN_ABOVE_CHUMP_ANIM);           
            timer?.Start(ANIM_TIME, ChangeIdleState);
            //isRunAboveChump = true;

            void ChangeIdleState()
            {
                t.StateMachine.ChangeState(StateEnum.Idle);
            }

            void PlayParticle()
            {
                Quaternion rotation = FORWARD_ROTATION;
                switch (oldDirection)
                {
                    case Direction.Left:
                        rotation = RIGHT_ROTATION;
                        break;
                    case Direction.Right:
                        rotation = LEFT_ROTATION;
                        break;
                    case Direction.Forward:
                        rotation = BACK_ROTATION;
                        break;
                    case Direction.Back:
                        rotation = FORWARD_ROTATION;
                        break;

                }
                ParticlePool.Play(DataManager.Ins.VFXData.GetParticleSystem(VFXType.WaterSplash_Continuous), chump.transform.position + WATER_SPLASH_OFFSET, rotation);
            }
        }

        public override void OnExecute(Player t)
        {
            //if (!isRunAboveChump) return;
            //if (t.Direction != Direction.None && t.Direction != oldDirection)
            //{
            //    isRunAboveChump = false;
            //    t.StateMachine.ChangeState(StateEnum.Idle);
            //}
        }

        public override void OnExit(Player t)
        {
            timer?.Stop();
            chump?.skin.DOKill();
        }
    }
}