using System.Collections.Generic;
using _Game._Scripts.Managers;
using _Game.Camera;
using _Game.GameGrid;
using _Game.Managers;
using _Game.UIs.Screen;
using AudioEnum;
using MEC;
using UnityEngine;
using UnityEngine.Playables;

public class FirstCutsceneHandler : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private Transform boat;
    [SerializeField] private PlayableDirector playableDirector;


    public void OnStartCutscene()
    {
        Timing.RunCoroutine(OnPlayCutscene());
    }

    private IEnumerator<float> OnPlayCutscene()
    {
        AudioManager.Ins.PlayBgm(BgmType.InGame, 1f);
        AudioManager.Ins.PlayEnvironment(EnvironmentType.Ocean);
        CameraManager.Ins.ChangeCamera(ECameraType.CutsceneCamera);
        CameraManager.Ins.ChangeCameraTarget(ECameraType.CutsceneCamera, player);
        UIManager.Ins.indicatorParentTf.gameObject.SetActive(false);
        playableDirector.Play();
        // // Wait for 408 frame (time to player come to island) to change camera and start tutorial
        // for (int i = 0; i < 408; i++)
        // {
        //     yield return Timing.WaitForOneFrame;
        // }
        yield return Timing.WaitForSeconds(6.8f);
        CameraManager.Ins.ChangeCamera(ECameraType.InGameCamera);
        LevelManager.Ins.HidePlayer(false);
        player.gameObject.SetActive(false);
        yield return Timing.WaitForSeconds(2f); // Time change camera
        TutorialManager.Ins.TutorialList[0].OnForceShowTutorial(0);
        UIManager.Ins.indicatorParentTf.gameObject.SetActive(true);
        yield return Timing.WaitForSeconds(2f);
        // Stop cutscene
        playableDirector.Stop();
        Destroy(gameObject);
    }
}
