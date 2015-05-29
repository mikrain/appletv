using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Common.Update
{
    public class Config
    {
        [XmlAttribute(AttributeName = "Changed")]
        public bool ShouldBeChanged { get; set; }

        public string Url { get; set; }
        public string Source { get; set; }
        public string Target { get; set; }
    }
}
