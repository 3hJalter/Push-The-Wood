using System;
using System.Collections;
using _Game.Utilities;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VinhLB
{
    public class LoadingScreen : UICanvas
    {
        [SerializeField]
        private RectTransform _backgroundRectTF;
        [SerializeField]
        private Slider _progressSlider;
        [SerializeField]
        private TMP_Text _progressText;
        [SerializeField]
        private TMP_Text _loadingText;

        private bool _isFirstTime = true;
        private bool _isWidthFitting = false;
        private Coroutine _closeCoroutine;
        private Sequence _loadingTextSequence;

        public override void Setup(object param = null)
        {
            base.Setup(param);

            SceneGameManager.Ins.OnLoadingScene += SceneGameManager_OnLoadingScene;
            SceneGameManager.Ins.OnSceneLoaded += SceneGameManager_OnSceneLoaded;

            UpdateBackground();
        }

        public override void Open(object param = null)
        {
            base.Open(param);

            UpdateLoadingUI(0f);

            if (_loadingTextSequence == null)
            {
                float duration = 0.75f;
                _loadingTextSequence = DOTween.Sequence();
                _loadingTextSequence.SetDelay(duration / 3, true);
                _loadingTextSequence.Append(DOVirtual.Int(0, 2, duration, (value) =>
                {
                    string loadingText = "Loading.";
                    for (int i = 0; i < value; i++)
                    {
                        loadingText += ".";
                    }
                    _loadingText.text = loadingText;
                }));
                _loadingTextSequence.SetLoops(-1, LoopType.Restart);
            }
            else
            {
                _loadingTextSequence.Restart();
            }
        }

        private void UpdateBackground()
        {
            float currentScreenRatio = (float)Screen.width / Screen.height;
            // DevLog.Log(DevId.Vinh, $"{Constants.REF_SCREEN_RATIO} | {currentScreenRatio}");
            if (currentScreenRatio >= Constants.REF_SCREEN_RATIO && (_isFirstTime || !_isWidthFitting))
            {
                // Fit width
                _backgroundRectTF.anchorMin = new Vector2(0, 0.5f);
                _backgroundRectTF.anchorMax = new Vector2(1f, 0.5f);
                _backgroundRectTF.SetPadding(0, 0);
                _backgroundRectTF.SetSizeDeltaHeight(2500f);
                _isFirstTime = false;
                _isWidthFitting = true;
            }
            else if (currentScreenRatio < Constants.REF_SCREEN_RATIO && (_isFirstTime || _isWidthFitting))
            {
                // Fit height
                _backgroundRectTF.anchorMin = new Vector2(0.5f, 0);
                _backgroundRectTF.anchorMax = new Vector2(0.5f, 1f);
                _backgroundRectTF.SetPadding(0, 0);
                _backgroundRectTF.SetSizeDeltaWidth(1500f);
                _isFirstTime = false;
                _isWidthFitting = false;
            }
        }

        private void UpdateLoadingUI(float progress)
        {
            _progressSlider.value = progress;
            _progressText.text = $"{Mathf.RoundToInt(progress * 100f)}%";
        }

        private void SceneGameManager_OnLoadingScene(int id, float progress)
        {
            UpdateLoadingUI(progress);
        }

        private void SceneGameManager_OnSceneLoaded(int id)
        {
            SceneGameManager.Ins.OnLoadingScene -= SceneGameManager_OnLoadingScene;
            SceneGameManager.Ins.OnSceneLoaded -= SceneGameManager_OnSceneLoaded;

            _closeCoroutine = StartCoroutine(CloseCoroutine(0.5f));
        }

        private IEnumerator CloseCoroutine(float delayTime)
        {
            yield return new WaitForSeconds(delayTime);

            _loadingTextSequence.Pause();

            Close();
        }
    }
}