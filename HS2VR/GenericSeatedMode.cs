#define SEATED_CONTROLLERS
using HS2VR.StudioControl;
using System;
using System.Collections.Generic;
using System.Linq;
//using System.Text;
using UnityEngine;
//using Valve.VR;
using VRGIN.Controls;
using VRGIN.Controls.Tools;
using VRGIN.Core;
using VRGIN.Helpers;
using VRGIN.Modes;



namespace HS2VR
{
    class GenericSeatedMode : SeatedMode
    {
        //creates keyboard shortcuts

        protected bool firstTimeInit = true;
        protected override IEnumerable<IShortcut> CreateShortcuts()
        {
            return base.CreateShortcuts().Concat(new IShortcut[] {
                new MultiKeyboardShortcut(VR.Settings.Shortcuts.ChangeMode.GetKeyStrokes(), () => {
                    VRLoader.currentMode = "Standing";
                    VR.Manager.SetMode<GenericStandingMode>();
                    // StudioTool.CheckHandlers();
                }),
                new MultiKeyboardShortcut(((HS2VRSettings)VR.Settings).HS2Shortcuts.SuspendPOVToggle.GetKeyStrokes(), () =>
                {
                    VRPatcher.POVPaused = !VRPatcher.POVPaused;
                    VRPatcher.POVPaused = true;
                })
            });
        }




        // camera initalization > 0,0,0
        protected override void OnStart()
        {
            VR.Camera.SteamCam.origin.transform.position = Vector3.zero;
            VR.Camera.SteamCam.origin.transform.rotation = Quaternion.identity;
            base.OnStart();
            MoveToPosition(VRPlugin.CameraResetPos, VRPlugin.CameraResetRot, false);
        }
        
        
        //  in KKS_VR CheckInput() was called from the Tool, in HS2VR from Standing/Seated modes
        //  apparently if it's called twice it generates errors
        protected override void OnUpdate()
        
        {
            CheckInput();
        }

        protected void LateUpdate()
        {
        }
       

        //not sure what is the difference with VRGIN code (VR.Camera vs VR.Camera.SteamCam)
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
        protected override void CreateControllers()
        {
            // todo: make XML setting for seated mode with controllers in Studio
            if (Application.productName == "StudioNEOV2")
            {
                base.CreateControllers();
            }
        }

        /// <summary>
        /// Uncomment to automatically switch into Standing Mode when controllers have been detected.
        /// </summary>
        //protected override void ChangeModeOnControllersDetected()
        //{
        //    VR.Manager.SetMode<GenericStandingMode>();
        //}


#if SEATED_CONTROLLERS
        private void BuildTool(string tool, List<Type> toolList)
        {
            if (Application.productName == "StudioNEOV2")
            // do not load Tools on first time on Seated Mode
            // to try avoiding errors
            {
                if (firstTimeInit == false)
                {
                    toolList.Add(typeof(StudioControlTool));
                }
                else
                {
                    firstTimeInit = false;
                }
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
#endif
    }
}
