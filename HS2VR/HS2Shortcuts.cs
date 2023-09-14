//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
using System.Xml.Serialization;
using VRGIN.Core;

namespace HS2VR
{
    public class HS2Shortcuts
    {
        [XmlElement("SuspendPOVToggle")]
        public XmlKeyStroke SuspendPOVToggle = new XmlKeyStroke("L");

    }
}
