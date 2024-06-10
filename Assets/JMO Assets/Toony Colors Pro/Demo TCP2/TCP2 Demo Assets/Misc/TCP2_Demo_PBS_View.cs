﻿// Toony Colors Pro+Mobile 2
// (c) 2014-2023 Jean Moreno

using UnityEngine;

namespace ToonyColorsPro
{
    namespace Demo
    {
        public class TCP2_Demo_PBS_View : MonoBehaviour
        {
            private const float XMax = 60;

            private const float XMin = 300;
            //--------------------------------------------------------------------------------------------------
            // PUBLIC INSPECTOR PROPERTIES

            public Transform Pivot;

            [Header("Orbit")] public float OrbitStrg = 3f;

            public float OrbitClamp = 50f;

            [Header("Panning")] public float PanStrg = 0.1f;

            public float PanClamp = 2f;
            public float yMin, yMax;

            [Header("Zooming")] public float ZoomStrg = 40f;

            public float ZoomClamp = 30f;
            public float ZoomDistMin = 1f;
            public float ZoomDistMax = 2f;

            [Header("Misc")] public float Decceleration = 8f;

            public Rect ignoreMouseRect;

            private bool leftMouseHeld;
            private bool middleMouseHeld;


            //--------------------------------------------------------------------------------------------------
            // PRIVATE PROPERTIES

            private Vector3 mouseDelta;
            private Vector3 moveAcceleration;

            private Vector3 mResetCamPos, mResetPivotPos, mResetCamRot, mResetPivotRot;
            private Vector3 orbitAcceleration;
            private Vector3 panAcceleration;
            private bool rightMouseHeld;
            private float zoomAcceleration;

            //--------------------------------------------------------------------------------------------------
            // UNITY EVENTS

            private void Awake()
            {
                mResetCamPos = transform.position;
                mResetCamRot = transform.eulerAngles;
                mResetPivotPos = Pivot.position;
                mResetPivotRot = Pivot.eulerAngles;
            }

            private void Update()
            {
                mouseDelta = Input.mousePosition - mouseDelta;

                Rect rightAlignedRect = ignoreMouseRect;
                rightAlignedRect.x = Screen.width - ignoreMouseRect.width;
                bool ignoreMouse = rightAlignedRect.Contains(Input.mousePosition);

                if (Input.GetMouseButtonDown(0))
                    leftMouseHeld = !ignoreMouse;
                else if (Input.GetMouseButtonUp(0) || !Input.GetMouseButton(0))
                    leftMouseHeld = false;

                if (Input.GetMouseButtonDown(1))
                    rightMouseHeld = !ignoreMouse;
                else if (Input.GetMouseButtonUp(1) || !Input.GetMouseButton(1))
                    rightMouseHeld = false;

                if (Input.GetMouseButtonDown(2))
                    middleMouseHeld = !ignoreMouse;
                else if (Input.GetMouseButtonUp(2) || !Input.GetMouseButton(2))
                    middleMouseHeld = false;

                //Left Button held
                if (leftMouseHeld)
                {
                    orbitAcceleration.x += Mathf.Clamp(mouseDelta.x * OrbitStrg, -OrbitClamp, OrbitClamp);
                    orbitAcceleration.y += Mathf.Clamp(-mouseDelta.y * OrbitStrg, -OrbitClamp, OrbitClamp);
                }
                //Middle/Right Button held
                else if (middleMouseHeld || rightMouseHeld)
                {
                    //panAcceleration.x += Mathf.Clamp(-mouseDelta.x * PanStrg, -PanClamp, PanClamp);
                    panAcceleration.y += Mathf.Clamp(-mouseDelta.y * PanStrg, -PanClamp, PanClamp);
                }

                //Keyboard support
                //orbitAcceleration.x += Input.GetKey(KeyCode.LeftArrow) ? 15 : (Input.GetKey(KeyCode.RightArrow) ? -15 : 0);
                //orbitAcceleration.y += Input.GetKey(KeyCode.UpArrow) ? 15 : (Input.GetKey(KeyCode.DownArrow) ? -15 : 0);

                if (Input.GetKeyDown(KeyCode.R)) ResetView();

                //X Angle Clamping
                Vector3 angle = transform.localEulerAngles;
                if (angle.x < 180 && angle.x >= XMax && orbitAcceleration.y > 0) orbitAcceleration.y = 0;
                if (angle.x > 180 && angle.x <= XMin && orbitAcceleration.y < 0) orbitAcceleration.y = 0;

                //Rotate
                transform.RotateAround(Pivot.position, transform.right, orbitAcceleration.y * Time.deltaTime);
                transform.RotateAround(Pivot.position, Vector3.up, orbitAcceleration.x * Time.deltaTime);

                //Translate
                Vector3 pos = Pivot.transform.position;
                float yDiff = pos.y;
                pos.y += panAcceleration.y * Time.deltaTime;
                pos.y = Mathf.Clamp(pos.y, yMin, yMax);
                yDiff = pos.y - yDiff;
                Pivot.transform.position = pos;

                pos = transform.position;
                pos.y += yDiff;
                transform.position = pos;


                //Zoom
                float scrollWheel = Input.GetAxis("Mouse ScrollWheel");
                zoomAcceleration += scrollWheel * ZoomStrg;
                zoomAcceleration = Mathf.Clamp(zoomAcceleration, -ZoomClamp, ZoomClamp);
                float dist = Vector3.Distance(transform.position, Pivot.position);
                if ((dist >= ZoomDistMin && zoomAcceleration > 0) || (dist <= ZoomDistMax && zoomAcceleration < 0))
                    transform.Translate(Vector3.forward * zoomAcceleration * Time.deltaTime, Space.Self);

                //Deccelerate
                orbitAcceleration = Vector3.Lerp(orbitAcceleration, Vector3.zero, Decceleration * Time.deltaTime);
                panAcceleration = Vector3.Lerp(panAcceleration, Vector3.zero, Decceleration * Time.deltaTime);
                zoomAcceleration = Mathf.Lerp(zoomAcceleration, 0, Decceleration * Time.deltaTime);
                moveAcceleration = Vector3.Lerp(moveAcceleration, Vector3.zero, Decceleration * Time.deltaTime);

                mouseDelta = Input.mousePosition;
            }

            private void OnEnable()
            {
                mouseDelta = Input.mousePosition;
            }

            public void ResetView()
            {
                moveAcceleration = Vector3.zero;
                orbitAcceleration = Vector3.zero;
                panAcceleration = Vector3.zero;
                zoomAcceleration = 0f;

                transform.position = mResetCamPos;
                transform.eulerAngles = mResetCamRot;
                Pivot.position = mResetPivotPos;
                Pivot.eulerAngles = mResetPivotRot;
            }
        }
    }
}