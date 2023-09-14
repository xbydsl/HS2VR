#define KKS_VRCAM
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;
using System.Collections;
using VRGIN.Core;
using HS2VR.InterpretersMaingame;
using HS2VR.InterpretersStudio;
using HS2VR.StudioControl;
using HS2VR.Fixes;
//using System.Dynamic;
using System.Runtime.InteropServices;
//using Valve.VR;
//using static SteamVR_Controller;

namespace HS2VR
{
    class VRLoader : ProtectedBehaviour
    {
        private static string DeviceOpenVR = "OpenVR";
        private static string DeviceNone = "None";

        private static bool _isVREnable = false;
        private static VRLoader _Instance;

        public static string currentMode = "";

        public static VRLoader Instance
        {
            get
            {
                if (_Instance == null)
                {
                    throw new InvalidOperationException("VR Loader has not been created yet!");
                }
                return _Instance;
            }
        }

        public static VRLoader Create(bool isEnable)
        {
            _isVREnable = isEnable;
            _Instance = new GameObject("VRLoader").AddComponent<VRLoader>();

            return _Instance;
        }

        protected override void OnAwake()
        {
            if (_isVREnable)
            {
                StartCoroutine(LoadDevice(DeviceOpenVR));
            }
            else
            {
                StartCoroutine(LoadDevice(DeviceNone));
            }
        }

        #region Helper code

        private IVRManagerContext CreateContext(string path)
        {
            var serializer = new XmlSerializer(typeof(ConfigurableContext));

            if (File.Exists(path))
            {
                // Attempt to load XML
                using (var file = File.OpenRead(path))
                {
                    try
                    {
                        return serializer.Deserialize(file) as ConfigurableContext;
                    }
                    catch (Exception e)
                    {
                        VRLog.Error("Failed to deserialize {0} -- using default", path);
                    }
                }
            }

            // Create and save file
            var context = new ConfigurableContext();
            try
            {
                using (var file = new StreamWriter(path))
                {
                    file.BaseStream.SetLength(0);
                    serializer.Serialize(file, context);
                }
            }
            catch (Exception e)
            {
                VRLog.Error("Failed to write {0}", path);
            }

            return context;
        }
        #endregion

        /// <summary>
        /// Load VR device
        /// </summary>
        IEnumerator LoadDevice(string newDevice)
        {
            bool vrMode = newDevice != DeviceNone;
            VRLog.Info("HS2VR.LoadDevice: Initializing the plugin...");
            // Load specified device
            UnityEngine.XR.XRSettings.LoadDeviceByName(newDevice);
            // Wait next frame
            yield return null;
            // Enable VR mode
            UnityEngine.XR.XRSettings.enabled = vrMode;
            // Wait next frame
            yield return null;

            // Wait for device to finish loading
            while (UnityEngine.XR.XRSettings.loadedDeviceName != newDevice || UnityEngine.XR.XRSettings.enabled != vrMode)
            {
                yield return null;
            }


            //UnityEngine.XR.XRSettings.gameViewRenderMode = UnityEngine.XR.GameViewRenderMode.BothEyes;

            List<UnityEngine.XR.XRNodeState> states = new List<UnityEngine.XR.XRNodeState>();
            UnityEngine.XR.InputTracking.GetNodeStates(states);
            foreach (UnityEngine.XR.XRNodeState state in states)
            {
                string name = UnityEngine.XR.InputTracking.GetNodeName(state.uniqueID);
                Vector3 pos = new Vector3();
                bool got_pos = state.TryGetPosition(out pos);
                VRLog.Info("XRNode {0}, position available {1} {2}", name, got_pos, pos);
            }

            if (vrMode)
            {

                // initializes camera patch from HS2VR
                VRLog.Info("HS2VR.VRLoader: calling Patch");
                VRPatcher.Patch();

                // will use different interpreters for Maingame(from HS2VR) or Studio(from KKS_VR)
                // todo: change to KKS_VR way of storing settings, editable from the game menu
                // VRManager.Create<StudioNEOV2Interpreter>(ConfigurableContext.GetContext());

                if (Application.productName == "StudioNEOV2")
                {
#if KKS_VRCAM
                    // fixes Studio camera position for saving thumbnail and scene loading. from KKS_VR
                    VRLog.Info("HS2VR.VRLoader: installing SaveLoadSceneHook");
                    StudioSaveLoadSceneHook.InstallHook();
                    VRLog.Info("HS2VR.VRLoader: installing LoadFixSceneHook");
                    StudioLoadFixHook.InstallHook();
                    
                    // fixes positions of Studio tool icons (?) from KKS_VR, gives errors, disabled
                    // VRLog.Info("HS2VR.VRLoader: patching TopMostIcons");
                    // TopmostToolIcons.Patch();


#endif
                    // Starts VRManager scene interpreter with XML settings from file
                    VRLog.Info("HS2VR.VRLoader: calling VRManager.Create - Studio interpreter");
                    VRManager.Create<StudioNEOV2Interpreter>(CreateContext("VRContext.xml"));
                    var obj = new GameObject("StudioNEOV2VR");
                    DontDestroyOnLoad(obj);

                    // IK tools from KKS_VR Studio
                    VRLog.Info("HS2VR.VRLoader: calling Iktool.Create");
                    IKTool.Create(obj);

                    // camera movement from KKS_VR Studio
#if KKS_VRCAM

                    VRLog.Info("HS2VR.VRLoader: installing VRCameraMoveHelper");
                    VRCameraMoveHelper.Install(obj);
#endif

                    // object movement from KKS_VR Studio
                    VRLog.Info("HS2VR.VRLoader: VRItemMoverHelper.Install");
                    VRItemObjMoveHelper.Install(obj);
#if KKS_VRCAM
                    DontDestroyOnLoad(VRCamera.Instance.gameObject);
#endif
                }
                
                // Main Game just loads VRManager without Studio-specific patches
                else
                {
                    // Starts VRManager scene interpreter with XML settings from file
                    VRLog.Info("HS2VR.VRLoader: calling VRManager.Create - Main game interpreter");
                    VRManager.Create<HS2MaingameInterpreter>(CreateContext("VRContext.xml"));
                }

                // todo: test if this works well with Studio. fixes wrong clicks on GUI window
                VRLog.Info("HS2VR.VRLoader: Initializing GraphicRaycasterPatches");
                GraphicRaycasterPatches.Initialize();

                // ?
                VRLog.Info("HS2VR.VRLoader: calling user32.dll DisableProcessWindowsGhosting");
                NativeMethods.DisableProcessWindowsGhosting();


                // code from HS2VR
                // todo: initializing mode too early may cause controllers not showing properly on scene

                if (((HS2VRSettings)VR.Settings).DefaultMode.Equals("Seated"))
                {
                    VRLoader.currentMode = "Seated";
                    VRLog.Info("HS2VR.VRLoader: Seated mode");
                    VR.Manager.SetMode<GenericSeatedMode>();
                }
                else
                {
                    VRLoader.currentMode = "Standing";
                    VRLog.Info("HS2VR.VRLoader: Standing mode");
                    VR.Manager.SetMode<GenericStandingMode>();
                }


                // turns the GUI into a black window, will disable it for Studio
                // todo: find another way to provide privacy screen ?
                // todo: enable in Maingame and test it
                if (((HS2VRSettings)VR.Settings).PrivacyMode.Equals(true) && Application.productName != "StudioNEOV2")
                {
                    VRLog.Info("HS2VR.VRLoader: calling PrivacyMode.Enable");
                    PrivacyMode.Enable();
                }


                VRLog.Info("HS2VR.VRLoader: -- end loading sequence --");
            }
        }
        private static class NativeMethods
        {
            [DllImport("user32.dll")]
            public static extern void DisableProcessWindowsGhosting();
        }
    }
}
