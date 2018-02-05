using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Web;
using System.Xml;
using HtmlAgilityPack;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MikrainService;

namespace AppleTvLiar.AppleChannels.HtmlManager
{
    public class CartoonHd : AppleBase
    {

        public XDocument GetCategories()
        {
            var cacheDoc = ReadDoc("cartoonHd");
            if (cacheDoc != null)
            {
                return cacheDoc;
            }

            XDocument xDocument = GetXml(Path.Combine(MikrainProgramm._xmlPath, @"Content\categories.xml")).GetXDocument();
            var items = xDocument.Descendants(XName.Get("items"));
            int pageCount = 0;
            //for (int i = 1; i < 10; i++)
            //{

            int count = 0;

            var collectionDividerElement = new XElement(XName.Get("collectionDivider"));
            collectionDividerElement.SetAttributeValue(XName.Get("alignment"), "left");
            collectionDividerElement.SetAttributeValue(XName.Get("accessibilityLabel"), 1);
            collectionDividerElement.Add(new XElement(XName.Get("title"), string.Format("Page number {0}", 1)));
            var shelfElement = new XElement(XName.Get("shelf"));
            shelfElement.SetAttributeValue(XName.Get("id"), string.Format("shelf_{0}", 1));

            var sectionsElement = new XElement(XName.Get("sections"));
            var shelfSectionElement = new XElement(XName.Get("shelfSection"));
            var itemsElement = new XElement(XName.Get("items"));

            shelfSectionElement.Add(itemsElement);
            sectionsElement.Add(shelfSectionElement);
            shelfElement.Add(sectionsElement);

            var doc = new HtmlDocument();
            //GetHtmlRequest(doc, string.Format("{0}/page/{1}/", i));
            GetHtmlRequest(doc, string.Format("{0}/page/{1}/", "https://cartoonhd.be/full-movies/trending", 1));
            var divs = doc.DocumentNode.Descendants("div");

            foreach (var div in divs)
            {
                if (div.GetAttributeValue("class", "") == "flipBox")
                {
                    var flipBoxSides = div.Descendants("div");

                    var front = flipBoxSides.FirstOrDefault(obj => obj.GetAttributeValue("class", "") == "front");
                    var back = flipBoxSides.FirstOrDefault(obj => obj.GetAttributeValue("class", "") == "back");

                    var imgs = front.Descendants("img");
                    var asd = imgs.FirstOrDefault();
                    if (asd != null)
                    {
                        var image = asd.GetAttributeValue("src", "");

                        var a = back.Descendants("a").FirstOrDefault();
                        var href = a.GetAttributeValue("href", "");
                        var title = a.InnerText;

                        CreateElementList(count++,
                            string.Format(
                                "atv.loadURL('http://trailers.apple.com/CartoonHdMovie?movie={0}&title={1}&imgSource={2}')",
                                Uri.EscapeDataString(href), Uri.EscapeDataString(title), Uri.EscapeDataString(image)), title, image, itemsElement);
                    }
                }
            }

            items.First().Add(collectionDividerElement);
            items.First().Add(shelfElement);
            pageCount++;

            //}

            SaveDoc("cartoonHd", xDocument);
            return xDocument;
        }


        public async Task<XDocument> GetMovie(string url, string title, string image)
        {
            XDocument xDocument =
            XDocument.Load(Path.Combine(MikrainProgramm._xmlPath, @"Content\movieNoBottom.xml"));
            var element = xDocument.Descendants(XName.Get("image"));
            var elementDesc = xDocument.Descendants(XName.Get("summary"));
            var elementName = xDocument.Descendants(XName.Get("title"));
            var html = HttpRequestsString(url);

            try
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(html);


                var token = Regex.Match(html, "var tok    = \'(.*)'").Groups[1].Value;
                var idEl = Regex.Match(html, "elid = \"(.*)\"").Groups[1].Value;

                var result = await SendPostRequest("https://cartoonhd.cc/ajax/tnembeds.php", new Dictionary<string, string>()
                        {
                               {"token", "eCNBuxFGpRmFlWjUJjmjguCJI"},
                                {"idEl","104713"},
                                 {"action","getMovieEmb"},
                                 {"elid","MTQ5MTQzMDAzNQ%3D%3D"},
                        });


                var str = HttpRequestsString("https://openload.co/embed/hGNiV4ZyFac/");


                var href = Regex.Match(html, "defaultStream.movie = \"(.*)\"").Groups[1].Value;
                var sections = doc.DocumentNode.Descendants("div");
                var infoDiv = sections.FirstOrDefault(obj => obj.GetAttributeValue("class", "") == "info");
                var p = infoDiv.ChildNodes.FindFirst("p");
                var summary = p.InnerText;

                element.First().SetValue(image);
                elementName.First().SetValue(title);
                elementDesc.First().SetValue(summary);


                var actionButtonElement = xDocument.Descendants("actionButton");
                actionButtonElement.First().SetAttributeValue("onSelect", string.Format("atv.loadURL('http://trailers.apple.com/Playmovie?url={0}')", Uri.EscapeDataString(href)));
                actionButtonElement.First().SetAttributeValue("onPlay", string.Format("atv.loadURL('http://trailers.apple.com/Playmovie?url={0}')", Uri.EscapeDataString(href)));
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

        private bool GetHtmlRequest(HtmlDocument htmlDocument, string url)
        {
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(url);
                request.UserAgent =
               "Mozilla/5.0(iPad; U; CPU iPhone OS 3_2 like Mac OS X; en-us) AppleWebKit/531.21.10 (KHTML, like Gecko) Version/4.0.4 Mobile/7B314 Safari/531.21.10";
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
    }
}
