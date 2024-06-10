using System;
using System.Collections;
using System.Collections.Generic;
using _Game.GameGrid;
using UnityEngine;

namespace VinhLB
{
    public class RollingFog : HMonoBehaviour
    {
        [SerializeField]
        private Renderer _renderer;

        private int _maskPositionPropId;
        private int _maskScalePropId;

        private void Awake()
        {
            _maskPositionPropId = Shader.PropertyToID("_MaskPosition");
            _maskScalePropId = Shader.PropertyToID("_MaskScale");
            
            LevelManager.Ins.OnLevelGenerated += LevelManager_OnLevelGenerated;
        }

        private void OnDestroy()
        {
            LevelManager.Ins.OnLevelGenerated -= LevelManager_OnLevelGenerated;
        }

        public void UpdateFogColliderPositionAndScale()
        {
            Vector3 bottomLeftPosition = LevelManager.Ins.CurrentLevel.GetBottomLeftPos();
            Vector3 topRightPosition = LevelManager.Ins.CurrentLevel.GetTopRightPos();
            Vector3 middleCenterPosition = (bottomLeftPosition + topRightPosition) / 2;
            Vector2 maskPosition = new Vector2(middleCenterPosition.x, middleCenterPosition.z - 12f);
            _renderer.sharedMaterial.SetVector(_maskPositionPropId, maskPosition);
            
            Vector3 positionDifference = topRightPosition - bottomLeftPosition;
            Vector2 maskScale = new Vector2(positionDifference.x, positionDifference.z);
            maskScale.x = Mathf.Abs(maskScale.x) * 0.07f;
            maskScale.y = Mathf.Abs(maskScale.y) * 0.1f;
            _renderer.sharedMaterial.SetVector(_maskScalePropId, maskScale);
        }

        private void LevelManager_OnLevelGenerated()
        {
            UpdateFogColliderPositionAndScale();
        }
    }
}
