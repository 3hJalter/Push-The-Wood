using System;
using System.Collections;
using System.Globalization;
using _Game.DesignPattern;
using _Game.Managers;
using _Game.UIs.Popup;
using AudioEnum;
using DG.Tweening;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace VinhLB
{
    public class StatusBarScreen : UICanvas
    {
        [SerializeField]
        private CanvasGroup _canvasGroup;
        [SerializeField]
        private Image _blockPanel;
        [SerializeField]
        private TMP_Text _goldValueText;
        [SerializeField]
        private Transform _goldIconTF;
        [SerializeField]
        private TMP_Text _heartValueText;
        [SerializeField]
        private TMP_Text _regenHeartTime;
        [SerializeField]
        private Transform _heartIconTF;
        [SerializeField]
        private HButton _addButton;
        [SerializeField]
        private HButton[] _buyHeartButtons;

        private Coroutine _delayOpenCoroutine;

        private event Action _delayCollectingGold;
        private event Action _delayCollectingHearts;

        private void Awake()
        {
            GameManager.Ins.RegisterListenerEvent(EventID.OnChangeGold,
                data => ChangeGoldValue((ResourceChangeData)data));
            GameManager.Ins.RegisterListenerEvent(EventID.OnChangeHeart,
                data => ChangeHeartValue((ResourceChangeData)data));
            GameManager.Ins.RegisterListenerEvent(EventID.OnHeartTimeChange,
                time => ChangeHeartTime((int)time));

            _addButton.onClick.AddListener(() =>
            {
                UIManager.Ins.OpenUI<NotificationPopup>(Constants.FEATURE_COMING_SOON);
            });
            for (int i = 0; i < _buyHeartButtons.Length; i++)
            {
                _buyHeartButtons[i].onClick.AddListener(() =>
                {
                    UIManager.Ins.OpenUI<BuyMorePopup>();
                });
            }
        }

        private void OnDestroy()
        {
            GameManager.Ins.UnregisterListenerEvent(EventID.OnChangeGold,
                data => ChangeGoldValue((ResourceChangeData)data));
            GameManager.Ins.UnregisterListenerEvent(EventID.OnChangeHeart,
                data => ChangeHeartValue((ResourceChangeData)data));
            GameManager.Ins.UnregisterListenerEvent(EventID.OnHeartTimeChange,
                time => ChangeHeartTime((int)time));
        }

        public override void Setup(object param = null)
        {
            base.Setup(param);

            if (param is true)
            {
                _canvasGroup.alpha = 1f;
                _blockPanel.gameObject.SetActive(false);
            }
            else
            {
                _canvasGroup.alpha = 0f;
                _blockPanel.gameObject.SetActive(true);
            }

            int normalizedValue = Mathf.RoundToInt(GameManager.Ins.SmoothGold);
            _goldValueText.text = normalizedValue.ToString(Constants.VALUE_FORMAT, CultureInfo.InvariantCulture);
            normalizedValue = Mathf.RoundToInt(GameManager.Ins.SmoothHeart);
            _heartValueText.text = normalizedValue.ToString(Constants.VALUE_FORMAT, CultureInfo.InvariantCulture);
        }

        public override void Open(object param = null)
        {
            base.Open(param);

            if (param is not true)
            {
                DOVirtual.Float(0f, 1f, 1f, value => _canvasGroup.alpha = value)
                    .OnComplete(() => { _blockPanel.gameObject.SetActive(false); });
            }

            if (_delayOpenCoroutine != null)
            {
                StopCoroutine(_delayOpenCoroutine);
            }
            _delayOpenCoroutine = StartCoroutine(DelayOpenCoroutine());

            _heartValueText.text = GameManager.Ins.Heart.ToString();
            if (GameManager.Ins.Heart >= DataManager.Ins.ConfigData.maxHeart)
            {
                _regenHeartTime.text = "FULL";
            }
            else
            {
                ChangeHeartTime(GameManager.Ins.CurrentRegenHeartTime);
            }
        }

        private void ChangeGoldValue(ResourceChangeData data)
        {
            if (data.ChangedAmount > 0)
            {
                if (!gameObject.activeInHierarchy)
                {
                    // _delayCollectingGold += SpawnCollectingUICoins;
                    GameManager.Ins.SmoothGold += data.ChangedAmount;
                }
                else
                {
                    SpawnCollectingUICoins();
                }
            }
            else
            {
                _goldValueText.text = data.NewValue.ToString(Constants.VALUE_FORMAT, CultureInfo.InvariantCulture);
            }

            void SpawnCollectingUICoins()
            {
                int collectingCoinAmount = Mathf.Min((int)data.ChangedAmount, Constants.MAX_UI_UNIT);
                Vector3 spawnPosition = data.Source as Vector3? ??
                                        CameraManager.Ins.ViewportToWorldPoint(new Vector3(0.5f, 0.5f));
                CollectingResourceManager.Ins.SpawnCollectingUICoins(collectingCoinAmount, spawnPosition, _goldIconTF,
                    (progress) =>
                    {
                        GameManager.Ins.SmoothGold += data.ChangedAmount / collectingCoinAmount;
                        int normalizedValue = Mathf.RoundToInt(GameManager.Ins.SmoothGold);
                        _goldValueText.text = normalizedValue.ToString(Constants.VALUE_FORMAT, CultureInfo.InvariantCulture);
                    });
            }
        }

        private void ChangeHeartValue(ResourceChangeData data)
        {
            if (data.ChangedAmount > 0)
            {
                if (!gameObject.activeInHierarchy)
                {
                    // _delayCollectingAdTickets += SpawnCollectingUIAdTickets;
                    GameManager.Ins.SmoothHeart += data.ChangedAmount;
                    _heartValueText.transform.DOScale(Vector3.one * 1.5f, 0.15f).SetLoops(2, LoopType.Yoyo);
                }
                else
                {
                    SpawnCollectingUIHearts();
                }
            }
            else
            {
                _heartValueText.text =
                    data.NewValue.ToString(Constants.VALUE_FORMAT, CultureInfo.InvariantCulture);
            }

            void SpawnCollectingUIHearts()
            {
                int collectingHeartAmount = Mathf.Min((int)data.ChangedAmount, Constants.MAX_UI_UNIT);
                Vector3 spawnPosition = data.Source as Vector3? ??
                                        CameraManager.Ins.ViewportToWorldPoint(new Vector3(0.5f, 0.5f));
                CollectingResourceManager.Ins.SpawnCollectingUIHearts(collectingHeartAmount, spawnPosition,
                    _heartIconTF,
                    (progress) =>
                    {
                        GameManager.Ins.SmoothHeart += data.ChangedAmount / collectingHeartAmount;
                        int normalizedValue = Mathf.RoundToInt(GameManager.Ins.SmoothHeart);
                        _heartValueText.text = normalizedValue.ToString(Constants.VALUE_FORMAT, CultureInfo.InvariantCulture);
                    });
            }
        }
        private void ChangeHeartTime(int time)
        {
            if (time < 0)
            {
                _regenHeartTime.text = "FULL";
            }
            else
            {
                time = DataManager.Ins.ConfigData.regenHeartTime - time;
                int second = time % 60;
                int minute = time / 60;
                _regenHeartTime.text = $"{minute:00}:{second:00}";
            }          
        }
        private IEnumerator DelayOpenCoroutine()
        {
            while (UIManager.Ins.IsOpened<TransitionScreen>())
            {
                yield return null;
            }

            if (_delayCollectingGold != null)
            {
                _delayCollectingGold.Invoke();
                _delayCollectingGold = null;
            }
            if (_delayCollectingHearts != null)
            {
                _delayCollectingHearts?.Invoke();
                _delayCollectingHearts = null;
            }

            _delayOpenCoroutine = null;
        }
    }
}