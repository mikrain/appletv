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
using MikrainService;

namespace AppleTvLiar.AppleChannels.HtmlManager
{
    public class M4KinoSiteManager : AppleBase
    {
        public M4KinoSiteManager()
        {
            KillAce();
        }
        public XDocument GetCategories()
        {
            XDocument xDocument =
              XDocument.Load(Path.Combine(MikrainProgramm._xmlPath, @"Content\categories.xml"));
            var items = xDocument.Descendants(XName.Get("items"));
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

            var html = HttpRequests("http://www.lovekinozal.ru/news/");
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var divs = doc.GetElementbyId("allEntries");

            foreach (var div in divs.ChildNodes)
            {
                if (div.GetAttributeValue("id", "").Contains("entry"))
                {
                    var image = div.SelectSingleNode("//table/tr/td/div[2]/div/img").GetAttributeValue("src", "");
                    var name = div.SelectSingleNode("//table/tr/td/div[2]/text()[2]").InnerText;
                    var href = div.SelectSingleNode("//table/tr/td/div[3]/a[2]").GetAttributeValue("href", "");
                    CreateElementList(count++,
                                                 string.Format(
                                                     "atv.loadURL('http://trailers.apple.com/lovekinozalMovie?movie={0}')",
                                                     Uri.EscapeDataString(href)), name, image, itemsElement);
                }

            }

            items.First().Add(collectionDividerElement);
            items.First().Add(shelfElement);

            return xDocument;
        }


        public XDocument GetMovie(string url)
        {
            XDocument xDocument =
            XDocument.Load(Path.Combine(MikrainProgramm._xmlPath, @"Content\movie.xml"));
            var element = xDocument.Descendants(XName.Get("image"));
            var elementDesc = xDocument.Descendants(XName.Get("summary"));
            var elementName = xDocument.Descendants(XName.Get("title"));
            var html = HttpRequests(url);

            try
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(html);
                var divNoselect = doc.GetElementbyId("noselect");
                var imgValue = divNoselect.Descendants("img").First().GetAttributeValue("src", "");
                var summary = divNoselect.SelectSingleNode("/text()[1]");
                var title = divNoselect.SelectSingleNode("/text()[2]");

                element.First().SetValue(imgValue);
                elementName.First().SetValue(title);
                elementDesc.First().SetValue(summary);

                var videoNode = doc.GetElementbyId("video");
                var videoSourceNode = videoNode.Descendants("source");
                var href = videoSourceNode.First().GetAttributeValue("src", "");
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

        public string HttpRequests(string url)
        {

            var request = (HttpWebRequest)WebRequest.Create(url);
           // request.Headers.Add("UserAgent", userAgent);
          //  request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
            //request.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip,deflate,sdch");
          //  request.Headers.Add(HttpRequestHeader.AcceptLanguage, "en-US,en;q=0.8,he;q=0.6,ru;q=0.4");
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
