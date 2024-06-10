using _Game.DesignPattern.StateMachine;
using _Game.Managers;

namespace _Game.GameGrid.Unit.DynamicUnit.Player.PlayerState
{
    public class StunPlayerState : AbstractPlayerState
    {
        public override StateEnum Id => StateEnum.Stun;

        public override void OnEnter(Player t)
        {
            GameplayManager.Ins.IsCanUndo = false;
            GameplayManager.Ins.IsCanResetIsland = false;
            AudioManager.Ins.PlaySfx(AudioEnum.SfxType.PlayerBeAttacked);
        }

        public override void OnExecute(Player t)
        {
            if (t.IsDead)
            {
                t.StateMachine.ChangeState(StateEnum.Die);
                return;
            }
        }

        public override void OnExit(Player t)
        {
            GameplayManager.Ins.IsCanUndo = true;
            GameplayManager.Ins.IsCanResetIsland = true;
        }
    }
}