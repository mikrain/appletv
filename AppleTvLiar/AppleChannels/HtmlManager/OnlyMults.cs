using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using HtmlAgilityPack;
using MikrainService;

namespace AppleTvLiar.AppleChannels.HtmlManager
{
    public class OnlyMults : AppleBase
    {
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


            for (int i = 1; i < 15; i++)
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

            var html = HttpRequestsString(string.Format("{1}/{0}/", i, url));
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
                if (child.Name == "div")
                {
                    var classAttr = child.Attributes["class"].Value;
                    if (classAttr == "post")
                    {
                        var a = child.Element("a");
                        var href = a.Attributes["href"];
                        var div = child.Element("div");
                        var src = div.Element("img").GetAttributeValue("src", "");
                        var title = a.Element("titl").InnerText;
                        var hrefValue = href.Value;

                        src = src.StartsWith("http") ? src : "http://www.onlymults.ru" + src;
                        CreateElementList(count++,
                       string.Format(
                           "atv.loadURL('http://trailers.apple.com/getOnlyMult?movie={0}&imageUrl={1}&movieTitle={2}')",
                           Uri.EscapeDataString(hrefValue), Uri.EscapeDataString(src), Uri.EscapeDataString(title)), title,
                        src, itemsElement);

                    }
                }

            }

            if (elements != null)
                if (!elements.ContainsKey(i))
                    elements.Add(i, new List<XElement>() { collectionDividerElement, shelfElement });
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
                XDocument searchDoc =
                    XDocument.Load(Path.Combine(MikrainProgramm._xmlPath, @"Content\searchtrailers.xml"));
                var baseUrl = searchDoc.Descendants(XName.Get("baseURL"));
                var firstOrDefault = baseUrl.FirstOrDefault();
                if (firstOrDefault != null)
                    firstOrDefault.SetValue("http://trailers.apple.com/UcozSearch?query=");
                return searchDoc;
            }

            var html = HttpRequests("http://moonwalk.co/api/iframe/?type=film&name=" + movieTitle + "&w=640&h=480&s=moonwalkco&enc=utf-8&nb&nsor&rd=onlymults.ru");

            try
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                XDocument xDocument =
        XDocument.Load(Path.Combine(MikrainProgramm._xmlPath, @"Content\movieNoBottom.xml"));
                var element = xDocument.Descendants(XName.Get("image"));
                var elementDesc = xDocument.Descendants(XName.Get("summary"));
                var elementName = xDocument.Descendants(XName.Get("title"));
                var actionButtonElement = xDocument.Descendants("actionButton");
                actionButtonElement.Remove();

                var frames = doc.DocumentNode.Descendants("iframe");
                foreach (var htmlNode in frames)
                {
                    try
                    {
                        var source = htmlNode.GetAttributeValue("src", "");

                        string videoSource = source;// source.Replace("iframe", "index.m3u8");

                        if (videoSource.Contains("moonwalk.cc") || true)
                        {
                            var htmlSources = HttpRequestsString(source);
                            var document = new HtmlDocument();
                            document.LoadHtml(htmlSources);

                            var link = await GetLink(document);
                            if (string.IsNullOrEmpty(link)) continue;

                            if (link != null)
                            {
                                videoSource = link;
                            }
                        }

                        CreateActionButton(videoSource, xDocument, "hd");
                        break;
                    }
                    catch (Exception)
                    {


                    }
                }

                element.First().SetValue(imageUrl);
                elementName.First().SetValue(movieTitle);


                elementDesc.First().SetValue("");

                //SaveDoc(movieTitle, xDocument);
                return xDocument;
            }
            catch (Exception exc)
            {

            }

            return null;
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
}
