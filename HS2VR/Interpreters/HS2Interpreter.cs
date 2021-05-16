using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PostProcessing;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;
using VRGIN.Core;

namespace HS2VR.Interpreters
{
    class HS2Interpreter : GameInterpreter
    {
        public Dictionary<string, int> scenes = new Dictionary<string, int>()
                                            {
                                                {"NoScene", -1},
                                                {"Init", 0},
                                                {"Logo", 1},
                                                {"Title", 2},
                                                {"CharaCustom", 3},
                                                {"Home", 4},
                                                {"Select", 5},
                                                {"ADV", 6},
                                                {"HScene", 7},
                                                {"LobbyScene", 8},
                                                {"FursRoom", 9},
                                                {"Uploader", 10},
                                                {"Downloader", 11},
                                                {"EntryHandleName", 12},
                                                {"NetworkCheckScene", 13},
                                                {"CharaSearch", 14},
                                                {"Other", -2},
                                            };


        private int _SceneType;
        public SceneInterpreter currentSceneInterpreter;

        private bool suppressCamlightShadows;
        private bool replaceCamlightWSpotLight;
        bool loaded = false;

        protected override void OnAwake()
        {
            base.OnAwake();

            suppressCamlightShadows = ((HS2VRSettings)VR.Settings).SuppressCamlightShadows;
            replaceCamlightWSpotLight = ((HS2VRSettings)VR.Settings).ReplaceCamLightWSpotLight;

            _SceneType = scenes["NoScene"];
            currentSceneInterpreter = new OtherSceneInterpreter();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        protected void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();

            DetectScene();
            currentSceneInterpreter.OnUpdate();
            FixCamLight();
            if (!loaded)
            {
                StartCoroutine(FindCamlight());
                loaded = true;
            }
        }

        public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {            
            VRLog.Info($"Entering Scene {scene.name} Mode {mode}");
            StartCoroutine(FindCamlight());
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

        public bool isHScene { 
            get { return _SceneType == scenes["HScene"]; }
        }

        private void DetectScene()
        {
            int nextSceneType = _SceneType;
            SceneInterpreter nextInterpreter = null;


            if (GameObject.Find("HScene") != null)
            {
                if (_SceneType != scenes["HScene"])
                {
                    nextSceneType = scenes["HScene"];
                    nextInterpreter = new HSceneInterpreter();
                    VRLog.Info("Start HScene");
                }
            }
            else if (GameObject.Find("ADV") != null)
            {
                if (_SceneType != scenes["ADV"])
                {
                    nextSceneType = scenes["ADV"];
                    nextInterpreter = new OtherSceneInterpreter();
                    VRLog.Info("Start ADV");
                }
            }
            else if (GameObject.Find("Select") != null)
            {
                if (_SceneType != scenes["Select"])
                {
                    nextSceneType = scenes["Select"];
                    nextInterpreter = new OtherSceneInterpreter();
                    VRLog.Info("Start Select");
                }
            }
            else if (GameObject.Find("Home") != null)
            {
                if (_SceneType != scenes["Home"])
                {
                    nextSceneType = scenes["Home"];
                    nextInterpreter = new OtherSceneInterpreter();
                    VRLog.Info("Start Home");
                }
            }
            else if (GameObject.Find("LobbyScene") != null)
            {
                if (_SceneType != scenes["LobbyScene"])
                {
                    nextSceneType = scenes["LobbyScene"];
                    nextInterpreter = new OtherSceneInterpreter();
                    VRLog.Info("Start LobbyScene");
                }
            }

            else
            {
                if (_SceneType != scenes["Other"])
                {
                    nextSceneType = scenes["Other"];
                    nextInterpreter = new OtherSceneInterpreter();
                    VRLog.Info("Start OtherScene");
                }
            }

            if (nextSceneType != _SceneType)
            {
                VRLog.Info("Changing scenes.");
                currentSceneInterpreter.OnDisable();

                _SceneType = nextSceneType;
                currentSceneInterpreter = nextInterpreter;
                currentSceneInterpreter.OnStart();
                currentSceneInterpreter.OnEnable();
            }
            
        }
    }
}
