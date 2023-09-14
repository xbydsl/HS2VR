#define KKS_VRCAM
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
// using BepInEx.Logging;
using Manager;
using UnityEngine;
using UnityEngine.SceneManagement;
using VRGIN.Core;
using AIChara;
//using HS2VR.Fixes;


namespace HS2VR.InterpretersStudio
{
    internal class StudioNEOV2Interpreter : GameInterpreter
    {
        private List<StudioNEOV2Actor> _Actors = new List<StudioNEOV2Actor>();

#if KKS_VRCAM
        private Camera _SubCamera;
#endif
        private StudioScene studioScene;

        private int additionalCullingMask;

        private GameObject CommonSpaceGo;
                        
        public override IEnumerable<IActor> Actors => _Actors.Cast<IActor>();

        //private Fixes.Mirror.Manager _mirrorManager;

        private bool suppressCamlightShadows;
        private bool replaceCamlightWSpotLight;
        bool loaded = false;

        protected override void OnAwake()
        {
            base.OnAwake();
            suppressCamlightShadows = ((HS2VRSettings)VR.Settings).SuppressCamlightShadows;
            replaceCamlightWSpotLight = ((HS2VRSettings)VR.Settings).ReplaceCamLightWSpotLight;

            // todo: test mirror fixer
            // doesn't work. some mirrors will make all materials white. other mirrors will have incorrect reflections
            //_mirrorManager = new Fixes.Mirror.Manager();

            // _SceneType = scenes["NoScene"];
            // currentSceneInterpreter = new OtherSceneInterpreter();

            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, LoadSceneMode mode)
        {
            if (!CommonSpaceGo) CommonSpaceGo = Manager.Scene.commonSpace;
            FixMenuCanvasLayers();
            //todo test controllers not loading properly
            if  (VRLoader.currentMode == "Seated")
            {
                VRLog.Info("HS2VR.VRLoader: Seated mode");
                VR.Manager.SetMode<GenericSeatedMode>();
            }
            else
            {
                VRLoader.currentMode = "Standing";
                VRLog.Info("HS2VR.VRLoader: Standing mode");
                VR.Manager.SetMode<GenericStandingMode>();
            }

        }



        protected override void OnStart()
        {
            base.OnStart();
            studioScene = FindObjectOfType<StudioScene>();
            FixMenuCanvasLayers();
        }

#if KKS_VRCAM

        public override Camera FindCamera()
        {
            return null;
        }

        public override IActor FindNextActorToImpersonate()
        {
            var list = Actors.ToList();
            var actor = FindImpersonatedActor();
            if (actor == null) return list.FirstOrDefault();
            return list[(list.IndexOf(actor) + 1) % list.Count];
        }
#endif


        
        protected override void OnUpdate()
        {
            
            base.OnUpdate();
            // fix light position
            FixCamLight();
            if (!loaded)
            {
                StartCoroutine(FindCamlight());
                loaded = true;
            }
            try
            {
                if ((bool)VR.Manager)
                {
                    RefreshActors();

#if KKS_VRCAM
                    UpdateMainCameraCullingMask();
#endif
                    // SyncVRCameraSkybox();
                    // FixMissingMode();
                }
            }
            catch (Exception)
            {
            }
        }


#if KKS_VRCAM

        private void FixMissingMode()
        {
            if (VR.Mode == null)
            {
                VRLog.Error("VR.Mode is missing. Force reload as Standing Mode.");
                ForceResetAsStandingMode();
            }
        }

        private void SyncVRCameraSkybox()
        {
            // todo: not available on current VRGIN. apparently only changes background color
            // if ((bool)VR.Camera) VR.Camera.SyncSkybox();
        }



        // todo: find the use of culling masks in KKS_VR
        private void UpdateMainCameraCullingMask()
        {
            var component = VR.Camera.SteamCam.GetComponent<Camera>();
            var list = new List<string>();
            var obj = new List<string> { "Studio/Col", "Studio/Select" };
            if (Singleton<global::Studio.Studio>.Instance.workInfo.visibleAxis)
            {
                if (global::Studio.Studio.optionSystem.selectedState == 0) list.Add("Studio/Col");
                list.Add("Studio/Select");
            }

            var mask = LayerMask.GetMask(list.ToArray());
            var mask2 = LayerMask.GetMask(obj.ToArray());
            component.cullingMask &= ~mask2;
            component.cullingMask |= mask;
        }
#endif

        private void RefreshActors()
        {
            // added next line to fix error with RefreshActors()
            var character = new Character();
            _Actors.Clear();
            foreach (var value in character.dictEntryChara.Values)
                if ((bool)value.objBodyBone)
                    AddActor(DefaultActorBehaviour<ChaControl>.Create<StudioNEOV2Actor>(value));
        }

        private void AddActor(StudioNEOV2Actor actor)
        {
            if (!actor.Eyes)
                actor.Head.Reinitialize();
            else
                _Actors.Add(actor);
        }

#if KKS_VRCAM
        public void ForceResetVRMode()
        {
            StartCoroutine(ForceResetVRModeCo());
        }

        private IEnumerator ForceResetVRModeCo()
        {
            VRLog.Info("HS2VR.StudioControlNEOV2Interpreter.ForceResetVRmodeCo: Check and reset to StandingMode if not");
            yield return null;
            yield return null;
            yield return null;
            yield return null;
            yield return null;
            // todo enable seating mode later on
            if (!VRManager.Instance.Mode)
            {
                VRLog.Info("HS2VR.StudioControlNEOV2Interpreter.ForceResetVRmodeCo: Mode is not StandingMode. Force reset as Standing Mode.");
                ForceResetAsStandingMode();
            }
            else
            {
                VRLog.Info("HS2VR.StudioControlNEOV2Interpreter.ForceResetVRmodeCo: Mode already StandingMode. Skip.");
            }
            
        }

#endif

        public static void DisableDefaultAudioListener()
        {
            if (SingletonInitializer<Manager.Sound>.instance != null)
            {
                var componentInChildren = SingletonInitializer<Manager.Sound>.instance.transform.GetComponentInChildren<AudioListener>(true);
                if (componentInChildren != null) componentInChildren.enabled = false;
            }
        }

#if KKS_VRCAM
        public static void ForceResetAsStandingMode()
        {
            try
            {
                VR.Manager.SetMode<GenericStandingMode>();
                if ((bool)VR.Camera)
                {
                    _ = VR.Camera.Blueprint;
                    var mainCmaera = Singleton<global::Studio.Studio>.Instance.cameraCtrl.mainCmaera;
                    //VRPlugin.Logger.Log(LogLevel.Debug, $"Force replace blueprint camera with {mainCmaera}");
                    VRLog.Info($"HS2VR.StudioControlNEOV2Interpreter.ForceAsStandingMode: Force replace blueprint camera with {mainCmaera}");
                    var camera = VR.Camera.SteamCam.camera;
                    var camera2 = mainCmaera;
                    camera.nearClipPlane = VR.Context.NearClipPlane;
                    camera.farClipPlane = Mathf.Max(camera2.farClipPlane, 10f);
                    camera.clearFlags = camera2.clearFlags == CameraClearFlags.Skybox ? CameraClearFlags.Skybox : CameraClearFlags.Color;
                    camera.renderingPath = camera2.renderingPath;
                    camera.clearStencilAfterLightingPass = camera2.clearStencilAfterLightingPass;
                    camera.depthTextureMode = camera2.depthTextureMode;
                    camera.layerCullDistances = camera2.layerCullDistances;
                    camera.layerCullSpherical = camera2.layerCullSpherical;
                    camera.useOcclusionCulling = camera2.useOcclusionCulling;
                    camera.allowHDR = camera2.allowHDR;
                    camera.backgroundColor = camera2.backgroundColor;
                    var component = camera2.GetComponent<Skybox>();
                    if (component != null)
                    {
                        var skybox = camera.gameObject.GetComponent<Skybox>();
                        if (skybox == null) skybox = skybox.gameObject.AddComponent<Skybox>();
                        skybox.material = component.material;
                    }

                    VR.Camera.CopyFX(camera2);
                    
                    // todo AmplifyColorEffect not working at the moment ?
                    // var component2 = VR.Camera.gameObject.GetComponent<AmplifyColorEffect>();
                    // if ((bool)component2) component2.enabled = true;
                }
                else
                {
                    VRLog.Info("HS2VR.StudioControlNEOV2Interpreter.ForceAsStandingMode: VR.Camera is null.");
                }
            }
            catch (Exception value)
            {
                Console.WriteLine(value);
            }
        }

#endif

        // move light position behind the camera
        // code from HS2VR
        private GameObject camLight;
        private Light camLightComponent;
        public void FixCamLight()
        {
            if (camLight != null && camLight.transform != null && VR.Camera.Head != null)
            {
                camLight.transform.SetParent(VR.Camera.Head.transform);
                camLight.transform.position = VR.Camera.Head.position;
                camLight.transform.localRotation = Quaternion.identity;
            }
        }

        private IEnumerator FindCamlight()
        {
            camLight = null;
            camLightComponent = null;
            for (int i = 0; i < 10; i++)
            {
                if (camLight == null)
                    camLight = GameObject.Find("Cam Light");
                if (camLight == null)
                    camLight = GameObject.Find("Directional Light Key");
                if (camLight != null && camLightComponent == null)
                {
                    camLightComponent = camLight.GetComponent<Light>();
                    if (camLightComponent != null && suppressCamlightShadows && !replaceCamlightWSpotLight)
                    {
                        VRLog.Info($"Found Camlight {camLight} {camLightComponent}");
                        camLightComponent.shadowStrength = 0;
                        break;
                    }
                    else if (camLightComponent != null && replaceCamlightWSpotLight)
                    {
                        camLightComponent.type = LightType.Spot;
                        camLightComponent.spotAngle = 60;
                        camLightComponent.range = 500;
                        camLightComponent.shadowStrength = .8f;
                        VRLog.Info($"Found Camlight - Changed to Spot {camLight} {camLightComponent}");
                    }
                }
                // If we don't have the cam light in 10 frames it's not coming.
                yield return null;
            }

            if (camLight == null)
                VRLog.Info("No Camlight Found");
        }


        public void FixMenuCanvasLayers()
        {
            foreach (var item in ((IDictionary)typeof(VRGUI).GetField("_Registry", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(VRGUI.Instance)).Keys as
                ICollection<Canvas>)
                if (!item.transform.IsChildOf(VR.Camera.Origin.transform) && !IsIgnoredCanvas(item))
                {
                    var componentsInChildren = item.GetComponentsInChildren<Transform>(true);
                    for (var i = 0; i < componentsInChildren.Length; i++) componentsInChildren[i].gameObject.layer = LayerMask.NameToLayer(VR.Context.UILayer);
                }
        }

        public override bool IsIgnoredCanvas(Canvas canvas)
        {
            if ((bool)CommonSpaceGo && canvas.transform.IsChildOf(CommonSpaceGo.transform)) return true;
            return base.IsIgnoredCanvas(canvas);
        }

    }
}
