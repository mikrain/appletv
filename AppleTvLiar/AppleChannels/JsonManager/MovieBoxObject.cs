using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppleTvLiar.AppleChannels.JsonManager
{
    public class MovieBoxObject
    {
        private int _seasons = 1;
        public string id { get; set; }
        public int rating { get; set; }
        public string title { get; set; }
        public string year { get; set; }
        public string poster { get; set; }
        public string cats { get; set; }
        public int seasons
        {
            get { return _seasons; }
            set { _seasons = value; }
        }
    }
}
