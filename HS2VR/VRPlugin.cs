﻿using BepInEx;
using System;
using VRGIN.Helpers;
//using HS2VR.StudioControl;
using UnityEngine;
using VRGIN.Core;
//using HarmonyLib;
//using Obi;
//using BepInEx.Logging;
//using Studio;


namespace HS2VR
{

    /// <summary>
    /// This is an example for a VR plugin. At the same time, it also functions as a generic one.
    /// </summary>
    [BepInPlugin(GUID: "HS2VR.unofficial", Name: "HS2VR", Version: "0.9.2.0")]
    [BepInProcess("HoneySelect2")]
    [BepInProcess("StudioNEOV2")]
    public class VRPlugin : BaseUnityPlugin
    {
        public static bool VR_ACTIVATED = false;
        // todo: temporary fixes to merge code form HS2VR and KKS_VR work together
        // remove after fixing bugs


        public static VRPlugin Instance;

        public static BepInEx.Logging.ManualLogSource MessageLogger => Instance.Logger;

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
                return "0.9.2.0";
            }
        }

        public static Vector3 CameraResetPos { get; set; }
        public static Quaternion CameraResetRot { get; set; }


        /// <summary>
        /// Determines when to boot the VR code. In most cases, it makes sense to do the check as described here.
        /// </summary>
        void Awake()
        {
            Instance = this;
            CameraResetPos = Vector3.zero;
            CameraResetRot = Quaternion.identity;

            bool vrDeactivated = Environment.CommandLine.Contains("--novr");
            bool vrActivated = Environment.CommandLine.Contains("--vr");

            VRLog.Info($"Screen Size {Screen.width} x {Screen.height}");

            if (vrActivated || (!vrDeactivated && SteamVRDetector.IsRunning))
            {
                //VR_ACTIVATED = true;
                VRLoader.Create(true);
                VR_ACTIVATED = true;

                VRLog.Info("VRPlugin: setting up VRColliderHelper");
                VRColliderHelper.pluginInstance = this;
                VRColliderHelper.TriggerHelperCoroutine();

            }
            else
            {
                VRLog.Info("Not using VR");
                VR_ACTIVATED = false;
                // Don't do anything
                // VRLoader.Create(false);
            }
        }


        // sync cameras each frame
        public void Update()
        {
            if (!VR_ACTIVATED) 
                return;
            // VRLog.Info($"Main Cam: {Camera.main} VR Cam: {VRCamera.Instance.name}");

            VRPatcher.handlePOVXStatus();
            // without syncing, controller movement is possible for seating mode, but mouse is lost
            
            if (Application.productName == "StudioNEOV2" && Studio.Studio.Instance?.ociCamera != null)
            {
                VRPatcher.SyncToMainTransform(Studio.Studio.Instance.ociCamera.objectItem.transform, false);
            }
            else if (Application.productName == "StudioNEOV2" && VRManager.Instance.Mode.GetType().Equals(typeof(GenericSeatedMode)))
            {
                if (!VRPatcher.povEnabledValue)
                {
                    VRPatcher.SyncToMainTransform(Studio.Studio.Instance.cameraCtrl.transform, false);
                }

            }
            
        }
    }
}
