using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace _Game._Scripts.UIs.Component
{
    public class HandPointerEffectUI : MonoBehaviour
    {
        [SerializeField] private Vector3 handRotation;
        [SerializeField] private Image handPointer;
        [SerializeField] private GameObject handClick;
        [SerializeField] private Image circleFx;
        [SerializeField] private VideoPlayer video;
        [SerializeField] private float triggerTime;
        
        private bool _isTrigger;
        private Transform _tf;
        private Transform Tf => _tf ??= GetComponent<Transform>();
        private Sequence _sequence;

        private Vector3 _initRotation;
        
        private void Start()
        {
            handPointer.color = new Color(1, 1, 1, 0);
            circleFx.gameObject.SetActive(false);
            handClick.SetActive(false);
            _initRotation = Tf.localEulerAngles;
            video.prepareCompleted += OnPrepared;
            video.loopPointReached += EndReached;
        }

        private void OnDestroy()
        {
            _sequence?.Kill();
            if (video) video.loopPointReached -= EndReached;
        }

        private void Update()
        {
            if (_isTrigger) return;
            if (video.time > triggerTime)
            {
                _isTrigger = true;
                DoAnimation();
            }
        }

        private void OnPrepared(VideoPlayer vp)
        {
            handPointer.color = new Color(1, 1, 1, 1);
            video.prepareCompleted -= OnPrepared;
        }
        
        private void EndReached(VideoPlayer vp)
        {
            _isTrigger = false;
        }

        [Button("Test")]
        private void DoAnimation()
        {
            _sequence?.Kill();
            _sequence = DOTween.Sequence();
            _sequence
                .Append(Tf.DOLocalRotate(handRotation, 0.3f).SetEase(Ease.InOutSine)
                    .OnComplete(() =>
                    {
                        circleFx.gameObject.SetActive(true);
                        handClick.SetActive(true);
                    }))
                .Append(circleFx.transform.DOScale(Vector3.one * 1.5f, 0.3f).OnComplete(() =>
                {
                    circleFx.gameObject.SetActive(false);
                    handClick.SetActive(false);
                }))
                .Join(circleFx.DOFade(0, 0.3f))
                .Append(Tf.DOLocalRotate(_initRotation, 0.3f).SetEase(Ease.InOutSine))
                .OnKill(() =>
                {
                    handClick.SetActive(false);
                    circleFx.gameObject.SetActive(false);
                    circleFx.transform.localScale = Vector3.one;
                    circleFx.color = new Color(1, 1, 1, 1);
                    Tf.localEulerAngles = _initRotation;
                });
        }
    }
}
