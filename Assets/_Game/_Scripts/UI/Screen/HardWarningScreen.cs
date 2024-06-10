using _Game.Camera;
using _Game.GameGrid;
using _Game.Managers;
using UnityEngine;

namespace _Game.UIs.Screen
{
    public class HardWarningScreen : UICanvas
    {
        [SerializeField] private WarningEffect warningEffect;

        public override void Setup(object param = null)
        {
            base.Setup(param);
            // Pause game
            UIManager.Ins.CloseUI<InGameScreen>();
            CameraManager.Ins.ChangeCamera(ECameraType.ZoomOutCamera, 0f);
            GameManager.Ins.ChangeState(GameState.Pause);
            LevelManager.Ins.SetCameraToPlayerIsland(0f);
        }

        public override void Open(object param = null)
        {
            base.Open(param);
            AudioManager.Ins.StopBgm();
            AudioManager.Ins.PlaySfx(AudioEnum.SfxType.WarningLevel);
            warningEffect.RunSequence(() =>
            {
                GameManager.Ins.ChangeState(GameState.InGame);
                UIManager.Ins.OpenUI<InGameScreen>();
                UIManager.Ins.HideUI<InGameScreen>();
            }, () =>
            {
                UIManager.Ins.ShowUI<InGameScreen>();
                Close();
            });
        }
    }
}
