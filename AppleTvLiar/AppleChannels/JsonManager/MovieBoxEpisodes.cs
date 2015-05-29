using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace AppleTvLiar.AppleChannels.JsonManager
{
    [JsonArray]
    public class MovieBoxEpisodes
    {
        private List<MovieBoxEpisode> _movieBoxEpisode = new List<MovieBoxEpisode>();
        public List<MovieBoxEpisode> movieBoxEpisode
        {
            get { return _movieBoxEpisode; }
            set { _movieBoxEpisode = value; }
        }
    }
}
