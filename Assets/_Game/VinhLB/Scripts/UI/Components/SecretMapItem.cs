using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VinhLB
{
    public class SecretMapItem : HMonoBehaviour
    {
        public enum State
        {
            Locked = 0,
            InProgress = 1,
            Playable = 2,
            Completed = 3
        }
        
        public event Action<int> OnPlayClick;

        [SerializeField]
        private int index;
        [SerializeField]
        private GameObject _statusGO;
        [SerializeField]
        private GameObject _lockGO;
        [SerializeField]
        private GameObject _unlockProgressGO;
        [SerializeField]
        private TMP_Text _unlockProgressText;
        [SerializeField]
        private Button _playButton;
        [SerializeField]
        private GameObject _completeGO;

        private void Awake()
        {
            _playButton.onClick.AddListener(OnPlayButtonClick);
        }

        public void SetState(State state, int compassAmount = 0)
        {
            switch (state)
            {
                case State.Locked:
                    _statusGO.SetActive(true);
                    _lockGO.SetActive(true);
                    _unlockProgressGO.SetActive(false);
                    _playButton.gameObject.SetActive(false);
                    _completeGO.gameObject.SetActive(false);
                    break;
                case State.InProgress:
                    _statusGO.SetActive(true);
                    _lockGO.SetActive(false);
                    _unlockProgressGO.SetActive(true);
                    _unlockProgressText.text = $"{compassAmount}/8";
                    _playButton.gameObject.SetActive(false);
                    _completeGO.gameObject.SetActive(false);
                    break;
                case State.Playable:
                    _statusGO.SetActive(false);
                    _lockGO.SetActive(false);
                    _unlockProgressGO.SetActive(false);
                    _playButton.gameObject.SetActive(true);
                    _completeGO.gameObject.SetActive(false);
                    break;
                case State.Completed:
                    _statusGO.SetActive(true);
                    _lockGO.SetActive(false);
                    _unlockProgressGO.SetActive(false);
                    _playButton.gameObject.SetActive(false);
                    _completeGO.gameObject.SetActive(true);
                    break;
            }
        }

        private void OnPlayButtonClick()
        {
            OnPlayClick?.Invoke(index);
        }

        private void OnDestroy()
        {
            _playButton.onClick.RemoveAllListeners();
        }
    }
}