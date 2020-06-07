using BepInEx;
using System;
using VRGIN.Helpers;

namespace HS2VR
{

    /// <summary>
    /// This is an example for a VR plugin. At the same time, it also functions as a generic one.
    /// </summary>
    [BepInPlugin(GUID: "HS2VR.unofficial", Name: "HS2VR", Version: "0.0.0.1")]
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
                return "0.0.0.1";
            }
        }

        /// <summary>
        /// Determines when to boot the VR code. In most cases, it makes sense to do the check as described here.
        /// </summary>
        void Awake()
        {
            VRPatcher.Patch();

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
