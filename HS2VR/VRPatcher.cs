/* Taken from Ooetksh's Ai-Shoujo VR mod */

using System;
using BepInEx.Harmony;
using HarmonyLib;
using HS2;
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

        [HarmonyPostfix]
		[HarmonyPatch(typeof(HS2.TitleScene), "SetPosition")]
		public static void TitleSceneSetPositionPostfix(ref Camera ___mainCamera)
		{
            VRLog.Info("Setting VR Camera to game camera");
            VRPatcher.MoveVRCameraToMainCamera(true);
			//VRPatcher.MoveVRCameraToTarget(___mainCamera.transform);
		}

        [HarmonyPostfix]
		[HarmonyPatch(typeof(Manager.LobbySceneManager), "SetCameraPosition")]
		public static void LobbySceneManagerSetCameraPositionPostfix()
		{
            VRLog.Info("Setting VR Camera to game camera");
            VRPatcher.MoveVRCameraToMainCamera();
		}

        [HarmonyPostfix]
		[HarmonyPatch(typeof(Manager.LobbySceneManager), "SetCameraAndCharaPosition")]
		public static void LobbySceneManagerSetCameraAndCharaPositionPostfix()
		{
            VRLog.Info("Setting VR Camera to game camera");
            VRPatcher.MoveVRCameraToMainCamera();
		}

        [HarmonyPostfix]
		[HarmonyPatch(typeof(Manager.HomeSceneManager), "SetCameraPosition")]
		public static void HomeSceneManagerSetCameraPositionPostfix()
		{
            VRLog.Info("Setting VR Camera to game camera");
            VRPatcher.MoveVRCameraToMainCamera();
		}

        [HarmonyPostfix]
		[HarmonyPatch(typeof(BaseCameraControl_Ver2), "Reset")]
		public static void BaseCameraControl_Ver2ResetPostfix()
		{
            VRLog.Info("Setting VR Camera to game camera");
            VRPatcher.MoveVRCameraToMainCamera();
		}

        [HarmonyPostfix]
		[HarmonyPatch(typeof(GlobalMethod), "loadCamera")]
		public static void GlobalMethodloadCamerayPostfix()
		{
            VRLog.Info("Setting VR Camera to game camera");
            VRPatcher.MoveVRCameraToMainCamera();
		}

        [HarmonyPostfix]
		[HarmonyPatch(typeof(GlobalMethod), "loadResetCamera")]
		public static void GlobalMethodloadResetCamerayPostfix()
		{
            VRLog.Info("Setting VR Camera to game camera");
            VRPatcher.MoveVRCameraToMainCamera();
		}

		[HarmonyPostfix]
		[HarmonyPatch(typeof(TitleScene), "LateUpdate")]
		public static bool TitleSceneLateUpdate(EyeLookController __instance)
		{
			VRLog.Info("Setting VR Camera to game camera");
            VRPatcher.MoveVRCameraToMainCamera();
			return true;
		}

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
				if (main != null)
				{
					Transform transform = main.transform;
					if (transform != null)
					{
						Transform head = VR.Camera.HeadHead;
						if (head != null)
						{
							//VRLog.Info("Before movetoVR Main: {0}, VR E: {1}", transform.position, head.position);
							transform.SetPositionAndRotation(head.position, head.rotation);
							//VRLog.Info("After movetoVR Main: {0}, VR E: {1}", transform.position, head.position);
						}
					}
				}
			}
			catch (Exception ex)
			{
				VRLog.Error(ex.ToString(), Array.Empty<object>());
			}
		}

		private static void MoveVRCameraToMainCamera(bool remove_head_tracking = false)
		{
			Camera main_camera = Camera.main;
			if(main_camera != null)
            {
				if(remove_head_tracking)
				{
					// Unity XR already tracks the VR headset and applies that to the main camera 
					// This can probably be solved nicer
					Transform origin = main_camera.transform;
					Vector3 head_pos = UnityEngine.XR.InputTracking.GetLocalPosition(UnityEngine.XR.XRNode.CenterEye);
					//VRLog.Info("headpos: {0}", head_pos);
					origin.position -= head_pos*VR.Camera.Origin.localScale.x;
					// Probaly should also remove head rotation
					MoveVRCameraToTarget(origin);
				}
				else
				{
					MoveVRCameraToTarget(main_camera.transform);
				}
			}
		}
        private static void MoveVRCameraToTarget(Transform target)
		{
			Transform origin = VR.Camera.Origin;
			Transform head = VR.Camera.Head;
			// Account for IPD (origin.localScale)

			//VRLog.Info("Before Main: {0}, VR O: {1}, VR E: {2}", target.position, origin.position, head.position);

			// origin.rotation = main_camera.transform.rotation;
			Vector3 forward_horizontal = Vector3.ProjectOnPlane(target.forward, Vector3.up).normalized;
			Vector3 VR_forward_horizontal = Vector3.ProjectOnPlane(head.forward, Vector3.up).normalized;
			float rot = -Vector3.Angle(forward_horizontal, VR_forward_horizontal);
			origin.Rotate(Vector3.up * rot);
			// origin.rotation = main_camera.transform.rotation;
			// float rot = (headHead.rotation.eulerAngles.y - main_camera.transform.eulerAngles.y);
			// origin.Rotate(Vector3.up * rot);

			Vector3 position = target.position;
			origin.position = position - (head.position - origin.position);
			// Vector3 translation = Vector3.ProjectOnPlane(target.position - head.position, Vector3.up);
			// origin.position += translation;
			//VRLog.Info("After Main: {0}, VR O: {1}, VR E: {2}", target.position, origin.position, head.position);
		}
	}
}
