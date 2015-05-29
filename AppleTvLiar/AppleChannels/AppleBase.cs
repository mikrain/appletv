using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using MikrainService;

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
