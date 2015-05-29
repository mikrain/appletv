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
using System.Xml.Linq;
using HtmlAgilityPack;
using MikrainService;

namespace AppleTvLiar.AppleChannels.HtmlManager
{
    public class ShowSiteManager:AppleBase
    {
        private const string BaseUrl = "http://moiserialy.net";

        public ShowSiteManager()
        {
            KillAce();
        }

        public XDocument GetConent()
        {
            XDocument xDocument = XDocument.Load(Path.Combine(MikrainProgramm._xmlPath, @"Content\categories.xml"));
            var items = xDocument.Descendants(XName.Get("items"));
            int shelfIdCount = 0;
            var doc = new HtmlDocument();
            // if (doc.GetHtmlRequest("http://moiserialy.net/serialy#en"))
            if (doc.GetHtmlRequest("http://moiserialy.net/"))
            {
                var div = doc.DocumentNode.Descendants("div");
                int count = 0;


                foreach (var childNode in div)
                {
                    //if (childNode.GetAttributeValue("class", "") == "list-item serials-item") 
                    if (childNode.GetAttributeValue("class", "") == "top-serial-list")
                    {
                        shelfIdCount++;

                        var collectionDividerElement = new XElement(XName.Get("collectionDivider"));
                        collectionDividerElement.SetAttributeValue(XName.Get("alignment"), "left");
                        collectionDividerElement.SetAttributeValue(XName.Get("accessibilityLabel"), shelfIdCount);
                        collectionDividerElement.Add(new XElement(XName.Get("title"), string.Format("Page number {0}", shelfIdCount)));

                        XElement shelfElement = new XElement(XName.Get("shelf"));
                        shelfElement.SetAttributeValue(XName.Get("id"), string.Format("shelf_{0}", shelfIdCount));
                        shelfElement.SetAttributeValue(XName.Get("columnCount"), "5");


                        var sectionsElement = new XElement(XName.Get("sections"));
                        var shelfSectionElement = new XElement(XName.Get("shelfSection"));
                        XElement itemsElement = new XElement(XName.Get("items"));

                        shelfSectionElement.Add(itemsElement);
                        sectionsElement.Add(shelfSectionElement);
                        shelfElement.Add(sectionsElement);

                        var netstedDivs = childNode.Descendants("div");
                        foreach (var netstedDivTmp in netstedDivs)
                        {
                            string image = "";
                            string href = "";
                            string title = "";



                            if (netstedDivTmp.GetAttributeValue("class", "") == "list-item serials-item")
                            {
                                foreach (var htmlNodeTmp in netstedDivTmp.ChildNodes)
                                {
                                    var imgList = htmlNodeTmp.Descendants("img");
                                    foreach (var imgNode in imgList)
                                    {
                                        image = BaseUrl + imgNode.GetAttributeValue("data-href", "");
                                        break;
                                    }

                                    if (htmlNodeTmp.GetAttributeValue("class", "") == "item-desc")
                                    {
                                        var itemtitle =
                                            htmlNodeTmp.ChildNodes.FirstOrDefault(htmlNode => htmlNode.Name == "div");
                                        var a = itemtitle.ChildNodes.FirstOrDefault(htmlNode => htmlNode.Name == "a");
                                        href = a.GetAttributeValue("href", "");
                                        title = a.InnerText;
                                        continue;
                                    }
                                }
                                CreateElementList(count++,
                                            string.Format(
                                                "loadTrailerDetailPage('http://trailers.apple.com/Show?show={0}{1}')",
                                                Uri.EscapeDataString(BaseUrl), Uri.EscapeDataString(href)),
                                            title, image, itemsElement);
                               
                            }
                        }

                        items.First().Add(collectionDividerElement);
                        items.First().Add(shelfElement);
                    }
                }
            }
            return xDocument;
        }

        internal XDocument GetSeasons(string show)
        {
            XDocument xDocument = XDocument.Load(Path.Combine(MikrainProgramm._xmlPath, @"Content\showList.xml"));

            var imageElement = xDocument.Descendants(XName.Get("image"));
            var items = xDocument.Descendants(XName.Get("items"));
            var titleElement = xDocument.Descendants(XName.Get("title"));
            var descElement = xDocument.Descendants(XName.Get("summary"));
            //  imageElement.First().SetValue(imageUrl);

            var doc = new HtmlDocument();
            if (doc.GetHtmlRequest(show))
            {
                var div = doc.DocumentNode.Descendants("div");

                int count = 0;

                foreach (var childNode in div)
                {

                    if (childNode.GetAttributeValue("class", "") == "serial-image")
                    {
                        var a = childNode.ChildNodes.FirstOrDefault(node => node.Name == "a");
                        if (a != null)
                        {
                            var title = a.GetAttributeValue("title", "");
                            var image = a.ChildNodes.FirstOrDefault(node => node.Name == "img").GetAttributeValue("data-href", "");
                            imageElement.First().SetValue(BaseUrl + image);
                            titleElement.First().SetValue(title);
                        }
                        continue;
                    }

                    if (childNode.GetAttributeValue("class", "") == "descript")
                    {
                        var a = childNode.ChildNodes.FirstOrDefault(node => node.Name == "p");
                        if (a != null)
                        {
                            var desc = a.InnerText;
                            descElement.First().SetValue(desc);
                        }
                        continue;
                    }


                    if (childNode.GetAttributeValue("class", "") == "sliderGallery")
                    {
                        var ul = childNode.ChildNodes.FirstOrDefault(node => node.Name == "ul");
                        var li = ul.Descendants("li");
                        foreach (var liNode in li)
                        {
                            var liA = liNode.ChildNodes.FirstOrDefault(node => node.Name == "a");
                            var liHref = liA.GetAttributeValue("href", "");
                            var liTitle = liA.InnerText;
                            var liDiv = liNode.ChildNodes.FirstOrDefault(node => node.Name == "div");
                            var image = BaseUrl + liDiv.ChildNodes.FirstOrDefault(node => node.Name == "img").GetAttributeValue("data-href", "");
                            CreateElementList(count++, string.Format("loadTrailerDetailPage('http://trailers.apple.com/ShowSeries?showSeries={0}&name={1}')", Uri.EscapeDataString(BaseUrl + liHref), Uri.EscapeDataString(liTitle)), liTitle.ToString(), image, items.First());
                        }
                    }
                }
            }
            return xDocument;
        }

        internal XDocument GetSeries(string showSeries, string name = "")
        {
            XDocument xDocument = XDocument.Load(Path.Combine(MikrainProgramm._xmlPath, @"Content\showList.xml"));

            var imageElement = xDocument.Descendants(XName.Get("image"));
            var items = xDocument.Descendants(XName.Get("items"));
            var titleElement = xDocument.Descendants(XName.Get("title"));
            var descElement = xDocument.Descendants(XName.Get("summary"));
            titleElement.First().SetValue(name);
            descElement.First().SetValue("");

            var doc = new HtmlDocument();
            if (doc.GetHtmlRequest(showSeries))
            {
                int count = 0;
                var div = doc.DocumentNode.Descendants("div");
                foreach (var childNode in div)
                {
                    if (childNode.GetAttributeValue("class", "") == "serial-image")
                    {
                        var a = childNode.ChildNodes.FirstOrDefault(node => node.Name == "a");
                        var image = a.ChildNodes.FirstOrDefault(node => node.Name == "img").GetAttributeValue("data-href", "");
                        imageElement.First().SetValue(BaseUrl + image);

                        continue;
                    }

                    if (childNode.GetAttributeValue("class", "") == "list-item serials-item")
                    {
                        var netstedDivs = childNode.Descendants("div");
                        string imageNode = "";
                        string href = "";
                        string title = "";
                        foreach (var netstedDiv in netstedDivs)
                        {
                            if (netstedDiv.GetAttributeValue("class", "") == "item-image")
                            {
                                var imgborder = netstedDiv.ChildNodes.FirstOrDefault(htmlNode => htmlNode.Name == "div");
                                var img = imgborder.ChildNodes.FirstOrDefault(htmlNode => htmlNode.Name == "img");
                                imageNode = img.GetAttributeValue("data-href", "");
                                continue;
                            }
                            if (netstedDiv.GetAttributeValue("class", "") == "item-desc")
                            {
                                var itemtitle = netstedDiv.ChildNodes.FirstOrDefault(htmlNode => htmlNode.Name == "div");
                                var a = itemtitle.ChildNodes.FirstOrDefault(htmlNode => htmlNode.Name == "a");
                                if (a == null)
                                {
                                    continue;
                                }
                                href = a.GetAttributeValue("href", "");
                                title = a.InnerText;
                                continue;
                            }
                        }
                        CreateElementList(count++, string.Format("loadTrailerDetailPage('http://trailers.apple.com/ShowSer?ser={0}&title={1}&image={2}')", Uri.EscapeDataString(BaseUrl + href), Uri.EscapeDataString(title), Uri.EscapeDataString(imageNode)), title, imageNode, items.First());

                    }
                }
            }
            return xDocument;
        }

        internal XDocument ShowSer(string ser, string title, string image)
        {
            XDocument xDocument =
                     XDocument.Load(Path.Combine(MikrainProgramm._xmlPath, @"Content\movie.xml"));
            var element = xDocument.Descendants(XName.Get("image"));
            var elementDesc = xDocument.Descendants(XName.Get("summary"));
            var elementName = xDocument.Descendants(XName.Get("title"));
            elementName.First().SetValue(title);
            elementDesc.First().SetValue("");
            element.First().SetValue(image);

            var doc = new HtmlDocument();
            if (doc.GetHtmlRequest(ser))
            {
                //var div = doc.DocumentNode.Descendants("div");
                //foreach (var htmlNodeTmp in div)
                //{
                //    if (htmlNodeTmp.GetAttributeValue("class", "") == "item-image")
                //    {
                //        var imgborder =
                //            htmlNodeTmp.ChildNodes.FirstOrDefault(htmlNode => htmlNode.Name == "div");
                //        var img = imgborder.ChildNodes.FirstOrDefault(htmlNode => htmlNode.Name == "img");
                //       // var image = img.GetAttributeValue("data-href", "");

                //        break;
                //    }
                //}

                var regex = new Regex("(url: ')(http://.*.mp4)'");
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
    }
}
