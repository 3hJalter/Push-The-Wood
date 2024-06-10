using System;
using _Game.DesignPattern.StateMachine;
using _Game.Managers;
using AudioEnum;
using DG.Tweening;
using UnityEngine;

namespace _Game.GameGrid.Unit.DynamicUnit.Player.PlayerState
{
    public class HappyPlayerState : AbstractPlayerState
    {
        public override StateEnum Id => StateEnum.Happy;

        public override void OnEnter(Player t)
        {
            t.ChangeAnim(Constants.HAPPY_ANIM, true);
            AudioManager.Ins.PlaySfx(SfxType.Happy);
            // Rotate local skin to 0 -180 0
            t.skin.DOLocalRotate(new Vector3(0, -180, 0), 0.3f);
            // Sfx Happy
        }

        public override void OnExecute(Player t)
        {
            
        }

        public override void OnExit(Player t)
        {
            
        }
    }
}
