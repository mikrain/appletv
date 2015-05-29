using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace AppleTvLiar.AppleChannels.TvManager.UniTv
{
    public class Group
    {
        [XmlElement(ElementName = "item")]
        public item[] items { get; set; }

        [XmlAttribute(AttributeName = "Name")]
        public string Name { get; set; }
    }
}
