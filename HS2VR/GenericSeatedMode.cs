using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Valve.VR;
using VRGIN.Controls;
using VRGIN.Core;
using VRGIN.Helpers;
using VRGIN.Modes;

namespace HS2VR
{
    class GenericSeatedMode : SeatedMode
    {
        protected override IEnumerable<IShortcut> CreateShortcuts()
        {
            return base.CreateShortcuts().Concat(new IShortcut[] {
                new MultiKeyboardShortcut(VR.Settings.Shortcuts.ChangeMode.GetKeyStrokes(), () => {
                    VR.Manager.SetMode<GenericStandingMode>();
                })
            });
        }

        protected override void OnStart()
        {
            VR.Camera.SteamCam.origin.transform.position = Vector3.zero;
            VR.Camera.SteamCam.origin.transform.rotation = Quaternion.identity;
            base.OnStart();

            MoveToPosition(VRPlugin.CameraResetPos, VRPlugin.CameraResetRot, false);

        }

        protected override void OnUpdate()
        {
            CheckInput();
       //     VRLog.Info($"Main Cam Height: {Camera.main.transform.position.y} VR Cam Height {VR.Camera.SteamCam.origin.position.y}");
        }

        protected void LateUpdate()
        {           
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

        /// <summary>
        /// Disables controllers for seated mode.
        /// </summary>
        protected override void CreateControllers()
        {
        }

        /// <summary>
        /// Uncomment to automatically switch into Standing Mode when controllers have been detected.
        /// </summary>
        //protected override void ChangeModeOnControllersDetected()
        //{
        //    VR.Manager.SetMode<GenericStandingMode>();
        //}

    }
}
