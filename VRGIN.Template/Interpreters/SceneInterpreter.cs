using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

using VRGIN.Core;

namespace HS2VR.Interpreters
{
    abstract class SceneInterpreter
    {
        private bool _reset_VR_camera; 
        private int _camera_movements;
        private HashSet<Camera> _main_cameras = new HashSet<Camera>();
        public virtual void OnStart()
        {
            _reset_VR_camera = true;
            _camera_movements = 0;
        }
        public virtual void OnEnable()
        {
            VRLog.Info("Adding OnSceneLoaded hook.");
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        public virtual void OnDisable()
        {
            VRLog.Info("Removing OnSceneLoaded hook.");
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        protected void OnSceneLoaded(Scene scene, LoadSceneMode sceneMode)
		{
            VRLog.Info("Loaded scene {0}, mode {1}", scene.name, sceneMode);
			this._main_cameras.Clear();
		}
        public virtual void OnUpdate()
        {
            foreach (Camera camera in Camera.allCameras.Except(_main_cameras).ToList<Camera>())
			{
                if(camera.CompareTag("MainCamera"))
                {
                    VRLog.Info("Main camera has changed");
                    _main_cameras.Add(camera);
                    _reset_VR_camera = true;
                    _camera_movements = 0;
                }
            }
			
            Camera main_camera = Camera.main;
            if(main_camera != null)
            {
                // Hack to find starting camera postion. Doesn't really work.
                if((_camera_movements < 16) && main_camera.transform.hasChanged) {
                    _reset_VR_camera = true;
                    _camera_movements += 1;
                    main_camera.transform.hasChanged = false;
                    VRLog.Info("Main camera has moved {0} times", _camera_movements);
                }
            }
            if(_reset_VR_camera && main_camera != null )
            {
                VRLog.Info("Moving VR camera ({0}, {1}) to main camera ({2}, {3})", VR.Camera.Head.position, VR.Camera.Head.rotation, main_camera.transform.position, main_camera.transform.rotation);
                //VR.Camera.Origin.SetPositionAndRotation(main_camera.transform.position, main_camera.transform.rotation);
                MoveVRCamera(main_camera.transform, true, true);
                //VRLog.Info("Moved VR camera ({0}, {1}) to main camera ({2}, {3})", VR.Camera.Head.position, VR.Camera.Head.rotation, main_camera.transform.position, main_camera.transform.rotation);
                _reset_VR_camera = false;
            }
        }

        public static void MoveVRCamera(Transform target, bool rotating, bool positioning)
		{
			Transform origin = VR.Camera.Origin;
			Transform headHead = VR.Camera.Head;
			if(target == null || origin == null || headHead == null)
			{
				return;
			}
			if(rotating)
			{
				origin.rotation = target.rotation;
				float rot = origin.rotation.eulerAngles.y - headHead.rotation.eulerAngles.y;
				origin.Rotate(Vector3.up * rot);
			}
			if(positioning)
			{
				origin.position = target.position - (headHead.position - origin.position);
			}
		}
    }
}
