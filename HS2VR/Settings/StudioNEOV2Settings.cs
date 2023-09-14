using System.Xml.Serialization;
using VRGIN.Core;

namespace HS2VR.Settings
{
    [XmlRoot("Settings")]
    public class CharaStudioSettings : VRSettings
    {
        private bool _LockRotXZ = true;

        private float _MaxVoiceDistance = 300f;

        private float _MinVoiceDistance = 7f;

        [XmlComment("Lock XZ Axis (pitch / roll) rotation.")]
        public bool LockRotXZ
        {
            get => _LockRotXZ;
            set
            {
                _LockRotXZ = value;
                TriggerPropertyChanged("LockRotXZ");
            }
        }

        [XmlComment("Max Voice distance (in unit. 300 = 30m in real (HS2 uses 10 unit = 1m scale).")]
        public float MaxVoiceDistance
        {
            get => _MaxVoiceDistance;
            set
            {
                _MaxVoiceDistance = value;
                TriggerPropertyChanged("MaxVoiceDistance");
            }
        }

        [XmlComment("Min Voice distance (in unit. 7 = 70 cm in real (HS2 uses 10 unit = 1m scale).")]
        public float MinVoiceDistance
        {
            get => _MinVoiceDistance;
            set
            {
                _MinVoiceDistance = value;
                TriggerPropertyChanged("MinVoiceDistance");
            }
        }

        [XmlIgnore]
        public override Shortcuts Shortcuts
        {
            get => base.Shortcuts;
            protected set => base.Shortcuts = value;
        }

        public CharaStudioSettings()
        {
            IPDScale = 1f;
            GrabRotationImmediateMode = false;
        }

        public static CharaStudioSettings Load(string path)
        {
            return VRSettings.Load<CharaStudioSettings>(path);
        }
    }
}
