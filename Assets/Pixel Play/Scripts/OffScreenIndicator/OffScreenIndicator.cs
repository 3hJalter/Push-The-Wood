using PixelPlay.OffScreenIndicator;
using System;
using System.Collections.Generic;
using _Game.Managers;
using UnityEngine;

/// <summary>
/// Attach the script to the off screen indicator panel.
/// </summary>
[DefaultExecutionOrder(-1)]
public class OffScreenIndicator : MonoBehaviour
{
    [Range(0.5f, 0.9f)]
    [Tooltip("Distance offset of the indicators from the centre of the screen")]
    [SerializeField] private float screenBoundOffset = 0.9f;

    [SerializeField] private Camera mainCamera;
    [SerializeField] private Camera uiCamera;
    private Vector3 screenCentre;
    private Vector3 screenBounds;

    private static readonly List<Target> Targets = new();

    private static Action<Target, bool> _targetStateChanged;
    
    public static Action<Target, bool> TargetStateChanged => _targetStateChanged ??= HandleTargetStateChanged;
    
    private static Transform _centreTransform;

    private float _increasePosFromRatio = 1;

    private void Start()
    {
        screenCentre = new Vector3(Screen.width, Screen.height, 0) / 2;
        screenBounds = screenCentre * screenBoundOffset;
        if (GameManager.Ins.IsReduce) _increasePosFromRatio = GameManager.Ins.ReduceRatio;
    }

    private void LateUpdate()
    {
        DrawIndicators();
    }

    /// <summary>
    /// Draw the indicators on the screen and set their position and rotation and other properties.
    /// </summary>
    private void DrawIndicators()
    {
        if (!mainCamera.gameObject.activeSelf)
        {
            return;
        }
        foreach(Target target in Targets)
        {
            if (!target.NeedArrowIndicator) continue;
            Vector3 screenPosition = OffScreenIndicatorCore.GetScreenPosition(mainCamera, target.transform.position);
            bool isTargetVisible = OffScreenIndicatorCore.IsTargetVisible(screenPosition);
            // float distanceFromCamera = target.NeedDistanceText ? target.GetDistanceFromCamera(mainCamera.transform.position) : float.MinValue;// Gets the target distance from the camera.
            Indicator indicator = null;

            if(target.NeedBoxIndicator && isTargetVisible)
            {
                screenPosition.z = 0;
                indicator = GetIndicator(ref target.indicator, IndicatorType.BOX); // Gets the box indicator from the pool.
            }
            else if(target.NeedArrowIndicator && !isTargetVisible)
            {
                float angle = float.MinValue;
                OffScreenIndicatorCore.GetArrowIndicatorPositionAndAngle(ref screenPosition, ref angle, screenCentre, screenBounds);
                indicator = GetIndicator(ref target.indicator, IndicatorType.ARROW); // Gets the arrow indicator from the pool.
                indicator.transform.rotation = Quaternion.Euler(0, 0, angle * Mathf.Rad2Deg); // Sets the rotation for the arrow indicator.
                indicator.SetArrowImage(target.ArrowIndicatorType);
            }
            if(indicator)
            {
                indicator.SetImageColor(target.TargetColor);// Sets the image color of the indicator.
                // if (!isTargetVisible && target.NeedDistanceText)
                // {
                //     indicator.SetDistanceText(target.GetDistanceFromUnit(LevelManager.Ins.player));   //Set the distance text for the indicator.
                // }
                indicator.transform.position = uiCamera.ScreenToWorldPoint(screenPosition);; //Sets the position of the indicator on the screen.
                indicator.SetTextRotation(Quaternion.identity); // Sets the rotation of the distance text of the indicator.
            }
        }
    }

    /// <summary>
    /// 1. Add the target to targets list if <paramref name="active"/> is true.
    /// 2. If <paramref name="active"/> is false deactivate the targets indicator, 
    ///     set its reference null and remove it from the targets list.
    /// </summary>
    /// <param name="target"></param>
    /// <param name="active"></param>
    private static void HandleTargetStateChanged(Target target, bool active)
    {
        if(active)
        {
            Targets.Add(target);
        }
        else
        {
            if (target.indicator)
            {
                target.indicator.Activate(false);
            }
            target.indicator = null;
            Targets.Remove(target);
        }
    }

    /// <summary>
    /// Get the indicator for the target.
    /// 1. If its not null and of the same required <paramref name="type"/> 
    ///     then return the same indicator;
    /// 2. If its not null but is of different type from <paramref name="type"/> 
    ///     then deactivate the old reference so that it returns to the pool 
    ///     and request one of another type from pool.
    /// 3. If its null then request one from the pool of <paramref name="type"/>.
    /// </summary>
    /// <param name="indicator"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    private Indicator GetIndicator(ref Indicator indicator, IndicatorType type)
    {
        if(indicator != null)
        {
            if(indicator.Type != type)
            {
                indicator.Activate(false);
                indicator = type == IndicatorType.BOX ? BoxObjectPool.current.GetPooledObject() : ArrowObjectPool.current.GetPooledObject();
                indicator.Activate(true); // Sets the indicator as active.
            }
        }
        else
        {
            indicator = type == IndicatorType.BOX ? BoxObjectPool.current.GetPooledObject() : ArrowObjectPool.current.GetPooledObject();
            indicator.Activate(true); // Sets the indicator as active.
        }
        return indicator;
    }

    private void OnDestroy()
    {
        _targetStateChanged = null;
    }
}
