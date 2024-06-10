using DG.Tweening;
using UnityEngine;

public class ShakeAnim : MonoBehaviour
{
    private  void Start()
    {
        Sequence s = DOTween.Sequence();
        s.SetDelay(0.5f);
        s.Append(transform.DOShakeRotation(0.5f, Vector3.forward * 10f, 10, 45f, true,
            ShakeRandomnessMode.Harmonic));
        s.AppendInterval(0.5f);
        s.SetLoops(-1, LoopType.Yoyo);
    }
}
