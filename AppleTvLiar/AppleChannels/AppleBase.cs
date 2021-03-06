﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using HtmlAgilityPack;
using MikrainService;
using Newtonsoft.Json.Linq;
using System.Web;

namespace AppleTvLiar.AppleChannels
{
    public class AppleBase
    {

        protected string userAgent = "Mozilla/5.0 (iPad; U; CPU OS 3_2 like Mac OS X; en-us) AppleWebKit/531.21.10 (KHTML, like Gecko) Version/4.0.4 Mobile/7B334b Safari/531.21.10";
        protected XmlDocument GetXml(string p)
        {
            var doc = new XmlDocument();
            doc.Load(Path.Combine(MikrainProgramm._xmlPath, p));
            return doc;
        }

        protected void KillAce()
        {
            ProccessManager.ProccessManager.KillAce();
        }

        protected XDocument Error(Exception exc)
        {
            string errorXML = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n<atv>\n<body>\n<dialog id=\"com.sample.error-dialog\">\n<title> " + "Call your dad to fix the error" + " </title>\n<description> " + exc.Message + "</description>\n</dialog>\n</body>\n</atv>\n";
            return XDocument.Parse(errorXML);
        }

        protected async Task<string> GetLink(HtmlDocument document)
        {
            var text = document.DocumentNode.InnerText;
            //var urlEnd = Regex.Match(text, "var window_surl = '(.*?)'").Groups[1].Value;
            //var urlEnd = Regex.Match(text, "var aaa65ebb48c7e879b184a4cae05fd95f = '(.*?)'").Groups[1].Value;

            //var urlEnd = "/sessions/new_session";
            //var lookIntoThis = Regex.Match(text, "session_url, (.*?(\n))+.*?");
            //var lookIntoThis = Regex.Match(text, "var post_method = {(.|\n)*}");
            //var eval = Regex.Match(text, "setRequestHeader\\|\\|(.*?)\\|beforeSend").Groups[1].Value;
            var eval = Regex.Match(text, "user_token: '(.*)'").Groups[1].Value;
            //var xmoonExp = Regex.Match(text, "X-MOON-EXPIRED', \"(.*)\"").Groups[1].Value;
            var xmoonExp = Regex.Match(document.DocumentNode.InnerHtml, "\"csrf-token\" content=\"(.*)\"").Groups[1].Value;
            var xmoonToken = Regex.Match(text, "X-MOON-TOKEN', \"(.*)\"").Groups[1].Value;
            var video_token = Regex.Match(text, "video_token: '(.*)'").Groups[1].Value;
            var access_key = Regex.Match(text, "mw_key: '(.*)'").Groups[1].Value;
            //var d_id = Regex.Match(lookIntoThis.Value, "mw_pid: (.*),").Groups[1].Value;
            var p_domain_id = Regex.Match(text, "domain_id: (.*),").Groups[1].Value;
            var uuid = Regex.Match(text, "uuid: '(.*)'").Groups[1].Value;
            //var p_domain_id = Regex.Match(lookIntoThis.Value, "p_domain_id: (.*),").Groups[1].Value;
            var mw_key = "1ffd4aa558cc51f5a9fc6888e7bc5cb4";// Regex.Match(text, "var mw_key = '(.*)'").Groups[1].Value;
            var mw_pid = Regex.Match(text, "partner_id: (.*),").Groups[1].Value;
            var runner_go = Regex.Match(text, "window.runner_go = '(.*)'").Groups[1].Value;
            var urlEnd = $"/manifests/video/{video_token}/all";
            ;
            if (string.IsNullOrEmpty(text)) return "";

            var result =
                await
                    SendMoonRequest(xmoonExp, eval, "http://moonwalk.cc" + urlEnd,
                        new Dictionary<string, string>()
                        {
                               {"video_token", video_token},
                                {"content_type", "movie"},
                                 {"mw_key","1ffd4aa558cc51f5a9fc6888e7bc5cb4"},
                            {"mw_pid", mw_pid},
                            {"p_domain_id",p_domain_id},
                            {"ad_attr", "0"},
                            {"debug","false"},
                            {"c90b4ca500a12b91e2b54b2d4a1e4fb7","cc5610c93fa23befc2d244a76500ee6c"},
                            //{"runner_go","b68d648926a%D0%B50762%D1%81fbd924%D0%B55%D1%81b79586"},
                           
                            //{ "uuid",uuid}
                        });

            var jO = JObject.Parse(result);
            var manifests = jO["mans"];
            var link = Uri.EscapeDataString(manifests["manifest_m3u8"].Value<string>());
            var manifest_mp4 = manifests["manifest_mp4"].Value<string>();


            var resultMoon = HttpRequestsString($"http://moonwalk.cc/video/html5?manifest_m3u8={link}&manifest_mp4=null&token={video_token}&pid=26");
            var hls = Regex.Match(resultMoon, "hls = '(.*)'.replace").Groups[1].Value;
            var resultHls = HttpRequestsString(HttpUtility.UrlDecode(hls));

            var links = resultHls.Split(Environment.NewLine.ToCharArray()).Where(obj => !string.IsNullOrEmpty(obj));

            return links.LastOrDefault();
        }

        protected XDocument ReadDoc(string cacheName)
        {
            if (!Directory.Exists(Path.Combine(MikrainProgramm._xmlPath, "AppleCache"))) Directory.CreateDirectory(Path.Combine(MikrainProgramm._xmlPath, "AppleCache"));

            cacheName = Path.Combine(MikrainProgramm._xmlPath, "AppleCache\\" + cacheName);
            try
            {
                if (File.Exists(cacheName))
                {
                    var info = new FileInfo(cacheName);
                    if (info.CreationTime < DateTime.Now.AddHours(-2))
                    {
                        File.Delete(cacheName);
                    }
                    else
                    {
                        return XDocument.Load(cacheName);
                    }
                }
            }
            catch (Exception exception)
            {
            }
            return null;
        }

        protected void SaveDoc(string cacheName, XDocument xDocument)
        {
            cacheName = Path.Combine(MikrainProgramm._xmlPath, "AppleCache\\" + cacheName);
            try
            {
                if (!File.Exists(cacheName))
                {
                    xDocument.Save(cacheName);
                }
            }
            catch (Exception)
            {
            }
        }

        protected Task<string> SendPostRequest(string url, Dictionary<string, string> parameters, string UserAgent = "")
        {
            var tcs = new TaskCompletionSource<string>();
            var request = (HttpWebRequest)WebRequest.Create(url);
            //xhr.setRequestHeader('X-MOON-EXPIRED', "1445476086");
            //xhr.setRequestHeader('X-MOON-TOKEN', "a33e0508a7a5e053d21fe15bc6d1576d");
            request.Headers.Add("Authorization", "Bearer false");
            request.Headers.Add("X-Requested-With", "XMLHttpRequest");
            request.UserAgent =
                           "Mozilla/5.0(iPad; U; CPU iPhone OS 3_2 like Mac OS X; en-us) AppleWebKit/531.21.10 (KHTML, like Gecko) Version/4.0.4 Mobile/7B314 Safari/531.21.10";
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            if (!string.IsNullOrEmpty(UserAgent))
            {
                request.UserAgent = UserAgent;
            }
            request.BeginGetRequestStream(rStream =>
            {
                try
                {
                    if (parameters != null)
                    {
                        using (var postStream = request.EndGetRequestStream(rStream))
                        {
                            using (var memStream = new MemoryStream())
                            {
                                var postData = parameters.Keys.Aggregate("", (current, key) => current + (key + "=" + parameters[key] + "&"));

                                var bytes = Encoding.UTF8.GetBytes(postData);

                                memStream.Write(bytes, 0, bytes.Length);

                                memStream.Position = 0;
                                var tempBuffer = new byte[memStream.Length];
                                memStream.Read(tempBuffer, 0, tempBuffer.Length);

                                postStream.Write(tempBuffer, 0, tempBuffer.Length);
                                memStream.Flush();
                            }
                        }
                    }

                    request.BeginGetResponse(r =>
                    {
                        try
                        {
                            var httpRequest = (HttpWebRequest)r.AsyncState;
                            var httpResponse = (HttpWebResponse)httpRequest.EndGetResponse(r);

                            using (var reader = new StreamReader(httpResponse.GetResponseStream()))
                            {
                                var result = reader.ReadToEnd();
                                tcs.SetResult(result);

                                Debug.WriteLine("Finish request " + request.RequestUri);
                            }
                        }
                        catch (WebException ex)
                        {
                            if (ex.Response is HttpWebResponse)
                            {
                                var raw = ex.Response as HttpWebResponse;
                                using (var reader = new StreamReader(raw.GetResponseStream()))
                                {
                                    var result = reader.ReadToEnd();
                                    tcs.TrySetException(new WebException(result));
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            tcs.TrySetException(ex);
                            Debug.WriteLine(ex);
                        }

                    }, request);

                }
                catch (WebException ex)
                {
                    if (ex.Response is HttpWebResponse)
                    {
                        var raw = ex.Response as HttpWebResponse;
                        using (var reader = new StreamReader(raw.GetResponseStream()))
                        {
                            var result = reader.ReadToEnd();
                            tcs.TrySetException(new WebException(result));
                        }
                    }
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }
            }, request);

            return tcs.Task;
        }

        protected void CreateActionButton(string link, XDocument xDocument, string name = "")
        {
            var actionButton = new XElement(XName.Get("actionButton"));
            actionButton.Add(new XAttribute(XName.Get("id"), "play" + name));
            actionButton.Add(new XAttribute(XName.Get("onSelect"), string.Format(
                                                      "atv.loadURL('http://trailers.apple.com/Playmovie?url={0}')",
                                                      Uri.EscapeDataString(link))));
            actionButton.Add(new XAttribute(XName.Get("onPlay"), string.Format(
                                                      "atv.loadURL('http://trailers.apple.com/Playmovie?url={0}')",
                                                      Uri.EscapeDataString(link))));
            actionButton.Add(new XElement(XName.Get("title"), "play " + name));
            actionButton.Add(new XElement(XName.Get("image"), "resource://Play.png"));
            actionButton.Add(new XElement(XName.Get("focusedImage"), "resource://PlayFocused.png"));
            xDocument.Descendants("items").First().Add(actionButton);
        }


        protected void CreateElementList(int count, string baseUrl, string title, string image, XElement itemsElement, bool ifNewLine = false)
        {
            var element = new XElement(XName.Get("moviePoster"));
            element.SetAttributeValue(XName.Get("id"), string.Format("shelf_item_{0}", count));

            element.SetAttributeValue(XName.Get("accessibilityLabel"), "");


            element.SetAttributeValue(XName.Get("alwaysShowTitles"), "true");

            element.SetAttributeValue(XName.Get("onSelect"), baseUrl);
            element.SetAttributeValue(XName.Get("onPlay"), baseUrl);
            if (title != null)
            {
                element.SetElementValue(XName.Get("title"), title);
            }
            element.SetElementValue(XName.Get("image"), image);
            element.SetElementValue(XName.Get("defaultImage"), "resource://Poster.png");
            itemsElement.Add(element);
        }

        protected string HttpRequestsString(string url)
        {

            var request = (HttpWebRequest)WebRequest.Create(url);
            request.UserAgent =
                "Mozilla/5.0(iPad; U; CPU iPhone OS 3_2 like Mac OS X; en-us) AppleWebKit/531.21.10 (KHTML, like Gecko) Version/4.0.4 Mobile/7B314 Safari/531.21.10";
            //request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
            //request.Headers.Add("Upgrade-Insecure-Requests","1");
            using (var response = (HttpWebResponse)request.GetResponse())
            {
                using (var reader = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding("windows-1251")))
                {
                    string result = reader.ReadToEnd();
                    return result;
                }
            }
        }

        private Task<string> SendMoonRequest(string exp, string TOKEN, string url, Dictionary<string, string> parameters, string UserAgent = "")
        {
            var tcs = new TaskCompletionSource<string>();
            var request = (HttpWebRequest)WebRequest.Create(url);
            //request.Headers.Add("X-CSRF-Token", exp);
            request.Headers.Add("X-Access-Level", TOKEN);

            //request.Headers.Add("Content-Data", Base64Encode(TOKEN));
            //request.Headers.Add("Encoding-Pool", Base64Encode(TOKEN));
            request.Headers.Add("X-Requested-With", "XMLHttpRequest");
            request.UserAgent = "Mozilla/5.0 (iPad; CPU OS 8_1_3 like Mac OS X) AppleWebKit/600.1.4 (KHTML, like Gecko) Version/8.0 Mobile/12B466 Safari/600.1.4";

            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
            if (!string.IsNullOrEmpty(UserAgent))
            {
                request.UserAgent = UserAgent;
            }

            request.BeginGetRequestStream(rStream =>
            {
                try
                {
                    if (parameters != null)
                    {
                        using (var postStream = request.EndGetRequestStream(rStream))
                        {
                            using (var memStream = new MemoryStream())
                            {
                                var postData = parameters.Keys.Aggregate("", (current, key) => current + (key + "=" + parameters[key] + "&"));
                                //var postData = "video_token=488331dcd4786b0f&content_type=serial&mw_key=1152%D1%81b1dd4c4d544&mw_pid=26&p_domain_id=2132&ad_attr=0&debug=false&version_control=271e1376a60c9063d9cb7486a56951ca";

                                var bytes = Encoding.UTF8.GetBytes(postData);

                                memStream.Write(bytes, 0, bytes.Length);

                                memStream.Position = 0;
                                var tempBuffer = new byte[memStream.Length];
                                memStream.Read(tempBuffer, 0, tempBuffer.Length);

                                postStream.Write(tempBuffer, 0, tempBuffer.Length);
                                memStream.Flush();
                            }
                        }
                    }

                    request.BeginGetResponse(r =>
                    {
                        try
                        {
                            var httpRequest = (HttpWebRequest)r.AsyncState;
                            var httpResponse = (HttpWebResponse)httpRequest.EndGetResponse(r);

                            using (var reader = new StreamReader(httpResponse.GetResponseStream()))
                            {
                                var result = reader.ReadToEnd();
                                tcs.SetResult(result);

                                Debug.WriteLine("Finish request " + request.RequestUri);
                            }
                        }
                        catch (WebException ex)
                        {
                            if (ex.Response is HttpWebResponse)
                            {
                                var raw = ex.Response as HttpWebResponse;
                                using (var reader = new StreamReader(raw.GetResponseStream()))
                                {
                                    var result = reader.ReadToEnd();
                                    tcs.TrySetException(new WebException(result));
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            tcs.TrySetException(ex);
                            Debug.WriteLine(ex);
                        }

                    }, request);

                }
                catch (WebException ex)
                {
                    if (ex.Response is HttpWebResponse)
                    {
                        var raw = ex.Response as HttpWebResponse;
                        using (var reader = new StreamReader(raw.GetResponseStream()))
                        {
                            var result = reader.ReadToEnd();
                            tcs.TrySetException(new WebException(result));
                        }
                    }
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }
            }, request);

            return tcs.Task;
        }

        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

    }
}
