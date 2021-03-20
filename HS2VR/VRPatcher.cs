/* Taken from Ooetksh's Ai-Shoujo VR mod */

using System;
using System.Linq;
using System.Reflection;
using Actor;
using ADV;
using BepInEx.Harmony;
using CharaCustom;
using HarmonyLib;
using HS2;
using Manager;
using UnityEngine;
using VRGIN.Core;
using VRGIN.Modes;

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

        /*        Type povxController = AccessTools.TypeByName("HS2_PovX.Controller");
                if (povxController != null)
                {
                    povEnabled = AccessTools.Field(povxController, "povEnabled");
                    harmony.Patch(AccessTools.Method(povxController, "Update"), null, new HarmonyMethod(typeof(VRPatcher), "syncPOVXCamera"), null, null);
                } */
            }
            catch (Exception ex)
            {
                VRLog.Error(ex.ToString(), Array.Empty<object>());
            }
        }

   //     public static FieldInfo povEnabled;

   /*     public static void syncPOVXCamera()
        {
            if ((bool)povEnabled.GetValue(null))
            {
                VRPatcher.SyncToMainTransform(Camera.main.transform, false);
            }
        }*/

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
        public static void TitleSceneSetPositionPostfix(ref Camera ___mainCamera, Heroine ___heroine)
        {
            if (VRManager.Instance.Mode.GetType().Equals(typeof(GenericSeatedMode)))
            {
                ((GenericSeatedMode)VRManager.Instance.Mode).Recenter();
            }
            VRLog.Info("Setting VR Camera to game camera (Title SetPOS)");
            //VRPatcher.MoveVRCameraToMainCamera();

            if (___heroine != null)
                AdjustForFOVDifference(___mainCamera.transform, ___heroine.transform, TITLE_FOV, VR_FOV, TITLE_DISTANCE_ADJ_RATIO);

            VRPatcher.MoveVRCameraToTarget(___mainCamera.transform);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Manager.LobbySceneManager), "SetCharaAnimationAndPosition")]
        public static void LobbySceneManagerSetCameraAndCharaPositionPostfix(Camera ___cam, LobbySceneManager __instance)
        {
            VRLog.Info("Setting VR Camera to game camera (Lobby SetCamChar)");

            if (__instance.heroines != null && __instance.heroines[0] != null)
                AdjustForFOVDifference(___cam.transform, __instance.heroines[0].transform, LOBBY_FOV, VR_FOV, LOBBY_DISTANCE_ADJ_RATIO, true);

            VRPatcher.MoveVRCameraToTarget(___cam.transform);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Manager.SpecialTreatmentRoomManager), "SetCameraPosition")]
        public static void SpecialTreatmentManagerSetCameraAndCharaPositionPostfix(Camera ___cam, Manager.SpecialTreatmentRoomManager __instance)
        {
            VRLog.Info("Setting VR Camera to game camera (Special TreatmentRoom SetCameraPosition)");

            if (__instance.ConciergeHeroine != null)
                AdjustForFOVDifference(___cam.transform, __instance.ConciergeHeroine.transform, TITLE_FOV, VR_FOV, TITLE_DISTANCE_ADJ_RATIO, true);

            VRPatcher.MoveVRCameraToTarget(___cam.transform);
        }

        private const float VR_FOV = 109f;
        private const float LOBBY_FOV = 23f;
        private const float TITLE_FOV = 40f;
        private const float TITLE_DISTANCE_ADJ_RATIO = .5f;
        private const float LOBBY_DISTANCE_ADJ_RATIO = .8f;

        private static void AdjustForFOVDifference(Transform cam, Transform target, float initialFov, float targetFov, float distanceAdjustmentFactor, bool skipYAdjustment = false)
        {
            float originalY = cam.position.y + (skipYAdjustment ? 0 : (VRManager.Instance.Mode.GetType().Equals(typeof(GenericSeatedMode)) ? ((HS2VRSettings)VR.Settings).SeatedDialogueHeightAdjustment : ((HS2VRSettings)VR.Settings).StandingDialogueHeightAdjustment));
            Vector3 camPos = new Vector3(cam.position.x, 0, cam.position.z);
            Vector3 targetPos = new Vector3(target.position.x, 0, target.position.z);

            float distance = Vector3.Distance(camPos, targetPos);
            float sizeOnScreen = distance * (2f * Mathf.Tan(Mathf.Deg2Rad * (initialFov / 2f)));

            float newDistance = sizeOnScreen / (2f * Mathf.Tan(Mathf.Deg2Rad * (targetFov / 2f)));

            // Note - actual distance adjustment is a bit too close, back it off a bit
            cam.position = Vector3.MoveTowards(camPos, targetPos, (distance - newDistance) * distanceAdjustmentFactor);
            cam.position += new Vector3(0, originalY, 0);

            VRLog.Info($"Adjusting for FOV Change Distance Adj: {distance - newDistance} Corrected Distance Adj: {(distance - newDistance) * distanceAdjustmentFactor} Old Distance: {distance} New Distance: {newDistance}");
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Manager.HomeSceneManager), "SetCameraPosition")]
        public static void HomeSceneManagerSetCameraPositionPostfix()
        {
            VRLog.Info("Setting VR Camera to game camera (Home SetPOS)");
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
        [HarmonyPatch(typeof(CharaCustom.CharaCustom), "Start")]
        public static void CharaCustomStartPostfix(CustomControl ___customCtrl)
        {
            VRLog.Info("Setting VR Camera to game camera");
            if (VRManager.Instance.Mode.GetType().Equals(typeof(GenericSeatedMode)))
            {
                VRPatcher.MoveVRCameraToTarget(___customCtrl.camCtrl.transform, false);
            }
            else
            {
                VRPatcher.MoveVRCameraToMainCamera(false);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CharaCustom.CharaCustom), "OnDestroy")]
        public static void CharaCustomDestroyPostfix(CustomControl ___customCtrl)
        {
            VRLog.Info("Resetting VR Camera");
            VRManager.Instance.Mode.MoveToPosition(Vector3.zero, Quaternion.identity, false);
            if (VRManager.Instance.Mode.GetType().Equals(typeof(GenericSeatedMode)))
            {
                ((GenericSeatedMode)VRManager.Instance.Mode).Recenter();
            }
        }


        [HarmonyPostfix]
        [HarmonyPatch(typeof(TextScenario), "_RequestNextLine")]
        public static void TextScenarioRequestNextLine(TextScenario __instance, int ___currentLine)
        {
            Command command = __instance.CommandPacks[___currentLine].Command;
            VRLog.Info($"Executing Command Line: (CommandList Add - {command} {string.Join(",", __instance.CommandPacks[___currentLine].Args)} {__instance.advScene.advCamera.transform.position}");

            if (command == Command.NullSet)
            {
                if (__instance.CommandPacks[___currentLine].Args[1].Equals("Camera"))
                {
                    string camEventName = __instance.CommandPacks[___currentLine].Args[0];                    
                    GameObject camGO = GameObject.Find(camEventName);
                    Transform camTransform = camGO == null ? __instance.advScene.advCamera.transform : camGO.transform;
                    VRLog.Info($"Looking for GO {camEventName} Found: {camGO}");
                    
                    foreach (ADV.CharaData chara in __instance.commandController.Characters.Values)
                    {
                        if (chara.heroine != null)
                        {
                            VRLog.Info($"Adjusting towards: {chara.heroine.chaFile.parameter.fullname}");
                            AdjustForFOVDifference(camTransform, chara.heroine.transform, TITLE_FOV, VR_FOV, TITLE_DISTANCE_ADJ_RATIO);
                        }
                    }
                    VRLog.Info($"Setting VR Camera to game camera (CommandList Add) {camTransform}");
                    VRPatcher.MoveVRCameraToTarget(camTransform, false);
                }
            }
            else if (command == Command.CameraPositionSet)
            {
                foreach (ADV.CharaData chara in __instance.commandController.Characters.Values)
                {
                    if (chara.heroine != null)
                    {
                        VRLog.Info($"Adjusting towards: {chara.heroine.chaFile.parameter.fullname}");
                        AdjustForFOVDifference(__instance.advScene.advCamera.transform, chara.heroine.transform, TITLE_FOV, VR_FOV, TITLE_DISTANCE_ADJ_RATIO);
                    }
                }
                VRLog.Info($"Setting VR Camera to game camera (CommandList Add) {__instance.advScene.advCamera.transform.position}");
                VRPatcher.MoveVRCameraToTarget(__instance.advScene.advCamera.transform, false);
            }            
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ADVScene), "Init")]
        public static void ADVSceneInitPostfix(ADVScene __instance, TextScenario ___scenario)
        {
            VRLog.Info("Setting VR Camera to game camera (ADV INIT)");

            foreach (ADV.CharaData chara in ___scenario.commandController.Characters.Values)
            {
                if (chara.heroine != null)
                {
                    VRLog.Info($"Adjusting towards: {chara.heroine.chaFile.parameter.fullname}");
                    AdjustForFOVDifference(__instance.advCamera.transform, chara.heroine.transform, TITLE_FOV, VR_FOV, TITLE_DISTANCE_ADJ_RATIO);
                }
            }

            VRPatcher.MoveVRCameraToTarget(__instance.advCamera.transform, false);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ADVScene), "SetCamera")]
        public static void ADVSceneSetCameraPostfix(ADVScene __instance, TextScenario ___scenario)
        {
            VRLog.Info("Setting VR Camera to game camera (ADV SET CAM)");

            foreach (ADV.CharaData chara in ___scenario.commandController.Characters.Values)
            {
                if (chara.heroine != null)
                {
                    VRLog.Info($"Adjusting towards: {chara.heroine.chaFile.parameter.fullname}");
                    AdjustForFOVDifference(__instance.advCamera.transform, chara.heroine.transform, TITLE_FOV, VR_FOV, LOBBY_DISTANCE_ADJ_RATIO);
                }
            }

            VRPatcher.MoveVRCameraToTarget(__instance.advCamera.transform, false);
        }  

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Studio.CameraControl), "Import")]
        public static void ImportCameraData(Studio.CameraControl __instance)
        {
            __instance.InternalUpdateCameraState(Vector3.zero, 0);
            VRPatcher.MoveVRCameraToTarget(__instance.transform, false);
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

        [HarmonyPrefix]
        [HarmonyPatch(typeof(EyeLookController), "LateUpdate")]
        public static bool EyeLookControllerLateUpdate(EyeLookController __instance)
        {
    //        if (!VRManager.Instance.Mode.GetType().Equals(typeof(GenericSeatedMode)))
            {
                VRPatcher.MoveMainCameraToVRCamera(__instance.target);
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(NeckLookController), "LateUpdate")]
        public static bool NeckLookControllerLateUpdate(NeckLookController __instance)
        {
    //        if (!VRManager.Instance.Mode.GetType().Equals(typeof(GenericSeatedMode)))
            {
               VRPatcher.MoveMainCameraToVRCamera(__instance.target);
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(NeckLookControllerVer2), "LateUpdate")]
        public static bool NeckLookControllerVer2LateUpdate(NeckLookControllerVer2 __instance)
        {
  //          if (!VRManager.Instance.Mode.GetType().Equals(typeof(GenericSeatedMode)))
            {
               VRPatcher.MoveMainCameraToVRCamera(__instance.target);
            }
            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CameraControl_Ver2), "LateUpdate")]
        public static void CameraControlV2(CameraControl_Ver2 __instance)
        {
            if (VRManager.Instance.Mode.GetType().Equals(typeof(GenericSeatedMode)))
            {
                VRPatcher.SyncToMainTransform(__instance.transform, false);                
            }
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
            Camera main = Camera.main;
            if (main != null)
            {
                Transform transform = main.transform;
                if (transform != null)
                {
                    Transform head = VR.Camera.HeadHead;
                    if (head != null)
                    {
                        transform.SetPositionAndRotation(head.position, head.rotation);
                    }
                }
            }
        }

        public static void SyncToMainTransform(Transform target, bool positionOnly = false)
        {
            Transform origin = VR.Camera.Origin;
            Transform head = VR.Camera.Head;
            if (!positionOnly)
            {
                origin.rotation = target.rotation;

            }
            Vector3 position = target.position;
            origin.position = position - (head.position - origin.position); 
        }

        public static void MoveVRCameraToMainCamera(bool positionOnly = false)
        {
            VRPlugin.CameraResetPos = Camera.main.transform.position;
            VRPlugin.CameraResetRot = Camera.main.transform.rotation;

            VRLog.Info($"Moving VR Camera to {Camera.main.transform.position} Head Cam Y: {VR.Camera.SteamCam.head.position.y}");
            if (!positionOnly)
            {
                VRManager.Instance.Mode.MoveToPosition(Camera.main.transform.position, Camera.main.transform.rotation, false);
            }
            else
            {
                VRManager.Instance.Mode.MoveToPosition(Camera.main.transform.position, false);
            }
            VRLog.Info($"New VR Camera Pos: {VR.Camera.Origin.position}");
        }

        public static void MoveVRCameraToTarget(Transform target, bool positionOnly = false)
        {
            VRPlugin.CameraResetPos = target.position;
            VRPlugin.CameraResetRot = target.rotation;

            VRLog.Info($"Moving VR Camera to {target.position} Head Cam Y: {VR.Camera.SteamCam.head.position.y}");
            if (!positionOnly)
            {
                VRManager.Instance.Mode.MoveToPosition(target.position, target.rotation, false);
            }
            else
            {
                VRManager.Instance.Mode.MoveToPosition(target.position, false);
            }
            VRLog.Info($"New VR Camera Pos: {VR.Camera.Origin.position}");
        }
    }
}
