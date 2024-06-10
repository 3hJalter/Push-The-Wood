using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VinhLB
{
    public class LookAtCamera : HMonoBehaviour
    {
        private enum Mode
        {
            LookAt = 0,
            LookAtInverted = 1,
            CameraForward = 2,
            CameraForwardInverted = 3,
        }

        [SerializeField]
        private Mode _mode;

        private Transform _cameraTransform;

        private void Start()
        {
            _cameraTransform = Camera.main.transform;
        }

        private void LateUpdate()
        {
            if (_cameraTransform == null)
            {
                return;
            }
            
            switch (_mode)
            {
                case Mode.LookAt:
                    Tf.LookAt(_cameraTransform);
                    break;
                case Mode.LookAtInverted:
                    Vector3 directionFromCamera = Tf.position - _cameraTransform.position;
                    Tf.LookAt(Tf.position + directionFromCamera);
                    break;
                case Mode.CameraForward:
                    Tf.forward = _cameraTransform.forward;
                    break;
                case Mode.CameraForwardInverted:
                    Tf.forward = -_cameraTransform.forward;
                    break;
            }
        }
    }

}