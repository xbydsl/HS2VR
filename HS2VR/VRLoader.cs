using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

using System.Collections;
using VRGIN.Core;
using HS2VR.Interpreters;

namespace HS2VR
{
    class VRLoader : ProtectedBehaviour
    {
        private static string DeviceOpenVR = "OpenVR";
        private static string DeviceNone = "None";

        private static bool _isVREnable = false;
        private static VRLoader _Instance;
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
        /// VRデバイスのロード。
        /// </summary>
        IEnumerator LoadDevice(string newDevice)
        {
            bool vrMode = newDevice != DeviceNone;

            // 指定されたデバイスの読み込み.
            UnityEngine.XR.XRSettings.LoadDeviceByName(newDevice);
            // 次のフレームまで待つ.
            yield return null;
            // VRモードを有効にする.
            UnityEngine.XR.XRSettings.enabled = vrMode;
            // 次のフレームまで待つ.
            yield return null;

            // デバイスの読み込みが完了するまで待つ.
            while (UnityEngine.XR.XRSettings.loadedDeviceName != newDevice || UnityEngine.XR.XRSettings.enabled != vrMode)
            {
                yield return null;
            }


            //UnityEngine.XR.XRSettings.gameViewRenderMode = UnityEngine.XR.GameViewRenderMode.BothEyes;

            List<UnityEngine.XR.XRNodeState> states = new List<UnityEngine.XR.XRNodeState>();
            UnityEngine.XR.InputTracking.GetNodeStates(states);
            foreach(UnityEngine.XR.XRNodeState state in states)
            {
                string name = UnityEngine.XR.InputTracking.GetNodeName(state.uniqueID);
                Vector3 pos = new Vector3();
                bool got_pos = state.TryGetPosition(out pos);
                VRLog.Info("XRNode {0}, position available {1} {2}", name, got_pos, pos);
            }

            if (vrMode)
            {
                VRPatcher.Patch();

                // Boot VRManager!
                // Note: Use your own implementation of GameInterpreter to gain access to a few useful operatoins
                // (e.g. characters, camera judging, colliders, etc.)
                VRManager.Create<HS2Interpreter>(CreateContext("VRContext.xml"));
                if (((HS2VRSettings)VR.Settings).DefaultMode.Equals("Seated"))
                {
                    VR.Manager.SetMode<GenericSeatedMode>();
                }
                else
                {
                    VR.Manager.SetMode<GenericStandingMode>();
                }
                
                if (((HS2VRSettings)VR.Settings).PrivacyMode.Equals(true))
                {
                    PrivacyMode.Enable();
                }
            }
        }

      

       

    }
}
