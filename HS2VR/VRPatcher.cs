/* Taken from Ooetksh's Ai-Shoujo VR mod */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Actor;
using ADV;
using AIChara;
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

                Type povxController = AccessTools.TypeByName("HS2_PovX.Controller");
                if (povxController != null)
                {
                    povEnabledField = AccessTools.Field(povxController, "povEnabled");
                    enablePOVMethod = AccessTools.Method(povxController, "EnablePoV");
                    povFocusField = AccessTools.Field(povxController, "povFocus");
                    targetFocusField = AccessTools.Field(povxController, "targetFocus");
                    getValidFocusMethod = AccessTools.Method(povxController, "GetValidFocus");
                    getValidCharacterFromFocusMethod = AccessTools.Method(povxController, "GetValidCharacterFromFocus");
                    setPovCharacterMethod = AccessTools.Method(povxController, "SetPoVCharacter");
                    setTargetCharacterMethod = AccessTools.Method(povxController, "SetTargetCharacter");
                    povCharacterField = AccessTools.Field(povxController, "povCharacter");
                    harmony.Patch(AccessTools.Method(povxController, "UpdatePoVCamera"), null, new HarmonyMethod(typeof(VRPatcher), "syncPOVXCamera"), null, null);
                    harmony.Patch(AccessTools.Method(povxController, "UpdateMouseLook"), new HarmonyMethod(typeof(VRPatcher), "POVMouseLookOverride"));
                    POVAvailable = true;
                }               
            }
            catch (Exception ex)
            {
                VRLog.Error(ex.ToString(), Array.Empty<object>());
            }
            povEnabledValue = false;
        }      

        public static bool POVAvailable = false;

        private static MethodInfo enablePOVMethod;
        private static MethodInfo getValidFocusMethod;
        private static MethodInfo getValidCharacterFromFocusMethod;
        private static MethodInfo setPovCharacterMethod;
        private static MethodInfo setTargetCharacterMethod;
        private static FieldInfo povEnabledField;
        private static FieldInfo targetFocusField;
        private static FieldInfo povFocusField;
        private static FieldInfo povCharacterField;

        public static bool povEnabledValue { get; set; }
        public static bool POVPaused { get; set; }

        public static void CharaCycleKeyPress()
        {
            int focusTarget = (int)getValidFocusMethod.Invoke(null, new object[] { (int)povFocusField.GetValue(null) + 1 });
            targetFocusField.SetValue(null, focusTarget);
            povFocusField.SetValue(null, focusTarget);
            ChaControl cha = (ChaControl)getValidCharacterFromFocusMethod.Invoke(null, new object[] { focusTarget });
            setPovCharacterMethod.Invoke(null, new object[] { cha });
            setTargetCharacterMethod.Invoke(null, new object[] { cha });            
        }

        public static bool POVMouseLookOverride()
        {
            return false;
        }

        public static void LockOnCyclePress()
        {
            int focusTarget = (int)getValidFocusMethod.Invoke(null, new object[] { (int)targetFocusField.GetValue(null) + 1 });
            targetFocusField.SetValue(null, focusTarget);
            ChaControl cha = (ChaControl)getValidCharacterFromFocusMethod.Invoke(null, new object[] { focusTarget });
            setTargetCharacterMethod.Invoke(null, new object[] { cha });
        }

        public static void POVEnabledKeypress()
        {
            enablePOVMethod.Invoke(null, new object[] { !povEnabledValue });
        }

        private static Vector3 povxOriginalPosition;
        private static Quaternion povxOriginalRotation;

        public static void handlePOVXStatus()
        {
            if (povEnabledField == null)
                return;

            bool povCurrentlyEnabled = (bool)povEnabledField.GetValue(null);

            if (!povCurrentlyEnabled && povEnabledValue)
            {
                POVPaused = false;
                if (VRManager.Instance.Mode.GetType().Equals(typeof(GenericStandingMode)))
                {
                    VR.Camera.Origin.position = povxOriginalPosition;
                    VR.Camera.Origin.rotation = povxOriginalRotation;
                }
            }
            else if (povCurrentlyEnabled && !povEnabledValue)
            {
                povxOriginalPosition = VR.Camera.Origin.position;
                povxOriginalRotation = VR.Camera.Origin.rotation;
            }
            povEnabledValue = povCurrentlyEnabled;
            if (originalPOVScale == 0.0f)
                originalPOVScale = VR.Settings.IPDScale;

            ChaControl povCharacter = (ChaControl)povCharacterField.GetValue(null);
            if (povEnabledValue && !POVPaused && povCharacter != null && ((HS2VRSettings)VR.Settings).ScalePOVToImpersonatedCharacter)
            {
                // Formula is IPDScale * ( Character Height difference from 75 height * Scaling Coefficient )
                float scaleAdjustment = (FindDescendant(povCharacter.objAnim.transform, "cf_N_height").localScale.y - .95f) * ((HS2VRSettings)VR.Settings).ScalePOVToImpersonatedCharacterScaleCoeff;
                VR.Settings.IPDScale = originalPOVScale * (1 + scaleAdjustment);
            }
            else
            {
                VR.Settings.IPDScale = originalPOVScale;
            }

        }

        private static float originalPOVScale = 0.0f;

        public static void syncPOVXCamera()
        {

            handlePOVXStatus();
            if (povEnabledValue && !POVPaused)
            {
                VRPatcher.SyncToMainTransform(Camera.main.transform, positionOnly: false, adjustHead: true);
            }
        }

        // Not allowed to change FOV in VR...just, prevent it totally
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Camera), "set_fieldOfView")]
        public static bool SetFOV(Camera __instance)
        {
            if (__instance.name == "MainCamera")
                return false;
            else
                return true;
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
        public static void TitleSceneSetPositionPostfix(ref Camera ___mainCamera, Heroine ___heroine)
        {
            if (VRManager.Instance.Mode.GetType().Equals(typeof(GenericSeatedMode)))
            {
                ((GenericSeatedMode)VRManager.Instance.Mode).Recenter();
            }
            VRLog.Info("Setting VR Camera to game camera (Title SetPOS)");
            //VRPatcher.MoveVRCameraToMainCamera();

            if (___heroine != null)
                AdjustForFOVDifference(___mainCamera.transform, ___heroine.transform, TITLE_FOV, GetVRFOV(), TITLE_DISTANCE_ADJ_RATIO);

            VRPatcher.MoveVRCameraToTarget(___mainCamera.transform);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Manager.LobbySceneManager), "SetCharaAnimationAndPosition")]
        public static void LobbySceneManagerSetCameraAndCharaPositionPostfix(Camera ___cam, LobbySceneManager __instance)
        {
            VRLog.Info("Setting VR Camera to game camera (Lobby SetCamChar)");

            if (__instance.heroines != null && __instance.heroines[0] != null)
                AdjustForFOVDifference(___cam.transform, __instance.heroines[0].transform, LOBBY_FOV, GetVRFOV(), LOBBY_DISTANCE_ADJ_RATIO, true);

            VRPatcher.MoveVRCameraToTarget(___cam.transform);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Manager.SpecialTreatmentRoomManager), "SetCameraPosition")]
        public static void SpecialTreatmentManagerSetCameraAndCharaPositionPostfix(Camera ___cam, Manager.SpecialTreatmentRoomManager __instance)
        {
            VRLog.Info("Setting VR Camera to game camera (Special TreatmentRoom SetCameraPosition)");

            if (__instance.ConciergeHeroine != null)
                AdjustForFOVDifference(___cam.transform, __instance.ConciergeHeroine.transform, TITLE_FOV, GetVRFOV(), TITLE_DISTANCE_ADJ_RATIO, true);

            VRPatcher.MoveVRCameraToTarget(___cam.transform);
        }

        private const float LOBBY_FOV = 23f;
        private const float TITLE_FOV = 40f;
        private const float TITLE_DISTANCE_ADJ_RATIO = .5f;
        private const float LOBBY_DISTANCE_ADJ_RATIO = .8f;

        private static float GetVRFOV()
        {
            return VR.Camera.SteamCam.camera.fieldOfView;
        }

        private static float AdjustForFOVDifference(Transform cam, Transform target, float initialFov, float targetFov, float distanceAdjustmentFactor, bool skipYAdjustment = false)
        {
            float originalY = cam.position.y + (skipYAdjustment ? 0 : (VRManager.Instance.Mode.GetType().Equals(typeof(GenericSeatedMode)) ? ((HS2VRSettings)VR.Settings).SeatedDialogueHeightAdjustment : ((HS2VRSettings)VR.Settings).StandingDialogueHeightAdjustment));
            Vector3 camPos = new Vector3(cam.position.x, 0, cam.position.z);
            Vector3 targetPos = new Vector3(target.position.x, 0, target.position.z);

            float distance = Vector3.Distance(camPos, targetPos);
            float sizeOnScreen = distance * (2f * Mathf.Tan(Mathf.Deg2Rad * (initialFov / 2f)));

            float newDistance = sizeOnScreen / (2f * Mathf.Tan(Mathf.Deg2Rad * (targetFov / 2f)));

            // Note - actual distance adjustment is a bit too close, back it off a bit
            float moveTowardsDistance = (distance - newDistance) * distanceAdjustmentFactor;
            cam.position = Vector3.MoveTowards(camPos, targetPos, moveTowardsDistance);
            cam.position += new Vector3(0, originalY, 0);

            VRLog.Info($"Adjusting for FOV Change Distance Adj: {distance - newDistance} Corrected Distance Adj: {moveTowardsDistance} Old Distance: {distance} New Distance: {newDistance}");
            return moveTowardsDistance;
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
                            AdjustForFOVDifference(camTransform, chara.heroine.transform, TITLE_FOV, GetVRFOV(), TITLE_DISTANCE_ADJ_RATIO);
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
                        AdjustForFOVDifference(__instance.advScene.advCamera.transform, chara.heroine.transform, TITLE_FOV, GetVRFOV(), TITLE_DISTANCE_ADJ_RATIO);
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
                    AdjustForFOVDifference(__instance.advCamera.transform, chara.heroine.transform, TITLE_FOV, GetVRFOV(), TITLE_DISTANCE_ADJ_RATIO);
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
                    AdjustForFOVDifference(__instance.advCamera.transform, chara.heroine.transform, TITLE_FOV, GetVRFOV(), LOBBY_DISTANCE_ADJ_RATIO);
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

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Studio.CameraControl), "Export")]
        public static void ExportCameraData(Studio.CameraControl __instance)
        {
            if (VRManager.Instance.Mode.GetType().Equals(typeof(GenericStandingMode)))
                __instance.SetCamera(VR.Camera.HeadHead.transform.position, VR.Camera.HeadHead.transform.rotation.eulerAngles, VR.Camera.HeadHead.transform.rotation, Vector3.zero);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(GlobalMethod), "loadCamera")]
        public static void GlobalMethodloadCamerayPostfix(CameraControl_Ver2 _ctrl)
        {
            ChaControl heroine = HSceneManager.Instance?.females?[0];
            if (heroine != null)
            {
                VRLog.Info($"Adjusting towards: {heroine.chaFile.parameter.fullname}");
                float moveDistance = AdjustForFOVDifference(_ctrl.transform, heroine.transform, TITLE_FOV, GetVRFOV(), LOBBY_DISTANCE_ADJ_RATIO, true);
                _ctrl.TargetPos = _ctrl.transform.InverseTransformPoint(_ctrl.transform.position);
                _ctrl.CameraDir = Vector3.MoveTowards(_ctrl.CameraDir, _ctrl.TargetPos, moveDistance);
            }

            VRLog.Info("Setting VR Camera to game camera");            
            VRPatcher.MoveVRCameraToMainCamera();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(GlobalMethod), "loadResetCamera")]
        public static void GlobalMethodloadResetCamerayPostfix(CameraControl_Ver2 _ctrl)
        {
            ChaControl heroine = HSceneManager.Instance?.females?[0];
            if (heroine != null)
            {
                VRLog.Info($"Adjusting towards: {heroine.chaFile.parameter.fullname}");
                float moveDistance = AdjustForFOVDifference(_ctrl.transform, heroine.transform, TITLE_FOV, GetVRFOV(), LOBBY_DISTANCE_ADJ_RATIO, true);
                _ctrl.TargetPos = _ctrl.transform.InverseTransformPoint(_ctrl.transform.position);
                _ctrl.CameraDir = Vector3.MoveTowards(_ctrl.CameraDir, _ctrl.TargetPos, moveDistance);
            }

            VRLog.Info("Setting VR Camera to game camera");
            VRPatcher.MoveVRCameraToMainCamera();
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(EyeLookController), "LateUpdate")]
        public static bool EyeLookControllerLateUpdate(EyeLookController __instance)
        {
            if (__instance.ptnNo == 1 || __instance.ptnNo == 2)
            {
                __instance.target = VR.Camera.Head;
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(NeckLookControllerVer2), "LateUpdate")]
        public static bool NeckLookControllerVer2LateUpdate(NeckLookControllerVer2 __instance)
        {
            if (__instance.ptnNo == 1 || __instance.ptnNo == 2)                
                __instance.target = VR.Camera.Head; 

            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CameraControl_Ver2), "LateUpdate")]
        public static void CameraControlV2(CameraControl_Ver2 __instance)
        {
            if (povEnabledValue)
            {
                return;
            }

            if (VRManager.Instance.Mode.GetType().Equals(typeof(GenericSeatedMode)))
            {
                VRPatcher.SyncToMainTransform(__instance.transform, false);                
            }
        }

        private static void MoveMainCameraToVRCamera()
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

        public static void SyncToMainTransform(Transform target, bool positionOnly = false, bool adjustHead = false)
        {
            Transform origin = VR.Camera.Origin;
            Transform head = VR.Camera.Head;
            if (!positionOnly)
            {
                origin.rotation = target.rotation;

            }
            Vector3 position = target.position;
            if (adjustHead)
                origin.position = position - (head.position - origin.position);
            else
                origin.position = position;
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

        public static Transform FindDescendant(Transform start, string name)
        {
            if (start == null)
            {
                return null;
            }

            if (start.name.Equals(name))
                return start;
            foreach (Transform t in start)
            {
                Transform res = FindDescendant(t, name);
                if (res != null)
                    return res;
            }
            return null;
        }
    }
}
