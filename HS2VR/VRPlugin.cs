using BepInEx;
using System;
using VRGIN.Helpers;

using UnityEngine;
using VRGIN.Core;
namespace HS2VR
{

    /// <summary>
    /// This is an example for a VR plugin. At the same time, it also functions as a generic one.
    /// </summary>
    [BepInPlugin(GUID: "HS2VR.unofficial", Name: "HS2VR", Version: "0.0.0.2")]
    public class VRPlugin : BaseUnityPlugin
    {

        /// <summary>
        /// Put the name of your plugin here.
        /// </summary>
        public string Name
        {
            get
            {
                return "HS2VR";
            }
        }

        public string Version
        {
            get
            {
                return "0.0.0.2";
            }
        }

        /// <summary>
        /// Determines when to boot the VR code. In most cases, it makes sense to do the check as described here.
        /// </summary>
        void Awake()
        {
            bool vrDeactivated = Environment.CommandLine.Contains("--novr");
            bool vrActivated = Environment.CommandLine.Contains("--vr");
            
            if (vrActivated || (!vrDeactivated && SteamVRDetector.IsRunning))
            {
                VRLoader.Create(true);
            }
            else
            {
                VRLog.Info("Not using VR");
                // Don't do anything
                //VRLoader.Create(false);
            }
        }

        public void Update()
        {
            // Vector3 head_pos = UnityEngine.XR.InputTracking.GetLocalPosition(UnityEngine.XR.XRNode.CenterEye);
            // VRLog.Info("XRNode.CenterEye: {0}", head_pos);
            // foreach (Camera camera in Camera.allCameras)
            // {
            //     VRLog.Info("Camera {0}: {1}", camera.name, camera.transform.position);
            //     //UnityEngine.XR.XRDevice.DisableAutoXRCameraTracking(camera, true);
            // }
        }
    }
}
