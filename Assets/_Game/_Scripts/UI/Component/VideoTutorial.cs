using System;
using System.Collections.Generic;
using _Game.Utilities;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UnityEngine.Video;

namespace _Game._Scripts.UIs.Component
{
    public class VideoTutorial : MonoBehaviour
    {
        public bool isFadeUIOnAppear;
        
        [Title("Video Player")]
        [SerializeField] private VideoPlayer videoPlayer;

        public VideoPlayer VideoPlayer => videoPlayer;
        
        [SerializeField] private RawImage rawImage;
        [SerializeField] private Image maskImage;
        
        public RawImage RawImage => rawImage;

        [SerializeField] private List<GameObject> ui;

        private int _numberVideoLoop;
        private int _currentVideoLoop;

        public event Action OnCallback;
        public event Action OnPrepared;
        
        private void Awake()
        {
            ShowUI(!isFadeUIOnAppear);
            videoPlayer.loopPointReached += EndReached;
            // videoPlayer.started += Started;
            videoPlayer.prepareCompleted += Prepared;
        }

        private void OnDestroy()
        {
            videoPlayer.loopPointReached -= EndReached;
            // videoPlayer.started -= Started;
            videoPlayer.prepareCompleted -= Prepared;
            OnPrepared = null;
            OnCallback = null;
        }
        
        
        public void Play(int numberVideoLoop = 1, Action callback = null)
        {
            if (!gameObject.activeSelf) gameObject.SetActive(true);
            _numberVideoLoop = numberVideoLoop;
            OnCallback = callback;
            videoPlayer.Play();
        }
        
        public void Stop()
        {
            videoPlayer.Stop();
            _currentVideoLoop = 0;
            if (gameObject.activeSelf) gameObject.SetActive(false);
        }
        
        private void Prepared(VideoPlayer source)
        {
           if (isFadeUIOnAppear)
           {
               ShowUI(true);
           }
           OnPrepared?.Invoke();
           videoPlayer.prepareCompleted -= Prepared; // Unsubscribe so that it won't be called again
        }
        
        private void EndReached(VideoPlayer source)
        {
            if (_currentVideoLoop < _numberVideoLoop)
            {
                _currentVideoLoop++;
                videoPlayer.Play();
            }
            else
            {
                Stop();
                OnCallback?.Invoke();
            }
        }

        private void ShowUI(bool isShow)
        {
            rawImage.enabled = isShow;
            maskImage.enabled = isShow;
            for (int i = 0; i < ui.Count; i++)
            {
                ui[i].SetActive(isShow);
            }
        }
    }
}
