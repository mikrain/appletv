using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using AppleTvLiar.AppleChannels.HtmlManager;
using HtmlAgilityPack;
using Ionic.Zip;
using MikrainService;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading;

namespace AppleTvLiar.AppleChannels.JsonManager
{
    public class MovieBoxManager : AppleBase
    {
        private static List<MovieBoxObject> sortedList;
        private static bool isShow = false;

        public MovieBoxManager()
        {
            KillAce();
        }

        public XDocument GetListOfMovies()
        {
            isShow = false;
            XDocument xDocument =
            XDocument.Load(Path.Combine(MikrainProgramm._xmlPath, @"Content\categories.xml"));
            int count = 537;//GetAllId() + 10;
            GetMainList(xDocument, "movieBoxMovieDetails", "movies_lite.json", count);

            return xDocument;
        }

        public int GetAllId()
        {
            var request = (HttpWebRequest)WebRequest.Create("http://mobapps.cc/data/data_en.zip");
            using (var response = (HttpWebResponse)request.GetResponse())
            {
                byte[] result = null;
                int byteCount = Convert.ToInt32(response.ContentLength);
                using (var reader = new BinaryReader(response.GetResponseStream()))
                {
                    result = reader.ReadBytes(byteCount);
                }

                using (ZipFile zip = ZipFile.Read(new MemoryStream(result)))
                {
                    foreach (ZipEntry e in zip)
                    {
                        e.Extract(MikrainProgramm._xmlPath + @"Content\ExtractMovies", ExtractExistingFileAction.OverwriteSilently);  // overwrite == true  
                    }
                    zip.Save(MikrainProgramm._xmlPath + @"Content\ExtractMovies\data_lite.zip");
                }

            }
            var files = Directory.GetFiles(MikrainProgramm._xmlPath + @"Content\ExtractMovies");
            List<MovieBoxObject> movieBoxShows = new List<MovieBoxObject>();

            foreach (var file in files)
            {
                if ((Path.GetFileName(file) == "movies_lite.json" || Path.GetFileName(file) == "tv_lite.json"))
                {
                    using (var reader = new StreamReader(file))
                    {
                        var json = reader.ReadToEnd();
                        JToken jFoo = JValue.Parse(json);
                        foreach (var jvalue in jFoo)
                        {
                            try
                            {
                                if (jvalue["id"] != null)
                                {
                                    var box = JsonSerializer.Create().Deserialize<MovieBoxObject>(new JTokenReader(jvalue));
                                    movieBoxShows.Add(box);

                                    //if (movieBoxObjects.FirstOrDefault(o => o.id == box.id) == null)
                                    //{
                                    //    movieBoxObjects.Add(box);
                                    //}
                                    //else
                                    //{
                                    //    var exist = movieBoxObjects.FirstOrDefault(o => o.id == box.id);
                                    //}
                                }
                            }
                            catch (Exception)
                            {


                            }

                        }
                    }
                }
            }

            return movieBoxShows.Count;
        }


        private void GetMainList(XDocument xDocument, string endPoint, string fileName, int countPlus)
        {
            var items = xDocument.Descendants(XName.Get("items"));
            int count = 0;
            XElement collectionDividerElement = null;
            XElement shelfElement = null;
            XElement itemsElement = null;
            string json;
            if (!File.Exists(MikrainProgramm._xmlPath + @"Content\ExtractMovies\" + fileName))
            {
                json = HttpRequests("http://mobapps.cc/data/data_en.zip", fileName);
            }
            else
            {
                if (File.GetCreationTime(MikrainProgramm._xmlPath + @"Content\ExtractMovies\" + fileName).AddHours(2) >= DateTime.Now)
                {
                    using (var reader = new StreamReader(MikrainProgramm._xmlPath + @"Content\ExtractMovies\" + fileName))
                    {
                        json = reader.ReadToEnd();
                    }
                }
                else
                {
                    json = HttpRequests("http://mobapps.cc/data/data_en.zip", fileName);
                }
            }


            //string json = "";
            //using (var reader = new StreamReader(MikrainProgramm._xmlPath + @"Content\ExtractMovies\" + fileName))
            //{
            //    json = reader.ReadToEnd();
            //}

            JToken jFoo = JValue.Parse(json);
            sortedList = new List<MovieBoxObject>();

            var search = new MovieBoxObject
                {
                    rating = int.MaxValue,
                    title = "search",
                    id = int.MaxValue.ToString(),
                    poster = "http://www.kudoschatsearch.com/images/search.png"
                };
            sortedList.Add(search);

            foreach (var jvalue in jFoo)
            {
                sortedList.Add(JsonSerializer.Create().Deserialize<MovieBoxObject>(new JTokenReader(jvalue)));
            }
            sortedList.Sort((o, boxObject) => boxObject.rating.CompareTo(o.rating));
            foreach (var value in sortedList)
            {
                if (count == 0 || count % 20 == 0)
                {
                    int shelfIdCount = 0;
                    collectionDividerElement = new XElement(XName.Get("collectionDivider"));
                    collectionDividerElement.SetAttributeValue(XName.Get("alignment"), "left");
                    collectionDividerElement.SetAttributeValue(XName.Get("accessibilityLabel"), shelfIdCount);
                    collectionDividerElement.Add(new XElement(XName.Get("title"), ""));
                    shelfElement = new XElement(XName.Get("shelf"));
                    shelfElement.SetAttributeValue(XName.Get("id"), string.Format("shelf_{0}", 1));

                    var sectionsElement = new XElement(XName.Get("sections"));
                    var shelfSectionElement = new XElement(XName.Get("shelfSection"));
                    itemsElement = new XElement(XName.Get("items"));

                    shelfSectionElement.Add(itemsElement);
                    sectionsElement.Add(shelfSectionElement);
                    shelfElement.Add(sectionsElement);
                }

                var title = value.title;// value["title"].Value<string>();
                var image = value.poster;// value["poster"].Value<string>();
                var id = value.id;// value["id"].Value<string>();

                CreateElementList(count,
                                  string.Format(
                                      "atv.loadURL('http://trailers.apple.com/{0}?movieId={1}&image={2}&title={3}&seasons={4}&count={5}')", endPoint,
                                      Uri.EscapeDataString(id), Uri.EscapeDataString(image), Uri.EscapeDataString(title), value.seasons, int.Parse(id) + countPlus), title,
                                  image, itemsElement);
                if (count % 20 == 0)
                {
                    items.First().Add(collectionDividerElement);
                    items.First().Add(shelfElement);
                }
                count++;

            }

        }

        private int Target(MovieBoxObject movieBoxObject, MovieBoxObject boxObject)
        {
            return movieBoxObject.rating > boxObject.rating ? 0 : 1;
        }

        public XDocument GetListOfShows()
        {
            isShow = true;
            XDocument xDocument =
          XDocument.Load(Path.Combine(MikrainProgramm._xmlPath, @"Content\categories.xml"));
            var countPlus = 537;// GetAllId();
            GetMainList(xDocument, "movieBoxShowDetails", "tv_lite.json", countPlus);

            return xDocument;

        }



        public XDocument GetMovieDetails(string id, string image, string title, string plusCount)
        {

            if (title == "search")
            {
                XDocument searchDoc = XDocument.Load(Path.Combine(MikrainProgramm._xmlPath, @"Content\searchtrailers.xml"));
                return searchDoc;
            }

            XDocument xDocument =
            XDocument.Load(Path.Combine(MikrainProgramm._xmlPath, @"Content\movie.xml"));
            xDocument.Descendants(XName.Get("image")).First().SetValue(image);
            var elementDesc = xDocument.Descendants(XName.Get("summary"));
            xDocument.Descendants(XName.Get("title")).First().SetValue(title);
            xDocument.Descendants(XName.Get("actionButton")).First().Remove();

            string json = HttpRequestsStringMovie("http://mobapps.cc/api/serials/get_movie_data/?id=" + id);
            JToken jFoo = JValue.Parse(json);

            var summary = jFoo["description"].Value<string>();
            elementDesc.First().SetValue(summary);
            var langs = jFoo["langs"].Value<JToken>();
            int intCount = int.Parse(plusCount);

            foreach (var lang in langs)
            {
                var apple = lang["apple"].Value<int>() + intCount;
                var google = lang["google"].Value<int>() + intCount;
                var microsoft = lang["microsoft"].Value<string>();

                var href = string.Format("https://vk.com/video_ext.php?oid={0}&id={1}&hash={2}", apple, google, microsoft); //lang["link"].Value<string>();
                if (!string.IsNullOrEmpty(href))
                {


                    var html = HttpRequestsString(href);
                    //var regex = new Regex("src=(\"http.*.mp4\")");
                    var regex = new Regex("<source(.*)</source>");
                    if (regex.IsMatch(html))
                    {
                        var match = regex.Match(html);
                        var groups = match.Groups;
                        var result = groups[0].Value;

                        var doc = new HtmlDocument();
                        doc.LoadHtml(result);
                        var sources = doc.DocumentNode.Descendants("source");
                        var langauge = lang["lang"].Value<string>();

                        foreach (var source in sources)
                        {
                            var link = source.GetAttributeValue("src", "");
                            if (link.Contains("360"))
                            {
                                CreateActionButton(link, xDocument, "360 " + langauge);
                                //Createlink(links, xDocument);
                            }
                            if (link.Contains("480"))
                            {
                                CreateActionButton(link, xDocument, "480 " + langauge);
                            }
                            if (link.Contains("720"))
                            {
                                CreateActionButton(link, xDocument, "720 " + langauge);
                            }
                        }

                        //var splits = result.Split("src=".ToCharArray());
                        //var urls = splits.ToList().FindAll(s => s.Contains("url"));

                        //foreach (var url in urls)
                        //{

                        //    var links = url.Split(':');
                        //    if (links[0].Contains("360"))
                        //    {
                        //        var link = (links[1].Remove(0, 1) + ":" + links[2].Remove(links[2].Length - 1)).Replace("\\", "");
                        //        CreateActionButton(link, xDocument, "360 " + langauge);
                        //        //Createlink(links, xDocument);
                        //    }
                        //    if (links[0].Contains("480"))
                        //    {
                        //        var link = (links[1].Remove(0, 1) + ":" + links[2].Remove(links[2].Length - 1)).Replace("\\", "");
                        //        CreateActionButton(link, xDocument, "480 " + langauge);
                        //    }
                        //    if (links[0].Contains("720"))
                        //    {
                        //        var link = (links[1].Remove(0, 1) + ":" + links[2].Remove(links[2].Length - 1)).Replace("\\", "");
                        //        CreateActionButton(link, xDocument, "720 " + langauge);
                        //    }
                        //}

                    }
                }

                //("http.*.mp4")

                //foreach (var childNode in video.ChildNodes)
                //{
                //    if (childNode.Name == "source")
                //    {
                //        var source = childNode.GetAttributeValue("src", "");
                //        var actionButtonElement = xDocument.Descendants("actionButton");
                //        actionButtonElement.First().SetAttributeValue("onSelect", string.Format("atv.loadURL('http://trailers.apple.com/Playmovie?url={0}')", Uri.EscapeDataString(source)));
                //        actionButtonElement.First().SetAttributeValue("onPlay", string.Format("atv.loadURL('http://trailers.apple.com/Playmovie?url={0}')", Uri.EscapeDataString(source)));
                //        break;
                //    }
                //}
            }

            return xDocument;
        }

        private static void Createlink(string[] links, XDocument xDocument)
        {
            var link = (links[1].Remove(0, 1) + ":" + links[2].Remove(links[2].Length - 1)).Replace("\\", "");
            var actionButtonElement = xDocument.Descendants("actionButton");
            actionButtonElement.First()
                               .SetAttributeValue("onSelect",
                                                  string.Format(
                                                      "atv.loadURL('http://trailers.apple.com/Playmovie?url={0}')",
                                                      Uri.EscapeDataString(link.ToString())));
            actionButtonElement.First()
                               .SetAttributeValue("onPlay",
                                                  string.Format(
                                                      "atv.loadURL('http://trailers.apple.com/Playmovie?url={0}')",
                                                      Uri.EscapeDataString(link.ToString())));
        }


        private string HttpRequestsStringMovie(string url)
        {

            var request = (HttpWebRequest)WebRequest.Create(url);
            // request.Headers.Add("User-Agent", "Movie Box 2.6 (iPhone; iPhone OS 6.0; ru_RU)");
            //request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
            //request.Headers.Add(HttpRequestHeader.AcceptLanguage, "en-US,en;q=0.8,he;q=0.6,ru;q=0.4");
            request.UserAgent = "Movie Box 2.6 (iPhone; iPhone OS 6.0; ru_RU)";

            using (var response = (HttpWebResponse)request.GetResponse())
            {
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    string result = reader.ReadToEnd();
                    return result;
                }
            }
        }

        public string HttpRequests(string url, string fileName)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);



            using (var response = (HttpWebResponse)request.GetResponse())
            {
                byte[] result = null;
                int byteCount = Convert.ToInt32(response.ContentLength);
                using (var reader = new BinaryReader(response.GetResponseStream()))
                {
                    result = reader.ReadBytes(byteCount);
                }

                using (ZipFile zip = ZipFile.Read(new MemoryStream(result)))
                {
                    foreach (ZipEntry e in zip)
                    {
                        e.Extract(MikrainProgramm._xmlPath + @"Content\ExtractMovies", ExtractExistingFileAction.OverwriteSilently);  // overwrite == true  
                    }
                    zip.Save(MikrainProgramm._xmlPath + @"Content\ExtractMovies\data_lite.zip");
                }

            }
            using (var reader = new StreamReader(MikrainProgramm._xmlPath + @"Content\ExtractMovies\" + fileName))
            {
                return reader.ReadToEnd();
            }
        }

        internal XDocument GetMovieShowDetails(string movieId, string image, string title, string seasons)
        {
            if (title == "search")
            {
                XDocument searchDoc = XDocument.Load(Path.Combine(MikrainProgramm._xmlPath, @"Content\searchtrailers.xml"));
                return searchDoc;
            }

            XDocument xDocument = GetXml(Path.Combine(MikrainProgramm._xmlPath, @"Content\showList.xml")).GetXDocument();

            for (int i = 1; i <= int.Parse(seasons); i++)
            {
                //http://mobapps.cc/api/serials/es/?id=23&season=4
                string url = string.Format("http://mobapps.cc/api/serials/es/?id={0}&season={1}", movieId, i);
                var summarylement = xDocument.Descendants(XName.Get("summary"));
                var titleElement = xDocument.Descendants(XName.Get("title"));
                var imageElement = xDocument.Descendants(XName.Get("image"));
                var items = xDocument.Descendants(XName.Get("items"));
                imageElement.First().SetValue(image);
                titleElement.First().SetValue(title);
                summarylement.First().SetValue("");

                CreateElementList(i, string.Format("loadTrailerDetailPage('http://trailers.apple.com/GetMoviewShowSeries?href={0}&imageHref={1}&title={2}&season={3}')", Uri.EscapeDataString(url), Uri.EscapeDataString(image), Uri.EscapeDataString(title), Uri.EscapeDataString("Season " + i)), "Season " + i, image, items.First());
            }
            return xDocument;
        }

        internal async Task<XDocument> GetMovieShowSeries(string href, string image, string title, string season)
        {
            XDocument xDocument = GetXml(Path.Combine(MikrainProgramm._xmlPath, @"Content\showList.xml")).GetXDocument();
            var result = HttpRequestsStringMovie(href);

            var query = new Uri(href).Query;
            var splittedValue = query.Split('=');
            var id = splittedValue[1].Replace("&season", "");
            var seasonNum = splittedValue[2];

            var doc = new HtmlDocument();
            doc.LoadHtml(result);

            JToken jFoo = JValue.Parse(doc.DocumentNode.InnerText);
            var thumbs = jFoo["thumbs"];
            var movieBoxSeries = new MovieBoxSeries();
            foreach (var thumbTmp in thumbs)
            {
                var splitted = thumbTmp.ToString().Split(':');
                if (splitted.Count() > 1)
                {
                    var thumb = new Thumbs();
                    thumb.id = splitted[0];

                    thumb.thumb = splitted[1] + ":" + splitted[2];
                    thumb.thumb = thumb.thumb.Remove(0, 2);
                    thumb.thumb = thumb.thumb.Remove(thumb.thumb.Length - 1);
                    movieBoxSeries.thumbs.Add(thumb);

                    //http://mobapps.cc/api/serials/e/?h=61&u=12&y=16
                    var movieBoxEpisodes = new MovieBoxEpisodes();
                    movieBoxEpisodes.movieBoxEpisode.Add(new MovieBoxEpisode() { id = id, episode = thumb.id.Replace("\"", ""), season = seasonNum, link = string.Format("http://mobapps.cc/api/serials/e/?h={0}&u={1}&y={2}", id, seasonNum, thumb.id.Replace("\"", "")) });
                    movieBoxSeries.movieBoxEpisodeses.Add(movieBoxEpisodes);
                }
            }

            //var episodes = jFoo["episodes"];
            //for (int i = episodes.Count() - 1; i >= 0; i--)
            //{
            //    var movieBoxEpisodes = new MovieBoxEpisodes();
            //    foreach (var episodeTmp in episodes.ElementAt(i))
            //    {
            //        foreach (var tmp in episodeTmp)
            //        {
            //            var epi = JsonSerializer.Create().Deserialize<MovieBoxEpisode>(new JTokenReader(tmp));
            //            movieBoxEpisodes.movieBoxEpisode.Add(epi);
            //        }
            //        movieBoxSeries.movieBoxEpisodeses.Add(movieBoxEpisodes);
            //    }
            //}


            //foreach (var episode in episodes)
            //{

            //}


            var description = jFoo["description"];

            var imageElement = xDocument.Descendants(XName.Get("image"));
            var titleElement = xDocument.Descendants(XName.Get("title"));
            var summaryElement = xDocument.Descendants(XName.Get("summary"));
            var items = xDocument.Descendants(XName.Get("items"));
            imageElement.First().SetValue(image);
            titleElement.First().SetValue(title);
            summaryElement.First().SetValue(description.ToString());

            for (int i = 0; i < movieBoxSeries.movieBoxEpisodeses.Count; i++)
            {
                try
                {


                    string ru = "";
                    string en = "";

                    foreach (var episode in movieBoxSeries.movieBoxEpisodeses[i].movieBoxEpisode)
                    {

                        var links = await Test(episode);
                        foreach (var linkTmp in links)
                        {
                            if (!string.IsNullOrEmpty(linkTmp.link))
                            {
                                if (linkTmp.lang == "en")
                                {
                                    en = linkTmp.link;
                                }
                                else
                                {
                                    ru = linkTmp.link;
                                }
                            }
                        }

                    }
                    CreateElementList(i, string.Format("loadTrailerDetailPage('http://trailers.apple.com/GetMoviewShowEpisode?hrefEn={0}&hrefRu={1}&imageHref={2}&title={3}')", Uri.EscapeDataString(en), Uri.EscapeDataString(ru), Uri.EscapeDataString(movieBoxSeries.thumbs[i].thumb), Uri.EscapeDataString(title)), "Episode " + i, movieBoxSeries.thumbs[i].thumb, items.First());
                }
                catch (Exception)
                {


                }
            }


            return xDocument;
        }

        private async Task<List<MovieBoxEpisode>> Test(MovieBoxEpisode episode)
        {
            var result = await SendPostRequest(episode.link, new Dictionary<string, string>() { { "h", episode.id }, { "u", episode.season }, { "y", episode.episode } }, "Movie Box 2.6 (iPhone; iPhone OS 6.0; ru_RU)");
            JToken jFoo = JValue.Parse(result);
            var plus = int.Parse(episode.id) + int.Parse(episode.season) + int.Parse(episode.episode);
            List<MovieBoxEpisode> apisodes = new List<MovieBoxEpisode>();
            foreach (var link in jFoo)
            {
                var episodeTmp = new MovieBoxEpisode();
                var numPlus = link["id"].Value<int>() + 537;
                var appleLink = link["apple"].Value<int>() + plus;
                var googleLink = link["google"].Value<int>() + plus;
                var lang = link["lang"].Value<string>();
                var micr = link["microsoft"].Value<string>();
                episodeTmp.link = string.Format("https://vk.com/video_ext.php?oid={0}&id={1}&hash={2}&rand=1398544684", appleLink, googleLink, micr);
                episodeTmp.lang = lang;
                apisodes.Add(episodeTmp);
            }
            return apisodes;
        }

        internal XDocument Searchmovie(string q)
        {
            XDocument xDocument =
         XDocument.Load(Path.Combine(MikrainProgramm._xmlPath, @"Content\searchResult.xml"));
            int count = 0;

            string endPoint = "movieBoxMovieDetails";
            if (isShow)
            {
                endPoint = "movieBoxShowDetails";
            }

            var items = xDocument.Descendants(XName.Get("items"));
            int countPlus = 537;// GetAllId() + 10;
            foreach (var movieBoxObject in sortedList.FindAll(o => o.title.ToLower().Contains(q.ToLower())))
            {
                var posterMenuItem = new XElement(XName.Get("posterMenuItem"));
                posterMenuItem.Add(new XAttribute(XName.Get("id"), "movieBoxObject_" + count++));
                posterMenuItem.Add(new XAttribute(XName.Get("accessibilityLabel"), movieBoxObject.title.Replace(" ", "")));
                posterMenuItem.Add(new XAttribute(XName.Get("onSelect"), string.Format(
                                                      "atv.loadURL('http://trailers.apple.com/{3}?movieId={0}&image={1}&title={2}&seasons={4}&count={5}')",
                                                      movieBoxObject.id, Uri.EscapeDataString(movieBoxObject.poster), Uri.EscapeDataString(movieBoxObject.title), endPoint, movieBoxObject.seasons, int.Parse(movieBoxObject.id) + countPlus)));
                posterMenuItem.Add(new XAttribute(XName.Get("onPlay"), string.Format(
                                                      "atv.loadURL('http://trailers.apple.com/{3}?movieId={0}&image={1}&title={2}&seasons={4}&count={5}')",
                                                      movieBoxObject.id, Uri.EscapeDataString(movieBoxObject.poster), Uri.EscapeDataString(movieBoxObject.title), endPoint, movieBoxObject.seasons, int.Parse(movieBoxObject.id) + countPlus)));
                posterMenuItem.Add(new XElement(XName.Get("label"), movieBoxObject.title));
                posterMenuItem.Add(new XElement(XName.Get("image"), movieBoxObject.poster));
                items.First().Add(posterMenuItem);
            }
            return xDocument;
        }


        internal XDocument GetMoviewShowEpisode(string hrefEn, string hrefRu, string imageHref, string title)
        {


            XDocument xDocument =
            XDocument.Load(Path.Combine(MikrainProgramm._xmlPath, @"Content\movie.xml"));
            xDocument.Descendants(XName.Get("image")).First().SetValue(imageHref);
            var elementDesc = xDocument.Descendants(XName.Get("summary"));
            xDocument.Descendants(XName.Get("title")).First().SetValue(title);
            xDocument.Descendants(XName.Get("actionButton")).First().Remove();

            if (!string.IsNullOrEmpty(hrefEn))
            {
                var htmlEn = HttpRequestsString(hrefEn);
                GetPlayPage(htmlEn, xDocument, "en");
            }
            if (!string.IsNullOrEmpty(hrefRu))
            {
                var htmlRu = HttpRequestsString(hrefRu);
                GetPlayPage(htmlRu, xDocument, "ru");
            }
            return xDocument;
        }



        private void GetPlayPage(string html, XDocument xDocument, string langauge)
        {

            // var html = HttpRequestsString(htmlEn);
            //var regex = new Regex("src=(\"http.*.mp4\")");
            var regex = new Regex("<source(.*)</source>");
            if (regex.IsMatch(html))
            {
                var match = regex.Match(html);
                var groups = match.Groups;
                var result = groups[0].Value;

                var doc = new HtmlDocument();
                doc.LoadHtml(result);
                var sources = doc.DocumentNode.Descendants("source");
                //var langauge = lang["lang"].Value<string>();

                foreach (var source in sources)
                {
                    var link = source.GetAttributeValue("src", "");
                    if (link.Contains("360"))
                    {
                        CreateActionButton(link, xDocument, "360 " + langauge);
                        //Createlink(links, xDocument);
                    }
                    if (link.Contains("480"))
                    {
                        CreateActionButton(link, xDocument, "480 " + langauge);
                    }
                    if (link.Contains("720"))
                    {
                        CreateActionButton(link, xDocument, "720 " + langauge);
                    }
                }

                //var splits = result.Split("src=".ToCharArray());
                //var urls = splits.ToList().FindAll(s => s.Contains("url"));

                //foreach (var url in urls)
                //{

                //    var links = url.Split(':');
                //    if (links[0].Contains("360"))
                //    {
                //        var link = (links[1].Remove(0, 1) + ":" + links[2].Remove(links[2].Length - 1)).Replace("\\", "");
                //        CreateActionButton(link, xDocument, "360 " + langauge);
                //        //Createlink(links, xDocument);
                //    }
                //    if (links[0].Contains("480"))
                //    {
                //        var link = (links[1].Remove(0, 1) + ":" + links[2].Remove(links[2].Length - 1)).Replace("\\", "");
                //        CreateActionButton(link, xDocument, "480 " + langauge);
                //    }
                //    if (links[0].Contains("720"))
                //    {
                //        var link = (links[1].Remove(0, 1) + ":" + links[2].Remove(links[2].Length - 1)).Replace("\\", "");
                //        CreateActionButton(link, xDocument, "720 " + langauge);
                //    }
                //}

            }



            //var regex = new Regex("(\"url.*\":\"http.*.mp4\")");
            //if (regex.IsMatch(htmlEn))
            //{
            //    var match = regex.Match(htmlEn);
            //    var groups = match.Groups;
            //    var result = groups[0].Value;
            //    var splits = result.Split(',');
            //    var urls = splits.ToList().FindAll(s => s.Contains("url"));

            //    foreach (var url in urls)
            //    {
            //        var links = url.Split(':');
            //        if (links[0].Contains("360"))
            //        {
            //            var link = (links[1].Remove(0, 1) + ":" + links[2].Remove(links[2].Length - 1)).Replace("\\", "");
            //            CreateActionButton(link, xDocument, "360 " + langauge);
            //            //Createlink(links, xDocument);
            //        }
            //        if (links[0].Contains("480"))
            //        {
            //            var link = (links[1].Remove(0, 1) + ":" + links[2].Remove(links[2].Length - 1)).Replace("\\", "");
            //            CreateActionButton(link, xDocument, "480 " + langauge);
            //        }
            //        if (links[0].Contains("720"))
            //        {
            //            var link = (links[1].Remove(0, 1) + ":" + links[2].Remove(links[2].Length - 1)).Replace("\\", "");
            //            CreateActionButton(link, xDocument, "720 " + langauge);
            //        }
            //    }
            //}
        }
    }
}
