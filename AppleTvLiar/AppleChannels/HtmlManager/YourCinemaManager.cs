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
    public class YourCinemaManager : AppleBase
    {
        public XDocument GetContent(string url = "http://yourcinema-tv.com/cinema/page/{0}/", string cacheName = "YourCinema")
        {


            XDocument xDocument =
            XDocument.Load(Path.Combine(MikrainProgramm._xmlPath, @"Content\categories.xml"));
            var items = xDocument.Descendants(XName.Get("items"));
            var elements = new Dictionary<int, List<XElement>>();



            Console.WriteLine(DateTime.Now);
            for (int i = 1; i < 15; i++)
            {
                AddToContent(url, i, elements);
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

            elements.Clear();
            //threads.Clear();
            Console.WriteLine(DateTime.Now);
            //SaveDoc(cacheName, xDocument);
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
            doc.OptionReadEncoding = true;
            doc.LoadHtml(html);


            //var divs = doc.GetElementbyId("allEntries");

            if (i == 1)
            {
                CreateElementList(count++, string.Format(
                    "atv.loadURL('http://trailers.apple.com/getUcozMovie?movie={0}&imageUrl={1}&movieTitle={2}')",
                    Uri.EscapeDataString("search"), "", ""), "search", "http://www.kudoschatsearch.com/images/search.png",
                    itemsElement);
            }


            var divs = doc.DocumentNode.Descendants("a");

            foreach (var div in divs)
            {
                if (div.GetAttributeValue("class", "").Contains("item"))
                {

                    var img = div.ChildNodes.FirstOrDefault(node => node.Name == "img");

                    var span = div.ChildNodes.FirstOrDefault(node => node.Name == "span");

                    var inner = new HtmlDocument();
                    inner.LoadHtml(div.InnerHtml);

                    if (span != null && img != null)
                    {
                        var image = img.GetAttributeValue("src", "");
                        var name = span.InnerText;
                        var href = div.GetAttributeValue("href", "");


                        CreateElementList(count++,
                            string.Format(
                                "atv.loadURL('http://trailers.apple.com/getYourCinemaMovie?movie={0}&imageUrl={1}&movieTitle={2}')",
                                Uri.EscapeDataString(href), Uri.EscapeDataString(image), Uri.EscapeDataString(name)), name,
                            image, itemsElement);
                    }
                }
            }

            if (elements != null)
                if (!elements.ContainsKey(i))
                    elements.Add(i, new List<XElement>() { collectionDividerElement, shelfElement });
            //items.First().Add(collectionDividerElement);
            //items.First().Add(shelfElement);
            //});
            //th1.Start();
            //threads.Add(th1);
        }

        public XDocument GetMovie(string url, string imageUrl, string movieTitle)
        {
            //var cacheDoc = ReadDoc(movieTitle);
            //if (cacheDoc != null)
            //{
            //    return cacheDoc;
            //}


            if (url == "search")
            {
                XDocument searchDoc = XDocument.Load(Path.Combine(MikrainProgramm._xmlPath, @"Content\searchtrailers.xml"));
                var baseUrl = searchDoc.Descendants(XName.Get("baseURL"));
                var firstOrDefault = baseUrl.FirstOrDefault();
                if (firstOrDefault != null)
                    firstOrDefault.SetValue("http://trailers.apple.com/UcozSearch?query=");
                return searchDoc;
            }

            var html = HttpRequests(url);

            try
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(html);


                
                XDocument xDocument =
       XDocument.Load(Path.Combine(MikrainProgramm._xmlPath, @"Content\movie.xml"));
                var element = xDocument.Descendants(XName.Get("image"));
                var elementDesc = xDocument.Descendants(XName.Get("summary"));
                var elementName = xDocument.Descendants(XName.Get("title"));
                var actionButtonElement = xDocument.Descendants("actionButton");
                actionButtonElement.Remove();

                ////*[@id="contanier"]/table[1]/tbody/tr[2]/td[2]/div/table[2]/tbody/tr[2]/td/br[7]
                ////*[@id="contanier"]
                /// ////*[@id="dle-content"]/div[2]/div[1]/div[5]/div[2]/ul/li[1]
                /// 
                var li =
                    doc.DocumentNode.SelectSingleNode(
                        "//*[@id=\"dle-content\"]/div[2]/div[1]/div[5]/div[2]/ul/li[1]");

               var description= li.InnerText;

                try
                {
                    var frames = doc.DocumentNode.Descendants("div");
                    foreach (var htmlNode in frames)
                    {
                        var source = htmlNode.GetAttributeValue("id", "");
                        if (source.Contains("dle-content"))
                        {
                            var descNode= htmlNode.ChildNodes.FirstOrDefault();
                            if (descNode != null)
                            {
                                var desc= descNode.InnerText;
                            }
                        }


                        var htmlSources = HttpRequests(source);
                        var document = new HtmlDocument();
                        document.LoadHtml(htmlSources);

                        var video = document.GetElementbyId("video");

                        foreach (var childNode in video.ChildNodes)
                        {
                            if (childNode.Name == "source")
                            {
                                var videoSource = childNode.GetAttributeValue("src", "");
                                var points = videoSource.Split('.');

                                CreateActionButton(videoSource, xDocument, points[points.Count() - 2]);
                            }
                        }
                    }

                    element.First().SetValue("http://hd-720.ucoz.ru/" + imageUrl);
                    elementName.First().SetValue(movieTitle);
                    elementDesc.First().SetValue("Temprory unavailable");
                }
                catch (Exception)
                {

                }
                SaveDoc(movieTitle, xDocument);
                return xDocument;



            }
            catch (Exception)
            {


            }


            return null;
        }


        public string HttpRequests(string url)
        {

            var request = (HttpWebRequest)WebRequest.Create(url);
            request.UserAgent = "Mozilla/5.0 (iPad; CPU OS 5_1 like Mac OS X; en-us) AppleWebKit/534.46 (KHTML, like Gecko) Version/5.1 Mobile/9B176 Safari/7534.48.3";
            request.ContentType = "application/x-www-form-urlencoded;";
            // request.Accept = "text/html; charset=UTF-8";
            // request.Headers.Add("DNT", "1");
            //request.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip,deflate,sdch");
            //request.Headers.Add(HttpRequestHeader.AcceptEncoding, "utf8");
            //request.Headers.Add(HttpRequestHeader.AcceptLanguage, "en-US,en;q=0.8,he;q=0.6,ru;q=0.4");
            using (var response = (HttpWebResponse)request.GetResponse())
            {

                using (var reader = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding(1251)))
                {
                    string result = reader.ReadToEnd();
                    return result;
                }
            }
        }

    }
}
