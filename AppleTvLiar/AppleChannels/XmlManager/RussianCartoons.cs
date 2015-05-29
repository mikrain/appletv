using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using MikrainService;

namespace AppleTvLiar.AppleChannels.XmlManager
{
    public class RussianCartoons
    {
        private static XDocument _films;

        public XDocument GetCatalogue()
        {
            bool isCached = false;
            if (_films != null)
            {
                var time = _films.Element(XName.Get("items")).Attribute(XName.Get("time")).Value;

                if (DateTime.Now > DateTime.Parse(time).AddDays(1))
                {
                    InitCache();
                    isCached = false;
                }
                else
                {
                    isCached = true;
                }

            }
            else
            {
                InitCache();
            }
            IEnumerable<XElement> catalogues;

            if (!isCached)
            {
                XDocument document = XDocument.Load("http://vitzu.com/youtube_videos/rus/cataloge.xml");
                catalogues = document.Descendants(XName.Get("cataloge"));
            }
            else
            {
                catalogues = _films.Descendants(XName.Get("info"));
            }

            XDocument xDocument = XDocument.Load(Path.Combine(MikrainProgramm._xmlPath, @"Content\categories.xml"));
            var items = xDocument.Descendants(XName.Get("items"));

            var shelfElement = new XElement(XName.Get("shelf"));
            shelfElement.SetAttributeValue(XName.Get("id"), string.Format("shelf_{0}", 1));
            shelfElement.SetAttributeValue(XName.Get("columnCount"), "5");


            var sectionsElement = new XElement(XName.Get("sections"));
            var shelfSectionElement = new XElement(XName.Get("shelfSection"));
           
            var itemsElement = new XElement(XName.Get("items"));

            shelfSectionElement.Add(itemsElement);
            sectionsElement.Add(shelfSectionElement);
            shelfElement.Add(sectionsElement);
            int count = 0;

            foreach (var catalogue in catalogues)
            {
                XElement info;
                if (!isCached)
                {
                    XDocument doc = XDocument.Load(string.Format("http://vitzu.com/youtube_videos/rus/xmls/{0}.xml",
                                                   catalogue.Attribute(XName.Get("name")).Value));
                    info = doc.Element(XName.Get("info"));
                }
                else
                {
                    info = catalogue;
                }

                var subCat = info.Element(XName.Get("cataloge"));
                var titleCat = subCat.Attribute(XName.Get("real_name")).Value;
                var href = subCat.Attribute(XName.Get("name")).Value;
                var imageCat = string.Format("http://vitzu.com/youtube_videos/rus/images/{0}@2x.png", subCat.Attribute(XName.Get("image")).Value);

                if (!isCached)
                {
                    _films.Element(XName.Get("items")).Add(info);
                }


                CreateElement(count++, string.Format("loadTrailerDetailPage('http://trailers.apple.com/russianCartoonMoview?russianCartoonMoview={0}')", Uri.EscapeDataString(href)),
                    titleCat, imageCat, itemsElement);
            }
            items.First().Add(shelfElement);
            return xDocument;
        }

        private static void InitCache()
        {
            _films = new XDocument();
            var items = new XElement(XName.Get("items"));
            items.Add(new XAttribute(XName.Get("time"), DateTime.Now));
            _films.Add(items);

        }

        private void CreateElement(int count, string baseUrl, string title, string image, XElement itemsElement)
        {
            var element = new XElement(XName.Get("moviePoster"));
            element.SetAttributeValue(XName.Get("id"), string.Format("shelf_item_{0}", count));

            element.SetAttributeValue(XName.Get("accessibilityLabel"), "");


            element.SetAttributeValue(XName.Get("alwaysShowTitles"), "true");

            element.SetAttributeValue(XName.Get("onSelect"), baseUrl);
            element.SetAttributeValue(XName.Get("onPlay"), baseUrl);
            if (title != null)
            {
                element.SetElementValue(XName.Get("title"), string.Format(title));
            }
            element.SetElementValue(XName.Get("image"), image);
            element.SetElementValue(XName.Get("defaultImage"), "resource://Poster.png");
            itemsElement.Add(element);
        }


        internal XDocument GetCartoonSeries(string russianCartoonMoview)
        {
            var films = _films.Descendants("info");

            var item = films.FirstOrDefault(element => element.Element(XName.Get("cataloge")).Attribute(XName.Get("name")).Value == russianCartoonMoview);
            var cataloge = item.Element(XName.Get("cataloge"));
            var titleCat = cataloge.Attribute(XName.Get("real_name")).Value;
            var imageCat = string.Format("http://vitzu.com/youtube_videos/rus/images/{0}@2x.png", cataloge.Attribute(XName.Get("image")).Value);

            XDocument xDocument = XDocument.Load(Path.Combine(MikrainProgramm._xmlPath, @"Content\showList.xml"));

            var imageElement = xDocument.Descendants(XName.Get("image"));
            var titleElement = xDocument.Descendants(XName.Get("title"));
            var items = xDocument.Descendants(XName.Get("items"));
            imageElement.First().SetValue(imageCat);
            titleElement.First().SetValue(titleCat);

            int count = 0;
            foreach (var film in item.Descendants(XName.Get("film")))
            {
                var href = film.Attribute(XName.Get("url")).Value;
                var title = film.Attribute(XName.Get("name")).Value;
                var id = film.Attribute(XName.Get("id")).Value;
                var image = string.Format("http://vitzu.com/youtube_videos/rus/images/{0}@2x.png", film.Attribute(XName.Get("image")).Value);
                CreateElement(count++, string.Format("loadTrailerDetailPage('http://trailers.apple.com/GetRCartoonShow?russianCartoonMoview={0}&filmId={1}')", Uri.EscapeDataString(russianCartoonMoview), Uri.EscapeDataString(id)), title, image, items.First());
            }

            return xDocument;
        }

        internal XDocument GetRCartoonShow(string russianCartoonMoview, string filmId)
        {

            var films = _films.Descendants("info");

            var item = films.FirstOrDefault(Filmelement => Filmelement.Element(XName.Get("cataloge")).Attribute(XName.Get("name")).Value == russianCartoonMoview);
            var film = item.Descendants("film").FirstOrDefault(xElement => xElement.Attribute(XName.Get("id")).Value == filmId);
            var name = film.Attribute(XName.Get("name"));
            var image = film.Attribute(XName.Get("image"));
            var description = film.Attribute(XName.Get("description"));
            var url = film.Attribute(XName.Get("url"));

            XDocument xDocument =
          XDocument.Load(Path.Combine(MikrainProgramm._xmlPath, @"Content\movie.xml"));
            var element = xDocument.Descendants(XName.Get("image"));
            var elementDesc = xDocument.Descendants(XName.Get("summary"));
            var elementName = xDocument.Descendants(XName.Get("title"));
            element.First().SetValue(string.Format("http://vitzu.com/youtube_videos/rus/images/{0}@2x.png", image.Value));
            elementDesc.First().SetValue(description.Value);
            elementName.First().SetValue(name.Value);
            var actionButtonElement = xDocument.Descendants("actionButton");
            actionButtonElement.First().SetAttributeValue("onSelect", string.Format("atv.loadURL('http://trailers.apple.com/Playmovie?url={0}')", Uri.EscapeDataString("http://www.youtube.com/embed/Xxf7W7R-NPs?v=Xxf7W7R-NPs&showinfo=0")));
            actionButtonElement.First().SetAttributeValue("onPlay", string.Format("atv.loadURL('http://trailers.apple.com/Playmovie?url={0}')", Uri.EscapeDataString("http://www.youtube.com/embed/Xxf7W7R-NPs?v=Xxf7W7R-NPs&showinfo=0")));




            return xDocument;
        }
    }
}
