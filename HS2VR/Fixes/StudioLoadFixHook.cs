#define KKS_VRCAM
#if KKS_VRCAM
using System;
//using BepInEx.Logging;
using HarmonyLib;
using HS2VR.InterpretersStudio;
using Studio;
using VRGIN.Core;

namespace HS2VR.Fixes
{
    public static class StudioLoadFixHook
    {

        public static bool forceSetStandingMode;

        private static bool standingMode;

        public static void InstallHook()
        {
            new Harmony("HS2VRStudioNEOV2VR.LoadFixHook").PatchAll(typeof(StudioLoadFixHook));
        }

        
        [HarmonyPrefix]
        [HarmonyPatch(typeof(SceneLoadScene), "OnClickLoad", new Type[] { })]
        public static bool LoadScenePreHook(global::Studio.Studio __instance)
        {

            try
            {
                // reset standingmode if standingmode ??
                VRLog.Info("HS2VR.LoadFixHook: Start scene loading.");
                if (VRManager.Instance.Mode is GenericStandingMode) ((StudioNEOV2Interpreter)VR.Manager.Interpreter).ForceResetVRMode();
            }
            catch (Exception obj)
            {
                VRLog.Error(obj);
            }
            return true;
        }

        //[HarmonyPrefix]
        //[HarmonyPatch(typeof(Scene), "UnloadBaseScene", new Type[] { })]
        //public static void UnloadBaseScenePreHook()
        //{
        //    try
        //    {
        //        KKSCharaStudioVRPlugin.PluginLogger.Log(LogLevel.Debug, "Start Scene or Map Unloading.");
        //        if (VRManager.Instance.Mode is GenericStandingMode)
        //            standingMode = true;
        //        else
        //            standingMode = false;
        //    }
        //    catch (Exception obj)
        //    {
        //        VRLog.Error(obj);
        //    }
        //}

        // todo invalid patch target
        //[HarmonyPostfix]
        //[HarmonyPatch(typeof(Map), nameof(Map.OnLoadAfter), new Type[] { typeof(string) })]
        //public static void OnLoadAfter(Map __instance, string levelName)
        //{
        //	if (standingMode)
        //	{
        //		KKSCharaStudioVRPlugin.PluginLogger.Log(LogLevel.Debug, "Start Scene or Map Unloading.");
        //		(VR.Manager.Interpreter as KKSCharaStudioInterpreter).ForceResetVRMode();
        //	}
        //}
    }
}
#endif