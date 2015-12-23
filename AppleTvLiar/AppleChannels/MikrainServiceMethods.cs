using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Pipes;
using System.Linq;
using System.Net.Cache;
using System.Net.Mime;
using System.Net.Sockets;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Web;
using System.Net;
using System.IO;
using System.ServiceModel.Web;
using System.Xml;
using AppleTvLiar.AppleChannels;
using AppleTvLiar.AppleChannels.JsonManager;
using AppleTvLiar.AppleChannels.XmlManager;
using AppleTvLiar.Helper;
using AppleTvLiar.AppleChannels.HtmlManager;
using System.Threading.Tasks;
using AppleTvLiar.AppleChannels.TvManager;
using System.Net.Http;
using AppleTvLiar.ProccessManager;
using System.Threading;
using System.Xml.Linq;

namespace MikrainService
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class MikrainServiceMethods : AppleBase, IMikrainService
    {
        public static DateTime DateChannel;

        public XmlDocument appleChannels(string channel)
        {
            DateChannel = DateTime.Now;

            Console.WriteLine("appleChannels");
            try
            {

                return new TvManager().GetChannel(channel);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                return null;
            }
        }

        public XmlDocument appletv()
        {

            Console.WriteLine("appletv");
            //http://trailers.apple.com/appletv/index.xml
            return GetXml(@"Content\index.xml");
            //doc.LoadXml(HttpRequests("http://trailers.apple.com/appletv/index.xml"));
        }

        public XmlDocument playChannel(string href)
        {
            return new TvManager().PlayChannel(href);
        }

        public XmlDocument appleusTv()
        {
            Console.WriteLine("appleusTv");
            return GetXml(@"Content\index.xml");
            //  doc.LoadXml(HttpRequests("http://trailers.apple.com/appletv/us/index.xml"));
        }

        public XmlDocument appleMoviesOnline(string page = "0")
        {
            Console.WriteLine("appleMoviesOnline");
            XmlDocument xmlDocument = new BaskinoSiteManager().GetCategories("http://baskino.com/mobile/page", "movie").GetXmlDocument();
            return xmlDocument;
        }

        public XmlDocument BaskinoMutliOnline(string page = "0")
        {
            try
            {
                Console.WriteLine("appleMoviesOnline");
                XmlDocument xmlDocument = new BaskinoSiteManager().GetCategories("http://baskino.com/films/multfilmy/page", "multBas").GetXmlDocument();
                return xmlDocument;
            }
            catch (Exception ex)
            {
                EventLog myLog = new EventLog();
                myLog.Source = "AppleSource";

                // Write an informational entry to the event log.    
                myLog.WriteEntry(ex.Message);
            }
            return null;
        }


        static HttpClient client = new HttpClient();

        // Code for getting IP
        private RemoteEndpointMessageProperty getClientIP()
        {
            //WebOperationContext webContext = WebOperationContext.Current;

            OperationContext context = OperationContext.Current;

            MessageProperties messageProperties = context.IncomingMessageProperties;

            var endpointProperty = messageProperties[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty;
            return endpointProperty;
        }

        public XmlDocument Playmovie(string url)
        {
            //HttpContext context = HttpContext.Current;

            Console.WriteLine("Playmovie " + url);

            XmlDocument xmlDocument = new BaskinoSiteManager().PlayMovie(url).GetXmlDocument();
            return xmlDocument;
        }

        public Stream PlaymovieStream(string url)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                return response.GetResponseStream();
            }
        }

        public XmlDocument Arrr()
        {
            new ArrrSiteManager().GetConent();
            return null;
        }

        public XmlDocument FriendsSeason()
        {
            Console.WriteLine("FriendsSeason ");
            return new FriendsSiteManager().GetConent("http://friends-online.su/eng/", @"Content\categories.xml").GetXmlDocument();
        }

        public XmlDocument FriendsShow(string friendsSeason, string imgUrl)
        {
            Console.WriteLine("FriendsShow " + friendsSeason);
            return new FriendsSiteManager().GetFriendSeries(friendsSeason, imgUrl).GetXmlDocument();
        }



        public XmlDocument MultPoisk(string ext)
        {
            try
            {
           
               return new MultPoiskSiteManager().GetConent(string.Format("http://multpoisk.net/{0}", ext), @"Content\categories.xml", "multPoisk").GetXmlDocument();

            }
            catch (Exception ex)
            {
                EventLog myLog = new EventLog();
                myLog.Source = "AppleSource";

                // Write an informational entry to the event log.    
                myLog.WriteEntry(ex.Message);
            }
            return null;
        }

        public XmlDocument MultPoiskSerial()
        {
            try
            {
                return new MultPoiskSiteManager().GetConent("http://multpoisk.net/multserial", @"Content\categories.xml", "multPosikSerial").GetXmlDocument();

            }
            catch (Exception ex)
            {
                EventLog myLog = new EventLog();
                myLog.Source = "AppleSource";

                // Write an informational entry to the event log.    
                myLog.WriteEntry(ex.Message);
            }
            return null;
        }

        public XmlDocument MultPoiskNew()
        {
            try
            {
                return new MultPoiskSiteManager().GetConent("http://multpoisk.net/mult2013", @"Content\categories.xml", "multPoiskNew").GetXmlDocument();

            }
            catch (Exception ex)
            {
                EventLog myLog = new EventLog();
                myLog.Source = "AppleSource";

                // Write an informational entry to the event log.    
                myLog.WriteEntry(ex.Message);
            }
            return null;
        }

        public XmlDocument MultPoiskList()
        {
            try
            {
                return new MultPoiskSiteManager().GetList();

            }
            catch (Exception ex)
            {
                EventLog myLog = new EventLog();
                myLog.Source = "AppleSource";

                // Write an informational entry to the event log.    
                myLog.WriteEntry(ex.Message);
            }
            return null;
        }

        public XmlDocument multiDetails(string movie, string imgSource)
        {
            try
            {
                return new MultPoiskSiteManager().GetMultSeries(movie, imgSource).GetXmlDocument();

            }
            catch (Exception ex)
            {
                EventLog myLog = new EventLog();
                myLog.Source = "AppleSource";

                // Write an informational entry to the event log.    
                myLog.WriteEntry(ex.Message);
            }
            return null;
        }

        public XmlDocument multiSingle(string multiPlayHref, string imgSource)
        {
            try
            {
                return new MultPoiskSiteManager().GetSingleMovie(multiPlayHref, imgSource).GetXmlDocument();

            }
            catch (Exception ex)
            {
                EventLog myLog = new EventLog();
                myLog.Source = "AppleSource";

                // Write an informational entry to the event log.    
                myLog.WriteEntry(ex.Message);
            }
            return null;
        }

        public XmlDocument RussianCartoon()
        {
            return new RussianCartoons().GetCatalogue().GetXmlDocument();
        }

        public XmlDocument RussianCartoonMoview(string russianCartoonMoview)
        {
            return new RussianCartoons().GetCartoonSeries(russianCartoonMoview).GetXmlDocument();
        }

        public XmlDocument GetRCartoonShow(string russianCartoonMoview, string filmId)
        {
            return new RussianCartoons().GetRCartoonShow(russianCartoonMoview, filmId).GetXmlDocument();
        }

        public XmlDocument GetShows()
        {
            Console.WriteLine("GetShows ");
            return new ShowSiteManager().GetConent().GetXmlDocument();
        }

        public XmlDocument Shows(string show)
        {
            Console.WriteLine("Shows " + show);
            return new ShowSiteManager().GetSeasons(show).GetXmlDocument();
        }

        public XmlDocument ShowSeries(string showSeries, string name)
        {
            Console.WriteLine("ShowSeries " + showSeries);
            return new ShowSiteManager().GetSeries(showSeries, name).GetXmlDocument();
        }

        public XmlDocument ShowSer(string ser, string title, string image)
        {
            Console.WriteLine("ShowSer " + ser);
            return new ShowSiteManager().ShowSer(ser, title, image).GetXmlDocument();
        }

        public XmlDocument FriendsPlay(string friendsPlayHref, string imageHref)
        {
            Console.WriteLine("FriendsPlay " + friendsPlayHref);
            return new FriendsSiteManager().GetShow(friendsPlayHref, imageHref).GetXmlDocument();
        }

        public XmlDocument megafindMovie(string movie, string image, string title)
        {
            MegaserialSiteManager manager = new MegaserialSiteManager();
            return manager.GetMovie(movie, image, title).GetXmlDocument();
        }

        public XmlDocument megaserial()
        {
            MegaserialSiteManager manager = new MegaserialSiteManager();
            return manager.GetCategories("http://megaserial.net/zapadnye/page", "megaserial").GetXmlDocument();
        }

        public XmlDocument ShowMegaEpisodes(string href, string imageHref, string title, string season)
        {
            MegaserialSiteManager manager = new MegaserialSiteManager();
            return manager.GetEpisodes(href, imageHref, title, season).GetXmlDocument();
        }

        public XmlDocument ShowMegaSer(string ser, string image, string title)
        {
            MegaserialSiteManager manager = new MegaserialSiteManager();
            return manager.ShowMegaSer(ser, image, title).GetXmlDocument();
        }

        public XmlDocument lovekinozalNew()
        {

            XmlDocument xmlDocument = new M4KinoSiteManager().GetCategories().GetXmlDocument();
            return xmlDocument;
        }

        public XmlDocument lovekinozalMovie(string movie)
        {
            XmlDocument xmlDocument = new M4KinoSiteManager().GetMovie(movie).GetXmlDocument();
            return xmlDocument;
        }

        public async Task<XmlDocument> SearchBaskino(string query)
        {
            return (await new BaskinoSiteManager().SearchBaskino(query)).GetXmlDocument();
        }

        public XmlDocument movieBoxMovieShow()
        {
            return new MovieBoxManager().GetListOfShows().GetXmlDocument();
        }

        public async Task<XmlDocument> GetUcoz()
        {
            try
            {
                return (await new Ucoz().GetContent()).GetXmlDocument();

            }
            catch (Exception ex)
            {
                EventLog myLog = new EventLog();
                myLog.Source = "AppleSource";

                // Write an informational entry to the event log.    
                myLog.WriteEntry(ex.Message);
            }
            return null;
        }

        public XmlDocument GetYourCinema()
        {
            return new YourCinemaManager().GetContent().GetXmlDocument();
        }

        public XmlDocument GetYourCinemaMove(string movie, string imageUrl, string movieTitle)
        {
            return new YourCinemaManager().GetMovie(movie, imageUrl, movieTitle).GetXmlDocument();
        }

        public async Task<XmlDocument> UcozSearch(string query)
        {
            try
            {
                return (await new Ucoz().SearchUkoz(query)).GetXmlDocument();

            }
            catch (Exception ex)
            {
                EventLog myLog = new EventLog();
                myLog.Source = "AppleSource";

                // Write an informational entry to the event log.    
                myLog.WriteEntry(ex.Message);
            }
            return null;
        }

        public async Task<XmlDocument> GetUcozMultSerial()
        {
            try
            {
                return (await new Ucoz().GetContent("http://hd-720.ucoz.ru/load/multserialy/27-{0}-2", "multSerialy")).GetXmlDocument();

            }
            catch (Exception ex)
            {
                EventLog myLog = new EventLog();
                myLog.Source = "AppleSource";

                // Write an informational entry to the event log.    
                myLog.WriteEntry(ex.Message);
            }
            return null;
        }

        public async Task<XmlDocument> GetUcozMult()
        {
            try
            {
                return (await new Ucoz().GetContent("http://hd-720.ucoz.ru/load/multfilm/24-{0}-2", "mult")).GetXmlDocument();

            }
            catch (Exception ex)
            {
                EventLog myLog = new EventLog();
                myLog.Source = "AppleSource";

                // Write an informational entry to the event log.    
                myLog.WriteEntry(ex.Message);
            }
            return null;
        }

        public XmlDocument GetKino1080()
        {
            return new AdultMult().GetContent().GetXmlDocument();
        }
        public XmlDocument GetKino1080Mult()
        {
            return new Kino1080Manager().GetContent("http://kino1080.com/category/muljtfiljm/page/{0}/").GetXmlDocument();
        }

        public async Task<XmlDocument> GetUcozSerial()
        {
            try
            {
                return (await new Ucoz().GetContent("http://hd-720.ucoz.ru/load/serialy/26-{0}-2", "serialy")).GetXmlDocument();

            }
            catch (Exception ex)
            {
                EventLog myLog = new EventLog();
                myLog.Source = "AppleSource";

                // Write an informational entry to the event log.    
                myLog.WriteEntry(ex.Message);
            }
            return null;
        }


        public async Task<XmlDocument> ShowUcozSer(string ser, string title, string image)
        {
            try
            {
                return (await new Ucoz().ShowUcozSer(ser, title, image)).GetXmlDocument();

            }
            catch (Exception ex)
            {
                EventLog myLog = new EventLog();
                myLog.Source = "AppleSource";

                // Write an informational entry to the event log.    
                myLog.WriteEntry(ex.Message);
            }
            return null;
        }


        public XmlDocument ShowUcozEpisodes(string href, string imageHref, string title, string season)
        {
            return new Ucoz().ShowUcozEpisodes(href, imageHref, title, season).GetXmlDocument();
        }

        public async Task<XmlDocument> GetUcozMovie(string movie, string imageUrl, string movieTitle)
        {
            try
            {
                return (await new Ucoz().GetMovie(movie, imageUrl, movieTitle)).GetXmlDocument();

            }
            catch (Exception ex)
            {
                EventLog myLog = new EventLog();
                myLog.Source = "AppleSource";

                // Write an informational entry to the event log.    
                myLog.WriteEntry(ex.Message);
            }
            return null;
        }

        public XmlDocument GetKino1080Movie(string movie, string imageUrl, string movieTitle)
        {
            return new Kino1080Manager().GetMovie(movie, imageUrl, movieTitle).GetXmlDocument();
        }

        public XmlDocument movieBoxShowDetails(string movieId, string image, string title, string seasons)
        {
            return new MovieBoxManager().GetMovieShowDetails(movieId, image, title, seasons).GetXmlDocument();
        }

        public async Task<XmlDocument> GetMoviewShowSeries(string href, string imageHref, string title, string season)
        {

            return (await new MovieBoxManager().GetMovieShowSeries(href, imageHref, title, season)).GetXmlDocument();

        }

        public XmlDocument GetMoviewShowEpisode(string hrefEn, string hrefRu, string imageHref, string title)
        {
            return new MovieBoxManager().GetMoviewShowEpisode(hrefEn, hrefRu, imageHref, title).GetXmlDocument();
        }

        public XmlDocument searchMovieBox(string q)
        {
            return new MovieBoxManager().Searchmovie(q).GetXmlDocument();
        }

        public XmlDocument movieBoxMovie()
        {
            return new MovieBoxManager().GetListOfMovies().GetXmlDocument();
        }

        public XmlDocument movieBoxMovieDetails(string movieId, string image, string title, string count)
        {
            return new MovieBoxManager().GetMovieDetails(movieId, image, title, count).GetXmlDocument();
        }

        public XmlDocument findMovie(string movieUrl)
        {
            Console.WriteLine("findMovie " + movieUrl);
            XmlDocument xmlDocument = new BaskinoSiteManager().GetMovie(movieUrl).GetXmlDocument();
            return xmlDocument;
        }

        public XmlDocument appletvnav()
        {
            var xml = GetXml(@"Content\index.xml").InnerXml;
            //   xml = xml.Replace("localhost", Helpers.GetIpAddress().ToString());
            var doc = new XmlDocument();
            doc.LoadXml(xml);
            return doc;
            // doc.LoadXml(HttpRequests("http://trailers.apple.com/appletv/us/nav.xml"));
        }

        public Stream appletvjs()
        {
            // return new FileStream(Path.Combine(MikrainService.MikrainProgramm._xmlPath, "Content\\js\\application.js"), FileMode.Open);
            var js = HttpRequests("http://trailers.apple.com/appletv/us/js/application.js");
            return new MemoryStream(Encoding.UTF8.GetBytes(js));
        }

        public Stream appletvShow()
        {
            //return new FileStream(Path.Combine(MikrainService.MikrainProgramm._xmlPath, "Content\\js\\Trailers.Showtimes.js"), FileMode.Open);
            var js = HttpRequests("http://trailers.apple.com/appletv/us/js/Trailers.Showtimes.js");
            return new MemoryStream(Encoding.UTF8.GetBytes(js));
        }

        public Stream appletvTrailers()
        {
            //return new FileStream(Path.Combine(MikrainService.MikrainProgramm._xmlPath, "Content\\js\\Trailers.js"), FileMode.Open);
            var js = HttpRequests("http://trailers.apple.com/appletv/us/js/Trailers.js");
            return new MemoryStream(Encoding.UTF8.GetBytes(js));
        }

        public Stream appletvMain()
        {
            //return new FileStream(Path.Combine(MikrainService.MikrainProgramm._xmlPath, "Content\\js\\main.js"), FileMode.Open);
            var js = HttpRequests("http://trailers.apple.com/appletv/us/js/main.js");
            return new MemoryStream(Encoding.UTF8.GetBytes(js));
        }

        public Stream appletvConfig()
        {
            return new FileStream(Path.Combine(MikrainService.MikrainProgramm._xmlPath, "Content\\js\\showtimesconfig.js"), FileMode.Open);
            var js = HttpRequests("http://trailers.apple.com/appletv/us/js/showtimesconfig.js");
            return new MemoryStream(Encoding.UTF8.GetBytes(js));
        }

        public Stream appletvLog()
        {
            return new FileStream(Path.Combine(MikrainService.MikrainProgramm._xmlPath, "Content\\js\\Trailers.Log.js"), FileMode.Open);
            var js = HttpRequests("http://trailers.apple.com/appletv/us/js/Trailers.Log.js");
            return new MemoryStream(Encoding.UTF8.GetBytes(js));
        }

        DateTime nowTime;

        public Stream GetStream()
        {
            if (!File.Exists(Path.Combine(MikrainService.MikrainProgramm._xmlPath, @"streamtv\mystream.m3u8")))
            {
                System.String directory = Path.Combine(MikrainService.MikrainProgramm._xmlPath, @"streamtv");
                using (var watcher = new System.IO.FileSystemWatcher(directory, "*.m3u8"))
                {
                    watcher.EnableRaisingEvents = true;
                    var result = watcher.WaitForChanged(System.IO.WatcherChangeTypes.All, 40000);
                    if (result.TimedOut)
                    {
                        throw new Exception();
                    }
                    watcher.EnableRaisingEvents = false;
                }
            }

            if (File.Exists(Path.Combine(MikrainService.MikrainProgramm._xmlPath, @"streamtv\mystream.m3u8")))
            {
                WebOperationContext.Current.OutgoingResponse.Headers.Add("Content-Disposition", "attachment; filename=mystream.m3u8");
                WebOperationContext.Current.OutgoingResponse.ContentType = "application/x-mpegURL";
                var opened = File.Open(Path.Combine(MikrainService.MikrainProgramm._xmlPath, @"streamtv\mystream.m3u8"), FileMode.Open, FileAccess.Read);
                return opened;
            }

            throw new Exception();

        }

        public Stream Cert()
        {
            return File.Open(@"C:\client01.crt", FileMode.Open);
        }

        void watcher_Created(object sender, FileSystemEventArgs e)
        {
            throw new NotImplementedException();
        }

        void fileSystemWatcher_Created(object sender, FileSystemEventArgs e)
        {
        }

        public Stream GetStreamTs()
        {
            return GetRequests();
        }

        public string GetAllRequests()
        {
            //WebClient webClient = new WebClient();
            //webClient.DownloadFile("http://mysite.com/myfile.txt", @"c:\myfile.txt");

            //HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://streamer8de.allfreetv.net:80/ch/df2d2653ad862b1be6c7c3095beae355");
            //request.ContentType = @"application/octet-stream";

            //request.UserAgent = "VLC/3.0.0-git LibVLC/3.0.0-git";
            //// request.Headers.Add(HttpRequestHeader.ContentType, "application/octet-stream");

            //using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            //{
            //    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            //    {
            //        string result = reader.ReadToEnd();
            //        return result;
            //    }
            //}

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(WebOperationContext.Current.IncomingRequest.UriTemplateMatch.RequestUri.ToString());
            

            var req = HttpRequests(WebOperationContext.Current.IncomingRequest.UriTemplateMatch.RequestUri.ToString());
            return req;
        }

        public Stream GetRequests()
        {
            if (System.ServiceModel.Web.WebOperationContext.Current != null)
            {
                //WebOperationContext.Current.OutgoingResponse.Headers.Add("Content-Disposition", "attachment; filename=mystream.m3u8");
                WebOperationContext.Current.OutgoingResponse.ContentType = "video/MP2T";
                var asfd = System.ServiceModel.Web.WebOperationContext.Current.IncomingRequest.UriTemplateMatch.RequestUri;
                var segm = asfd.Segments;
                var asc = File.Open(Path.Combine(MikrainService.MikrainProgramm._xmlPath, Path.Combine("streamtv", segm[2])), FileMode.Open, FileAccess.Read);
                return asc;
            }


            return null;
        }

        public string HttpRequests(string url)
        {
            Debug.WriteLine(url);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Headers.Add("X-Apple-TV-Resolution", "1080");
            request.Headers.Add("X-Apple-TV-Version", "7.2");
            request.UserAgent =
               "AppleTV/7.2 iOS/8.3 AppleTV/7.2 model/AppleTV3,2 build/12F69 (3; dt:12)";
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    string result = reader.ReadToEnd();
                    return result;
                }
            }
        }

        public XmlDocument GetBestRussian()
        {
            var cacheDoc = ReadDoc("bestRussian");
            if (cacheDoc != null)
            {
                return cacheDoc.GetXmlDocument();
            }

            var doc= new BestRussian().GetChannelsList();
            SaveDoc("bestRussian",XDocument.Parse(doc.InnerXml));
            return doc;
        }

        public Stream GetBestRussianImage(string channelId)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(string.Format("http://images.bestrussiantv.com/ui/ImageHandler.ashx?t=10&e={0}", channelId));
            return request.GetResponse().GetResponseStream();
        }
    }
}
