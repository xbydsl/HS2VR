using Studio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Valve.VR;
using VRGIN.Controls.Tools;
using VRGIN.Core;
using VRGIN.Helpers;
using static SteamVR_Controller;

namespace HS2VR
{
    class CameraTool : Tool
    {
        public override Texture2D Image
        {
            get
            {
                return UnityHelper.LoadImage("icon_cam.png");
            }
        }

        private int currentCamera = -1;
        private int currentOCICamera = -1;
        private float triggerStartTime = float.MaxValue;
        private bool rewindCamera = false;
        private float backStartTime = float.MaxValue;
        private bool rewindOCICamera = false;
        private List<OCICamera> listCamera;
        private Vector3 _PrevControllerPos;
        private Quaternion _PrevControllerRot;
        private float? _GripStartTime = null;
        private TravelDistanceRumble _TravelRumble;
        private const float GRIP_TIME_THRESHOLD = 0.1f;
        private const float GRIP_DIFF_THRESHOLD = 0.01f;

        protected override void OnAwake()
        {
            base.OnAwake();

            _TravelRumble = new TravelDistanceRumble(500, 0.1f, transform);
            _TravelRumble.UseLocalPosition = true;
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();

            var device = this.Controller;

            if (Studio.Studio.Instance?.ociCamera != null && (Controller.GetPress(EVRButtonId.k_EButton_Grip) || Controller.GetPressDown(Valve.VR.EVRButtonId.k_EButton_Grip) || Controller.GetPressUp(EVRButtonId.k_EButton_Grip)))
            {
                if (Controller.GetPressDown(Valve.VR.EVRButtonId.k_EButton_Grip))
                {
                    _PrevControllerPos = transform.position;
                    _GripStartTime = Time.unscaledTime;
                    _TravelRumble.Reset();
                    _PrevControllerPos = Studio.Studio.Instance.ociCamera.objectItem.transform.position - transform.position;
                    _PrevControllerRot = Quaternion.Inverse(Studio.Studio.Instance.ociCamera.objectItem.transform.rotation) * transform.rotation;
                    Owner.StartRumble(_TravelRumble);
                }
                if (Controller.GetPress(EVRButtonId.k_EButton_Grip))
                {
                    var diffPos = (Studio.Studio.Instance.ociCamera.objectItem.transform.position - transform.position) - _PrevControllerPos;
                    var diffRot = Quaternion.Inverse(_PrevControllerRot * Quaternion.Inverse(Quaternion.Inverse(Studio.Studio.Instance.ociCamera.objectItem.transform.rotation) * transform.rotation)) * (Quaternion.Inverse(Studio.Studio.Instance.ociCamera.objectItem.transform.rotation) * transform.rotation * Quaternion.Inverse(Quaternion.Inverse(Studio.Studio.Instance.ociCamera.objectItem.transform.rotation) * transform.rotation));
                    if (Time.unscaledTime - _GripStartTime > GRIP_TIME_THRESHOLD || Calculator.Distance(diffPos.magnitude) > GRIP_DIFF_THRESHOLD)
                    {
                        var forwardA = Vector3.forward;
                        var forwardB = diffRot * Vector3.forward;
                        var angleDiff = Calculator.Angle(forwardA, forwardB) * VR.Settings.RotationMultiplier;

                        Studio.Studio.Instance.ociCamera.objectItem.transform.position -= diffPos;
                        if (!VR.Settings.GrabRotationImmediateMode && Controller.GetPress(ButtonMask.Trigger | ButtonMask.Touchpad))
                        {
                            Studio.Studio.Instance.ociCamera.objectItem.transform.RotateAround(VR.Camera.Head.position, Vector3.up, -angleDiff);
                        }
                        _GripStartTime = 0; // To make sure that pos is not reset
                    }
                }

                if (Controller.GetPressUp(EVRButtonId.k_EButton_Grip))
                {
                    Owner.StopRumble(_TravelRumble);
                }

                _PrevControllerPos = Studio.Studio.Instance.ociCamera.objectItem.transform.position - transform.position;
                _PrevControllerRot = Quaternion.Inverse(Studio.Studio.Instance.ociCamera.objectItem.transform.rotation) * transform.rotation;
            }
            else if (Studio.Studio.Instance?.ociCamera == null && (Controller.GetPress(EVRButtonId.k_EButton_Grip) || Controller.GetPressDown(Valve.VR.EVRButtonId.k_EButton_Grip) || Controller.GetPressUp(EVRButtonId.k_EButton_Grip)))
            {
                if (Controller.GetPressDown(Valve.VR.EVRButtonId.k_EButton_Grip))
                {
                    _PrevControllerPos = transform.position;
                    _GripStartTime = Time.unscaledTime;
                    _TravelRumble.Reset();
                    _PrevControllerPos = Studio.Studio.Instance.ociCamera.objectItem.transform.position - transform.position;
                    _PrevControllerRot = Quaternion.Inverse(Studio.Studio.Instance.ociCamera.objectItem.transform.rotation) * transform.rotation;
                    Owner.StartRumble(_TravelRumble);
                }
                if (Controller.GetPress(EVRButtonId.k_EButton_Grip))
                {
                    var diffPos = transform.position - _PrevControllerPos;
                    var diffRot = Quaternion.Inverse(_PrevControllerRot * Quaternion.Inverse(transform.rotation)) * (transform.rotation * Quaternion.Inverse(transform.rotation)); 
                    if (Time.unscaledTime - _GripStartTime > GRIP_TIME_THRESHOLD || Calculator.Distance(diffPos.magnitude) > GRIP_DIFF_THRESHOLD)
                    {
                        var forwardA = Vector3.forward;
                        var forwardB = diffRot * Vector3.forward;
                        var angleDiff = Calculator.Angle(forwardA, forwardB) * VR.Settings.RotationMultiplier;

                        VR.Camera.SteamCam.origin.transform.position -= diffPos;
                        if (!VR.Settings.GrabRotationImmediateMode && Controller.GetPress(ButtonMask.Trigger | ButtonMask.Touchpad))
                        {
                            VR.Camera.SteamCam.origin.transform.RotateAround(VR.Camera.Head.position, Vector3.up, -angleDiff);
                        }
                        _GripStartTime = 0; // To make sure that pos is not reset
                    }
                }

                if (Controller.GetPressUp(EVRButtonId.k_EButton_Grip))
                {
                    Owner.StopRumble(_TravelRumble);
                }

                _PrevControllerPos = transform.position;
                _PrevControllerRot = transform.rotation;
            }
            else if (Controller.GetPressDown(Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger))
            {
                triggerStartTime = Time.unscaledTime;
                rewindCamera = false;

            }
            else if (Controller.GetPressDown(Valve.VR.EVRButtonId.k_EButton_SteamVR_Touchpad))
            {
                rewindOCICamera = false;
                List<ObjectInfo> list = ObjectInfoAssist.Find(5);
                listCamera = list.Select((ObjectInfo i) => Studio.Studio.GetCtrlInfo(i.dicKey) as OCICamera).ToList();
                backStartTime = Time.unscaledTime;
            }
            else if (Controller.GetPressUp(Valve.VR.EVRButtonId.k_EButton_SteamVR_Touchpad) && Time.unscaledTime - backStartTime <= .5f && !rewindOCICamera)
            {

                currentOCICamera++;
                if (currentOCICamera + 1 >= listCamera.Count)
                {
                    currentOCICamera = -1;
                    Singleton<Studio.Studio>.Instance.ChangeCamera(Singleton<Studio.Studio>.Instance.ociCamera, _active: false);
                }
                else
                {
                    Singleton<Studio.Studio>.Instance.ChangeCamera(listCamera[currentOCICamera], _active: true);
                }
                VRPlugin.MessageLogger.LogMessage($"Set Current Camera {Singleton<Studio.Studio>.Instance.ociCamera?.name}");
            }
            else if (Controller.GetPressUp(Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger) && Time.unscaledTime - triggerStartTime <= .5f && !rewindCamera)
            {
                if (currentCamera == Studio.Studio.Instance.sceneInfo.cameraData.Length - 1)
                    currentCamera = 0;
                else
                    currentCamera++;
                Singleton<Studio.Studio>.Instance.cameraCtrl.Import(Singleton<Studio.Studio>.Instance.sceneInfo.cameraData[currentCamera]);

                VRPlugin.MessageLogger.LogMessage($"Set Current Camera {currentCamera + 1}");
            }
            else if (Controller.GetPress(Valve.VR.EVRButtonId.k_EButton_SteamVR_Touchpad) && Time.unscaledTime - backStartTime > .5f)
            {
                backStartTime = float.MaxValue;
                rewindOCICamera = true;
                currentOCICamera--;
                if (currentOCICamera == -2)
                {
                    currentOCICamera = listCamera.Count - 1;
                }
                if (currentOCICamera == -1)
                { 
                    Singleton<Studio.Studio>.Instance.ChangeCamera(Singleton<Studio.Studio>.Instance.ociCamera, _active: false);
                }
                else
                {
                    Singleton<Studio.Studio>.Instance.ChangeCamera(listCamera[currentOCICamera], _active: true);
                }
                VRPlugin.MessageLogger.LogMessage($"Set Current Camera {Singleton<Studio.Studio>.Instance.ociCamera?.name}");

            }
            else if (Controller.GetPress(Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger) && Time.unscaledTime - triggerStartTime > .5f)
            {
                triggerStartTime = float.MaxValue;
                rewindCamera = true;
                if (currentCamera == 0)
                    currentCamera = 9;
                else if (currentCamera == -1)
                    currentCamera = 9;
                else
                    currentCamera--;

                Singleton<Studio.Studio>.Instance.cameraCtrl.Import(Singleton<Studio.Studio>.Instance.sceneInfo.cameraData[currentCamera]);

                VRPlugin.MessageLogger.LogMessage($"Set Current Camera {currentCamera + 1}");
            }

        }
        public enum TouchpadDirection
        {
            Center,
            Left,
            Right,
            Up,
            Down
        };

        public static TouchpadDirection GetTouchpadDirection(Vector2 position, float threshold = 0.3f)
        {
            if (position.x > 0.0f && Mathf.Abs(position.x) >= Mathf.Abs(position.y) && position.magnitude > threshold)
            {
                return TouchpadDirection.Right;
            }
            else if (position.x < 0.0f && Mathf.Abs(position.x) >= Mathf.Abs(position.y) && position.magnitude > threshold)
            {
                return TouchpadDirection.Left;
            }
            else if (position.y > 0.0f && Mathf.Abs(position.x) <= Mathf.Abs(position.y) && position.magnitude > threshold)
            {
                return TouchpadDirection.Up;
            }
            else if (position.y < 0.0f && Mathf.Abs(position.x) <= Mathf.Abs(position.y) && position.magnitude > threshold)
            {
                return TouchpadDirection.Down;
            }
            else
            {
                return TouchpadDirection.Center;
            }
        }
    }


}
