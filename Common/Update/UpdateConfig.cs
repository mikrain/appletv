using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Common.Update;

namespace Common
{
    [XmlRoot(ElementName = "UpdateConfig")]
    public class UpdateConfig
    {
        [XmlAttribute(AttributeName = "UpdateVersion")]
        public string UpdateVersion { get; set; }

        [XmlElement(ElementName = "Config")]
        public List<Config> Config { get; set; }

    }
}
