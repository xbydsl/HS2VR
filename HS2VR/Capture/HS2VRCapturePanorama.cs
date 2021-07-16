using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Valve.VR;
using VRGIN.Controls;
using VRGIN.Core;
using VRGIN.Helpers;

namespace HS2VR.Capture
{
    public class HS2VRCapturePanorama : ProtectedBehaviour
    {
        private IShortcut _Shortcut;

        protected override void OnStart()
        {
            _Shortcut = new MultiKeyboardShortcut(((HS2VRSettings)VR.Settings).Capture.Shortcut, delegate
            {
                VRLog.Info($"Initiating VR Screenshot Capture");             
                CaptureStart();
           
            });

            AccessTools.Field(typeof(SteamVR_SphericalProjection), "material").SetValue(null, new Material(UnityHelper.GetShader("Custom/SteamVR_SphericalProjection")));
            VRLog.Info($"Set Material {AccessTools.Field(typeof(SteamVR_SphericalProjection), "material").GetValue(null)}");

            base.OnStart();

        }

        protected void OnEnable()
        {
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();

            _Shortcut.Evaluate();
        }

        private uint ssHandle = 51001010u;
        public void CaptureStart()
        {
            var screenshotHandle = ssHandle + 1;
            var screenshotType = EVRScreenshotType.StereoPanorama;

            VRLog.Info($"Capturing Screenshot {screenshotHandle} {screenshotType}");

            if (screenshotType == EVRScreenshotType.StereoPanorama)
            {
                string timestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                string previewFilename = Application.dataPath + $"\\..\\UserData\\cap\\HS2_{timestamp}_Preview.png";
                VRLog.Info($"Using Preview File Name {previewFilename}");
                string VRFilename = Application.dataPath + $"\\..\\UserData\\cap\\HS2_{timestamp}_Stereo.png";
                VRLog.Info($"Using VR File Name {VRFilename}");

                if (previewFilename == null || VRFilename == null)
                    return;

                // Do the stereo panorama screenshot
                // Figure out where the view is
                GameObject screenshotPosition = VRCamera.Instance.SteamCam.camera.gameObject;
                SteamVR_Utils.TakeStereoScreenshot(screenshotHandle, screenshotPosition, 32, 0.064f, ref previewFilename, ref VRFilename);

                // and submit it
           //     OpenVR.Screenshots.SubmitScreenshot(screenshotHandle, screenshotType, previewFilename, VRFilename);
            }
        }
    }
}
