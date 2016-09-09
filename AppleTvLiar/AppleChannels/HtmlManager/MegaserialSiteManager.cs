using AppleTvLiar.AppleChannels;
using HtmlAgilityPack;
using Microsoft.CSharp.RuntimeBinder;
using MikrainService;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AppleTvLiar.AppleChannels.HtmlManager
{
    public class MegaserialSiteManager : AppleBase
    {
        private List<Thread> threads = new List<Thread>();

        private void AddContentInThread(string url, int i, Dictionary<int, List<XElement>> elements)
        {
            int num2;
            int num = 0;
            XElement element = new XElement(XName.Get("collectionDivider"));
            element.SetAttributeValue(XName.Get("alignment"), "left");
            element.SetAttributeValue(XName.Get("accessibilityLabel"), i);
            element.Add(new XElement(XName.Get("title"), string.Format("Page number {0}", i)));
            XElement element2 = new XElement(XName.Get("shelf"));
            element2.SetAttributeValue(XName.Get("id"), string.Format("shelf_{0}", i));
            XElement content = new XElement(XName.Get("sections"));
            XElement element4 = new XElement(XName.Get("shelfSection"));
            XElement element5 = new XElement(XName.Get("items"));
            element4.Add(element5);
            content.Add(element4);
            element2.Add(content);
            string str = this.HttpRequests(string.Format("{1}/{0}/", i, url));
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(str);
            HtmlNode elementbyId = document.GetElementbyId("dle-content");
            if (i == 1)
            {
                num2 = num;
                num = num2 + 1;
                base.CreateElementList(num2, string.Format("atv.loadURL('http://trailers.apple.com/findMovie?movie={0}')", Uri.EscapeDataString("search")), "search", "http://www.kudoschatsearch.com/images/search.png", element5, false);
            }
            foreach (HtmlNode node2 in elementbyId.ChildNodes)
            {
                string str2 = "";
                string str3 = "";
                string str4 = "";
                foreach (HtmlNode node3 in node2.ChildNodes)
                {
                    if ((node3.Name != "div") || !node3.HasAttributes)
                    {
                        continue;
                    }
                    string message = node3.Attributes["class"].Value;
                    //Debug.WriteLine(message);
                    if (message == "h2")
                    {
                        HtmlNode node4 = node3.Element("a");
                        HtmlAttribute attribute = node4.Attributes["href"];
                        str3 = node4.InnerText;
                        str2 = attribute.Value;
                    }
                    if (message == "main-news-content")
                    {
                        IEnumerable<HtmlNode> enumerable = node3.Descendants("div");
                        foreach (HtmlNode node5 in enumerable)
                        {
                            if (node5.Attributes["class"].Value == "main-news-image")
                            {
                                str4 = "http://megaserial.net" + node5.Element("a").Element("img").Attributes["src"].Value;
                                break;
                            }
                        }
                        if ((!string.IsNullOrEmpty(str2) && !string.IsNullOrEmpty(str3)) && !string.IsNullOrEmpty(str4))
                        {
                            num2 = num;
                            num = num2 + 1;
                            base.CreateElementList(num2, string.Format("atv.loadURL('http://trailers.apple.com/megafindMovie?movie={0}&image={1}&title={2}')", Uri.EscapeDataString(str2), Uri.EscapeDataString(str4), Uri.EscapeDataString(str3)), str3, str4, element5, false);
                        }
                        break;
                    }
                }
            }
            if ((elements != null) && !elements.ContainsKey(i))
            {
                List<XElement> list = new List<XElement> {
                    element,
                    element2
                };
                elements.Add(i, list);
            }
        }

        public XDocument GetCategories(string url, string cacheName)
        {
            int num2;
            XDocument document = base.ReadDoc(cacheName);
            if (document != null)
            {
                return document;
            }
            XDocument xDocument = XDocument.Load(Path.Combine(MikrainProgramm._xmlPath, @"Content\categories.xml"));
            IEnumerable<XElement> source = xDocument.Descendants(XName.Get("items"));
            Dictionary<int, List<XElement>> elements = new Dictionary<int, List<XElement>>();
            for (int i = 1; i < 0x11; i = num2 + 1)
            {
                this.AddContentInThread(url, i, elements);
                num2 = i;
            }
            for (int j = 1; j < elements.Count; j = num2 + 1)
            {
                if (elements.ContainsKey(j))
                {
                    source.First<XElement>().Add(elements[j][0]);
                    source.First<XElement>().Add(elements[j][1]);
                }
                num2 = j;
            }
            elements.Clear();
            base.SaveDoc(cacheName, xDocument);
            return xDocument;
        }

        internal XDocument GetEpisodes(string href, string image, string title, string season)
        {
            int num2;
            XDocument xDocument = base.GetXml(Path.Combine(MikrainProgramm._xmlPath, @"Content\showList.xml")).GetXDocument();
            string str = this.HttpRequests(href);
            HtmlDocument document2 = new HtmlDocument();
            document2.LoadHtml(str);
            string str3 = Regex.Match(document2.GetElementbyId("player").ParentNode.InnerText, "playlistLinkHtml = '(.*)'").Groups[1].Value;
            object obj2 = (JObject)JsonConvert.DeserializeObject(this.HttpRequests("http://megaserial.net" + str3));
            JArray array = ((Newtonsoft.Json.Linq.JObject)obj2).GetValue("playlist") as JArray;
            IEnumerable<XElement> source = xDocument.Descendants(XName.Get("image"));
            IEnumerable<XElement> enumerable2 = xDocument.Descendants(XName.Get("title"));
            IEnumerable<XElement> enumerable3 = xDocument.Descendants(XName.Get("summary"));
            IEnumerable<XElement> enumerable4 = xDocument.Descendants(XName.Get("items"));
            source.First<XElement>().SetValue(image);
            enumerable2.First<XElement>().SetValue(title);
            for (int i = 0; i < array.Count; i = num2 + 1)
            {
                try
                {
                    string stringToEscape = array[i].Value<string>("file");
                    base.CreateElementList(i, string.Format("loadTrailerDetailPage('http://trailers.apple.com/ShowMegaSer?ser={0}&image={1}&title={2}')", Uri.EscapeDataString(stringToEscape), Uri.EscapeDataString(image), Uri.EscapeDataString(title)), "Episode " + (i + 1), image, enumerable4.First<XElement>(), false);
                }
                catch (Exception)
                {
                }
                num2 = i;
            }
            return xDocument;
        }

        public XDocument GetMovie(string url, string imageUrl, string title)
        {
            if (url == "search")
            {
                XDocument document3 = XDocument.Load(Path.Combine(MikrainProgramm._xmlPath, @"Content\searchtrailers.xml"));
                XElement element = document3.Descendants(XName.Get("baseURL")).FirstOrDefault<XElement>();
                if (element != null)
                {
                    element.SetValue("http://trailers.apple.com/searchbaskino?query=");
                }
                return document3;
            }
            XDocument xDocument = base.GetXml(Path.Combine(MikrainProgramm._xmlPath, @"Content\showList.xml")).GetXDocument();
            string str = this.HttpRequests(url);
            HtmlDocument document2 = new HtmlDocument();
            document2.LoadHtml(str);
            try
            {
                IEnumerable<HtmlNode> enumerable2 = document2.DocumentNode.Descendants("div");
                foreach (HtmlNode node in enumerable2)
                {
                    IEnumerable<XElement> source = xDocument.Descendants(XName.Get("summary"));
                    IEnumerable<XElement> enumerable4 = xDocument.Descendants(XName.Get("title"));
                    IEnumerable<XElement> enumerable5 = xDocument.Descendants(XName.Get("image"));
                    IEnumerable<XElement> enumerable6 = xDocument.Descendants(XName.Get("items"));
                    HtmlAttribute attribute = node.Attributes["class"];
                    if ((attribute != null) && (attribute.Value == "links_seasons"))
                    {
                        IEnumerable<HtmlNode> enumerable7 = node.Descendants("a");
                        if (enumerable7.Count<HtmlNode>() > 0)
                        {
                            int num2;
                            for (int i = 0; i < enumerable7.Count<HtmlNode>(); i = num2 + 1)
                            {
                                enumerable5.First<XElement>().SetValue(imageUrl);
                                enumerable4.First<XElement>().SetValue(title);
                                source.First<XElement>().SetValue("");
                                string stringToEscape = "http://megaserial.net" + enumerable7.ElementAt<HtmlNode>(i).GetAttributeValue("href", "");
                                string str3 = enumerable7.ElementAt<HtmlNode>(i).InnerText;
                                base.CreateElementList(i, string.Format("loadTrailerDetailPage('http://trailers.apple.com/ShowMegaEpisodes?href={0}&imageHref={1}&title={2}&season={3}')", new object[] { Uri.EscapeDataString(stringToEscape), Uri.EscapeDataString(imageUrl), Uri.EscapeDataString(title), Uri.EscapeDataString(str3) }), str3, imageUrl, enumerable6.First<XElement>(), false);
                                num2 = i;
                            }
                        }
                        HtmlNode node2 = node.Descendants("span").FirstOrDefault<HtmlNode>();
                        if (node2 != null)
                        {
                            string str4 = node2.InnerText;
                            base.CreateElementList(enumerable7.Count<HtmlNode>(), string.Format("loadTrailerDetailPage('http://trailers.apple.com/ShowMegaEpisodes?href={0}&imageHref={1}&title={2}&season={3}')", new object[] { Uri.EscapeDataString(url), Uri.EscapeDataString(imageUrl), Uri.EscapeDataString(title), Uri.EscapeDataString(str4) }), str4, imageUrl, enumerable6.First<XElement>(), false);
                        }
                        else if (node.InnerText == "")
                        {
                            base.CreateElementList(enumerable7.Count<HtmlNode>(), string.Format("loadTrailerDetailPage('http://trailers.apple.com/ShowMegaEpisodes?href={0}&imageHref={1}&title={2}&season={3}')", new object[] { Uri.EscapeDataString(url), Uri.EscapeDataString(imageUrl), Uri.EscapeDataString(title), Uri.EscapeDataString("Сезон 1") }), "Сезон 1", imageUrl, enumerable6.First<XElement>(), false);
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
            return xDocument;
        }

        public XDocument GetPages(string url)
        {
            return null;
        }

        public string HttpRequests(string url)
        {
            string str2;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.UserAgent = "Mozilla/5.0 (iPhone; CPU iPhone OS 9_0_2 like Mac OS X) AppleWebKit/601.1.46 (KHTML, like Gecko) Version/9.0 Mobile/13A452 Safari/601.1";
            request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
            request.Headers.Add("DNT", "1");
            request.Headers.Add(HttpRequestHeader.AcceptLanguage, "en-US,en;q=0.8,he;q=0.6,ru;q=0.4");
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding(0x4e3)))
                {
                    str2 = reader.ReadToEnd();
                }
            }
            return str2;
        }

        public XDocument PlayMovie(string url)
        {
            XDocument document = XDocument.Load(Path.Combine(MikrainProgramm._xmlPath, @"Content\Playmovie.xml"));
            document.Descendants(XName.Get("httpFileVideoAsset")).First<XElement>().AddFirst(new XElement(XName.Get("mediaURL"), url));
            return document;
        }


        public XDocument ShowMegaSer(string ser, string title, string image)
        {
            XDocument xDocument = XDocument.Load(Path.Combine(MikrainProgramm._xmlPath, @"Content\movieNoBottom.xml"));
            IEnumerable<XElement> source = xDocument.Descendants(XName.Get("image"));
            IEnumerable<XElement> enumerable2 = xDocument.Descendants(XName.Get("summary"));
            IEnumerable<XElement> enumerable3 = xDocument.Descendants(XName.Get("title"));
            xDocument.Descendants("actionButton").Remove<XElement>();
            base.CreateActionButton(ser, xDocument, "HD");
            source.First<XElement>().SetValue(title);
            enumerable3.First<XElement>().SetValue(image);
            enumerable2.First<XElement>().SetValue("Temprory unavailable");
            return xDocument;
        }

    }
}
