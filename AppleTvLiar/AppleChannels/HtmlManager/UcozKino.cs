using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Configuration;
using System.Xml;
using System.Xml.Linq;
using HtmlAgilityPack;
using MikrainService;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Net.Http;
using AppleTvLiar.Helper;

namespace AppleTvLiar.AppleChannels.HtmlManager
{
    public class UcozKino : AppleBase
    {
        public UcozKino()
        {

        }

        public async Task<XDocument> SearchUkoz(string query)
        {
            XDocument xDocument =
        XDocument.Load(Path.Combine(MikrainProgramm._xmlPath, @"Content\searchResult.xml"));
            int count = 0;

            string endPoint = "http://kino-fs.ucoz.net/load/";
            var dict = new Dictionary<string, string>();
            dict.Add("a", "2");
            dict.Add("query", query);


            var sresult = await SendPostRequest(endPoint, dict);

            var doc = new HtmlDocument();

            doc.OptionDefaultStreamEncoding = new UnicodeEncoding();
            doc.OptionOutputAsXml = true;
            doc.OptionAutoCloseOnEnd = true;
            doc.OptionWriteEmptyNodes = true;
            doc.OptionFixNestedTags = true;
            doc.OptionReadEncoding = false;
            doc.LoadHtml(sresult);



            //var divs = doc.GetElementbyId("allEntries");

            var divs = doc.DocumentNode.Descendants("div");
            var items = xDocument.Descendants(XName.Get("items"));
            foreach (var div in divs)
            {
                try
                {
                    if (div.GetAttributeValue("id", "").Contains("entryID"))
                    {
                        var inner = new HtmlDocument();
                        inner.LoadHtml(div.InnerHtml);

                        var imageDoc = inner.DocumentNode.SelectSingleNode("//table/tr/td/div[2]/img");
                        if (imageDoc == null) imageDoc = inner.DocumentNode.SelectSingleNode("//table/tr/td/div[2]/div/img");

                        string image = "";
                        if (imageDoc != null)
                            image = imageDoc.GetAttributeValue("src", "").Replace("http://kino-fs.ucoz.net", "");

                        var name = inner.DocumentNode.SelectSingleNode("//table/tr/td/div[1]/a/span/b/span").InnerText;
                        var href = inner.DocumentNode.SelectSingleNode("//table/tr/td/span[2]/a").GetAttributeValue("href", "");

                        name = name.Replace("Смотреть", "").Replace("онлайн", "");

                        var posterMenuItem = new XElement(XName.Get("posterMenuItem"));
                        posterMenuItem.Add(new XAttribute(XName.Get("id"), "movieBoxObject_" + count++));
                        posterMenuItem.Add(new XAttribute(XName.Get("accessibilityLabel"), name.Replace(" ", "")));
                        posterMenuItem.Add(new XAttribute(XName.Get("onSelect"), string.Format(
                                                              "atv.loadURL('http://trailers.apple.com/getUcozMovie?movie={0}&imageUrl={1}&movieTitle={2}')",
                                                             Uri.EscapeDataString(href), Uri.EscapeDataString(image), Uri.EscapeDataString(name))));
                        posterMenuItem.Add(new XAttribute(XName.Get("onPlay"), string.Format(
                                                              "atv.loadURL('http://trailers.apple.com/getUcozMovie?movie={0}&imageUrl={1}&movieTitle={2}')",
                                                             Uri.EscapeDataString(href), Uri.EscapeDataString(image), Uri.EscapeDataString(name))));
                        posterMenuItem.Add(new XElement(XName.Get("label"), name));
                        posterMenuItem.Add(new XElement(XName.Get("image"), "http://kino-fs.ucoz.net" + image));
                        items.First().Add(posterMenuItem);
                    }
                }
                catch (Exception ex)
                {
                }


            }

            return xDocument;
        }

        List<Task> threads = new List<Task>();
        public async Task<XDocument> GetContent(string url = "http://kino-fs.ucoz.net/load/novinki/46-{0}", string cacheName = "ucoz")
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



            Console.WriteLine(DateTime.Now);
            for (int i = 1; i < 15; i++)
            {
                int i1 = i;
                threads.Add(Task.Run(() => AddToContent(url, i1, elements)));

            }

            Task.WaitAll(threads.ToArray());
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
            doc.OptionReadEncoding = false;
            doc.LoadHtml(html);


            //var divs = doc.GetElementbyId("allEntries");

            if (i == 1)
            {
                CreateElementList(count++, string.Format(
                    "atv.loadURL('http://trailers.apple.com/getUcozMovie?movie={0}&imageUrl={1}&movieTitle={2}')",
                    Uri.EscapeDataString("search"), "", ""), "search", "http://www.kudoschatsearch.com/images/search.png",
                    itemsElement);
            }


            var divs = doc.DocumentNode.Descendants("div");

            foreach (var div in divs)
            {
                if (div.GetAttributeValue("id", "").Contains("entryID"))
                {
                    var strongs = div.ChildNodes.Descendants("strong");

                    var inner = new HtmlDocument();
                    inner.LoadHtml(div.InnerHtml);

                    var image = inner.DocumentNode.SelectSingleNode("//div[1]/div[1]/img").GetAttributeValue("src", "");
                    var name = inner.DocumentNode.SelectSingleNode("//div[1]/a").InnerText;
                    var href = inner.DocumentNode.SelectSingleNode("//div[1]/a").GetAttributeValue("href", "");

                    name = name.Replace("Смотреть", "").Replace("онлайн", "").Replace("сериал", "");

                    CreateElementList(count++,
                        string.Format(
                            "atv.loadURL('http://trailers.apple.com/getUcozMovie?movie={0}&imageUrl={1}&movieTitle={2}')",
                            Uri.EscapeDataString(href), Uri.EscapeDataString(image), Uri.EscapeDataString(name)), name,
                        "http://kino-fs.ucoz.net" + image, itemsElement);
                }
            }

            if (elements != null)
                if (!elements.ContainsKey(i))
                    elements.Add(i, new List<XElement>() { collectionDividerElement, shelfElement });
            //items.First().Add(collectionDividerElement);
            //items.First().Add(shelfElement);
            Completed(elements);
            //               });

            //th1.Start();
            //threads.Add(th1);
        }

        private void Completed(Dictionary<int, List<XElement>> elements)
        {
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
                XDocument searchDoc = XDocument.Load(Path.Combine(MikrainProgramm._xmlPath, @"Content\searchtrailers.xml"));
                var baseUrl = searchDoc.Descendants(XName.Get("baseURL"));
                var firstOrDefault = baseUrl.FirstOrDefault();
                if (firstOrDefault != null)
                    firstOrDefault.SetValue("http://trailers.apple.com/UcozSearch?query=");
                return searchDoc;
            }

            var html = HttpRequests("http://kino-fs.ucoz.net" + url);

            try
            {

                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                string desc = "";
                string info = "";
                string fullDesc = "";
                var tables = doc.DocumentNode.Descendants("div");
                foreach (var htmlNode in tables)
                {
                    if (htmlNode.GetAttributeValue("class", "") == "mi-label" || htmlNode.GetAttributeValue("class", "") == "mi-desc")
                    {
                        info += htmlNode.InnerText.Replace("\n", "");
                        if (htmlNode.GetAttributeValue("class", "") == "mi-desc")
                        {
                            info += Environment.NewLine;
                        }

                    }

                    if (htmlNode.GetAttributeValue("class", "") == "m-desc full-text clearfix")
                    {
                        fullDesc += Environment.NewLine;
                        fullDesc += htmlNode.InnerText;
                    }
                }

                var infos = doc.DocumentNode.Descendants("i");
                foreach (var infoNode in infos)
                {
                    if (infoNode.GetAttributeValue("class", "") == "fa fa-clock-o")
                    {
                        desc += infoNode.ParentNode.InnerText + Environment.NewLine;
                        break;
                    }
                }
                //fa fa-clock-o
                desc += info;
                desc += fullDesc;

                var options = doc.DocumentNode.Descendants("iframe");

                if (options.Any(node => node.GetAttributeValue("src", "").Contains("hdgo.cc")))
                {
                    XDocument xDocument = GetXml(Path.Combine(MikrainProgramm._xmlPath, @"Content\showList.xml")).GetXDocument();
                    imageUrl = "http://kino-fs.ucoz.net" + imageUrl;

                    var sourceUrl = options.FirstOrDefault(node => node.GetAttributeValue("src", "").Contains("hdgo.cc"));
                    var htmlSources = HttpRequests(sourceUrl.GetAttributeValue("src", ""));
                    var document = new HtmlDocument();
                    document.LoadHtml(htmlSources);
                    var goFrame = document.DocumentNode.Descendants("iframe").FirstOrDefault();
                    var goFrameSource = "http:" + goFrame.GetAttributeValue("src", "") + "kino-fs.ucoz.net";
                    var goSource = HttpRequestsTest(goFrameSource);
                    var goSourcedocument = new HtmlDocument();
                    goSourcedocument.LoadHtml(goSource);

                    var season = goSourcedocument.GetElementbyId("season");

                    var seasonoptions = season.Descendants("option");


                    for (int i = 0; i < seasonoptions.Count(); i++)
                    {


                        var seasonValue = seasonoptions.ElementAt(i).GetAttributeValue("value", "");


                        var summarylement = xDocument.Descendants(XName.Get("summary"));
                        var titleElement = xDocument.Descendants(XName.Get("title"));
                        var imageElement = xDocument.Descendants(XName.Get("image"));
                        var items = xDocument.Descendants(XName.Get("items"));
                        imageElement.First().SetValue(imageUrl);
                        titleElement.First().SetValue(movieTitle);
                        summarylement.First().SetValue(desc);
                        CreateElementList(i, string.Format("loadTrailerDetailPage('http://trailers.apple.com/ShowUcozEpisodesGo?href={0}&imageHref={1}&title={2}&season={3}')", Uri.EscapeDataString(goFrameSource), Uri.EscapeDataString(imageUrl), Uri.EscapeDataString(movieTitle), Uri.EscapeDataString(seasonValue)), "Season " + (i + 1), imageUrl, items.First());
                    }


                    return xDocument;
                }
                else if (url.Contains("serialy") && options.Any() && options.Any(node => node.GetAttributeValue("src", "").Contains("moonwalk") || node.GetAttributeValue("src", "").Contains("serpens")))
                {

                    var srcRedirect = options.FirstOrDefault(node => node.GetAttributeValue("src", "").Contains("moonwalk") || node.GetAttributeValue("src", "").Contains("serpens"));
                    var urlRedirect = srcRedirect.GetAttributeValue("src", "");

                    var htmlRedirect = HttpRequests(urlRedirect);

                    doc.LoadHtml(htmlRedirect);


                    var text = doc.DocumentNode.InnerText;
                    var seasons = Regex.Match(text, "seasons: \\[(.*)\\]").Groups[1].Value.Split(',');
                    var refEnc = Regex.Match(text, "ref: encodeURIComponent\\('(.*)'\\),").Groups[1].Value;

                    //http://moonwalk.cc/serial/3c60e78b59f81d1d6fdfb72586deb4b1/iframe?season=5&episode=1&nocontrols=&nocontrols_translations=&nocontrols_seasons=&ref=MXJabjNFai9mRmJLUENNclhpYlo1VThRdzNvNjhDOWtyM3RWZ3lwQVYrVnRramVqM3NwNWxBRU5SUjhGdEdmVkIyLzAzQ0hDRC9Wc25PQ1lOUmxJQ00xUlM1SlJiUnBsUWVtSmJ4blNZbG96TFdKeEE2Rk8vRlhibW1LRG5NSk9idFFYOUk2R3R6djJsL1pLSXdZM1J1MDhLZURFbWtHMHhpTWxMa0h2amlaRkU2WG0vVWlwbXRRY0E0Ty9RMDVlYUMzK2RKZ1owYWFHTy9nN1lXdjJkemZRckVPLy9QVmY1K1BqWlBUM3IwUnh2NmxmQ3ZwajQ4U2dqcnFBV0ZFby0tc2RZMGlBSFdId1VVR1QzbytUajB3Zz09--e2b3c383556bc48cd1873c07b0e0f6d0458a7d73
                    //http://moonwalk.cc/serial/9b2221ae7967aad9195750a5dccf2bb5/iframe?season=2&episode=1&nocontrols=&nocontrols_translations=&nocontrols_seasons=&nocontrols_episodes=&ref=VVFxUDBXTjRqeThZUTRlelhIODJkUGt6Y0tFQU05WEdwNkhya1NDaHdjNWtVWklYR2FlbU9CeUtiVm85RFFra0l5cnkwM1JITlEwRnluZi8yT2NiZmxWa09CRHVKdGFNT3FweC9Gcld2ZWhOUXZnNFdGeGx6SzVoMWlacFltK2htL0Jxd05YNWRFaUdZOHpvdTBxTGRYWTFodEc0RXQzQ296aHcyNmxIRk5yUzlPL0ZaTEQyelgyS2tSS0d4Wk8xNVYrMjlhOGE2KzhpYUhaVXlQTFl3T1dGR1N1ajBIMS9KUStrZytHYmhyRT0tLVNVMzFUdzlZQmlvRDB1RUhEL3dJS3c9PQ%3D%3D--2c12ee8ab8d1ea6354102036b18d72b3f3687aa1&autoplay=null&start_time=null


                    XDocument xDocument = GetXml(Path.Combine(MikrainProgramm._xmlPath, @"Content\showList.xml")).GetXDocument();
                    imageUrl = "http://kino-fs.ucoz.net" + imageUrl;

                    for (int i = 0; i < seasons.Count(); i++)
                    {
                        var idxSeas = urlRedirect.IndexOf("season=");
                        string urlSeason = "";
                        if (idxSeas >= 0)
                        {
                            urlSeason = urlRedirect.Remove(idxSeas + 7, 1).Replace("season=", "season=" + seasons[i]);

                        }
                        else
                        {
                            urlSeason = urlRedirect + "?season=" + seasons[i];
                        }

                        var summarylement = xDocument.Descendants(XName.Get("summary"));
                        var titleElement = xDocument.Descendants(XName.Get("title"));
                        var imageElement = xDocument.Descendants(XName.Get("image"));
                        var items = xDocument.Descendants(XName.Get("items"));
                        imageElement.First().SetValue(imageUrl);
                        titleElement.First().SetValue(movieTitle);
                        summarylement.First().SetValue(desc);

                        // "loadTrailerDetailPage('http://trailers.apple.com/ShowUcozSer?ser={0}&title={1}&image={2}')",
                        //
                        CreateElementList(i, string.Format("loadTrailerDetailPage('http://trailers.apple.com/ShowUcozEpisodes?href={0}&imageHref={1}&title={2}&season={3}')", Uri.EscapeDataString(urlSeason), Uri.EscapeDataString(imageUrl), Uri.EscapeDataString(movieTitle), Uri.EscapeDataString("Season " + seasons[i])), "Season " + seasons[i], imageUrl, items.First());
                    }



                    return xDocument;

                    var xdoc = GetSeries(options, movieTitle, imageUrl);
                    SaveDoc(movieTitle, xdoc);
                    return xdoc;
                }
                else
                {

                    XDocument xDocument =
           XDocument.Load(Path.Combine(MikrainProgramm._xmlPath, @"Content\movie.xml"));
                    var element = xDocument.Descendants(XName.Get("image"));
                    var elementDesc = xDocument.Descendants(XName.Get("summary"));
                    var elementName = xDocument.Descendants(XName.Get("title"));
                    var actionButtonElement = xDocument.Descendants("actionButton");
                    actionButtonElement.Remove();

                    ////*[@id="contanier"]/table[1]/tbody/tr[2]/td[2]/div/table[2]/tbody/tr[2]/td/br[7]
                    ////*[@id="contanier"]
                    var strongs =
                        doc.DocumentNode.SelectSingleNode(
                            "//*[@id=\"contanier\"]/table[7]");

                    try
                    {
                        var frames = doc.DocumentNode.Descendants("iframe");
                        foreach (var htmlNode in frames)
                        {
                            try
                            {
                                var source = htmlNode.GetAttributeValue("src", "");

                                string videoSource = source;// source.Replace("iframe", "index.m3u8");

                                if (videoSource.Contains("kodik.biz/"))
                                {
                                    var htmlSources = HttpRequests(source);
                                }


                                if (videoSource.Contains("moonwalk.cc") || true)
                                {
                                    var htmlSources = HttpRequests(source);
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

                        element.First().SetValue("http://kino-fs.ucoz.net" + imageUrl);
                        elementName.First().SetValue(movieTitle);


                        elementDesc.First().SetValue(desc);
                    }
                    catch (Exception exc)
                    {

                    }


                    var ucozCache = url.Contains("multfilm") ? ReadDoc("mult") : ReadDoc("ucoz");
                    if (ucozCache != null)
                    {
                        var posters = ucozCache.Descendants(XName.Get("moviePoster")).ToList();
                        posters.ShuffleList();
                        foreach (var xElement in posters.Take(10))
                        {
                            xDocument.Descendants("items").ToList()[1].Add(xElement);
                        }
                    }

                    //SaveDoc(movieTitle, xDocument);
                    return xDocument;
                }


            }
            catch (WebException e)
            {
                if (e.Status == WebExceptionStatus.ProtocolError)
                {
                    WebResponse resp = e.Response;
                    using (StreamReader sr = new StreamReader(resp.GetResponseStream()))
                    {
                        var resEWrror = sr.ReadToEnd();
                        return Error(new Exception(resEWrror));
                    }
                }
            }
            catch (Exception exc)
            {

                return Error(exc);
            }

            return null;
        }

        public XDocument ShowUcozEpisodesGo(string href, string image, string title, string season)
        {
            XDocument xDocument = GetXml(Path.Combine(MikrainProgramm._xmlPath, @"Content\showList.xml")).GetXDocument();

            var goSource = HttpRequestsTest(href);
            var goSourcedocument = new HtmlDocument();
            goSourcedocument.LoadHtml(goSource);

            var episode = goSourcedocument.GetElementbyId("episode");

            var episodeoptions = episode.Descendants("option");

            //http://go.8p69ao4bo6dex.ru/video/serials/JcI2jgwv0TBQFwf8FjdSjbKAIrlChT9v/14517/?ref=kino-fs.ucoz.net&e=390188

            var imageElement = xDocument.Descendants(XName.Get("image"));
            var titleElement = xDocument.Descendants(XName.Get("title"));
            var summaryElement = xDocument.Descendants(XName.Get("summary"));
            var items = xDocument.Descendants(XName.Get("items"));
            imageElement.First().SetValue(image);
            titleElement.First().SetValue(title);

            for (int i = 0; i < episodeoptions.Count(); i++)
            {
                var episodeValue = episodeoptions.ElementAt(i).GetAttributeValue("value", "");

                goSource = HttpRequestsTest(href + "&e="+ episodeValue);
                goSourcedocument = new HtmlDocument();
                goSourcedocument.LoadHtml(goSource);

                var seasonUrl = Regex.Match(goSourcedocument.DocumentNode.InnerText, "media: \\[(.*)\\]").Groups[1].Value;
                var jArray = JArray.Parse("[" + seasonUrl + "]");
                var generatedUrl = jArray.First()["url"];
              
                var splittedUrl = generatedUrl.ToString().Split('/');
                string newUrl = "";

                for (int j = 0; j < splittedUrl.Count(); j++)
                {
                    if (j == 4)
                    {
                        newUrl += episodeValue + '/';
                    }
                    else if (j == 5)
                    {
                        newUrl += season + '/';
                    }
                    else
                    {
                        newUrl += splittedUrl[j] + '/';
                    }
                }

                newUrl = newUrl.Remove(newUrl.Length - 1);

                CreateElementList(i, string.Format("loadTrailerDetailPage('http://trailers.apple.com/ShowUcozSerGo?ser={0}&image={1}&title={2}')", Uri.EscapeDataString(href + "&e=" + episodeValue), Uri.EscapeDataString(image), Uri.EscapeDataString(title)), "Episode " + (i + 1), image, items.First());
            }

            return xDocument;
        }

        public XDocument ShowUcozEpisodes(string href, string image, string title, string season)
        {

            XDocument xDocument = GetXml(Path.Combine(MikrainProgramm._xmlPath, @"Content\showList.xml")).GetXDocument();

            var htmlRedirect = HttpRequests(href);
            var doc = new HtmlDocument();
            doc.LoadHtml(htmlRedirect);

            var episodes = Regex.Match(doc.DocumentNode.InnerText, "episodes: \\[(.*)\\]").Groups[1].Value.Split(',');

            //var episode = doc.GetElementbyId("episode");
            ////episodes: 
            //var episodeoptions = episode.Descendants("option");

            var imageElement = xDocument.Descendants(XName.Get("image"));
            var titleElement = xDocument.Descendants(XName.Get("title"));
            var summaryElement = xDocument.Descendants(XName.Get("summary"));
            var items = xDocument.Descendants(XName.Get("items"));
            imageElement.First().SetValue(image);
            titleElement.First().SetValue(title);
            //summaryElement.First().SetValue(description.ToString());

            for (int i = 0; i < episodes.Count(); i++)
            {
                var idxSeas = href.IndexOf("episode=");
                string urlEpisode = href;
                if (idxSeas > 0)
                {
                    urlEpisode = href.Remove(idxSeas + 7, 1).Replace("episode=", "episode=" + episodes[i]);

                }
                else
                {
                    urlEpisode += "&episode=" + episodes[i];
                }

                CreateElementList(i, string.Format("loadTrailerDetailPage('http://trailers.apple.com/ShowUcozSer?ser={0}&image={1}&title={2}')", Uri.EscapeDataString(urlEpisode), Uri.EscapeDataString(image), Uri.EscapeDataString(title)), "Episode " + (i + 1), image, items.First());

            }



            //for (int i = 0; i < movieBoxSeries.movieBoxEpisodeses.Count; i++)
            //{ }

            return xDocument;

        }


        public async Task<XDocument> ShowUcozSerGo(string ser, string title, string image)
        {
            XDocument xDocument =
         XDocument.Load(Path.Combine(MikrainProgramm._xmlPath, @"Content\movie.xml"));
            var element = xDocument.Descendants(XName.Get("image"));
            var elementDesc = xDocument.Descendants(XName.Get("summary"));
            var elementName = xDocument.Descendants(XName.Get("title"));
            var actionButtonElement = xDocument.Descendants("actionButton");
            actionButtonElement.Remove();


            var responseUri = await HttpRequestsGo(ser);

            CreateActionButton(responseUri, xDocument, "HD");

            element.First().SetValue(image);
            elementName.First().SetValue(title);
            elementDesc.First().SetValue("Temprory unavailable");

            var ucozCache = ser.Contains("multfilm") ? ReadDoc("mult") : ReadDoc("serialy");
            if (ucozCache != null)
            {
                var posters = ucozCache.Descendants(XName.Get("moviePoster")).ToList();
                posters.ShuffleList();
                foreach (var xElement in posters.Take(10))
                {
                    xDocument.Descendants("items").ToList()[1].Add(xElement);
                }
            }

            return xDocument;
        }




        public async Task<XDocument> ShowUcozSer(string ser, string title, string image)
        {
            XDocument xDocument =
         XDocument.Load(Path.Combine(MikrainProgramm._xmlPath, @"Content\movie.xml"));
            var element = xDocument.Descendants(XName.Get("image"));
            var elementDesc = xDocument.Descendants(XName.Get("summary"));
            var elementName = xDocument.Descendants(XName.Get("title"));
            var actionButtonElement = xDocument.Descendants("actionButton");
            actionButtonElement.Remove();


            var htmlSources = HttpRequests(ser);
            var document = new HtmlDocument();
            document.LoadHtml(htmlSources);

            var translations = Regex.Match(document.DocumentNode.InnerText, "translations: \\[(.*)\\]").Groups[1].Value;
            var jArray = JArray.Parse("[" + translations + "]");
            var transOrig = Regex.Match(ser, "serial/(.*)/iframe").Groups[1].Value;

            int count = 0;

            foreach (var itransJson in jArray)
            {
                try
                {
                    var transHash = itransJson.First();
                    var transName = itransJson.Last();

                    var newSerTrans = ser.Replace(transOrig, transHash.ToString());

                    var htmlWithTrans = HttpRequests(newSerTrans);
                    var newDoc = new HtmlDocument();
                    newDoc.LoadHtml(htmlWithTrans);

                    var link = await GetLink(newDoc);
                    if (link != null)
                    {
                        CreateActionButton(link, xDocument, transName.ToString());
                    }
                }
                catch (Exception)
                {
                    count++;
                }
            }

            if (count == jArray.Count())
            {
                var link = await GetLink(document);
                if (link != null)
                {
                    CreateActionButton(link, xDocument, "HD");
                }
            }

            element.First().SetValue(image);
            elementName.First().SetValue(title);
            elementDesc.First().SetValue("Temprory unavailable");

            var ucozCache = ser.Contains("multfilm") ? ReadDoc("mult") : ReadDoc("serialy");
            if (ucozCache != null)
            {
                var posters = ucozCache.Descendants(XName.Get("moviePoster")).ToList();
                posters.ShuffleList();
                foreach (var xElement in posters.Take(10))
                {
                    xDocument.Descendants("items").ToList()[1].Add(xElement);
                }
            }

            return xDocument;
        }




        private Thread GetInfo(int count, HtmlNode optionNode, string image, Dictionary<int, SerInfo> info)
        {
            var th1 = new Thread(() =>
            {
                try
                {
                    var valueVk = optionNode.GetAttributeValue("value", "");
                    var titleSer = optionNode.NextSibling.InnerText;

                    if (valueVk.StartsWith("//"))
                    {
                        valueVk = valueVk.Replace("//", "http://");
                    }

                    var htmlSources = HttpRequests(valueVk);
                    var document = new HtmlDocument();

                    document.LoadHtml(htmlSources);
                    var video = document.GetElementbyId("video");
                    string poster = "http://kino-fs.ucoz.net" + image;
                    if (video != null)
                    {
                        poster = video.GetAttributeValue("poster", "");


                    }

                    info.Add(count, new SerInfo() { image = poster, title = titleSer, url = valueVk });


                }
                catch (Exception)
                {

                }
            });
            return th1;
        }

        internal XDocument GetSeries(IEnumerable<HtmlNode> options, string name, string image)
        {
            XDocument xDocument = XDocument.Load(Path.Combine(MikrainProgramm._xmlPath, @"Content\showList.xml"));

            var imageElement = xDocument.Descendants(XName.Get("image"));
            var items = xDocument.Descendants(XName.Get("items"));
            var titleElement = xDocument.Descendants(XName.Get("title"));
            var descElement = xDocument.Descendants(XName.Get("summary"));
            titleElement.First().SetValue(name);
            descElement.First().SetValue("");
            imageElement.First().SetValue("http://kino-fs.ucoz.net" + image);
            int count = 0;
            var threads = new List<Thread>();
            var info = new Dictionary<int, SerInfo>();


            foreach (var optionNode in options)
            {
                var th1 = GetInfo(count++, optionNode, image, info);
                th1.Start();
                threads.Add(th1);
            }

            foreach (var thread in threads)
            {
                thread.Join();
            }


            for (int i = 0; i < info.Count; i++)
            {
                if (info.ContainsKey(i))
                {
                    CreateElementList(i,
                        string.Format(
                            "loadTrailerDetailPage('http://trailers.apple.com/ShowUcozSer?ser={0}&title={1}&image={2}')",
                            Uri.EscapeDataString(info[i].url), Uri.EscapeDataString(info[i].title),
                            Uri.EscapeDataString(info[i].image)), info[i].title, info[i].image, items.First());
                }
            }

            info.Clear();
            threads.Clear();

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

        public string HttpRequestsTest(string url)
        {

            var request = (HttpWebRequest)WebRequest.Create(url);
            request.UserAgent = "Mozilla/5.0 (iPhone; CPU iPhone OS 11_1_2 like Mac OS X) AppleWebKit/604.3.5 (KHTML, like Gecko) Version/11.0 Mobile/15B202 Safari/604.1";
            request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
            request.Headers.Add("Upgrade-Insecure-Requests", "1");
            request.Referer = "http://hdgo.cc/";
            //request.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip, deflate");
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

        public async Task<string> HttpRequestsGo(string url, int count = 0)
        {
            await Task.Delay(500);
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.UserAgent = "Mozilla/5.0 (iPhone; CPU iPhone OS 11_1_2 like Mac OS X) AppleWebKit/604.3.5 (KHTML, like Gecko) Version/11.0 Mobile/15B202 Safari/604.1";
            request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
            request.Referer = "http://to.8p69ao4bo6dex.ru/";
            //request.Headers.Add(HttpRequestHeader.Range, "bytes=0-1");
            //request.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip,deflate,sdch");
            request.Headers.Add(HttpRequestHeader.AcceptLanguage, "en-ca");
            try
            {
                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    using (var reader = new StreamReader(response.GetResponseStream()))
                    {
                        return response.ResponseUri.ToString();
                        //string result = reader.ReadToEnd();
                        //return result;
                    }
                }
            }
            catch (Exception)
            {
                if (count > 6)
                {
                    return url;
                }
                else
                {
                    return await HttpRequestsGo(url, count + 1);
                }
            }
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
