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
    public class FriendsSiteManager : AppleBase
    {

        public FriendsSiteManager()
        {
            KillAce();
        }
        public XDocument GetFriendSeries(string htmlUrl, string imageUrl)
        {
            XDocument xDocument = GetXml(Path.Combine(MikrainProgramm._xmlPath, @"Content\showList.xml")).GetXDocument();

            var imageElement = xDocument.Descendants(XName.Get("image"));
            var items = xDocument.Descendants(XName.Get("items"));
            imageElement.First().SetValue(imageUrl);

            var doc = new HtmlDocument();
            if (doc.GetHtmlRequest(htmlUrl))
            {
                var div = doc.DocumentNode.Descendants("a");
                int count = 0;

                foreach (var childNode in div)
                {
                    if (childNode.Name == "a" && !string.IsNullOrEmpty(childNode.GetAttributeValue("href", "")))
                    {
                        var node = childNode.ChildNodes.FirstOrDefault(htmlNode => htmlNode.Name == "img");
                        if (node != null)
                        {
                            var href = childNode.GetAttributeValue("href", "");
                            var image = node.GetAttributeValue("src", "");
                            var childSpan = childNode.ChildNodes.FirstOrDefault(htmlNode => htmlNode.Name == "span");
                            HtmlNode title = null;
                            if (childSpan != null)
                            {
                                title = childSpan.FirstChild;
                            }
                            CreateElementList(count++, string.Format("loadTrailerDetailPage('http://trailers.apple.com/FriendsPlay?friendsPlayHref={0}&imageHref={1}')", Uri.EscapeDataString(href), Uri.EscapeDataString(image)), title.InnerText, image, items.First());
                        }
                    }
                }
            }
            return xDocument;
        }

        public XDocument GetConent(string url, string fileName, bool isfFull = true)
        {
            // XDocument xDocument = XDocument.Load(Path.Combine(MikrainProgramm._xmlPath, @"Content\baskino.xml"));
            XDocument xDocument = GetXml(Path.Combine(MikrainProgramm._xmlPath, fileName)).GetXDocument();
            var items = xDocument.Descendants(XName.Get("items"));
            var doc = new HtmlDocument();
            if (doc.GetHtmlRequest(url))
            {
                var div = doc.DocumentNode.Descendants("a");
                int count = 0;
                XElement collectionDividerElement = null;
                XElement shelfElement = null;
                XElement itemsElement = null;

                foreach (var childNode in div)
                {
                    if (childNode.Name == "a" && !string.IsNullOrEmpty(childNode.GetAttributeValue("href", "")))
                    {
                        var node = childNode.ChildNodes.FirstOrDefault(htmlNode => htmlNode.Name == "img");
                        if (node != null)
                        {
                            if (count == 0 || count % 5 == 0)
                            {
                                int shelfIdCount = 0;
                                collectionDividerElement = new XElement(XName.Get("collectionDivider"));
                                collectionDividerElement.SetAttributeValue(XName.Get("alignment"), "left");
                                collectionDividerElement.SetAttributeValue(XName.Get("accessibilityLabel"), shelfIdCount);
                                collectionDividerElement.Add(new XElement(XName.Get("title"), string.Empty));


                                shelfElement = new XElement(XName.Get("shelf"));
                                shelfElement.SetAttributeValue(XName.Get("id"), string.Format("shelf_{0}", 1));
                                shelfElement.SetAttributeValue(XName.Get("columnCount"), "5");


                                var sectionsElement = new XElement(XName.Get("sections"));
                                var shelfSectionElement = new XElement(XName.Get("shelfSection"));
                                itemsElement = new XElement(XName.Get("items"));

                                shelfSectionElement.Add(itemsElement);
                                sectionsElement.Add(shelfSectionElement);
                                shelfElement.Add(sectionsElement);
                            }

                            var href = childNode.GetAttributeValue("href", "");
                            var image = node.GetAttributeValue("src", "");
                            var childSpan = childNode.ChildNodes.FirstOrDefault(htmlNode => htmlNode.Name == "span");
                            HtmlNode title = null;
                            if (childSpan != null)
                            {
                                title = childSpan.FirstChild;
                            }
                            CreateElementList(count++, string.Format("loadTrailerDetailPage('http://trailers.apple.com/FriendsShow?friendsSeason={0}&imgUrl={1}')", Uri.EscapeDataString(href), Uri.EscapeDataString(image)),
                                 title.InnerText, image, itemsElement);
                            if (count % 5 == 0)
                            {
                                items.First().Add(collectionDividerElement);
                                items.First().Add(shelfElement);
                            }

                        }
                    }
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
            if (doc.GetHtmlRequest(friendsPlayHref))
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
    }
}
