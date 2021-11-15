using HS2VR.Capture;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using Valve.VR;
using VRGIN.Controls;
using VRGIN.Controls.Tools;
using VRGIN.Core;
using VRGIN.Helpers;
using VRGIN.Modes;

namespace HS2VR
{
    class GenericStandingMode : StandingMode
    {
        protected override IEnumerable<IShortcut> CreateShortcuts()
        {
            return base.CreateShortcuts().Concat(new IShortcut[] {

                new MultiKeyboardShortcut(VR.Settings.Shortcuts.ChangeMode.GetKeyStrokes(), () => { 
                    VR.Manager.SetMode<GenericSeatedMode>();
                }),
                new MultiKeyboardShortcut(VR.Settings.Shortcuts.ResetView, () => {
                    VR.Camera.Origin.Rotate(new Vector3(0, VR.Camera.Origin.rotation.y, 0), Space.World);
                    VR.Camera.Head.Rotate(new Vector3(0, VR.Camera.Head.rotation.y, 0), Space.World);
                }),
                new MultiKeyboardShortcut(((HS2VRSettings)VR.Settings).HS2Shortcuts.SuspendPOVToggle.GetKeyStrokes(), () =>
                {
                    VRPatcher.POVPaused = !VRPatcher.POVPaused;
                })
            });
        }

        public override void MoveToPosition(Vector3 targetPosition, Quaternion rotation = default(Quaternion), bool ignoreHeight = true)
        {
            var targetForward = Calculator.GetForwardVector(rotation);
            var currentForward = Calculator.GetForwardVector(VR.Camera.Head.rotation);

            float yAngle = rotation.eulerAngles.y;

            VR.Camera.Origin.rotation *= Quaternion.FromToRotation(currentForward, targetForward);
            VR.Camera.Origin.eulerAngles = new Vector3(VR.Camera.Origin.eulerAngles.x, yAngle, VR.Camera.Origin.eulerAngles.z);


            float targetY = ignoreHeight ? 0 : targetPosition.y;
            float myY = ignoreHeight ? 0 : VR.Camera.Head.position.y;
            var newTargetPosition = new Vector3(targetPosition.x, targetY, targetPosition.z);
            var myPosition = new Vector3(VR.Camera.Head.position.x, myY, VR.Camera.Head.position.z);
            VR.Camera.Origin.position = (newTargetPosition - (myPosition - VR.Camera.Origin.position));
        }

        protected override void OnStart()
        {
            VR.Camera.SteamCam.origin.transform.position = Vector3.zero;
            VR.Camera.SteamCam.origin.transform.rotation = Quaternion.identity;

            base.OnStart();

            _CapturePanorama = VR.Camera.SteamCam.gameObject.AddComponent<HS2VRCapturePanorama>();

            //  SteamVR.instance.overlay.ShowKeyboard(0, 0, "Keyboard", 256, "", false, 0);
            //  SteamVR_Events.System(Valve.VR.EVREventType.VREvent_KeyboardClosed).Listen(OnKeyboardClosed);


            MoveToPosition(VRPlugin.CameraResetPos, VRPlugin.CameraResetRot, false);
        }

        private HS2VRCapturePanorama _CapturePanorama;

        private void OnKeyboardClosed(VREvent_t args)
        {
            
        }

        public override void OnDestroy()
        {            
            Destroy(_CapturePanorama);
            base.OnDestroy();
        }

        protected override void OnUpdate()
        {
            CheckInput();   
        }   

        private void BuildTool(string tool, List<Type> toolList)
        {
            switch (tool.Trim().ToUpper())
            {
                case "MENU":
                    toolList.Add(typeof(MenuTool));
                    break;
                case "WARP":
                    toolList.Add(typeof(WarpTool));
                    break;
                case "PLAY":
                    if (Application.productName != "StudioNEOV2")
                        toolList.Add(typeof(PlayTool));
                    break;
                case "CAM":
                    if (Application.productName == "StudioNEOV2")
                        toolList.Add(typeof(CameraTool));
                    break;
                case "POV":
                    if (VRPatcher.POVAvailable)
                        toolList.Add(typeof(POVTool));
                    break;
                case "ROT":
                    toolList.Add(typeof(RotationTool));
                    break;
            }
        }

        public override IEnumerable<Type> LeftTools
        {
            get
            {
                List<Type> toolList = new List<Type>();
                ((HS2VRSettings)VR.Settings).LeftTools.Split(',').ToList().ForEach(s => BuildTool(s, toolList));
                return toolList;              
            }
        }

        public override IEnumerable<Type> RightTools
        {
            get
            {
                List<Type> toolList = new List<Type>();
                ((HS2VRSettings)VR.Settings).RightTools.Split(',').ToList().ForEach(s => BuildTool(s, toolList));
                return toolList;
            }
        }

        public override IEnumerable<Type> Tools
        {
            get
            {
                return new Type[] { };

                
            }
        }
    }
}
