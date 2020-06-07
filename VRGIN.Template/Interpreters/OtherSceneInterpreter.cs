using UnityEngine;
using VRGIN.Core;
using Valve.VR;

namespace HS2VR.Interpreters
{
    class OtherSceneInterpreter : SceneInterpreter
    {
        public override void OnStart()
        {
            base.OnStart();
            VRLog.Info("Starting OtherSceneInterpreter.");
        }
        public override void OnEnable()
        {
            base.OnEnable();
        }
        public override void OnDisable()
        {
            base.OnDisable();
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
        }
    }
}
