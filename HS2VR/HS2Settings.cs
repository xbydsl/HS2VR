using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;
using UnityEngine;
using VRGIN.Controls;
using VRGIN.Core;
using VRGIN.Helpers;
using static VRGIN.Visuals.GUIMonitor;

namespace HS2VR
{
    /// <summary>
    /// Class that holds settings for VR. Saved as an XML file.
    /// 
    /// In order to create your own settings file, extend this class and add your own properties. Make sure to call <see cref="TriggerPropertyChanged(string)"/> if you want to use
    /// the events.
    /// IMPORTANT: When extending, add an XmlRoot annotation to the class like so:
    /// <code>[XmlRoot("Settings")]</code>
    /// </summary>
    [XmlRoot("Settings")]
    public class HS2VRSettings : VRSettings
    {
        public HS2VRSettings()
        {
            // Overwrite defaults
            this.IPDScale = 10f;
            this.GrabRotationImmediateMode = false;
        }
        // XMLSerializerは配列にデフォルト値をつけると、指定値とデフォルト値の両方を含む配列にしてしまうので
        public static HS2VRSettings Load(string path)
        {
            HS2VRSettings settings = VRSettings.Load<HS2VRSettings>(path);
            if (settings.KeySets.Count == 0)
            {
                settings.KeySets = new List<KeySet> { new KeySet() };
            }

            return settings;
        }

        private HS2Shortcuts _HS2Shortcuts = new HS2Shortcuts();
        [XmlComment("Additional HS2 Shortcuts. Refer to https://docs.unity3d.com/ScriptReference/KeyCode.html for a list of available keys.")]
        public HS2Shortcuts HS2Shortcuts { get { return _HS2Shortcuts; } set { _HS2Shortcuts = value; } }

        [XmlElement(Type = typeof(List<KeySet>))]
        public List<KeySet> KeySets { get { return _KeySets; } set { _KeySets = value; } }
        private List<KeySet> _KeySets = null;

        private string _DefaultMode = "Seated";
        [XmlComment("Seated - Mouse/KB, Standing - Controllers")]
        public string DefaultMode { get { return _DefaultMode; } set { _DefaultMode = value; } }

        private float _SeatedDialogueHeightAdjustment = 3.0f;
        public float SeatedDialogueHeightAdjustment { get { return _SeatedDialogueHeightAdjustment; } set { _SeatedDialogueHeightAdjustment = value; } }

        private float _StandingDialogueHeightAdjustment = 3.0f;
        public float StandingDialogueHeightAdjustment { get { return _StandingDialogueHeightAdjustment; } set { _StandingDialogueHeightAdjustment = value; } }

        private bool _ScalePOVToImpersonatedCharacter = true;
        [XmlComment("Scale POV View to Impersonated Character (Thinks look bigger when you're smaller)")]
        public bool ScalePOVToImpersonatedCharacter { get { return _ScalePOVToImpersonatedCharacter; } set { _ScalePOVToImpersonatedCharacter = value; } }

        private float _ScalePOVTOImpersonatedCharacterScaleCoeff = 1.0f;
        [XmlComment("Coefficient applied to POV scaling, multiplies the apparent size differential, 1.0 matches scaling change to POV character height. >1 increases apparent size difference, <1 decreases apparent difference")]
        public float ScalePOVToImpersonatedCharacterScaleCoeff { get { return _ScalePOVTOImpersonatedCharacterScaleCoeff; } set { _ScalePOVTOImpersonatedCharacterScaleCoeff = value; } }

        private bool _SuppressCamlightShadows = true;
        [XmlComment("Suppresses the shadows on the camlight directional light (if present)")]
        public bool SuppressCamlightShadows { get { return _SuppressCamlightShadows; } set { _SuppressCamlightShadows = value; } }

        private bool _ReplaceCamLightWSpotLight = false;
        [XmlComment("Replaces the camlight directional light with a spot light strapped to the head.")]
        public bool ReplaceCamLightWSpotLight { get { return _ReplaceCamLightWSpotLight; } set { _ReplaceCamLightWSpotLight = value; } }

        public bool UsingHeadPos { get { return _UsingHeadPos; } set { _UsingHeadPos = value; } }
        private bool _UsingHeadPos = false;

        public float StandingCameraPos { get { return _StandingCameraPos; } set { _StandingCameraPos = value; } }
        private float _StandingCameraPos = 1.5f;

        public float CrouchingCameraPos { get { return _CrouchingCameraPos; } set { _CrouchingCameraPos = value; } }
        private float _CrouchingCameraPos = 0.7f;

        public bool CrouchByHMDPos { get { return _CrouchByHMDPos; } set { _CrouchByHMDPos = value; } }
        private bool _CrouchByHMDPos = true;

        public float CrouchThrethould { get { return _CrouchThrethould; } set { _CrouchThrethould = value; } }
        private float _CrouchThrethould = 0.15f;

        public float StandUpThrethould { get { return _StandUpThrethould; } set { _StandUpThrethould = value; } }
        private float _StandUpThrethould = -0.55f;

        public float TouchpadThreshold { get { return _TouchpadThreshold; } set { _TouchpadThreshold = value; } }
        private float _TouchpadThreshold = 0.8f;

        public float RotationAngle { get { return _RotationAngle; } set { _RotationAngle = value; } }
        private float _RotationAngle = 45f;
        public CaptureConfig Capture { get { return _CaptureConfig; } set { _CaptureConfig = value; } }
        private CaptureConfig _CaptureConfig = new CaptureConfig();

        [XmlComment("Tools to use with the Left Controller Options (in order of appearance, comma separated): MENU, WARP, PLAY, CAM, POV, ROT")]
        public string LeftTools {  get { return _LeftTools; } set { _LeftTools = value; } }
        private string _LeftTools = "MENU, WARP, PLAY, CAM, POV, ROT";
        [XmlComment("Tools to use with the Right Controller Options (in order of appearance, comma separated): MENU, WARP, PLAY, CAM, POV, ROT")]
        public string RightTools { get { return _RightTools; } set { _RightTools = value; } }
        private string _RightTools = "MENU, WARP, PLAY, CAM, POV, ROT";
    }

    public class CaptureConfig
    {
        public XmlKeyStroke Shortcut = new XmlKeyStroke("Ctrl + F12");
        public bool Stereoscopic = true;
        public bool WithEffects = true;
        public bool SetCameraUpright = true;
        public bool HideGUI = false;
        //public bool HideControllers = false;

        public CaptureConfig()
        {

        }
    }
    

    [XmlRoot("KeySet")]
    public class KeySet
    {
        public KeySet()
        {
            Trigger = "WALK";
            Grip = "PL2CAM";
            Up = "F3";
            Down = "F4";
            Right = "RROTATION";
            Left = "LROTATION";
            Center = "RBUTTON";
        }

        public KeySet(string trigger, string grip, string Up, string Down, string Right, string Left, string Center)
        {
            this.Trigger = trigger;
            this.Grip = grip;
            this.Up = Up;
            this.Down = Down;
            this.Right = Right;
            this.Left = Left;
            this.Center = Center;
        }

        [System.Xml.Serialization.XmlElement("Trigger")]
        public String Trigger { get; set; }

        [System.Xml.Serialization.XmlElement("Grip")]
        public String Grip { get; set; }

        [System.Xml.Serialization.XmlElement("Up")]
        public String Up { get; set; }

        [System.Xml.Serialization.XmlElement("Down")]
        public String Down { get; set; }

        [System.Xml.Serialization.XmlElement("Right")]
        public String Right { get; set; }

        [System.Xml.Serialization.XmlElement("Left")]
        public String Left { get; set; }

        [System.Xml.Serialization.XmlElement("Center")]
        public String Center { get; set; }
    }
}
