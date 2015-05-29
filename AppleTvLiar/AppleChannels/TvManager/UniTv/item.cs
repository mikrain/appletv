using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace AppleTvLiar.AppleChannels.TvManager.UniTv
{
    public class item
    {
        [XmlAttribute(AttributeName = "linkIconLarge")]
        public string LinkIconLarge { get; set; }

        [XmlAttribute(AttributeName = "linkMovie")]
        public string LinkMovie { get; set; }

        [XmlAttribute(AttributeName = "idMovie")]
        public string IdMovie { get; set; }

        [XmlText]
        public string Text { get; set; }
    }
}
