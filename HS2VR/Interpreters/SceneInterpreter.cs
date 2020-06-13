using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

using VRGIN.Core;

namespace HS2VR.Interpreters
{
    abstract class SceneInterpreter
    {
        public virtual void OnStart()
        {
        }
        public virtual void OnEnable()
        {
            //VRLog.Info("Adding OnSceneLoaded hook.");
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        public virtual void OnDisable()
        {
            //VRLog.Info("Removing OnSceneLoaded hook.");
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        protected void OnSceneLoaded(Scene scene, LoadSceneMode sceneMode)
		{
            VRLog.Info("Loaded scene {0}, mode {1}", scene.name, sceneMode);
		}
        public virtual void OnUpdate()
        {
        }
    }
}
