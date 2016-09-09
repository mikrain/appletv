using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using HtmlAgilityPack;
using MikrainService;
using System.Threading;

namespace AppleTvLiar.AppleChannels.HtmlManager
{
    public class MultPoiskSiteManager : AppleBase
    {

        public MultPoiskSiteManager()
        {
            KillAce();
        }

        public XDocument GetConent(string url, string fileName,string cacheName, bool isfFull = true)
        {

            var cacheDoc = ReadDoc(cacheName);
            if (cacheDoc != null)
            {
                return cacheDoc;
            }

            XDocument xDocument = GetXml(Path.Combine(MikrainProgramm._xmlPath, fileName)).GetXDocument();
            var items = xDocument.Descendants(XName.Get("items"));
            int pageCount = 0;
            for (int i = 1; i < 10; i++)
            {

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

                var doc = new HtmlDocument();
                //GetHtmlRequest(doc, string.Format("{0}/page/{1}/", i));
                GetHtmlRequest(doc, string.Format("{0}/page/{1}/", url, i));
                var divs = doc.DocumentNode.Descendants("div");

                foreach (var div in divs)
                {
                    if (div.GetAttributeValue("class", "") == "basebox")
                    {
                        var imgs = div.Descendants("img");
                        var asd = imgs.FirstOrDefault();
                        if (asd != null)
                        {
                            var image = "http://multpoisk.net" + asd.GetAttributeValue("src", "");
                            var title = imgs.FirstOrDefault().GetAttributeValue("title", "");
                            var nestedDivs = div.Descendants("div");
                            var nestedDiv = nestedDivs.FirstOrDefault(node => node.GetAttributeValue("class", "") == "shortbtn");
                            var hrefs = nestedDiv.Descendants("a");
                            var href = hrefs.FirstOrDefault().GetAttributeValue("href", "");
                            CreateElementList(count++,
                                string.Format(
                                    "atv.loadURL('http://trailers.apple.com/multiDetails?movie={0}&imgSource={1}')",
                                    Uri.EscapeDataString(href), Uri.EscapeDataString(image)), title, image, itemsElement);
                        }
                    }
                }

                items.First().Add(collectionDividerElement);
                items.First().Add(shelfElement);
                pageCount++;

            }

            SaveDoc(cacheName, xDocument);
            return xDocument;
        }

        public XDocument GetMultSeries(string htmlUrl, string imageUrl)
        {
            if (File.Exists(Path.Combine(MikrainProgramm._xmlPath, "Content\\multCache\\" + imageUrl.Split('/').Last())))
            {
                return XDocument.Load(Path.Combine(MikrainProgramm._xmlPath, "Content\\multCache\\" + imageUrl.Split('/').Last()));
            }
            Directory.CreateDirectory(Path.Combine(MikrainProgramm._xmlPath, "Content\\multCache\\"));


            var doc = new HtmlDocument();

            if (GetHtmlRequest(doc, htmlUrl))
            {
                var frames = doc.DocumentNode.Descendants("iframe");

                if (frames.Count() == 1)
                {
                    return GetSingleMovie(frames.FirstOrDefault(), HttpUtility.HtmlDecode(imageUrl));
                }
                return GetListOfMovies(frames, HttpUtility.HtmlDecode(imageUrl));
            }
            return null;
        }

        public XDocument GetListOfMovies(IEnumerable<HtmlNode> frames, string imageUrl, bool isCount = false)
        {
            XDocument xDocument = GetXml(Path.Combine(MikrainProgramm._xmlPath, @"Content\showList.xml")).GetXDocument();

            var imageElement = xDocument.Descendants(XName.Get("image"));
            var items = xDocument.Descendants(XName.Get("items"));
            imageElement.First().SetValue(imageUrl);
            var doc = new HtmlDocument();
            int count = 0;
            int frameCount = 0;

            foreach (var frame in frames)
            {

                ThreadPool.QueueUserWorkItem(state =>
                {

                    Console.WriteLine(frames.Count() + " / " + frameCount);
                    if (!frame.OuterHtml.Contains("flash"))
                    {
                        var href = frame.GetAttributeValue("src", "");
                        var escapeUrl = HttpUtility.HtmlDecode(href);
                        doc.GetHtmlRequest(escapeUrl);
                        var videos = doc.DocumentNode.Descendants("video");
                        string poster = "";
                        var firstOrDefault = videos.FirstOrDefault();
                        if (firstOrDefault != null)
                            poster = firstOrDefault.GetAttributeValue("poster", "");

                        CreateElementList(count++,
                                          string.Format(
                                              "loadTrailerDetailPage('http://trailers.apple.com/multiSingle?multiPlayHref={0}&imgSource={1}')",
                                              Uri.EscapeDataString(escapeUrl), Uri.EscapeDataString(imageUrl)), "", HttpUtility.HtmlDecode(poster),
                                          items.First());
                    }

                    //if (count == 10 && !isCount)
                    //{
                    //    ThreadPool.QueueUserWorkItem(state =>
                    //    {
                    //        var fullDoc = GetListOfMovies(frames, imageUrl, true);
                    //        fullDoc
                    //    });
                    //    return xDocument;
                    //}
                    frameCount++;
                });


            }
            while (frameCount != frames.Count())
            {
                //return xDocument;
            }

            if (frames.Count() > 20)
            {
                xDocument.Save(Path.Combine(MikrainProgramm._xmlPath, "Content\\multCache\\" + imageUrl.Split('/').Last()));
            }

            return xDocument;
        }

        public XDocument GetSingleMovie(HtmlNode htmlNode, string imageUrl)
        {
            if (!htmlNode.OuterHtml.Contains("flash"))
            {
                var href = htmlNode.GetAttributeValue("src", "");
                return GetSingleMovie(href, imageUrl);
            }


            return null;
        }

        public XDocument GetSingleMovie(string href, string imageUrl)
        {
            XDocument xDocument =
         XDocument.Load(Path.Combine(MikrainProgramm._xmlPath, @"Content\movie.xml"));
            xDocument.Descendants(XName.Get("image")).First().SetValue(imageUrl);
            var elementDesc = xDocument.Descendants(XName.Get("summary"));
            // xDocument.Descendants(XName.Get("title")).First().SetValue(title);
            xDocument.Descendants(XName.Get("actionButton")).First().Remove();

            var escapedUri = HttpUtility.HtmlDecode(href);
            var doc = new HtmlDocument();
            doc.GetHtmlRequest(escapedUri);


            var videos = doc.DocumentNode.Descendants("video");
            var sources = videos.FirstOrDefault().Descendants("source");

            foreach (var source in sources)
            {
                var links = source.GetAttributeValue("src", "");
                if (links.Contains("360"))
                {
                    CreateActionButton(links, xDocument, "360 " + "en");
                }
                if (links.Contains("480"))
                {
                    CreateActionButton(links, xDocument, "480 " + "en");
                }
                if (links.Contains("720"))
                {
                    CreateActionButton(links, xDocument, "720 " + "en");
                }
            }

            return xDocument;
        }

        internal XDocument GetShow(string friendsPlayHref, string imageHref)
        {
            XDocument xDocument = GetXml(Path.Combine(MikrainProgramm._xmlPath, @"Content\movie.xml")).GetXDocument();
            var element = xDocument.Descendants(XName.Get("image"));
            var elementDesc = xDocument.Descendants(XName.Get("summary"));
            var elementName = xDocument.Descendants(XName.Get("title"));
            element.First().SetValue(imageHref);

            var doc = new HtmlDocument();
            var escapedUrl = HttpUtility.HtmlDecode(friendsPlayHref);
            if (doc.GetHtmlRequest(escapedUrl))
            {

                var div = doc.DocumentNode.Descendants("div");
                foreach (var htmlNode in div)
                {
                    if (htmlNode.GetAttributeValue("class", "") == "episode_text")
                    {
                        elementName.First().SetValue(htmlNode.ChildNodes.FirstOrDefault(node => node.Name == "h2").InnerText);
                        var divDesc = htmlNode.ChildNodes.FirstOrDefault(node => node.Name == "div");
                        elementDesc.First().SetValue(divDesc.FirstChild.InnerText);
                    }
                }

                var regex = new Regex("(file:\")(http://.*.mp4)");
                var text = doc.DocumentNode.InnerText;
                if (regex.IsMatch(text))
                {
                    var match = regex.Match(text);
                    var groups = match.Groups;
                    var movieUrl = groups[2];
                    var actionButtonElement = xDocument.Descendants("actionButton");
                    actionButtonElement.First().SetAttributeValue("onSelect", string.Format("atv.loadURL('http://trailers.apple.com/Playmovie?url={0}')", Uri.EscapeDataString(movieUrl.Value)));
                    actionButtonElement.First().SetAttributeValue("onPlay", string.Format("atv.loadURL('http://trailers.apple.com/Playmovie?url={0}')", Uri.EscapeDataString(movieUrl.Value)));
                }

            }

            return xDocument;
        }

        private bool GetHtmlRequest(HtmlDocument htmlDocument, string url)
        {
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(url);
                //request.Headers.Add("UserAgent", "Mozilla/5.0 (iPad; CPU OS 6_0 like Mac OS X) AppleWebKit/536.26 (KHTML, like Gecko) Version/6.0 Mobile/10A5376e Safari/8536.25");
                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    using (var reader = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding("windows-1251")))
                    {
                        string result = reader.ReadToEnd();
                        //Console.WriteLine(result);
                        htmlDocument.LoadHtml(result);
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }


        internal XmlDocument GetList()
        {
            XDocument xDocument = GetXml(Path.Combine(MikrainProgramm._xmlPath, @"Content\multiList.xml")).GetXDocument();
            return xDocument.GetXmlDocument();
        }
    }
}
