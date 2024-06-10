using System;
using System.Collections.Generic;
using _Game.Camera;
using _Game.DesignPattern;
using _Game.Utilities;
using Cinemachine;
using DG.Tweening;
using UnityEngine;

namespace _Game.Managers
{
    [DefaultExecutionOrder(-100)]
    public class CameraManager : Singleton<CameraManager>
    {
        private const int CAMERA_PRIORITY_ACTIVE = 99;
        private const int CAMERA_PRIORITY_INACTIVE = 1;
        [SerializeField]
        private CinemachineBrain brain;
        [SerializeField]
        private CinemachineVirtualCameraBase currentVirtualCamera;

        [SerializeField]
        private UnityEngine.Camera brainCamera;
        [SerializeField]
        private UnityEngine.Camera perspectiveCamera;
        [SerializeField]
        private UnityEngine.Camera uiCamera;
        [SerializeField]
        private Transform cameraTarget;

        [SerializeField]
        private float cameraMoveTime = 1f;

        // ReSharper disable once Unity.RedundantSerializeFieldAttribute
        // ReSharper disable once CollectionNeverUpdated.Local
        [SerializeField]
        private readonly Dictionary<ECameraType, CinemachineVirtualCameraBase> virtualCameraDic = new();
        private readonly Dictionary<ECameraType, CinemachineComponentBase> cameraComponentDic = new();

        private UnityEngine.Camera mainCamera;
        private ECameraType mainTypeCamera;
        public Vector3 CameraTargetPosition => cameraTarget.position;
        public UnityEngine.Camera BrainCamera => brainCamera;
        public CinemachineVirtualCameraBase CurrentVirtualCamera => currentVirtualCamera;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            mainCamera = brainCamera;
        }

        public bool IsCurrentCameraIs(ECameraType eCameraType)
        {
            return currentVirtualCamera == virtualCameraDic[eCameraType];
        }

        public void ChangeCamera(ECameraType eCameraType, float blendTime = 2f)
        {
            if (currentVirtualCamera is not null)
            {
                currentVirtualCamera.Priority = CAMERA_PRIORITY_INACTIVE;
                currentVirtualCamera.enabled = false;
            }
            currentVirtualCamera = virtualCameraDic[eCameraType];
            currentVirtualCamera.Priority = CAMERA_PRIORITY_ACTIVE;
            currentVirtualCamera.enabled = true;
            brain.m_DefaultBlend.m_Time = blendTime;
            mainTypeCamera = eCameraType;

            switch (eCameraType)
            {
                case ECameraType.PerspectiveCamera:
                    perspectiveCamera.gameObject.SetActive(true);
                    brainCamera.gameObject.SetActive(false);
                    mainCamera = perspectiveCamera;
                    break;
                default:
                    perspectiveCamera.gameObject.SetActive(false);
                    brainCamera.gameObject.SetActive(true);
                    mainCamera = brainCamera;
                    break;
            }
        }

        public T GetCameraCinemachineComponent<T>(ECameraType eCameraType) where T : CinemachineComponentBase
        {
            if (!cameraComponentDic.ContainsKey(eCameraType))
            {
                cameraComponentDic.Add(eCameraType,
                    virtualCameraDic[ECameraType.PerspectiveCamera].GetComponentInChildren<T>());
            }
            return cameraComponentDic[ECameraType.PerspectiveCamera] as T;
        }

        public void ChangeCameraTargetPosition(Vector3 position, float moveTime = -1f, Ease ease = Ease.Linear)
        {
            // if the same position, do nothing
            if ((cameraTarget.position - position).sqrMagnitude < 0.01f) return;

            // if moveTime between -0.01 to 0.01, move instantly
            if (Mathf.Abs(moveTime) < 0.01f)
            {
                ChangeCameraTargetPositionInstant(position);
                return;
            }
            if (moveTime < 0f) moveTime = cameraMoveTime;
            cameraTarget.DOKill();
            cameraTarget.DOMove(position, moveTime).SetEase(ease);
        }

        private void ChangeCameraTargetPositionInstant(Vector3 position)
        {
            cameraTarget.DOKill();
            cameraTarget.position = position;
        }

        public void ChangeCameraPosition(Vector3 position)
        {
            virtualCameraDic[mainTypeCamera].enabled = false;
            mainCamera.transform.position = position;
            virtualCameraDic[mainTypeCamera].enabled = true;
        }

        public void ChangeCameraTarget(ECameraType eCameraType, Transform target)
        {
            virtualCameraDic[eCameraType].Follow = target;
            virtualCameraDic[eCameraType].LookAt = target;
        }

        public Vector3 WorldToViewportPoint(Vector3 position)
        {
            return brainCamera.WorldToViewportPoint(position);
        }

        public Vector3 ViewportToWorldPoint(Vector3 position)
        {
            return uiCamera.ViewportToWorldPoint(position);
        }
    }
}