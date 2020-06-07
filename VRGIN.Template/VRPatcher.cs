/* Taken from Ooetksh's Ai-Shoujo VR mod */

using System;
using BepInEx.Harmony;
using HarmonyLib;
using Manager;
using UnityEngine;
using VRGIN.Core;

namespace HS2VR
{
	public static class VRPatcher
	{
		public static void Patch()
		{
			try
			{
                var harmony = new Harmony("com.killmar.HS2VR");
				harmony.PatchAll(typeof(VRPatcher));
			}
			catch (Exception ex)
			{
				VRLog.Error(ex.ToString(), Array.Empty<object>());
			}
		}

        [HarmonyPrefix]
		[HarmonyPatch(typeof(LogoScene), "Start")]
		public static bool NoWaitOnLogo(ref float ___waitTime)
		{
            VRLog.Info("Setting Logo waitTime to 0.");
            ___waitTime = 0.0f;
			return true;
		}

		// [HarmonyPostfix]
		// [HarmonyPatch(typeof(StartEventADV), "OnStart")]
		// public static void StartEventADV_OnStart(StartEventADV __instance)
		// {
		// 	try
		// 	{
		// 		VRPatcher.<>c__DisplayClass1_0 CS$<>8__locals1 = new VRPatcher.<>c__DisplayClass1_0();
		// 		CS$<>8__locals1.actorCameraControl = Singleton<Map>.Instance.Player.CameraControl;
		// 		if (CS$<>8__locals1.actorCameraControl != null)
		// 		{
		// 			CS$<>8__locals1.actorCameraControl.StartCoroutine(CS$<>8__locals1.<StartEventADV_OnStart>g__ForceOnCameraBlended|0());
		// 		}
		// 	}
		// 	catch (Exception ex)
		// 	{
		// 		VRLog.Error(ex.ToString(), Array.Empty<object>());
		// 	}
		// }

		[HarmonyPrefix]
		[HarmonyPatch(typeof(EyeLookController), "LateUpdate")]
		public static bool EyeLookControllerLateUpdate(EyeLookController __instance)
		{
			VRPatcher.MoveMainCameraToVRCamera(__instance.target);
			return true;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(NeckLookController), "LateUpdate")]
		public static bool NeckLookControllerLateUpdate(NeckLookController __instance)
		{
			VRPatcher.MoveMainCameraToVRCamera(__instance.target);
			return true;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(NeckLookControllerVer2), "LateUpdate")]
		public static bool NeckLookControllerVer2LateUpdate(NeckLookControllerVer2 __instance)
		{
			VRPatcher.MoveMainCameraToVRCamera(__instance.target);
			return true;
		}


		[HarmonyPrefix]
		[HarmonyPatch(typeof(HMotionEyeNeckFemale), "SetBehaviourNeck")]
		public static bool SetBehaviourNeck(HMotionEyeNeckFemale __instance, ref int _behaviour)
		{
			if ((Manager.Config.HData.NeckDir0 || Manager.Config.HData.NeckDir1) && _behaviour == 2)
			{
				_behaviour = 1;
			}
			return true;
		}

		private static void MoveMainCameraToVRCamera(Transform target)
		{
			try
			{
				Camera main = Camera.main;
				if (target == ((main != null) ? main.transform : null))
				{
					Camera main2 = Camera.main;
					Transform transform = (main2 != null) ? main2.transform : null;
					if (transform != null)
					{
						Transform head = VR.Camera.Head;
						if (head != null)
						{
							transform.SetPositionAndRotation(head.position, head.rotation);
						}
					}
				}
			}
			catch (Exception ex)
			{
				VRLog.Error(ex.ToString(), Array.Empty<object>());
			}
		}
	}
}
