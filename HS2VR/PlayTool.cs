using System;
using UnityEngine;
using VRGIN.Core;
using VRGIN.Controls.Tools;
using VRGIN.Helpers;
using VRGIN.Template;

using HS2VR.Interpreters;

using Valve.VR;

namespace HS2VR
{
    class PlayTool : Tool
    {
        public override Texture2D Image
        {
            get
            {
                return UnityHelper.LoadImage("icon_play.png");
            }
        }

        public enum TouchpadDirection {
            Center,
            Left,
            Right,
            Up,
            Down
        };

        private HS2VRSettings _Settings;

        private bool _was_touchpad_click_down = false;

        protected override void OnAwake()
        {
            base.OnAwake();

            _Settings = (VR.Context.Settings as HS2VRSettings);
        }

        protected override void OnStart()
        {
            base.OnStart();
        }

        protected override void OnDestroy()
        {
            // nothing to do.
        }

        protected override void OnDisable()
        {
            base.OnDisable();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();

            var device = this.Controller;
            var touchpad_position = device.GetAxis();
            var touchpad_direction = GetTouchpadDirection(touchpad_position);

            var touchpad_click_up = device.GetPressUp(EVRButtonId.k_EButton_SteamVR_Touchpad);
            var touchpad_touch = device.GetTouch(EVRButtonId.k_EButton_SteamVR_Touchpad);
            var touchpad_click_down = device.GetPress(EVRButtonId.k_EButton_SteamVR_Touchpad);

            var tGrip = device.GetPressUp(EVRButtonId.k_EButton_Grip);
            var tTriggerClicked = device.GetPressUp(EVRButtonId.k_EButton_SteamVR_Trigger);

            if((VR.Interpreter as HS2Interpreter).isHScene)
            {
                var scene = ((VR.Interpreter as HS2Interpreter).currentSceneInterpreter as HSceneInterpreter)._HScene;
                HSceneSprite scene_sprite_instance = Singleton<HSceneSprite>.Instance;

                //VRLog.Info("Touchpad direction: {0}", touchpad_direction);
                if(touchpad_direction == TouchpadDirection.Up && touchpad_touch) {
                    // Up
                    //VRLog.Info("Scroll up.");
                    SendInputHandler.MouseWheel(10);
                }
                else if(touchpad_direction == TouchpadDirection.Down && touchpad_touch) {
                    // Down
                    //VRLog.Info("Scroll down.");
                    SendInputHandler.MouseWheel(-10);
                }

                if(touchpad_click_down) {
                    
                    //VRLog.Info("Pressed touchpad.");

                    scene_sprite_instance.categoryFinish.onEnter = true;
                    if(!_was_touchpad_click_down) 
                    {
                        if(touchpad_direction == TouchpadDirection.Left) {
                            // Left
                            //VRLog.Info("Scroll up.");
                            SendInputHandler.MouseWheel(-10);
                        }
                        else if(touchpad_direction == TouchpadDirection.Right) {
                            // Right
                            //VRLog.Info("Scroll down.");
                            SendInputHandler.MouseWheel(10);
                        }
                    }
                    //VRLog.Info("Active finish option: {0}", scene_sprite_instance.categoryFinish.GetlstActive());
                    
                    _was_touchpad_click_down = true;
                    
                }
                if(touchpad_click_up)
                {
                    //VRLog.Info("Unpressed touchpad.");
                    //VRLog.Info("Got {0} buttons", scene_sprite_instance.categoryFinish.lstButton.Count);
                    if(touchpad_direction == TouchpadDirection.Center && _was_touchpad_click_down)
                    {
                        for(int i=0; i < scene_sprite_instance.categoryFinish.lstButton.Count; i++)
                        {
                            if (scene_sprite_instance.categoryFinish.GetEnable(i) && scene_sprite_instance.categoryFinish.lstButton[i].gameObject.activeSelf)
                            {
                                //VRLog.Info("Pressing button {0}", i);
                                scene_sprite_instance.categoryFinish.lstButton[i].onClick.Invoke();
                                break;
                            }
                        }
                    }
                    _was_touchpad_click_down = false;

                }
            }
        }

        public static TouchpadDirection GetTouchpadDirection(Vector2 position, float threshold=0.3f) {
            if(position.x > 0.0f && Mathf.Abs(position.x) >= Mathf.Abs(position.y) && position.magnitude > threshold)
            {
                return TouchpadDirection.Right;
            }
            else if(position.x < 0.0f && Mathf.Abs(position.x) >= Mathf.Abs(position.y) && position.magnitude > threshold)
            {
                return TouchpadDirection.Left;
            }
            else if(position.y > 0.0f && Mathf.Abs(position.x) <= Mathf.Abs(position.y) && position.magnitude > threshold)
            {
                return TouchpadDirection.Up;
            }
            else if(position.y < 0.0f && Mathf.Abs(position.x) <= Mathf.Abs(position.y) && position.magnitude > threshold)
            {
                return TouchpadDirection.Down;
            }
            else{
                return TouchpadDirection.Center;
            }
        }
    }
}