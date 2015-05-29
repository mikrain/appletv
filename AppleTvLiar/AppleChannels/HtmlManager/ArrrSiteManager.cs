using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace AppleTvLiar.AppleChannels.HtmlManager
{
    public class ArrrSiteManager
    {
        private HttpClient httpClient;
        private string baseUrl = "http://arrr.tv";

        private async Task LoginArrr()
        {
            var request = (HttpWebRequest)WebRequest.Create("http://arrr.tv/auth/login");
            request.Headers["email"] = "mikrain@gmail.com";
            request.Headers["password"] = "teipaeyi";
            request.Method = "POST";

            var parameters = new Dictionary<string, string>() { { "email", "mikrain@gmail.com" }, { "password", "teipaeyi" } };

            httpClient = new HttpClient();
            using (var form = new MultipartFormDataContent())
            {
                foreach (var keyValuePair in parameters)
                {
                    if (keyValuePair.Value != null)
                    {
                        form.Add(new StringContent(keyValuePair.Value), keyValuePair.Key);
                    }
                }

                httpClient.DefaultRequestHeaders.Add("email", "mikrain@gmail.com");
                httpClient.DefaultRequestHeaders.Add("password", "teipaeyi");

                using (var response = await httpClient.PostAsync("http://arrr.tv/auth/login", form))
                {
                    string result = await response.Content.ReadAsStringAsync();
                }
            }
        }

        public async Task GetConent()
        {
            await LoginArrr();
            var doc = new HtmlDocument();
            using (var response = await httpClient.GetAsync("http://arrr.tv/shows/"))
            {
                string result = await response.Content.ReadAsStringAsync();

                doc.LoadHtml(result);
            }

            var div = doc.DocumentNode.Descendants("div");
            foreach (var childNode in div)
            {
                if (childNode.Name == "div" && childNode.GetAttributeValue("class", "") == "cover centered-text notsubscribed ")
                {
                    foreach (var node in childNode.ChildNodes)
                    {
                        if (node.Name == "a")
                        {
                            var href = baseUrl+node.GetAttributeValue("href", "");
                            var image = baseUrl+node.ChildNodes.First(htmlNode => htmlNode.Name == "img").GetAttributeValue("src", "");
                        }

                        if (node.Name == "div")
                        {
                            var name = node.FirstChild.InnerText;
                        }
                    }
                }
            }

        }
    }
}
