using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using _Game.DesignPattern;
using _Game.Managers;
using _Game.Utilities;
using AudioEnum;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace VinhLB
{
    public class CollectingResourceManager : Singleton<CollectingResourceManager>
    {
        [Serializable]
        private class CollectingResourceConfig
        {
            public PoolType PrefabPoolType;
            public Vector3 MinSpreadPosition;
            public Vector3 MaxSpreadPosition;
            public float SpreadDuration;
            public float MoveDuration;
            public Vector3 ReactPunchScale;
            public float ReactDuration;

            [HideInInspector]
            public RectTransform CollectingResourceParentRectTF;
            public Tween ReactionTween;
        }
        
        [HideInInspector]
        public Func<Task> DelayCollectingRewardKeys;
        [HideInInspector]
        public Func<Task> DelayCollectingLevelStars;

        private const float HEIGHT_OFFSET = 60;
        private const float HEIGHT_INCREASE = 50;

        [Title("UI Gold")]
        [SerializeField]
        private CollectingResourceConfig _collectingUICoinConfig;

        [FormerlySerializedAs("_collectingUIAdTicketConfig")]
        [Title("UI Hearts")]
        [SerializeField]
        private CollectingResourceConfig _collectingUIHeartConfig;

        [Title("UI Reward Keys")]
        [SerializeField]
        private CollectingResourceConfig _collectingUIRewardKeyConfig;

        [Title("UI Level Stars")]
        [SerializeField]
        private CollectingResourceConfig _collectingUILevelStarConfig;

        [Title("In-game Reward Keys")]
        [SerializeField]
        private CollectingResourceConfig _collectingInGameRewardKeyConfig;

        [Title("In-game Compasses")]
        [SerializeField]
        private CollectingResourceConfig _collectingInGameCompassConfig;

        private OverlayScreen _overlayScreen;

        public void SpawnCollectingUICoins(int amount, Vector3 startPosition, Transform endPoint,
            Action<float> eachCompleted = null, Action allCompleted = null)
        {
            UpdateComponents();

            if (_collectingUICoinConfig.CollectingResourceParentRectTF == null)
            {
                _collectingUICoinConfig.CollectingResourceParentRectTF = _overlayScreen.UICoinParentRectTF;
            }

            SpawnCollectingUIResource(_collectingUICoinConfig, amount, startPosition, endPoint,
                eachCompleted, allCompleted);
        }

        public void SpawnCollectingUIHearts(int amount, Vector3 startPosition, Transform endPoint,
            Action<float> eachCompleted = null, Action allCompleted = null)
        {
            UpdateComponents();

            if (_collectingUIHeartConfig.CollectingResourceParentRectTF == null)
            {
                _collectingUIHeartConfig.CollectingResourceParentRectTF = _overlayScreen.UIAdTicketParentRectTF;
            }

            SpawnCollectingUIResource(_collectingUIHeartConfig, amount, startPosition, endPoint,
                eachCompleted, allCompleted);
        }

        public async Task SpawnCollectingUIRewardKeys(int amount, Vector3 startPosition, Transform endPoint,
            Action<float> eachCompleted = null, Action allCompleted = null)
        {
            UpdateComponents();

            if (_collectingUIRewardKeyConfig.CollectingResourceParentRectTF == null)
            {
                _collectingUIRewardKeyConfig.CollectingResourceParentRectTF = _overlayScreen.UIAdTicketParentRectTF;
            }

            await SpawnCollectingUIResource(_collectingUIRewardKeyConfig, amount, startPosition, endPoint,
                eachCompleted, allCompleted);
        }

        public async Task SpawnCollectingUILevelStars(int amount, Vector3 startPosition, Transform endPoint,
            Action<float> eachCompleted = null, Action allCompleted = null)
        {
            UpdateComponents();

            if (_collectingUILevelStarConfig.CollectingResourceParentRectTF == null)
            {
                _collectingUILevelStarConfig.CollectingResourceParentRectTF = _overlayScreen.UILevelStarParentRectTF;
            }

            await SpawnCollectingUIResource(_collectingUILevelStarConfig, amount, startPosition, endPoint,
                eachCompleted, allCompleted);
        }

        public void SpawnCollectingRewardKey(int amount, Transform objectTransform)
        {
            UpdateComponents();

            FloatingRewardKey unit =
                SimplePool.Spawn<FloatingRewardKey>(
                    DataManager.Ins.GetUIUnit(_collectingInGameRewardKeyConfig.PrefabPoolType));
            unit.Text.text = $"+{amount}";
            unit.CanvasGroup.alpha = 0;

            if (_collectingInGameRewardKeyConfig.CollectingResourceParentRectTF == null)
            {
                _collectingInGameRewardKeyConfig.CollectingResourceParentRectTF =
                    _overlayScreen.FloatingRewardKeyParentRectTF;
            }
            if (unit.Tf.parent != _collectingInGameRewardKeyConfig.CollectingResourceParentRectTF)
            {
                unit.Tf.SetParent(_collectingInGameRewardKeyConfig.CollectingResourceParentRectTF, false);
            }
            RectTransform parentRectTransform = _collectingInGameRewardKeyConfig.CollectingResourceParentRectTF;

            Sequence s = DOTween.Sequence();
            AudioManager.Ins.PlaySfx(SfxType.CollectItem);
            s.Append(unit.CanvasGroup.DOFade(1, _collectingInGameRewardKeyConfig.MoveDuration))
                .Join(DOVirtual.Float(0, HEIGHT_INCREASE, _collectingInGameRewardKeyConfig.MoveDuration, y =>
                {
                    Vector2 viewportPoint = CameraManager.Ins.WorldToViewportPoint(objectTransform.position) -
                                            Vector3.one * 0.5f;
                    unit.RectTf.anchoredPosition = new Vector2(parentRectTransform.rect.width * viewportPoint.x,
                        parentRectTransform.rect.height * viewportPoint.y + 60 + y);              
                }).SetEase(Ease.OutQuart))
                .Append(
                    unit.CanvasGroup.DOFade(0, _collectingInGameRewardKeyConfig.MoveDuration * 1.5f)
                        .SetEase(Ease.InQuint))
                .Join(DOVirtual.Float(0, HEIGHT_INCREASE, _collectingInGameRewardKeyConfig.MoveDuration * 1.5f, y =>
                {
                    Vector2 viewportPoint = CameraManager.Ins.WorldToViewportPoint(objectTransform.position) -
                                            Vector3.one * 0.5f;
                    unit.RectTf.anchoredPosition = new Vector2(parentRectTransform.rect.width * viewportPoint.x,
                        parentRectTransform.rect.height * viewportPoint.y + HEIGHT_OFFSET + HEIGHT_INCREASE);
                }).SetEase(Ease.OutQuart))
                .OnComplete(() => OnDespawnUnit(unit));
            s.Play();

            void OnDespawnUnit(UIUnit uiUnit)
            {
                uiUnit.Despawn();
            }
        }

        public void SpawnCollectingCompass(int amount, Transform objectTransform)
        {
            UpdateComponents();

            FloatingCompass unit =
                SimplePool.Spawn<FloatingCompass>(
                    DataManager.Ins.GetUIUnit(_collectingInGameCompassConfig.PrefabPoolType));
            unit.Text.text = $"+{amount}";
            unit.CanvasGroup.alpha = 0;

            if (_collectingInGameCompassConfig.CollectingResourceParentRectTF == null)
            {
                _collectingInGameCompassConfig.CollectingResourceParentRectTF =
                    _overlayScreen.FloatingRewardKeyParentRectTF;
            }
            if (unit.Tf.parent != _collectingInGameCompassConfig.CollectingResourceParentRectTF)
            {
                unit.Tf.SetParent(_collectingInGameCompassConfig.CollectingResourceParentRectTF, false);
            }
            RectTransform parentRectTransform = _collectingInGameCompassConfig.CollectingResourceParentRectTF;

            Sequence s = DOTween.Sequence();
            s.Append(unit.CanvasGroup.DOFade(1, _collectingInGameCompassConfig.MoveDuration))
                .Join(DOVirtual.Float(0, HEIGHT_INCREASE, _collectingInGameCompassConfig.MoveDuration, y =>
                {
                    Vector2 viewportPoint = CameraManager.Ins.WorldToViewportPoint(objectTransform.position) -
                                            Vector3.one * 0.5f;
                    unit.RectTf.anchoredPosition = new Vector2(parentRectTransform.rect.width * viewportPoint.x,
                        parentRectTransform.rect.height * viewportPoint.y + 60 + y);
                }).SetEase(Ease.OutQuart))
                .Append(
                    unit.CanvasGroup.DOFade(0, _collectingInGameCompassConfig.MoveDuration * 1.5f)
                        .SetEase(Ease.InQuint))
                .Join(DOVirtual.Float(0, HEIGHT_INCREASE, _collectingInGameCompassConfig.MoveDuration * 1.5f, y =>
                {
                    Vector2 viewportPoint = CameraManager.Ins.WorldToViewportPoint(objectTransform.position) -
                                            Vector3.one * 0.5f;
                    unit.RectTf.anchoredPosition = new Vector2(parentRectTransform.rect.width * viewportPoint.x,
                        parentRectTransform.rect.height * viewportPoint.y + HEIGHT_OFFSET + HEIGHT_INCREASE);
                }).SetEase(Ease.OutQuart))
                .OnComplete(() => OnDespawnUnit(unit));
            s.Play();

            void OnDespawnUnit(UIUnit uiUnit)
            {
                uiUnit.Despawn();
            }
        }

        private void UpdateComponents()
        {
            if (_overlayScreen is null)
            {
                _overlayScreen = UIManager.Ins.OpenUI<OverlayScreen>();
            }
            else if (!_overlayScreen.gameObject.activeInHierarchy)
            {
                _overlayScreen.Setup();
                _overlayScreen.Open();
            }
        }

        private async Task SpawnCollectingUIResource(CollectingResourceConfig config, int amount,
            Vector3 startPosition, Transform endPoint, Action<float> eachCompleted, Action allCompleted)
        {
            if (amount <= 0)
            {
                DevLog.Log(DevId.Vinh, $"Amount must be positive");
                return;
            }

            if (endPoint == null)
            {
                DevLog.Log(DevId.Vinh, $"EndPoint must be not null");
                return;
            }

            List<Task> taskList = new List<Task>();
            List<UIUnit> unitList = new List<UIUnit>();
            // Spawn units with random value
            for (int i = 0; i < amount; i++)
            {
                UIUnit unit = SimplePool.Spawn<UIUnit>(DataManager.Ins.GetUIUnit(config.PrefabPoolType));
                if (unit.transform.parent != config.CollectingResourceParentRectTF)
                {
                    unit.Tf.SetParent(config.CollectingResourceParentRectTF, false);
                }

                unit.Tf.position = startPosition;
                Vector3 targetPosition = new Vector3(
                    startPosition.x + Random.Range(config.MinSpreadPosition.x, config.MaxSpreadPosition.x),
                    startPosition.y + Random.Range(config.MinSpreadPosition.y, config.MaxSpreadPosition.y),
                    startPosition.z);
                // unit.Tf.DOMove(targetPosition, config.SpreadDuration).SetEase(Ease.OutCirc);
                float jumpPower = Random.Range(0.5f, 2f);
                Task task = unit.Tf.DOJump(targetPosition, jumpPower, 1, config.SpreadDuration).AsyncWaitForCompletion();

                unitList.Add(unit);
                taskList.Add(task);
            }

            await Task.WhenAll(taskList);
            taskList.Clear();

            // Delay before moving
            await Task.Delay(500);

            // Move units to the their corresponding label
            for (int i = 0; i < unitList.Count; i++)
            {
                UIUnit unit = unitList[i];
                float progress = (float)(i + 1) / unitList.Count;
                Task task = unit.Tf.DOMove(endPoint.position, config.MoveDuration).SetEase(Ease.InBack).OnComplete(() =>
                {
                    unit.Despawn();

                    if (config.ReactionTween == null)
                    {
                        // React to units
                        config.ReactionTween =  endPoint.DOPunchScale(config.ReactPunchScale, config.ReactDuration)
                            .SetEase(Ease.InOutElastic).OnComplete(() => { config.ReactionTween = null; });
                    }
                    
                    AudioManager.Ins.PlaySfx(SfxType.CollectCurrency);
                    
                    eachCompleted?.Invoke(progress);
                }).AsyncWaitForCompletion();
                taskList.Add(task);

                if (i < unitList.Count - 1)
                {
                    await Task.Delay(50);
                }
            }
            
            await Task.WhenAll(taskList);
            taskList.Clear();

            allCompleted?.Invoke();
        }
    }

    public class ResourceChangeData
    {
        public object Source;
        public float ChangedAmount;
        public float NewValue;
        public float OldValue;
    }
}