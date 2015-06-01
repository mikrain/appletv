using AppleTvLiar.ClientManager;
using AppleTvLiar.ContentManager;
using AppleTvLiar.MediaManager;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using System.Xml.Linq;

namespace AppleTvLiar.AppleChannels.XmlManager
{
    public class BestRussian : AppleBase
    {

        public XmlDocument GetChannelsList()
        {



            ClientServiceClient client = new ClientServiceClient();
            ClientAppSettings settings = new ClientAppSettings();
            settings.appSettings = new AppSettings() { appName = "IPHONE" };
            var clientCredentials = new AccessCredentials() { UserLogin = "320746", UserPassword = "123456" };
            settings.clientCredentials = clientCredentials;
            var trueSettings = client.Login(settings);

            ContentServiceClient service = new ContentServiceClient();
            var channels = service.GetClientChannels(trueSettings.clientCredentials.sessionID, new ContentRequest() { type = ContentManager.ContentType.LiveTV, paging = new ItemPaging() { itemsOnPage = 10 } });

            XElement root = CreateRoot();
            CreateHead(root);
            XElement items = CreateBody(root);


            for (int i = 1; i < channels.paging.totalPages; i++)
            {
                channels = service.GetClientChannels(trueSettings.clientCredentials.sessionID, new ContentRequest() { type = ContentManager.ContentType.LiveTV, paging = new ItemPaging() { itemsOnPage = 10, pageNumber = i } });

                CreateChannel(items, i, channels.items, trueSettings);


            }



            var doc = GetDocument(root);

            return doc;
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

        private void CreateChannel(XElement items, int page, AppleTvLiar.ContentManager.Channel[] channels, ClientAppSettings settings, bool addDiv = true)
        {
            if (addDiv)
            {
                var collectionDivider = new XElement(XName.Get("collectionDivider"));
                collectionDivider.Add(new XAttribute(XName.Get("alignment"), "left"));
                collectionDivider.Add(new XAttribute(XName.Get("accessibilityLabel"), "page " + page));
                var title = new XElement(XName.Get("title"));
                title.Value = "page " + page;
                collectionDivider.Add(title);
                items.Add(collectionDivider);
            }


            CreateShelf(items, channels, settings, !addDiv);
        }

        private void CreateShelf(XElement itemsRoot, AppleTvLiar.ContentManager.Channel[] channels, ClientAppSettings settings, bool header = false)
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

            MediaServiceClient mediaService = new MediaServiceClient();

             //var template = mediaService.MediaImageUrlTemplate(settings.appSettings.siteID);

            for (int i = 0; i < channels.Count(); i++)
            {
                XElement itemsTmp;
                string imageTmp = "";
                XElement moviePoster = null;

                if (header)
                {
                    itemsTmp = itemsRoot;
                    imageTmp = "resource://16x9-default.png";
                    moviePoster = new XElement(XName.Get("showcasePoster"));
                    moviePoster.Add(new XAttribute(XName.Get("id"), channels[i].id));
                }
                else
                {

                    imageTmp = "resource://Poster.png";
                    moviePoster = new XElement(XName.Get("moviePoster"));
                    itemsTmp = items;
                    moviePoster.Add(new XAttribute(XName.Get("id"), "shelf_item_" + channels[i].id));
                    moviePoster.Add(new XAttribute(XName.Get("alwaysShowTitles"), "true"));
                    var title = new XElement(XName.Get("title"));
                    title.SetValue(channels[i].name);
                    moviePoster.Add(title);
                }


                var stream = mediaService.GetClientStreamUri(settings.clientCredentials.sessionID, new MediaRequest() { item = new MediaManager.ProtoItem() { id = channels[i].id, contentType = MediaManager.ContentType.LiveTV }, streamSettings = new MediaManager.StreamSettings() { balancingArea = new MediaManager.BalancingArea() { id = channels[i].id } } });


                moviePoster.Add(new XAttribute(XName.Get("accessibilityLabel"), channels[i].name));
                moviePoster.SetAttributeValue("onSelect", string.Format("atv.loadURL('http://trailers.apple.com/playChannel?href={0}')", Uri.EscapeDataString(stream.URL)));
                moviePoster.SetAttributeValue("onPlay", string.Format("atv.loadURL('http://trailers.apple.com/playChannel?href={0}')", Uri.EscapeDataString(stream.URL)));



                var image = new XElement(XName.Get("image"));
                image.SetValue(imageTmp);// HttpUtility.HtmlEncode( string.Format("http://images.bestrussiantv.com/ui/ImageHandler.ashx?t=10&e={0}", channels[i].id)));
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

    }
}
