using System;
using _Game.DesignPattern;
using _Game.Managers;
using DG.Tweening;
using GameGridEnum;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class WarningEffect : MonoBehaviour
{
    [SerializeField] private Image container;
    [SerializeField] private float containerAlpha = 0.75f;
    [SerializeField] private TextMeshProUGUI tmp;
    [SerializeField] private Image bg;
    [SerializeField] private Color bgColor = new(1f,0.45f,0.25f,0.6f);
    [Title("Step 1")]
    [SerializeField] private float iconYPosition = 133f;
    [SerializeField] private Color outLineColor = new(1, 1f, 1f, 1f);
    [SerializeField] private float outLineThickness = 0.1f;
    [SerializeField] private float outLineSoftness = 0.2f;
    
    [SerializeField] private Vector3 beginScale = new Vector3(3,3,3);
    [SerializeField] private Vector3 endScale = Vector3.one;
    [SerializeField] private float timeScale = 0.3f;
    
    [SerializeField] private Ease firstStepEase = Ease.OutQuad;
    [Title("Step 2")]
    [SerializeField] private float timeChangeFaceColor = 0.2f;
    [SerializeField] private Image icon;
    [SerializeField] private float underlayOffsetX = -0.2f;
    [SerializeField] private float underlayOffsetY = -0.4f;
    [SerializeField] private float underLaySoftness = 0.2f;
    [SerializeField] private float nextOutLineThickness = 0.05f;
    private Sequence sequence;
    [SerializeField] private Vector2 particleSpawnPosition = new(0.4f, 0.55f);
    [Title("Step 3")] 
    [SerializeField] private Ease lastStepEase = Ease.InBack;
    [SerializeField] private float timeHidden = 0.3f;
    [SerializeField] private float nextIconYPosition = 250f;
    
    private ParticleSystem _par;
    private bool _isFirstInitialize;
   
    private void ApplyBase()
    {
        if (!_isFirstInitialize)
        {
            _isFirstInitialize = true;
            // make material instance
            Material mat = new(tmp.fontMaterial);
            tmp.fontMaterial = mat;
        }
        // set container alpha
        container.color = new Color(0,0,0,containerAlpha);
        container.enabled = true;
        bg.color = bgColor;
        bg.transform.localScale = Vector3.one;
        #region Font & Face
        // set face color alpha to 0
        tmp.fontMaterial.SetColor(ShaderUtilities.ID_FaceColor, new Color(1,1,1,0)); 
        // set face softness
        tmp.fontMaterial.SetFloat(ShaderUtilities.ID_OutlineSoftness, outLineSoftness);
        // set y position of icon
        Transform transform1 = icon.transform;
        Vector3 localPosition = transform1.localPosition;
        localPosition = new Vector3(localPosition.x, iconYPosition, localPosition.z);
        transform1.localPosition = localPosition;

        #endregion

        #region OutLine

        // Set color
        tmp.fontMaterial.SetColor(ShaderUtilities.ID_OutlineColor, outLineColor);
        // Set thickness
        tmp.fontMaterial.SetFloat(ShaderUtilities.ID_OutlineWidth, outLineThickness);

        #endregion
    }
    
    [Button]
    public void RunSequence(Action preCallback, Action callback)
    {
        ApplyBase();
        // Tween
        sequence?.Kill();
        sequence = DOTween.Sequence();
        // set icon color to alpha
        icon.color = new Color(1,1,1,0);
        tmp.fontMaterial.DisableKeyword(ShaderUtilities.Keyword_Underlay);
        tmp.transform.localScale = beginScale;
        // Append tween scale from 3 to 1 in timeChangeFontSize
        sequence.Append(tmp.transform.DOScale(endScale, timeScale)).SetEase(firstStepEase);
        // Append tween make face color alpha to 1 in 0.3s 
        sequence.Append(DOVirtual.Float(0, 1, timeChangeFaceColor, value =>
        {
            tmp.fontMaterial.SetColor(ShaderUtilities.ID_FaceColor, new Color(1,1,1,value));
        }).OnStart(() =>
        {
            // Change softness to zero
            tmp.fontMaterial.SetFloat(ShaderUtilities.ID_OutlineSoftness, 0);
            // Active underlay
            tmp.fontMaterial.EnableKeyword(ShaderUtilities.Keyword_Underlay);
            // set offset x  
            tmp.fontMaterial.SetFloat(ShaderUtilities.ID_UnderlayOffsetX, underlayOffsetX);
            // set offset y
            tmp.fontMaterial.SetFloat(ShaderUtilities.ID_UnderlayOffsetY, underlayOffsetY);
            // set softness to 0
            tmp.fontMaterial.SetFloat(ShaderUtilities.ID_UnderlaySoftness, underLaySoftness);
            // set underlay color alpha to 0
            tmp.fontMaterial.SetColor(ShaderUtilities.ID_UnderlayColor, new Color(1,1,1,0 ));
            Vector3 spawnPosition = CameraManager.Ins.ViewportToWorldPoint(particleSpawnPosition);
            _par = ParticlePool.Play(DataManager.Ins.VFXData.GetParticleSystem(VFXType.FlameWarning), 
                spawnPosition, Quaternion.identity);
        }));
        // Join with tween change outline thickness to 0.05
        sequence.Join(DOVirtual.Float(outLineThickness, nextOutLineThickness, timeChangeFaceColor, value =>
        {
            tmp.fontMaterial.SetFloat(ShaderUtilities.ID_OutlineWidth, value);
        }));
        // Join with tween change underlay color alpha to 1
        sequence.Join(DOVirtual.Float(0, 1, timeChangeFaceColor, value =>
        {
            tmp.fontMaterial.SetColor(ShaderUtilities.ID_UnderlayColor, new Color(outLineColor.r,outLineColor.g,outLineColor.b,value));
            icon.color = new Color(1,1,1,value);
        }));
        // Also join with tween shake the transform of text
        sequence.Join(tmp.transform.DOShakePosition(timeChangeFaceColor, 10, 10));
        // Join with tween scale the transform of text to 1.1
        sequence.Join(tmp.transform.DOScale(1.1f, timeChangeFaceColor));
        // Then append tween scale to 1
        sequence.Append(tmp.transform.DOScale(1, timeChangeFaceColor));
        // OnComplete, Debug log
        sequence.OnComplete(() =>
        {
            preCallback?.Invoke();
            _par.Stop();
            sequence = DOTween.Sequence();
            sequence.Append(bg.transform.DOScale(0, timeHidden).SetEase(lastStepEase));
            sequence.Join(tmp.transform.DOScale(0, timeHidden).SetEase(lastStepEase));
            sequence.Join(icon.transform.DOLocalMoveY(nextIconYPosition, timeHidden));
            sequence.Join(DOVirtual.Float(containerAlpha, 0, timeHidden, value =>
            {
                container.color = new Color(0,0,0,value);
            }).SetEase(Ease.InExpo));
            sequence.Join(DOVirtual.Float(bgColor.a, 0, timeHidden, value =>
            {
                bg.color = new Color(bgColor.r,bgColor.g,bgColor.b,value);
            }).SetEase(Ease.InExpo));
            sequence.OnComplete(() =>
            {
                container.enabled = false;
                callback?.Invoke();
            });
        });
    }
}
