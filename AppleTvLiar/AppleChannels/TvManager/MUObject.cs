using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using AppleTvLiar.AppleChannels.HtmlManager;
using MikrainService;

namespace AppleTvLiar.AppleChannels.TvManager
{
    public class MUObject
    {
        public readonly Dictionary<string, Dictionary<string, string>> _channels = new Dictionary<string, Dictionary<string, string>>();
        private string MainUrl;

        public static MUObject CreateDictionary(string mainUrl)
        {
            var muObj = new MUObject { MainUrl = mainUrl };

            var text = muObj.HttpRequestsString(mainUrl);

            var lines = text.Split('\n');

            for (var i = 1; i < lines.Length; i += 3)
            {
                if (!string.IsNullOrEmpty(lines[i]))
                {
                    var name = lines[i].Split(',')[1].Replace("\r", "");
                    var cat = lines[i + 1];
                    var url = lines[i + 2];
                    var catReplaced = cat.Split(':')[1].Replace("\r", "");
                    var replaced = url.Replace("\r", "");

                    AddChannel(name, catReplaced, replaced, muObj);
                }
            }


            //for (var i = 1; i < lines.Length; i += 2)
            //{
            //    if (!string.IsNullOrEmpty(lines[i]))
            //    {
            //        var name = lines[i].Split(',')[1].Replace("\r", "");
            //        var cat = lines[i].Split(',')[0];
            //        var url = lines[i + 1];
            //        var catReplaced = cat.Split(':')[1].Replace("\r", "");
            //        var replaced = url.Replace("\r", "");

            //        if (cat.Contains("Для взрослых"))
            //        {
            //            continue;
            //        }

            //        AddChannel(name, catReplaced, replaced, muObj);
            //    }
            //}

            return muObj;
        }

        private static void AddChannel(string name, string cat, string url, MUObject muObj)
        {
            if (muObj._channels.ContainsKey(cat))
            {
                if (!muObj._channels[cat].ContainsKey(name))
                {
                    muObj._channels[cat].Add(name, url);
                }
                else
                {
                    muObj._channels[cat][name] = url;
                }
            }
            else
            {
                muObj._channels[cat] = new Dictionary<string, string> { { name, url } };
            }
        }

        public XDocument CreateXCategories()
        {
            var doc = new XmlDocument();
            doc.Load(Path.Combine(MikrainProgramm._xmlPath, @"Content\Season.xml"));
            var xDoc = doc.GetXDocument();
            var items = xDoc.Descendants(XName.Get("items")).First();
            var listWithPreview = xDoc.Descendants(XName.Get("listWithPreview")).First();
            int count = 0;

            var crossFadePreview = new XElement(XName.Get("paradePreview"));
            var preview = new XElement(XName.Get("preview"));

            foreach (var cat in _channels.Keys)
            {
                var oneLineMenuItem = new XElement(XName.Get("oneLineMenuItem"));
                //oneLineMenuItem.Add(new XAttribute(XName.Get("onPlay"),string.Format( "atv.loadURL('http://trailers.apple.com/ShowCatChannels?cat={0}&url={1}')", cat, Uri.EscapeDataString(MainUrl))));
                oneLineMenuItem.Add(new XAttribute(XName.Get("onSelect"), string.Format("atvutils.loadURL('" + string.Format("http://trailers.apple.com/ShowCatChannels?cat={0}&url={1}", HttpUtility.UrlEncode(cat), Uri.EscapeDataString(MainUrl)) + "');")));
                oneLineMenuItem.Add(new XAttribute(XName.Get("id"), "shelf_" + count));
                oneLineMenuItem.Add(new XAttribute(XName.Get("accessibilityLabel"), "shelf_" + count));
                var label = new XElement(XName.Get("label"));
              
                //var crossFadePreview = new XElement(XName.Get("crossFadePreview"));
              

                int imageCount = 0;

                foreach (var value in _channels[cat].Keys)
                {
                    var image = new XElement(XName.Get("image"));
                    image.Add(new XAttribute(XName.Get("src720"),  string.Format("http://trailers.apple.com/photo={0}", HttpUtility.UrlEncode(value))));
                    image.Add(new XAttribute(XName.Get("src1080"), string.Format("http://trailers.apple.com/photo={0}", HttpUtility.UrlEncode(value))));
                    crossFadePreview.Add(image);
                    imageCount++;
                    if (imageCount >2)break;
                }

                //var image = new XElement(XName.Get("image"));
                //image.Add(new XAttribute(XName.Get("src720"), "http://cs624417.vk.me/v624417582/d7b0/MxuqtGpcwuo.jpg"));
                //image.Add(new XAttribute(XName.Get("src1080"), "http://cs624417.vk.me/v624417582/d7b0/MxuqtGpcwuo.jpg"));


                label.Value = cat;
                //image.Value = Uri.EscapeDataString("http://cs624417.vk.me/v624417582/d7b0/MxuqtGpcwuo.jpg");
                //crossFadePreview.Add(image);
               
                oneLineMenuItem.Add(label);
                //oneLineMenuItem.Add(preview);
                items.Add(oneLineMenuItem);
                count++;
            }

            listWithPreview.AddFirst(preview);
            preview.Add(crossFadePreview);
           
            
            //< preview >
            //        < crossFadePreview >
            //          < image src720 = "http://sample-web-server/sample-xml/images/720p/tv1.png" src1080 = "http://sample-web-server/sample-xml/images/1080p/tv1.png" />

            //           </ crossFadePreview >

            //         </ preview >

            return xDoc;
        }

        private string HttpRequestsString(string url)
        {

            var request = (HttpWebRequest)WebRequest.Create(url);
            request.UserAgent =
                "Mozilla/5.0(iPad; U; CPU iPhone OS 3_2 like Mac OS X; en-us) AppleWebKit/531.21.10 (KHTML, like Gecko) Version/4.0.4 Mobile/7B314 Safari/531.21.10";
            //request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
            //request.Headers.Add(HttpRequestHeader.AcceptLanguage, "en-US,en;q=0.8,he;q=0.6,ru;q=0.4");
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
