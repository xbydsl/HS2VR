using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VRGIN.Controls.Tools;
using VRGIN.Core;
using VRGIN.Helpers;
using static SteamVR_Controller;

namespace HS2VR
{
    public class RotationTool : Tool
    {   

        public override Texture2D Image
        {
            get
            {
                return UnityHelper.LoadImage("icon_rotation.png");
            }
        }

        protected override void OnStart()
        {
            base.OnStart();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
        }

        private Quaternion startOriginRotation;
        private Quaternion startControllerRotation;

        protected override void OnUpdate()
        {
            base.OnUpdate();
            
            if (Controller.GetPressDown(Valve.VR.EVRButtonId.k_EButton_Grip))
            {
                startControllerRotation = Owner.gameObject.transform.rotation;
                startOriginRotation = VR.Camera.SteamCam.origin.transform.rotation;
            }
            else if (Controller.GetPress(Valve.VR.EVRButtonId.k_EButton_Grip))
            {                
                var controllerDiff = Owner.gameObject.transform.rotation * Quaternion.Inverse(startControllerRotation);
                VR.Camera.SteamCam.origin.transform.rotation = startOriginRotation * controllerDiff;
                startControllerRotation = Owner.gameObject.transform.rotation;
                startOriginRotation = VR.Camera.SteamCam.origin.transform.rotation;
            }
        }
        private float NormalizeAngle(float angle)
        {
            return angle % 360f;
        }
    }
}
