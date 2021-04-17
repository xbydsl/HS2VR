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
        public string DefaultMode {  get { return _DefaultMode;  } set { _DefaultMode = value; } }

        private float _SeatedDialogueHeightAdjustment = 3.0f;
        public float SeatedDialogueHeightAdjustment { get { return _SeatedDialogueHeightAdjustment; } set { _SeatedDialogueHeightAdjustment = value; } }

        private float _StandingDialogueHeightAdjustment = 3.0f;
        public float StandingDialogueHeightAdjustment { get { return _StandingDialogueHeightAdjustment; } set { _StandingDialogueHeightAdjustment = value; } }

        private bool _SuppressCamlightShadows = true;
        [XmlComment("Suppresses the shadows on the camlight directional light (if present)")]
        public bool SuppressCamlightShadows {  get { return _SuppressCamlightShadows; } set { _SuppressCamlightShadows = value; } }

        private bool _ReplaceCamLightWSpotLight = false;
        [XmlComment("Replaces the camlight directional light with a spot light strapped to the head.")]
        public bool ReplaceCamLightWSpotLight {  get { return _ReplaceCamLightWSpotLight; } set { _ReplaceCamLightWSpotLight = value; } }

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
