using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VRGIN.Controls.Tools;
using VRGIN.Core;
using VRGIN.Helpers;
using VRGIN.Template;

namespace HS2VR
{
    public class POVTool : Tool
    {
        public override Texture2D Image
        {
            get
            {
                return UnityHelper.LoadImage("icon_eye.png");
            }
        }

        private float triggerStartTime = 0f;
        protected override void OnUpdate()
        {


            base.OnUpdate();

            foreach(Valve.VR.EVRButtonId eVRButton in Enum.GetValues(typeof(Valve.VR.EVRButtonId)))
            {
                if (Controller.GetPressDown(eVRButton))
                {
                    VRLog.Info($"Button: {eVRButton.ToString()}");
                }
            }
            if (Controller.GetPressDown(Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger))
            {
                triggerStartTime = Time.unscaledTime;
            }
            if (Controller.GetPressUp(Valve.VR.EVRButtonId.k_EButton_Dashboard_Back))
            {
                VRPatcher.POVEnabledKeypress();
            }
            if (Controller.GetPressUp(Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger))
            {
                if (Time.unscaledTime - triggerStartTime > 1.5f)
                {
                    VRPatcher.POVPaused = !VRPatcher.POVPaused;
                    triggerStartTime = 0f;
                }
                else
                {
                    VRPatcher.CharaCycleKeyPress();
                    triggerStartTime = 0f;
                }                
            }
            if (Controller.GetPressUp(Valve.VR.EVRButtonId.k_EButton_SteamVR_Touchpad))
            {
                VRPatcher.LockOnCyclePress();
            }
        }
    }
}
