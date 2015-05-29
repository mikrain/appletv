using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppleTvLiar.AppleChannels.JsonManager
{
    public class MovieBoxEpisode
    {
        public string id { get; set; }
        public string season { get; set; }
        public string episode { get; set; }
        public string active { get; set; }
        public string lang { get; set; }
        public string link { get; set; }
        public int apple { get; set; }
        public int google { get; set; }
        public string microsoft { get; set; }
    }
}
