using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using AppleTvLiar.AppleChannels.TvManager.UniTv;

namespace AppleTvLiar.AppleChannels.TvManager
{

    public class plist
    {
        [XmlElement(ElementName = "Group")]
        public Group[] _Group { get; set; }
    }
}