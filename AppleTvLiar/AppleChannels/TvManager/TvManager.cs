using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using AppleTvLiar.AppleChannels.HtmlManager;
using System.Xml.Linq;
using HtmlAgilityPack;

namespace AppleTvLiar.AppleChannels.TvManager
{
    public class TvManager : AppleBase
    {


        internal XmlDocument GetChannel(string channel)
        {

            if (channel.EndsWith(".xml"))
            {
                return GenereateChannels(channel);
            }


            string fileName = Path.Combine(MikrainService.MikrainProgramm._xmlPath, @"Content\channels.atl");

            using (var reader = new StreamReader(fileName))
            {
                string line;
                while (!string.IsNullOrEmpty(line = reader.ReadLine()))
                {
                    var data = line.Split('=');
                    if (data.Any())
                    {
                        if (data[0] == channel)
                        {
                            var schedule = CheckSchedule(channel);
                            if (string.IsNullOrEmpty(schedule))
                            {
                                return PlayChannel(data[1]);
                            }
                            try
                            {
                                var tvMovie = GetXml(@"Content\playSchedule.xml").GetXDocument();
                                FillSchedule(tvMovie, data[1], schedule);
                                return tvMovie.GetXmlDocument();
                            }
                            catch (Exception)
                            {
                                return PlayChannel(data[1]);
                            }
                        }
                    }
                }

            }
            var xml = GetXml(Path.Combine(MikrainService.MikrainProgramm._xmlPath, string.Format(@"Content\{0}.xml", channel)));
            return xml;
        }

        private XmlDocument GenereateChannels(string channels)
        {
            var asd = XDocument.Load(channels);

            var linkMovies = asd.Descendants(XName.Get("LinkMovies"));
            var link = linkMovies.ElementAt(0).Value;

            var xmlRaw = HttpRequestsString(link);
            xmlRaw = xmlRaw.Replace("group", "Group");
            var parsed = XDocument.Parse(xmlRaw);

            //var xDocment = XDocument.Load(Path.Combine(MikrainService.MikrainProgramm._xmlPath, string.Format(@"Content\{0}.xml", "menuGroups")));
            var x = new System.Xml.Serialization.XmlSerializer(typeof(plist));
            var playelist = (plist)x.Deserialize(parsed.CreateReader());

            XElement root = CreateRoot();
            CreateHead(root);
            XElement items = CreateBody(root);




            for (int i = 0; i < playelist._Group.Count(); i++)
            {
                if (false)
                {
                    var showcase = new XElement(XName.Get("showcase"));
                    var initialSelection = new XElement(XName.Get("initialSelection"));
                    var indexPath = new XElement(XName.Get("indexPath"));
                    var index = new XElement(XName.Get("index"));
                    index.SetValue("0");
                    indexPath.Add(index);
                    initialSelection.Add(indexPath);

                    showcase.Add(initialSelection);
                    showcase.Add(new XAttribute(XName.Get("id"), "showcase"));
                    var bigItems = new XElement(XName.Get("items"));
                    bigItems.Add(new XAttribute(XName.Get("id"), "showcase-items"));
                    showcase.Add(bigItems);
                    items.Add(showcase);
                    CreateChannel(bigItems, playelist._Group[i], false);
                }
                else
                {
                    CreateChannel(items, playelist._Group[i]);
                }

            }

            //foreach (var @group in playelist._Group)
            //{

            //}



            return GetDocument(root);
        }

        private XmlDocument GetDocument(XElement elemtn)
        {
            StringBuilder sb = new StringBuilder();
            XmlWriterSettings xws = new XmlWriterSettings();
            xws.OmitXmlDeclaration = true;
            xws.Indent = true;


            using (XmlWriter xw = XmlWriter.Create(sb, xws))
            {
                elemtn.WriteTo(xw);
            }
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(sb.ToString());
            return doc;
        }

        private void CreateChannel(XElement items, UniTv.Group group, bool addDiv = true)
        {
            if (addDiv)
            {
                var collectionDivider = new XElement(XName.Get("collectionDivider"));
                collectionDivider.Add(new XAttribute(XName.Get("alignment"), "left"));
                collectionDivider.Add(new XAttribute(XName.Get("accessibilityLabel"), group.Name));
                var title = new XElement(XName.Get("title"));
                title.Value = group.Name;
                collectionDivider.Add(title);
                items.Add(collectionDivider);
            }


            CreateShelf(items, group, !addDiv);
        }

        private void CreateShelf(XElement itemsRoot, UniTv.Group group, bool header = false)
        {

            var shelf = new XElement(XName.Get("shelf"));
            shelf.Add(new XAttribute(XName.Get("columnCount"), "4"));
            shelf.Add(new XAttribute(XName.Get("id"), "shelf_2"));
            var sections = new XElement(XName.Get("sections"));
            var shelfSection = new XElement(XName.Get("shelfSection"));
            var items = new XElement(XName.Get("items"));
            shelf.Add(sections);
            sections.Add(shelfSection);
            shelfSection.Add(items);


            for (int i = 0; i < group.items.Count(); i++)
            {
                XElement itemsTmp;
                string imageTmp = "";
                XElement moviePoster = null;

                if (header)
                {
                    itemsTmp = itemsRoot;
                    imageTmp = "resource://16x9-default.png";
                    moviePoster = new XElement(XName.Get("showcasePoster"));
                    moviePoster.Add(new XAttribute(XName.Get("id"), group.items[i].IdMovie));
                }
                else
                {

                    imageTmp = "resource://Poster.png";
                    moviePoster = new XElement(XName.Get("moviePoster"));
                    itemsTmp = items;
                    moviePoster.Add(new XAttribute(XName.Get("id"), "shelf_item_" + group.items[i].IdMovie));
                    moviePoster.Add(new XAttribute(XName.Get("alwaysShowTitles"), "true"));
                    var title = new XElement(XName.Get("title"));
                    title.SetValue(group.items[i].Text);
                    moviePoster.Add(title);
                }




                moviePoster.Add(new XAttribute(XName.Get("accessibilityLabel"), group.items[i].Text));
                moviePoster.SetAttributeValue("onSelect", string.Format("atv.loadURL('http://trailers.apple.com/playChannel?href={0}')", Uri.EscapeDataString(group.items[i].LinkMovie)));
                moviePoster.SetAttributeValue("onPlay", string.Format("atv.loadURL('http://trailers.apple.com/playChannel?href={0}')", Uri.EscapeDataString(group.items[i].LinkMovie)));

                var image = new XElement(XName.Get("image"));
                image.SetValue(group.items[i].LinkIconLarge);
                moviePoster.Add(image);
                var defaultImage = new XElement(XName.Get("defaultImage"));
                defaultImage.SetValue(imageTmp);
                moviePoster.Add(defaultImage);

                itemsTmp.Add(moviePoster);
            }

            if (!header)
            {
                itemsRoot.Add(shelf);
            }


        }

        private XElement CreateBody(XElement root)
        {
            var body = new XElement(XName.Get("body"));
            var scroller = new XElement(XName.Get("scroller"));
            scroller.Add(new XAttribute(XName.Get("id"), "com.sample.movie-shelf"));
            var items = new XElement(XName.Get("items"));
            scroller.Add(items);
            body.Add(scroller);
            root.Add(body);
            return items;
        }

        private void CreateHead(XElement root)
        {
            var head = new XElement(XName.Get("head"));
            var script = new XElement(XName.Get("script"));
            script.Add(new XAttribute(XName.Get("src"), "http://trailers.apple.com/appletv/us/js/main.js"));
            head.Add(script);
            root.Add(head);
        }

        private XElement CreateRoot()
        {
            XElement element = new XElement(XName.Get("atv"));
            return element;
        }

        public XmlDocument PlayChannel(string data)
        {
            var tvChannel = GetXml(@"Content\tvchannel.xml").GetXDocument();
            var element = tvChannel.Descendants(XName.Get("mediaURL"));
            if (data.StartsWith("http"))
            {
                element.First().SetValue(new XCData(data).Value);
            }
            else
            {
                var manager = new ProccessManager.ProccessManager();
                manager.StartChannel(data);
                element.First().SetValue(new XCData("http://trailers.apple.com/getstream").Value);
            }

            return tvChannel.GetXmlDocument();
        }

        private void FillSchedule(XDocument tvMovie, string m3U8, string scheduleHref)
        {
            var xml = GetXml(Path.Combine(MikrainService.MikrainProgramm._xmlPath, string.Format(@"Content\{0}.xml", "movieColumns"))).GetXDocument();
            var summary = tvMovie.Descendants(XName.Get("bottomShelf")).First();
            var sumNode = tvMovie.Descendants(XName.Get("summary")).First();

            sumNode.SetValue("");
            var rows = xml.Descendants("rows").First();

            var doc = new HtmlDocument();
            GetHtmlRequest(doc, GetScheduleHref(scheduleHref));
            var element = doc.GetElementbyId("print_div");
            var tds = element.Descendants("td");

            var row = new XElement(XName.Get("row"));
            int count = 0;
            foreach (var td in tds)
            {
                var divs = td.Descendants("div");
                foreach (var div in divs)
                {
                    if (count == 4)
                    {
                        count = 0;
                        row = new XElement(XName.Get("row"));
                    }

                    if (div.GetAttributeValue("id", "") == "ann_time")
                    {
                        DateTime convertedDate = DateTime.Parse(div.InnerText).AddHours(-3);
                        DateTime dt = convertedDate.ToLocalTime();
                        var label = new XElement(XName.Get("label"));
                        label.SetValue(string.Format(dt.ToString("H:mm")));
                        row.Add(label);
                        count++;
                    }
                    if (div.GetAttributeValue("id", "") == "text_prg")
                    {
                        var label = new XElement(XName.Get("label"));
                        label.SetValue(new XCData(div.InnerText).Value.Replace("&quot;", " ").TrimStart());
                        row.Add(label);

                        count++;
                        if (count == 2)
                        {
                            row.Add(new XElement(XName.Get("label")));
                        }
                    }

                    if (count == 4)
                    {
                        rows.Add(row);

                    }

                }
            }

            var actionButtonElement = tvMovie.Descendants("actionButton");
            actionButtonElement.First().SetAttributeValue("onSelect", string.Format("atv.loadURL('http://trailers.apple.com/playChannel?href={0}')", Uri.EscapeDataString(m3U8)));
            actionButtonElement.First().SetAttributeValue("onPlay", string.Format("atv.loadURL('http://trailers.apple.com/playChannel?href={0}')", Uri.EscapeDataString(m3U8)));


            if (summary != null) summary.AddBeforeSelf(xml.Descendants(XName.Get("table")).First());
        }


        private string GetScheduleHref(string scheduleHref)
        {
            var dayOfWeek = DateTime.UtcNow.AddHours(4).DayOfWeek;
            int dayNumber;
            switch (dayOfWeek)
            {
                case DayOfWeek.Sunday:
                    dayNumber = 6;
                    break;
                case DayOfWeek.Monday:
                    dayNumber = 0;
                    break;
                case DayOfWeek.Tuesday:
                    dayNumber = 1;
                    break;
                case DayOfWeek.Wednesday:
                    dayNumber = 2;
                    break;
                case DayOfWeek.Thursday:
                    dayNumber = 3;
                    break;
                case DayOfWeek.Friday:
                    dayNumber = 4;
                    break;
                case DayOfWeek.Saturday:
                    dayNumber = 5;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            string newSchedule = scheduleHref.Replace("{on_day}", dayNumber.ToString());
            return newSchedule;
        }

        private string CheckSchedule(string channel)
        {
            string fileName = Path.Combine(MikrainService.MikrainProgramm._xmlPath, @"Content\schedule.atl");

            var reader = new StreamReader(fileName);
            string line;
            while (!string.IsNullOrEmpty(line = reader.ReadLine()))
            {
                var data = line.Split('=');
                if (data.Any())
                {
                    if (data[0] == channel)
                    {
                        return data[1] + "=" + data[2] + "=" + data[3];

                    }
                }
            }
            return string.Empty;
        }

        public bool GetHtmlRequest(HtmlDocument htmlDocument, string url)
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
                        Console.WriteLine(result);
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
