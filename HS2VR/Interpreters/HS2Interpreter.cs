using System.Collections.Generic;
using UnityEngine;
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

        protected override void OnAwake()
        {
            base.OnAwake();

            _SceneType = scenes["NoScene"];
            currentSceneInterpreter = new OtherSceneInterpreter();
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();

            DetectScene();
            currentSceneInterpreter.OnUpdate();
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
