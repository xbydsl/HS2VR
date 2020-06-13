using UnityEngine;
using VRGIN.Core;
using VRGIN.Controls;
using VRGIN.Template;
using Valve.VR;


namespace HS2VR.Interpreters
{
    class HSceneInterpreter : SceneInterpreter
    {
        private bool _NeedsResetCamera;

        public HScene _HScene { get; private set; }


        public override void OnStart()
        {
            base.OnStart();

            _HScene = GameObject.FindObjectOfType<HScene>();
            //VRLog.Info("Got HScene object: {0}", _HScene != null);
            VRLog.Info("Starting HSceneInterpreter.");
        }

        public override void OnDisable()
        {
            base.OnDisable();
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            // Vector2 touchpad_direction = VR.Controller.Right.Input.GetAxis(EVRButtonId.k_EButton_Axis0);
            // //VRLog.Info("Touchpad direction: {0}", touchpad_direction);
            // if(touchpad_direction.y > 0.5f) {
            //     // Up
            //    VRLog.Info("Scroll up.");
            //    SendInputHandler.MouseWheel(10);
            // }
            // else if(touchpad_direction.y < -0.5f) {
            //     // Down
            //     VRLog.Info("Scroll down.");
            //     SendInputHandler.MouseWheel(-10);
            // }

            // if(VR.Controller.Right.Input.GetPressDown(EVRButtonId.k_EButton_SteamVR_Touchpad)) {
            //     VRLog.Info("Pressed touchpad.");
            //     _HScene.ctrlFlag.click = HSceneFlagCtrl.ClickKind.FinishBefore;
            // }
            

            // if (_NeedsResetCamera)
            // {
            //     ResetCamera();
            // }
        }

        private void ResetCamera()
        {
            VRLog.Info("HScene ResetCamera");

            // var cam = GameObject.FindObjectOfType<CameraControl_Ver2>();

            // if (cam != null)
            // {
            //     cam.enabled = false;
            //     _NeedsResetCamera = false;

            //     VRLog.Info("succeeded");
            // }
        }
    }
}
