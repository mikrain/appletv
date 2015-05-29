using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppleTvLiar.AppleChannels.JsonManager
{

    public class Thumbs
    {
        public string id;
        public string thumb;

    }

    public class MovieBoxSeries
    {
        private List<Thumbs> _thumbs = new List<Thumbs>();
        private List<MovieBoxEpisodes> _movieBoxEpisodeses = new List<MovieBoxEpisodes>();

        [JsonProperty("description")]
        public string Description { get; set; }

        
         public List<Thumbs> thumbs
         {
             get { return _thumbs; }
             set { _thumbs = value; }
         }

         public List<MovieBoxEpisodes> movieBoxEpisodeses
         {
             get { return _movieBoxEpisodeses; }
             set { _movieBoxEpisodeses = value; }
         }
    }
}
