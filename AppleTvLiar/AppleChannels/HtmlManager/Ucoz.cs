using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Configuration;
using System.Xml;
using System.Xml.Linq;
using HtmlAgilityPack;
using MikrainService;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Net.Http;
using AppleTvLiar.Helper;

namespace AppleTvLiar.AppleChannels.HtmlManager
{
    public class Ucoz : AppleBase
    {
        public Ucoz()
        {
            KillAce();
        }

        public async Task<XDocument> SearchUkoz(string query)
        {
            XDocument xDocument =
        XDocument.Load(Path.Combine(MikrainProgramm._xmlPath, @"Content\searchResult.xml"));
            int count = 0;

            string endPoint = "http://hd-720.ucoz.ru/load/";
            var dict = new Dictionary<string, string>();
            dict.Add("a", "2");
            dict.Add("query", query);


            var sresult = await SendPostRequest(endPoint, dict);

            var doc = new HtmlDocument();

            doc.OptionDefaultStreamEncoding = new UnicodeEncoding();
            doc.OptionOutputAsXml = true;
            doc.OptionAutoCloseOnEnd = true;
            doc.OptionWriteEmptyNodes = true;
            doc.OptionFixNestedTags = true;
            doc.OptionReadEncoding = false;
            doc.LoadHtml(sresult);



            //var divs = doc.GetElementbyId("allEntries");

            var divs = doc.DocumentNode.Descendants("div");
            var items = xDocument.Descendants(XName.Get("items"));
            foreach (var div in divs)
            {
                try
                {
                    if (div.GetAttributeValue("id", "").Contains("entryID"))
                    {
                        var inner = new HtmlDocument();
                        inner.LoadHtml(div.InnerHtml);

                        var imageDoc = inner.DocumentNode.SelectSingleNode("//table/tr/td/div[2]/img");
                        if (imageDoc == null) imageDoc = inner.DocumentNode.SelectSingleNode("//table/tr/td/div[2]/div/img");

                        string image = "";
                        if (imageDoc != null)
                            image = imageDoc.GetAttributeValue("src", "").Replace("http://hd-720.ucoz.ru", "");

                        var name = inner.DocumentNode.SelectSingleNode("//table/tr/td/div[1]/a/span/b/span").InnerText;
                        var href = inner.DocumentNode.SelectSingleNode("//table/tr/td/span[2]/a").GetAttributeValue("href", "");

                        name = name.Replace("Смотреть", "").Replace("онлайн", "");

                        var posterMenuItem = new XElement(XName.Get("posterMenuItem"));
                        posterMenuItem.Add(new XAttribute(XName.Get("id"), "movieBoxObject_" + count++));
                        posterMenuItem.Add(new XAttribute(XName.Get("accessibilityLabel"), name.Replace(" ", "")));
                        posterMenuItem.Add(new XAttribute(XName.Get("onSelect"), string.Format(
                                                              "atv.loadURL('http://trailers.apple.com/getUcozMovie?movie={0}&imageUrl={1}&movieTitle={2}')",
                                                             Uri.EscapeDataString(href), Uri.EscapeDataString(image), Uri.EscapeDataString(name))));
                        posterMenuItem.Add(new XAttribute(XName.Get("onPlay"), string.Format(
                                                              "atv.loadURL('http://trailers.apple.com/getUcozMovie?movie={0}&imageUrl={1}&movieTitle={2}')",
                                                             Uri.EscapeDataString(href), Uri.EscapeDataString(image), Uri.EscapeDataString(name))));
                        posterMenuItem.Add(new XElement(XName.Get("label"), name));
                        posterMenuItem.Add(new XElement(XName.Get("image"), "http://hd-720.ucoz.ru/" + image));
                        items.First().Add(posterMenuItem);
                    }
                }
                catch (Exception ex)
                {
                }


            }

            return xDocument;
        }

        List<Task> threads = new List<Task>();
        public async Task<XDocument> GetContent(string url = "http://hd-720.ucoz.ru/load/novinki/29-{0}-2", string cacheName = "ucoz")
        {

            var cacheDoc = ReadDoc(cacheName);
            if (cacheDoc != null)
            {
                return cacheDoc;
            }

            XDocument xDocument =
            XDocument.Load(Path.Combine(MikrainProgramm._xmlPath, @"Content\categories.xml"));
            var items = xDocument.Descendants(XName.Get("items"));
            var elements = new Dictionary<int, List<XElement>>();



            Console.WriteLine(DateTime.Now);
            for (int i = 1; i < 15; i++)
            {
                int i1 = i;
                threads.Add(Task.Run(() => AddToContent(url, i1, elements)));

            }

            Task.WaitAll(threads.ToArray());
            for (int i = 1; i < elements.Count; i++)
            {
                if (elements.ContainsKey(i))
                {
                    items.First().Add(elements[i][0]);
                    items.First().Add(elements[i][1]);
                }
            }

            elements.Clear();
            //threads.Clear();
            Console.WriteLine(DateTime.Now);
            SaveDoc(cacheName, xDocument);
            return xDocument;
        }

        private void AddToContent(string url, int i, Dictionary<int, List<XElement>> elements)
        {
            //var th1 = new Thread(() =>
            //               {
            int count = 0;
            var collectionDividerElement = new XElement(XName.Get("collectionDivider"));
            collectionDividerElement.SetAttributeValue(XName.Get("alignment"), "left");
            collectionDividerElement.SetAttributeValue(XName.Get("accessibilityLabel"), i);
            collectionDividerElement.Add(new XElement(XName.Get("title"), string.Format("Page number {0}", i)));
            var shelfElement = new XElement(XName.Get("shelf"));
            shelfElement.SetAttributeValue(XName.Get("id"), string.Format("shelf_{0}", i));

            var sectionsElement = new XElement(XName.Get("sections"));
            var shelfSectionElement = new XElement(XName.Get("shelfSection"));
            var itemsElement = new XElement(XName.Get("items"));

            shelfSectionElement.Add(itemsElement);
            sectionsElement.Add(shelfSectionElement);
            shelfElement.Add(sectionsElement);

            Console.WriteLine(string.Format(url, i));
            var html = HttpRequests(string.Format(url, i));
            var doc = new HtmlDocument();
            doc.OptionDefaultStreamEncoding = new UnicodeEncoding();
            doc.OptionOutputAsXml = true;
            doc.OptionAutoCloseOnEnd = true;
            doc.OptionWriteEmptyNodes = true;
            doc.OptionFixNestedTags = true;
            doc.OptionReadEncoding = false;
            doc.LoadHtml(html);


            //var divs = doc.GetElementbyId("allEntries");

            if (i == 1)
            {
                CreateElementList(count++, string.Format(
                    "atv.loadURL('http://trailers.apple.com/getUcozMovie?movie={0}&imageUrl={1}&movieTitle={2}')",
                    Uri.EscapeDataString("search"), "", ""), "search", "http://www.kudoschatsearch.com/images/search.png",
                    itemsElement);
            }


            var divs = doc.DocumentNode.Descendants("div");

            foreach (var div in divs)
            {
                if (div.GetAttributeValue("id", "").Contains("entryID"))
                {
                    var strongs = div.ChildNodes.Descendants("strong");

                    var inner = new HtmlDocument();
                    inner.LoadHtml(div.InnerHtml);

                    var image = inner.DocumentNode.SelectSingleNode("//table/tr/td/div[2]/img").GetAttributeValue("src", "");
                    var name = inner.DocumentNode.SelectSingleNode("//table/tr/td/div[1]/a/span/b/span").InnerText;
                    var href = inner.DocumentNode.SelectSingleNode("//table/tr/td/span[2]/a").GetAttributeValue("href", "");

                    name = name.Replace("Смотреть", "").Replace("онлайн", "").Replace("сериал", "");

                    CreateElementList(count++,
                        string.Format(
                            "atv.loadURL('http://trailers.apple.com/getUcozMovie?movie={0}&imageUrl={1}&movieTitle={2}')",
                            Uri.EscapeDataString(href), Uri.EscapeDataString(image), Uri.EscapeDataString(name)), name,
                        "http://hd-720.ucoz.ru/" + image, itemsElement);
                }
            }

            if (elements != null)
                if (!elements.ContainsKey(i))
                    elements.Add(i, new List<XElement>() { collectionDividerElement, shelfElement });
            //items.First().Add(collectionDividerElement);
            //items.First().Add(shelfElement);
            Completed(elements);
            //               });

            //th1.Start();
            //threads.Add(th1);
        }

        private void Completed(Dictionary<int, List<XElement>> elements)
        {
        }


        public async Task<XDocument> GetMovie(string url, string imageUrl, string movieTitle)
        {
            var cacheDoc = ReadDoc(movieTitle);
            if (cacheDoc != null)
            {
                return cacheDoc;
            }


            if (url == "search")
            {
                XDocument searchDoc = XDocument.Load(Path.Combine(MikrainProgramm._xmlPath, @"Content\searchtrailers.xml"));
                var baseUrl = searchDoc.Descendants(XName.Get("baseURL"));
                var firstOrDefault = baseUrl.FirstOrDefault();
                if (firstOrDefault != null)
                    firstOrDefault.SetValue("http://trailers.apple.com/UcozSearch?query=");
                return searchDoc;
            }

            var html = HttpRequests("http://hd-720.ucoz.ru/" + url);

            try
            {

                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                string desc = "";

                var tables = doc.DocumentNode.Descendants("table");
                foreach (var htmlNode in tables)
                {
                    if (htmlNode.GetAttributeValue("class", "") == "eBlock")
                    {
                        var splitted = htmlNode.InnerText.Split('\n').Where(s => !string.IsNullOrEmpty(s)).ToList();
                        for (int i = 0; i < splitted.Count(); i++)
                        {
                            if (i != 1 || i != 4)
                            {
                                if (i == 5)
                                {
                                    var actors = splitted[i];
                                    desc += actors.Remove(50, actors.Length-51) + "..." + Environment.NewLine;
                                }
                                else
                                {
                                    desc += splitted[i] + Environment.NewLine;
                                }
                            }
                        }
                        break;
                    }
                }

                var options = doc.DocumentNode.Descendants("iframe");

                if (url.Contains("serialy") && options.Any() && options.Any(node => node.GetAttributeValue("src", "").Contains("moonwalk") || node.GetAttributeValue("src", "").Contains("serpens")))
                {

                    ////*[@id="contanier"]/table[1]/tbody/tr[2]/td[2]/div/table[2]/tbody/tr[2]/td


                    var srcRedirect = options.FirstOrDefault(node => node.GetAttributeValue("src", "").Contains("moonwalk") || node.GetAttributeValue("src", "").Contains("serpens"));
                    var urlRedirect = srcRedirect.GetAttributeValue("src", "");

                    var htmlRedirect = HttpRequests(urlRedirect);

                    doc.LoadHtml(htmlRedirect);
                    var season = doc.GetElementbyId("season");

                    var seasonoptions = season.Descendants("option");

                    XDocument xDocument = GetXml(Path.Combine(MikrainProgramm._xmlPath, @"Content\showList.xml")).GetXDocument();
                    imageUrl = "http://hd-720.ucoz.ru/" + imageUrl;



                    for (int i = 0; i < seasonoptions.Count(); i++)
                    {
                        var idxSeas = urlRedirect.IndexOf("season=");
                        string urlSeason = "";
                        if (idxSeas >= 0)
                        {
                            urlSeason = urlRedirect.Remove(idxSeas + 7, 1).Replace("season=", "season=" + seasonoptions.ElementAt(i).GetAttributeValue("value", ""));

                        }
                        else
                        {
                            urlSeason = urlRedirect + "?season=" + seasonoptions.ElementAt(i).GetAttributeValue("value", "");
                        }

                        var summarylement = xDocument.Descendants(XName.Get("summary"));
                        var titleElement = xDocument.Descendants(XName.Get("title"));
                        var imageElement = xDocument.Descendants(XName.Get("image"));
                        var items = xDocument.Descendants(XName.Get("items"));
                        imageElement.First().SetValue(imageUrl);
                        titleElement.First().SetValue(movieTitle);
                        summarylement.First().SetValue(desc);

                        // "loadTrailerDetailPage('http://trailers.apple.com/ShowUcozSer?ser={0}&title={1}&image={2}')",
                        //
                        CreateElementList(i, string.Format("loadTrailerDetailPage('http://trailers.apple.com/ShowUcozEpisodes?href={0}&imageHref={1}&title={2}&season={3}')", Uri.EscapeDataString(urlSeason), Uri.EscapeDataString(imageUrl), Uri.EscapeDataString(movieTitle), Uri.EscapeDataString("Season " + i)), "Season " + (i + 1), imageUrl, items.First());
                    }



                    return xDocument;

                    var xdoc = GetSeries(options, movieTitle, imageUrl);
                    SaveDoc(movieTitle, xdoc);
                    return xdoc;
                }
                else
                {

                    XDocument xDocument =
           XDocument.Load(Path.Combine(MikrainProgramm._xmlPath, @"Content\movie.xml"));
                    var element = xDocument.Descendants(XName.Get("image"));
                    var elementDesc = xDocument.Descendants(XName.Get("summary"));
                    var elementName = xDocument.Descendants(XName.Get("title"));
                    var actionButtonElement = xDocument.Descendants("actionButton");
                    actionButtonElement.Remove();

                    ////*[@id="contanier"]/table[1]/tbody/tr[2]/td[2]/div/table[2]/tbody/tr[2]/td/br[7]
                    ////*[@id="contanier"]
                    var strongs =
                        doc.DocumentNode.SelectSingleNode(
                            "//*[@id=\"contanier\"]/table[7]");

                    try
                    {
                        var frames = doc.DocumentNode.Descendants("iframe");
                        foreach (var htmlNode in frames)
                        {
                            try
                            {
                                var source = htmlNode.GetAttributeValue("src", "");

                                string videoSource = source;// source.Replace("iframe", "index.m3u8");

                                if (videoSource.Contains("kodik.biz/"))
                                {
                                    var htmlSources = HttpRequests(source);
                                }

                                //if (!videoSource.Contains("moonwalk.cc"))
                                //{
                                //    //setRequestHeader|1450741237.77cf690302c1338469e923dd6ddd2ed8|beforeSend
                                //    var htmlSources = HttpRequests(source);
                                //    var document = new HtmlDocument();
                                //    document.LoadHtml(htmlSources);

                                //    var video = document.GetElementbyId("video");

                                //    foreach (var childNode in video.ChildNodes)
                                //    {
                                //        if (childNode.Name == "source")
                                //        {
                                //            videoSource = childNode.GetAttributeValue("src", "");
                                //            var points = videoSource.Split('.');

                                //            CreateActionButton(videoSource, xDocument, points[points.Count() - 2]);
                                //        }
                                //    }
                                //}

                                if (videoSource.Contains("moonwalk.cc") || true)
                                {
                                    var htmlSources = HttpRequests(source);
                                    var document = new HtmlDocument();
                                    document.LoadHtml(htmlSources);

                                    var link = await GetLink(document);
                                    if (string.IsNullOrEmpty(link)) continue;

                                    //var text = document.DocumentNode.InnerText;

                                    //var urlEnd = Regex.Match(text, "var session_url = '(.*?)'").Groups[1].Value;

                                    //var lookIntoThis = Regex.Match(text, "session_url, (.*?(\n))+.*?");
                                    //var eval = Regex.Match(text, "setRequestHeader\\|\\|(.*?)\\|beforeSend").Groups[1].Value;
                                    //var xmoonExp = Regex.Match(text, "X-MOON-EXPIRED', \"(.*)\"").Groups[1].Value;
                                    //var xmoonToken = Regex.Match(text, "X-MOON-TOKEN', \"(.*)\"").Groups[1].Value;
                                    //var video_token = Regex.Match(lookIntoThis.Value, "video_token: '(.*)'").Groups[1].Value;
                                    //var access_key = Regex.Match(lookIntoThis.Value, "access_key: '(.*)'").Groups[1].Value;
                                    //var d_id = Regex.Match(lookIntoThis.Value, "mw_did: (.*)").Groups[1].Value;

                                    //if (string.IsNullOrEmpty(eval)) continue;

                                    //string query;

                                    //                                    using (var client = new HttpClient())
                                    //                                    {
                                    //                                        using (var content = new FormUrlEncodedContent(new KeyValuePair<string, string>[]{
                                    //                                        new KeyValuePair<string, string>("ham", "Glazed?"),
                                    //                                        new KeyValuePair<string, string>("x-men", "Wolverine + Logan"),
                                    //                                        new KeyValuePair<string, string>("Time", DateTime.UtcNow.ToString()),
                                    //}))
                                    //                                        {
                                    //                                            query = content.ReadAsStringAsync().Result;


                                    //                                            var response = client.PostAsync("http://moonwalk.cc/sessions/create_session", content).Result;
                                    //                                            if (!response.IsSuccessStatusCode)
                                    //                                            {
                                    //                                                return null;
                                    //                                            }
                                    //                                            var res = response.Content.ReadAsStreamAsync().Result;

                                    //                                        }
                                    //                                    }


                                    //var result = await SendMoonRequest(xmoonExp, eval, "http://moonwalk.cc" + urlEnd, new Dictionary<string, string>() { { "mw_did", d_id }, { "video_token", video_token }, { "access_key", access_key }, { "content_type", "movie" } });


                                    //var jO = JObject.Parse(result);
                                    //var link = jO["manifest_m3u8"].Value<string>();
                                    if (link != null)
                                    {
                                        videoSource = link;
                                    }



                                    //var video = document.GetElementbyId("video");

                                    //foreach (var childNode in video.ChildNodes)
                                    //{
                                    //    if (childNode.Name == "source")
                                    //    {
                                    //        videoSource = childNode.GetAttributeValue("src", "");
                                    //        var points = videoSource.Split('.');

                                    //        CreateActionButton(videoSource, xDocument, points[points.Count() - 2]);
                                    //    }
                                    //}
                                }

                                CreateActionButton(videoSource, xDocument, "hd");
                                break;
                            }
                            catch (Exception)
                            {


                            }
                        }

                        element.First().SetValue("http://hd-720.ucoz.ru/" + imageUrl);
                        elementName.First().SetValue(movieTitle);


                        elementDesc.First().SetValue(desc);
                    }
                    catch (Exception exc)
                    {

                    }


                    var ucozCache = url.Contains("multfilm") ? ReadDoc("mult") : ReadDoc("ucoz");
                    if (ucozCache != null)
                    {
                        var posters = ucozCache.Descendants(XName.Get("moviePoster")).ToList();
                        posters.ShuffleList();
                        foreach (var xElement in posters.Take(10))
                        {
                            xDocument.Descendants("items").ToList()[1].Add(xElement);
                        }
                    }

                    //SaveDoc(movieTitle, xDocument);
                    return xDocument;
                }


            }
            catch (Exception exc)
            {
                return Error(exc);
            }

            return null;
        }


        public XDocument ShowUcozEpisodes(string href, string image, string title, string season)
        {

            XDocument xDocument = GetXml(Path.Combine(MikrainProgramm._xmlPath, @"Content\showList.xml")).GetXDocument();

            var htmlRedirect = HttpRequests(href);
            var doc = new HtmlDocument();
            doc.LoadHtml(htmlRedirect);
            var episode = doc.GetElementbyId("episode");

            var episodeoptions = episode.Descendants("option");

            var imageElement = xDocument.Descendants(XName.Get("image"));
            var titleElement = xDocument.Descendants(XName.Get("title"));
            var summaryElement = xDocument.Descendants(XName.Get("summary"));
            var items = xDocument.Descendants(XName.Get("items"));
            imageElement.First().SetValue(image);
            titleElement.First().SetValue(title);
            //summaryElement.First().SetValue(description.ToString());

            for (int i = 0; i < episodeoptions.Count(); i++)
            {
                var idxSeas = href.IndexOf("episode=");
                string urlEpisode = href;
                if (idxSeas > 0)
                {
                    urlEpisode = href.Remove(idxSeas + 7, 1).Replace("season=", "season=" + episodeoptions.ElementAt(i).GetAttributeValue("value", ""));

                }
                else
                {
                    urlEpisode += "&episode=" + episodeoptions.ElementAt(i).GetAttributeValue("value", "");
                }

                CreateElementList(i, string.Format("loadTrailerDetailPage('http://trailers.apple.com/ShowUcozSer?ser={0}&image={1}&title={2}')", Uri.EscapeDataString(urlEpisode), Uri.EscapeDataString(image), Uri.EscapeDataString(title)), "Episode " + (i + 1), image, items.First());

            }



            //for (int i = 0; i < movieBoxSeries.movieBoxEpisodeses.Count; i++)
            //{ }

            return xDocument;

        }



        public async Task<XDocument> ShowUcozSer(string ser, string title, string image)
        {
            XDocument xDocument =
         XDocument.Load(Path.Combine(MikrainProgramm._xmlPath, @"Content\movie.xml"));
            var element = xDocument.Descendants(XName.Get("image"));
            var elementDesc = xDocument.Descendants(XName.Get("summary"));
            var elementName = xDocument.Descendants(XName.Get("title"));
            var actionButtonElement = xDocument.Descendants("actionButton");
            actionButtonElement.Remove();


            var htmlSources = HttpRequests(ser);
            var document = new HtmlDocument();
            document.LoadHtml(htmlSources);

            var link = await GetLink(document);
            if (link != null)
            {
                CreateActionButton(link, xDocument, "HD");
            }


            element.First().SetValue(image);
            elementName.First().SetValue(title);
            elementDesc.First().SetValue("Temprory unavailable");

            //var tables = document.DocumentNode.Descendants("table");
            //foreach (var htmlNode in tables)
            //{
            //    if (htmlNode.GetAttributeValue("class", "") == "eBlock")
            //    {
            //        var desc = htmlNode.InnerText;
            //        elementDesc.First().SetValue(desc);
            //        break;
            //    }
            //}

            var ucozCache = ser.Contains("multfilm") ? ReadDoc("mult") : ReadDoc("serialy");
            if (ucozCache != null)
            {
                var posters = ucozCache.Descendants(XName.Get("moviePoster")).ToList();
                posters.ShuffleList();
                foreach (var xElement in posters.Take(10))
                {
                    xDocument.Descendants("items").ToList()[1].Add(xElement);
                }
            }

            return xDocument;
        }




        private Thread GetInfo(int count, HtmlNode optionNode, string image, Dictionary<int, SerInfo> info)
        {
            var th1 = new Thread(() =>
            {
                try
                {
                    var valueVk = optionNode.GetAttributeValue("value", "");
                    var titleSer = optionNode.NextSibling.InnerText;

                    if (valueVk.StartsWith("//"))
                    {
                        valueVk = valueVk.Replace("//", "http://");
                    }

                    var htmlSources = HttpRequests(valueVk);
                    var document = new HtmlDocument();

                    document.LoadHtml(htmlSources);
                    var video = document.GetElementbyId("video");
                    string poster = "http://hd-720.ucoz.ru/" + image;
                    if (video != null)
                    {
                        poster = video.GetAttributeValue("poster", "");


                    }

                    info.Add(count, new SerInfo() { image = poster, title = titleSer, url = valueVk });


                }
                catch (Exception)
                {

                }
            });
            return th1;
        }

        internal XDocument GetSeries(IEnumerable<HtmlNode> options, string name, string image)
        {
            XDocument xDocument = XDocument.Load(Path.Combine(MikrainProgramm._xmlPath, @"Content\showList.xml"));

            var imageElement = xDocument.Descendants(XName.Get("image"));
            var items = xDocument.Descendants(XName.Get("items"));
            var titleElement = xDocument.Descendants(XName.Get("title"));
            var descElement = xDocument.Descendants(XName.Get("summary"));
            titleElement.First().SetValue(name);
            descElement.First().SetValue("");
            imageElement.First().SetValue("http://hd-720.ucoz.ru/" + image);
            int count = 0;
            var threads = new List<Thread>();
            var info = new Dictionary<int, SerInfo>();


            foreach (var optionNode in options)
            {
                var th1 = GetInfo(count++, optionNode, image, info);
                th1.Start();
                threads.Add(th1);
            }

            foreach (var thread in threads)
            {
                thread.Join();
            }


            for (int i = 0; i < info.Count; i++)
            {
                if (info.ContainsKey(i))
                {
                    CreateElementList(i,
                        string.Format(
                            "loadTrailerDetailPage('http://trailers.apple.com/ShowUcozSer?ser={0}&title={1}&image={2}')",
                            Uri.EscapeDataString(info[i].url), Uri.EscapeDataString(info[i].title),
                            Uri.EscapeDataString(info[i].image)), info[i].title, info[i].image, items.First());
                }
            }

            info.Clear();
            threads.Clear();

            return xDocument;
        }


        public XDocument PlayMovie(string url)
        {
            XDocument xDocument =
           XDocument.Load(Path.Combine(MikrainProgramm._xmlPath, @"Content\Playmovie.xml"));
            var element = xDocument.Descendants(XName.Get("httpFileVideoAsset"));
            element.First().AddFirst(new XElement(XName.Get("mediaURL"), url));
            return xDocument;
        }





        public string HttpRequests(string url)
        {

            var request = (HttpWebRequest)WebRequest.Create(url);
            request.UserAgent = "Mozilla/5.0 (iPad; CPU OS 5_1 like Mac OS X; en-us) AppleWebKit/534.46 (KHTML, like Gecko) Version/5.1 Mobile/9B176 Safari/7534.48.3";
            request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
            request.Headers.Add("DNT", "1");
            //request.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip,deflate,sdch");
            //request.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip,deflate,sdch");
            request.Headers.Add(HttpRequestHeader.AcceptLanguage, "en-US,en;q=0.8,he;q=0.6,ru;q=0.4");
            using (var response = (HttpWebResponse)request.GetResponse())
            {


                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    string result = reader.ReadToEnd();
                    return result;
                }
            }
        }
    }

    public class SerInfo
    {
        public string url;
        public string title;
        public string image;
    }

}
