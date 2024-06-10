using System;
using System.Collections.Generic;
using _Game._Scripts.Utilities;
using _Game.Utilities;
using _Game.Utilities.Timer;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace GG.Infrastructure.Utils.Swipe
{
    [Serializable]
    public class SwipeListenerEvent : UnityEvent<string>
    {
    }

    public class SwipeListener : MonoBehaviour
    {
        public UnityEvent onSwipeCancelled;
        public SwipeListenerEvent onSwipe;
        public UnityEvent onCancelSwipe;
        public UnityEvent onUnHold;
        public bool isOverlappingUI;


        [SerializeField] private float sensitivity = 100;
        [SerializeField] private float quickSensitive = 50;
        [SerializeField] private float superSensitive = 150;

        [SerializeField] private float swipeTouchTime = 0.12f;
        [SerializeField] private float quickSwipeTouchTime = 0.035f;

        [SerializeField] private bool continuousDetection;

        [SerializeField] private SwipeDetectionMode swipeDetectionMode = SwipeDetectionMode.EightSides;
        [ReadOnly] [SerializeField] private List<Vector2> samplePoints = new();
        [ReadOnly] [SerializeField] private List<Vector2> quickSamplePoints = new();
        [ReadOnly] [SerializeField] private List<Vector2> superSamplePoint = new();

        [ReadOnly] [SerializeField] private bool _isPointerUI;
        private bool _countingSwipeTime;

        private VectorToDirection _directions;

        private float _holdTimer = Constants.HOLD_TOUCH_TIME;

        private bool _isHolding;

        private float _minMoveDistance = 0.1f;

        private Vector3 _offset;
        private float _quickMinMoveDistance = 0.1f;
        private float _superMinMoveDistance = 0.1f;

        private Vector3 _swipeStartPoint;
        private float _swipeTimer;

        private bool _waitForSwipe = true;

        private bool isPredicted;

        public bool ContinuousDetection
        {
            get => continuousDetection;
            set => continuousDetection = value;
        }

        public float SuperSensitive
        {
            get => superSensitive;
            set
            {
                superSensitive = value;
                UpdateSuperSensitivity();
            }
        }

        public float QuickSensitive
        {
            get => quickSensitive;
            set
            {
                quickSensitive = value;
                UpdateQuickSensitivity();
            }
        }

        public float Sensitivity
        {
            get => sensitivity;
            set
            {
                sensitivity = value;
                UpdateSensitivity();
            }
        }

        public SwipeDetectionMode SwipeDetectionMode
        {
            get => swipeDetectionMode;
            set => swipeDetectionMode = value;
        }

        private void Start()
        {
            samplePoints = new List<Vector2>();
            _swipeTimer = swipeTouchTime;
            UpdateSensitivity();
            UpdateQuickSensitivity();
            UpdateSuperSensitivity();
            if (SwipeDetectionMode != SwipeDetectionMode.Custom)
                SetDetectionMode(DirectionPresets.GetPresetByMode(SwipeDetectionMode));
        }

        private void OnEnable()
        {
            _isPointerUI = false;
        }

        private void Update()
        {
            // Version 2
#if UNITY_EDITOR
            if (Input.GetMouseButtonUp(0))
#else
            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended)
#endif
            {
                _isPointerUI = false;
                // Check if button Release
                if (_isHolding)
                {
                    onUnHold?.Invoke();
                    _isHolding = false;
                }

                if (!_waitForSwipe) OnCancelSwipe();
                else OnSuperCancelSwipe();
            }
            
#if UNITY_EDITOR
            else if (Input.GetMouseButtonDown(0))
            {
                // Check if button Pressed
                if (!isOverlappingUI && EventSystem.current.IsPointerOverGameObject())
                {
                    _isPointerUI = true;
                    return;
                }

                InitSwipe();
            }
#else
            else if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                // Check if button Pressed
                if (!isOverlappingUI && EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
                {
                    _isPointerUI = true;
                    return;
                }

                InitSwipe();
            }
#endif

#if UNITY_EDITOR
            else if (Input.GetMouseButton(0))
#else
            else if (Input.touchCount > 0 && (Input.GetTouch(0).phase == TouchPhase.Moved ||
                                              Input.GetTouch(0).phase == TouchPhase.Stationary))
#endif
            {
                // Check if button is being held
                if (!_isPointerUI && _waitForSwipe) CheckSwipe();
            }

            // Check if Swipe is continuous detection
            if (!continuousDetection) CheckSwipeCancellation();
            if (_countingSwipeTime)
                if (_swipeTimer <= Constants.HOLD_TOUCH_TIME)
                    _swipeTimer += Time.deltaTime;
        }

        public void SetDetectionMode(List<DirectionId> directions)
        {
            _directions = new VectorToDirection(directions);
        }

        private void UpdateSensitivity()
        {
            int screenShortSide = Screen.width < Screen.height ? Screen.width : Screen.height;
            _minMoveDistance = screenShortSide / sensitivity;
        }

        private void UpdateQuickSensitivity()
        {
            int screenShortSide = Screen.width < Screen.height ? Screen.width : Screen.height;
            _quickMinMoveDistance = screenShortSide / quickSensitive;
        }

        private void UpdateSuperSensitivity()
        {
            int screenShortSide = Screen.width < Screen.height ? Screen.width : Screen.height;
            _superMinMoveDistance = screenShortSide / superSensitive;
        }

        private void CheckSwipeCancellation()
        {
            if (Input.GetMouseButtonUp(0))
                if (_waitForSwipe)
                    if (onSwipeCancelled != null)
                        onSwipeCancelled.Invoke();
        }

        private void InitSwipe()
        {
            SampleSwipeStart();
            _waitForSwipe = true;
        }

        private void CheckSwipe()
        {
            if (isPredicted) return;

            _offset = Input.mousePosition - _swipeStartPoint;

            int superSampleCount = superSamplePoint.Count;

            if (superSampleCount < 2 && !isPredicted)
            {
                if (superSampleCount == 0) superSamplePoint.Add(Vector2.zero);
                if (_offset.magnitude >= _superMinMoveDistance) superSamplePoint.Add(_offset);
            }

            if (_swipeTimer < quickSwipeTouchTime)
            {
                // TODO
                if (quickSamplePoints.Count < 1)
                {
                    quickSamplePoints.Add(Vector2.zero);
                    _countingSwipeTime = true;
                }

                Vector2 quickDistance = _offset - (Vector3)quickSamplePoints[^1];
                if (quickDistance.magnitude >= _quickMinMoveDistance)
                {
                    quickSamplePoints.Add(_offset);
                    if (quickSamplePoints.Count >= 2 && !isPredicted)
                    {
                        isPredicted = true;
                        float y = HUtilities.PredictYFromLinearRegression(quickSamplePoints, _offset.x);
                        _offset.Set(_offset.x, y, 0);
                        _offset.Normalize();
                        // normalize the _offset
                        onSwipe?.Invoke(_directions.GetSwipeId(_offset));
                        if (!continuousDetection) _waitForSwipe = false;
                        // SampleSwipeStart();
                        _isHolding = false;
                        _countingSwipeTime = false;
                        return;
                    }
                }

            }

            if (samplePoints.Count < 1)
            {
                samplePoints.Add(Vector2.zero);
                _countingSwipeTime = true;
            }

            //NOTE: Calculate distance from old sample point to new sample point.
            Vector2 distance = _offset - (Vector3)samplePoints[^1];


            if (distance.magnitude >= _minMoveDistance)
            {
                samplePoints.Add(_offset);
                if ((samplePoints.Count >= 4 || _swipeTimer > swipeTouchTime) && !isPredicted)
                {
                    isPredicted = true;
                    float y = HUtilities.PredictYFromLinearRegression(samplePoints, _offset.x);
                    _offset.Set(_offset.x, y, 0);
                    _offset.Normalize();

                    onSwipe?.Invoke(_directions.GetSwipeId(_offset));
                    if (!continuousDetection) _waitForSwipe = false;
                    // SampleSwipeStart();
                    _isHolding = false;
                    _countingSwipeTime = false;
                }
            }
            else if (_holdTimer > 0) // Add Hold Gesture for this code
            {
                _holdTimer -= Time.deltaTime;
                if (_holdTimer > 0) return;
                _holdTimer = Constants.HOLD_TOUCH_TIME;
                onSwipe?.Invoke(Constants.NONE);
                if (!continuousDetection) _waitForSwipe = false;
                // SampleSwipeStart();
                _isHolding = true;
            }
        }

        private void OnSuperCancelSwipe()
        {
            _waitForSwipe = false;
            _holdTimer = Constants.HOLD_TOUCH_TIME;
            if (!isPredicted && superSamplePoint.Count > 1)
            {
                float y = HUtilities.PredictYFromLinearRegression(superSamplePoint, _offset.x);
                _offset.Set(_offset.x, y, 0);
                _offset.Normalize();
                // normalize the _offset
                onSwipe?.Invoke(_directions.GetSwipeId(_offset));
                _isHolding = false;
                TimerManager.Ins.WaitForFixedFrame(2, () => { onCancelSwipe?.Invoke(); });
            }
        }

        private void OnCancelSwipe()
        {
            _waitForSwipe = false;
            _holdTimer = Constants.HOLD_TOUCH_TIME;
            onCancelSwipe?.Invoke();
        }

        private void SampleSwipeStart()
        {
            isPredicted = false;
            superSamplePoint.Clear();
            samplePoints.Clear();
            quickSamplePoints.Clear();
            _swipeStartPoint = Input.mousePosition;
            _swipeTimer = 0;
            _offset = Vector3.zero;
        }
    }
}
