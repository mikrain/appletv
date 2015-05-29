using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using HtmlAgilityPack;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using MikrainService;

namespace AppleTvLiar.AppleChannels.HtmlManager
{
    public class BaskinoSiteManager : AppleBase
    {
        public BaskinoSiteManager()
        {
            KillAce();
        }

        public async Task<XDocument> SearchBaskino(string query)
        {
            XDocument xDocument =
        XDocument.Load(Path.Combine(MikrainProgramm._xmlPath, @"Content\searchResult.xml"));
            int count = 0;

            string endPoint = "http://baskino.com/index.php?do=search";
            var dict = new Dictionary<string, string>();
            dict.Add("do", "search");
            dict.Add("subaction", "search");
            dict.Add("actors_only", "0");
            dict.Add("search_start", "1");
            dict.Add("full_search", "1");
            dict.Add("result_from", "1");
            dict.Add("story", query);
            dict.Add("titleonly", "3");
            dict.Add("replyless", "0");
            dict.Add("replylimit", "0");
            dict.Add("searchdate", "0");
            dict.Add("beforeafter", "after");
            dict.Add("sortby", "date");
            dict.Add("resorder", "desc");
            dict.Add("showposts", "0");
            dict.Add("catlist[]", "25");

            var sresult = await SendPostRequest(endPoint, dict);

            var doc = new HtmlDocument();
            doc.LoadHtml(sresult);
            var sortNode = doc.GetElementbyId("dle-content");
            var items = xDocument.Descendants(XName.Get("items"));

            foreach (var child in sortNode.ChildNodes)
            {
                foreach (var childNode in child.ChildNodes)
                {
                    if (childNode.Name == "div")
                    {
                        if (childNode.HasAttributes)
                        {
                            var classAttr = childNode.Attributes["class"].Value;
                            if (classAttr == "postcover")
                            {
                                var a = childNode.Element("a");
                                var href = a.Attributes["href"];
                                var src = a.Element("img").GetAttributeValue("src", "");
                                var title = a.Element("img").GetAttributeValue("title", "");
                                var hrefValue = href.Value;

                                var posterMenuItem = new XElement(XName.Get("posterMenuItem"));
                                posterMenuItem.Add(new XAttribute(XName.Get("id"), "movieBoxObject_" + count++));
                                posterMenuItem.Add(new XAttribute(XName.Get("accessibilityLabel"), title.Replace(" ", "")));
                                posterMenuItem.Add(new XAttribute(XName.Get("onSelect"), string.Format(
                                                                      "atv.loadURL('http://trailers.apple.com/findMovie?movie={0}')",
                                                                     Uri.EscapeDataString(hrefValue))));
                                posterMenuItem.Add(new XAttribute(XName.Get("onPlay"), string.Format(
                                                                      "atv.loadURL('http://trailers.apple.com/findMovie?movie={0}')",
                                                                     Uri.EscapeDataString(hrefValue))));
                                posterMenuItem.Add(new XElement(XName.Get("label"), title));
                                posterMenuItem.Add(new XElement(XName.Get("image"), src));
                                items.First().Add(posterMenuItem);
                            }
                        }
                    }
                }
            }

            return xDocument;
        }

        public XDocument GetPages(string url)
        {
            return null;
        }

        List<Thread> threads = new List<Thread>();

        public XDocument GetCategories(string url, string cacheName)
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


            for (int i = 1; i < 17; i++)
            {
                AddContentInThread(url, i, elements);
            }

            //foreach (var thread in threads)
            //{
            //    thread.Join();
            //}

            for (int i = 1; i < elements.Count; i++)
            {
                if (elements.ContainsKey(i))
                {
                    items.First().Add(elements[i][0]);
                    items.First().Add(elements[i][1]);
                }
            }

          //  threads.Clear();
            elements.Clear();
            SaveDoc(cacheName, xDocument);
            // AddPages(page != null ? int.Parse(page) : 1, items, count);
            return xDocument;
        }

        private void AddContentInThread(string url, int i, Dictionary<int, List<XElement>> elements)
        {
             //var th1 = new Thread(() =>
             //              {
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

            var html = HttpRequests(string.Format("{1}/{0}/", i, url));
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            var sortNode = doc.GetElementbyId("dle-content");

            if (i == 1)
            {
                CreateElementList(count++, string.Format(
                    "atv.loadURL('http://trailers.apple.com/findMovie?movie={0}')",
                    Uri.EscapeDataString("search")), "search", "http://www.kudoschatsearch.com/images/search.png", itemsElement);
            }


            foreach (var child in sortNode.ChildNodes)
            {
                foreach (var childNode in child.ChildNodes)
                {
                    if (childNode.Name == "div")
                    {
                        if (childNode.HasAttributes)
                        {
                            var classAttr = childNode.Attributes["class"].Value;
                            if (classAttr == "postcover")
                            {
                                var a = childNode.Element("a");
                                var href = a.Attributes["href"];
                                var src = a.Element("img").GetAttributeValue("src", "");
                                var title = a.Element("img").GetAttributeValue("title", "");
                                var hrefValue = href.Value;

                                CreateElementList(count++,
                                    string.Format(
                                        "atv.loadURL('http://trailers.apple.com/findMovie?movie={0}')",
                                        Uri.EscapeDataString(hrefValue)), title, src, itemsElement);
                                break;
                            }
                        }
                    }
                }
            }

                               if (elements != null)
                                   if (!elements.ContainsKey(i))
                                       elements.Add(i, new List<XElement>() { collectionDividerElement, shelfElement });
             //              });
             //th1.Start();
             //threads.Add(th1);
            //items.First().Add(collectionDividerElement);
            //items.First().Add(shelfElement);
        }


        public XDocument GetMovie(string url)
        {
            if (url == "search")
            {
                XDocument searchDoc = XDocument.Load(Path.Combine(MikrainProgramm._xmlPath, @"Content\searchtrailers.xml"));
                var baseUrl = searchDoc.Descendants(XName.Get("baseURL"));
                var firstOrDefault = baseUrl.FirstOrDefault();
                if (firstOrDefault != null)
                    firstOrDefault.SetValue("http://trailers.apple.com/searchbaskino?query=");
                return searchDoc;
            }

            XDocument xDocument =
            XDocument.Load(Path.Combine(MikrainProgramm._xmlPath, @"Content\movie.xml"));
            var element = xDocument.Descendants(XName.Get("image"));
            var elementDesc = xDocument.Descendants(XName.Get("summary"));
            var elementName = xDocument.Descendants(XName.Get("title"));

            var html = HttpRequests(url);
            var regex = new Regex("(file:\")(http://.*.mp4)");
            var regexImage = new Regex("(<img .* src=\"(.*.jpg)\")");

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            try
            {


                var sortNode = doc.GetElementbyId("content");
                HtmlNodeCollection AllNodes = doc.DocumentNode.SelectNodes("//*[@id=\"dle-content\"]/div[2]/div/div/div/div/div/div/div/div[3]");

                ////*[@id="dle-content"]/div[2]/div/div/div/div/div/div/div/div[1]/div[2]/table/tbody/tr[1]/td[2]

                string description = "";
                foreach (var allNode in AllNodes.First().ChildNodes)
                {
                    if (allNode.Name == "div")
                    {
                        description = allNode.InnerText;
                        elementDesc.First().SetValue(description);
                        break;
                    }
                }

                HtmlNodeCollection AllNodesName = doc.DocumentNode.SelectNodes("//*[@id=\"dle-content\"]/div[2]/div/div/div/div/div/div/div/div[1]/div[2]");

                string name = "";
                foreach (var nameNode in AllNodesName.First().ChildNodes)
                {
                    if (nameNode.Name == "table")
                    {
                        var body = nameNode.ChildNodes[1];
                        //var tr = body.FirstChild;
                        foreach (var childNode in body.ChildNodes)
                        {
                            if (childNode.GetAttributeValue("itemprop", "") == "name")
                            {
                                name = childNode.InnerText;
                                elementName.First().SetValue(name);
                                break;
                            }
                        }
                    }
                }



                if (regexImage.IsMatch(html))
                {
                    var match = regexImage.Match(html);
                    var groups = match.Groups;
                    var movieUrl = groups[2];
                    element.First().SetValue(movieUrl);
                }

            }
            catch (Exception)
            {


            }

            try
            {
                if (regex.IsMatch(html))
                {
                    var match = regex.Match(html);
                    var groups = match.Groups;
                    var movieUrl = groups[2];
                    var actionButtonElement = xDocument.Descendants("actionButton");
                    actionButtonElement.First().SetAttributeValue("onSelect", string.Format("atv.loadURL('http://trailers.apple.com/Playmovie?url={0}')", Uri.EscapeDataString(movieUrl.Value)));
                    actionButtonElement.First().SetAttributeValue("onPlay", string.Format("atv.loadURL('http://trailers.apple.com/Playmovie?url={0}')", Uri.EscapeDataString(movieUrl.Value)));
                }
                else
                {
                    var dibBasePlayer = doc.GetElementbyId("basplayer_hd");
                    foreach (var node in dibBasePlayer.ChildNodes)
                    {
                        if (node.Name == "iframe")
                        {
                            var attValue = node.GetAttributeValue("src", "");
                            var htmlTmp = HttpRequests(attValue);


                            var docTmp = new HtmlDocument();
                            docTmp.LoadHtml(htmlTmp);
                            var video = docTmp.GetElementbyId("video");

                            foreach (var childNode in video.ChildNodes)
                            {
                                if (childNode.Name == "source")
                                {
                                    var actionButtonElement = xDocument.Descendants("actionButton");
                                    actionButtonElement.First().SetAttributeValue("onSelect", string.Format("atv.loadURL('http://trailers.apple.com/Playmovie?url={0}')", Uri.EscapeDataString(childNode.GetAttributeValue("src", ""))));
                                    actionButtonElement.First().SetAttributeValue("onPlay", string.Format("atv.loadURL('http://trailers.apple.com/Playmovie?url={0}')", Uri.EscapeDataString(childNode.GetAttributeValue("src", ""))));
                                    break;

                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {

            }

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
            using (var response = (HttpWebResponse)request.GetResponse())
            {


                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    string result = reader.ReadToEnd();
                    return result;
                }
            }
        }


       

        private IPEndPoint BindIpEndPointDelegate(ServicePoint servicePoint, IPEndPoint remoteEndPoint, int retryCount)
        {
            OperationContext context = OperationContext.Current;
            MessageProperties prop = context.IncomingMessageProperties;
            RemoteEndpointMessageProperty endpoint = prop[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty;

            return new IPEndPoint(IPAddress.Parse(endpoint.Address), endpoint.Port);
        }
    }
}
