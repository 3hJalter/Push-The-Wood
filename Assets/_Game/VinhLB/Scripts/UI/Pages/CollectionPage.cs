using System;
using System.Collections.Generic;
using _Game.GameGrid;
using _Game.Managers;
using _Game.Resource;
using Cinemachine;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;

namespace VinhLB
{
    public class CollectionPage : TabPage
    {
        [SerializeField]
        private TabGroup _topNavigationTabGroup;

        public override void Open(object param = null)
        {
            base.Open(param);

            if (param is true)
            {
                _topNavigationTabGroup.ResetSelectedTab(false);
            }
            else
            {
                _topNavigationTabGroup.ResetSelectedTab(true);
            }
            SetupCollectionCamera();
        }

        public override void Close()
        {
            base.Close();

            _topNavigationTabGroup.ClearSelectedTab();
        }

        private void SetupCollectionCamera()
        {
            Vector3 targetPosition = CameraManager.Ins.CameraTargetPosition;
            targetPosition.Set(targetPosition.x, -0.5f, targetPosition.z);
            CameraManager.Ins.ChangeCameraTargetPosition(targetPosition, 1f, Ease.OutQuad);
        }
    }
}