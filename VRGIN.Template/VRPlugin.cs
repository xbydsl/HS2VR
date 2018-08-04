using BepInEx;
using System;
using VRGIN.Helpers;

namespace KoikatuVR
{

    /// <summary>
    /// This is an example for a VR plugin. At the same time, it also functions as a generic one.
    /// </summary>
    [BepInPlugin(GUID: "KoikatsuVR.unofficial", Name: "KoikatuVR", Version: "0.7.1")]
    public class VRPlugin : BaseUnityPlugin
    {

        /// <summary>
        /// Put the name of your plugin here.
        /// </summary>
        public string Name
        {
            get
            {
                return "KoikatuVR";
            }
        }

        public string Version
        {
            get
            {
                return "0.7.1";
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
				VRLoader.Create(false);
            }
        }
    }
}
