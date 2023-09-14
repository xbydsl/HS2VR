using System;
using HarmonyLib;
using Unity.XR.OpenVR;

namespace HS2VR.Fixes
{
    /// <summary>
    /// No idea what exactly it does but it doesn't seem to hurt anything. Originally a part of KKS_CharaStudioVR
    /// It doesn't work with HS2 version of VIRGN (not openXR)
    /// </summary>
    public static class OpenVRHelperTempfixHook
    {
        public static void Patch()
        {
            new Harmony("OpenVRHelperTempfixHook").PatchAll(typeof(OpenVRHelperTempfixHook));
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(OpenVRHelpers), "IsUsingSteamVRInput", new Type[] { })]
        public static bool PreIsUsingSteamVRInput(ref bool __result)
        {
            __result = true;
            return false;
        }
    }
}
