using System.Collections;
using System.Collections.Generic;
using AudioEnum;
using Unity.VisualScripting;
using UnityEngine;

namespace _Game.GameGrid.Unit.DynamicUnit.Player.PlayerState
{
    using _Game.DesignPattern;
    using _Game.DesignPattern.StateMachine;
    using _Game.Managers;
    using _Game.Utilities.Timer;
    using GameGridEnum;

    public class SleepPlayerState : AbstractPlayerState
    {
        private const float SLEEP_UP_TIME = 0.6f;
        private const float SLEEP_SFX_INTERVAL = 8f;

        public override StateEnum Id => StateEnum.Sleep;
        private bool isSleeping;
        private STimer timer;
        private STimer sfxTimer;
        private float initAnimSpeed;
        ParticleSystem sleepingParticle;

        public override void OnEnter(Player t)
        {
            if (timer == null)
            {
                timer = TimerManager.Ins.PopSTimer();
                sfxTimer = TimerManager.Ins.PopSTimer();
            }
            t.ChangeAnim(Constants.SLEEP_ANIM);           
            isSleeping = true;
            initAnimSpeed = t.AnimSpeed;
            sleepingParticle = ParticlePool.Play(DataManager.Ins.VFXData.GetParticleSystem(VFXType.SleepingzZz), t.VFXPositions[0].position);
            AudioManager.Ins.PlaySfx(SfxType.Sleep);
            
            sfxTimer.Start(SLEEP_SFX_INTERVAL, () => AudioManager.Ins.PlaySfx(SfxType.Sleep), true);
        }

        public override void OnExecute(Player t)
        {
            if (!isSleeping) return;
            if (!GameManager.Ins.IsState(GameState.InGame)) return;
            if (t.InputDirection != Direction.None)
            {
                t.ChangeAnim(Constants.SLEEP_UP_ANIM);
                t.SetAnimSpeed(initAnimSpeed * Constants.SLEEP_UP_ANIM_TIME / SLEEP_UP_TIME);
                isSleeping = false;
                sleepingParticle?.Stop();
                timer.Start(SLEEP_UP_TIME, ChangeIdleState);
            }

            void ChangeIdleState()
            {
                t.StateMachine.ChangeState(StateEnum.Idle);
            }
        }

        public override void OnExit(Player t)
        {
            AudioManager.Ins.StopSfx(SfxType.Sleep);
            timer.Stop();
            sfxTimer.Stop();
            sleepingParticle?.Stop();
            t.SetAnimSpeed(initAnimSpeed);
        }
    }
}