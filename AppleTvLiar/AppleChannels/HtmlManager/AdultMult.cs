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
    public class AdultMult : AppleBase
    {
        public XDocument GetContent(string url = "http://adultmult.tv/other/walt_disney.html", string cacheName = "disney")
        {

            //var cacheDoc = ReadDoc(cacheName);
            //if (cacheDoc != null)
            //{
            //    return cacheDoc;
            //}

            XDocument xDocument =
            XDocument.Load(Path.Combine(MikrainProgramm._xmlPath, @"Content\categories.xml"));
            var items = xDocument.Descendants(XName.Get("items"));
            var elements = new Dictionary<int, List<XElement>>();



            Console.WriteLine(DateTime.Now);
            for (int i = 1; i < 10; i++)
            {
                try
                {
                    AddToContent(url, i, elements);
                }
                catch
                {
                    break;
                }

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
            SaveDoc(cacheName, xDocument);
            return xDocument;
        }

        private void AddToContent(string url, int i, Dictionary<int, List<XElement>> elements)
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
                    "atv.loadURL('http://trailers.apple.com/getKino1080Movie?movie={0}&imageUrl={1}&movieTitle={2}')",
                    Uri.EscapeDataString("search"), "", ""), "search", "http://www.kudoschatsearch.com/images/search.png",
                    itemsElement);
            }


            var tables = doc.DocumentNode.Descendants("table");

            foreach (var div in tables)
            {
                if (div.GetAttributeValue("id", "").Contains("movie-id"))
                {
                    var strongs = div.ChildNodes.Descendants("a");

                    var inner = strongs.FirstOrDefault();
                    // inner.LoadHtml(strongs.FirstOrDefault().InnerHtml);

                    var image = inner.FirstChild.GetAttributeValue("src", "");
                    var name = inner.InnerText;
                    var href = inner.GetAttributeValue("href", "");

                    name = name.Replace("Смотреть", "").Replace("онлайн", "").Replace("сериал", "");

                    CreateElementList(count++,
                        string.Format(
                            "atv.loadURL('http://trailers.apple.com/getKino1080Movie?movie={0}&imageUrl={1}&movieTitle={2}')",
                            Uri.EscapeDataString(href), Uri.EscapeDataString(image), Uri.EscapeDataString(name)), name,
                         image, itemsElement);
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
