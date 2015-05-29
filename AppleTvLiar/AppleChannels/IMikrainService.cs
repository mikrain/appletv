using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Net;
using System.Xml;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MikrainService
{
    [ServiceContract]
    public interface IMikrainService
    {
        [XmlSerializerFormat]
        [OperationContract]
        [WebGet(UriTemplate = "/appletv?channel={channel}", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Xml)]
        XmlDocument appleChannels(string channel);

        [XmlSerializerFormat]
        [OperationContract]
        [WebGet(UriTemplate = "/appletv/index.xml", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Xml)]
        XmlDocument appletv();

        [XmlSerializerFormat]
        [OperationContract]
        [WebGet(UriTemplate = "/playChannel?href={href}", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Xml)]
        XmlDocument playChannel(string href);

        [XmlSerializerFormat]
        [OperationContract]
        [WebGet(UriTemplate = "/appletv/us/index.xml", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Xml)]
        XmlDocument appleusTv();

        [XmlSerializerFormat]
        [OperationContract]
        [WebGet(UriTemplate = "/appletv/us/nav.xml", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Xml)]
        XmlDocument appletvnav();


        [XmlSerializerFormat]
        [OperationContract]
        [WebGet(UriTemplate = "/m4kinoNew", BodyStyle = WebMessageBodyStyle.Bare)]
        XmlDocument lovekinozalNew();

        [XmlSerializerFormat]
        [OperationContract]
        [WebGet(UriTemplate = "/lovekinozalMovie?movie={movie}", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Xml)]
        XmlDocument lovekinozalMovie(string movie);

        [XmlSerializerFormat]
        [OperationContractAttribute(AsyncPattern = true)]
        [WebGet(UriTemplate = "/SearchBaskino?query={query}", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Xml)]
        Task<XmlDocument> SearchBaskino(string query);

        [XmlSerializerFormat]
        [OperationContract]
        [WebGet(UriTemplate = "/movieBoxMovieShow", BodyStyle = WebMessageBodyStyle.Bare)]
        XmlDocument movieBoxMovieShow();

        [XmlSerializerFormat]
        [OperationContractAttribute(AsyncPattern = true)]
        [WebGet(UriTemplate = "/GetUcoz", BodyStyle = WebMessageBodyStyle.Bare)]
        Task<XmlDocument> GetUcoz(); 
        
        [XmlSerializerFormat]
        [OperationContract]
        [WebGet(UriTemplate = "/GetYourCinema", BodyStyle = WebMessageBodyStyle.Bare)]
        XmlDocument GetYourCinema();

        [XmlSerializerFormat]
        [OperationContract]
        [WebGet(UriTemplate = "/getYourCinemaMovie?movie={movie}&imageUrl={imageUrl}&movieTitle={movieTitle}", BodyStyle = WebMessageBodyStyle.Bare)]
        XmlDocument GetYourCinemaMove(string movie, string imageUrl, string movieTitle);

        [XmlSerializerFormat]
        [OperationContractAttribute(AsyncPattern = true)]
        [WebGet(UriTemplate = "/UcozSearch?query={query}", BodyStyle = WebMessageBodyStyle.Bare)]
        Task<XmlDocument> UcozSearch(string query);

        [XmlSerializerFormat]
        [OperationContractAttribute(AsyncPattern = true)]
        [WebGet(UriTemplate = "/GetUcozMultSerial", BodyStyle = WebMessageBodyStyle.Bare)]
        Task<XmlDocument> GetUcozMultSerial();

        [XmlSerializerFormat]
        [OperationContractAttribute(AsyncPattern = true)]
        [WebGet(UriTemplate = "/GetUcozMult", BodyStyle = WebMessageBodyStyle.Bare)]
        Task<XmlDocument> GetUcozMult();

        [XmlSerializerFormat]
        [OperationContract]
        [WebGet(UriTemplate = "/GetKino1080", BodyStyle = WebMessageBodyStyle.Bare)]
        XmlDocument GetKino1080();
        
        [XmlSerializerFormat]
        [OperationContract]
        [WebGet(UriTemplate = "/GetKino1080Mult", BodyStyle = WebMessageBodyStyle.Bare)]
        XmlDocument GetKino1080Mult();

        [XmlSerializerFormat]
        [OperationContractAttribute(AsyncPattern = true)]
        [WebGet(UriTemplate = "/GetUcozSerial", BodyStyle = WebMessageBodyStyle.Bare)]
        Task<XmlDocument> GetUcozSerial();

        [XmlSerializerFormat]
        [OperationContractAttribute(AsyncPattern = true)]
        [WebGet(UriTemplate = "/ShowUcozSer?ser={ser}&title={title}&image={image}", BodyStyle = WebMessageBodyStyle.Bare)]
        Task<XmlDocument> ShowUcozSer(string ser, string title, string image);

        [XmlSerializerFormat]
        [WebGet(UriTemplate = "/ShowUcozEpisodes?href={href}&imageHref={imageHref}&title={title}&season={season}", BodyStyle = WebMessageBodyStyle.Bare)]
        XmlDocument ShowUcozEpisodes(string href, string imageHref, string title, string season);

        [XmlSerializerFormat]
        [OperationContract]
        [WebGet(UriTemplate = "/getUcozMovie?movie={movie}&imageUrl={imageUrl}&movieTitle={movieTitle}", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Xml)]
        XmlDocument GetUcozMovie(string movie, string imageUrl, string movieTitle); 
        
        [XmlSerializerFormat]
        [OperationContract]
        [WebGet(UriTemplate = "/getKino1080Movie?movie={movie}&imageUrl={imageUrl}&movieTitle={movieTitle}", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Xml)]
        XmlDocument GetKino1080Movie(string movie, string imageUrl, string movieTitle);

        [XmlSerializerFormat]
        [OperationContract]
        [WebGet(UriTemplate = "/movieBoxShowDetails?movieId={movieId}&image={image}&title={title}&seasons={seasons}", BodyStyle = WebMessageBodyStyle.Bare)]
        XmlDocument movieBoxShowDetails(string movieId, string image, string title, string seasons);

        [XmlSerializerFormat]
        [OperationContractAttribute(AsyncPattern = true)]
        [WebGet(UriTemplate = "/GetMoviewShowSeries?href={href}&imageHref={imageHref}&title={title}&season={season}", BodyStyle = WebMessageBodyStyle.Bare)]
        Task<XmlDocument> GetMoviewShowSeries(string href, string imageHref, string title, string season);

        [XmlSerializerFormat]
        [OperationContract]
        [WebGet(UriTemplate = "/GetMoviewShowEpisode?hrefEn={hrefEn}&hrefRu={hrefRu}&imageHref={imageHref}&title={title}", BodyStyle = WebMessageBodyStyle.Bare)]
        XmlDocument GetMoviewShowEpisode(string hrefEn, string hrefRu, string imageHref, string title);

        [XmlSerializerFormat]
        [OperationContract]
        [WebGet(UriTemplate = "/searchMovieBox?q={q}", BodyStyle = WebMessageBodyStyle.Bare)]
        XmlDocument searchMovieBox(string q);

        [XmlSerializerFormat]
        [OperationContract]
        [WebGet(UriTemplate = "/movieBoxMovie", BodyStyle = WebMessageBodyStyle.Bare)]
        XmlDocument movieBoxMovie();

        [XmlSerializerFormat]
        [OperationContract]
        [WebGet(UriTemplate = "/movieBoxMovieDetails?movieId={movieId}&image={image}&title={title}&count={count}", BodyStyle = WebMessageBodyStyle.Bare)]
        XmlDocument movieBoxMovieDetails(string movieId, string image, string title, string count);

        [XmlSerializerFormat]
        [OperationContract]
        [WebGet(UriTemplate = "/findMovie?movie={movie}", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Xml)]
        XmlDocument findMovie(string movie);

        [XmlSerializerFormat]
        [OperationContract]
        [WebGet(UriTemplate = "/appleMoviesOnline?page={page}", BodyStyle = WebMessageBodyStyle.Bare)]
        XmlDocument appleMoviesOnline(string page = "0");

        [XmlSerializerFormat]
        [OperationContract]
        [WebGet(UriTemplate = "/baskinoMutliOnline?page={page}", BodyStyle = WebMessageBodyStyle.Bare)]
        XmlDocument BaskinoMutliOnline(string page = "0");


        [XmlSerializerFormat]
        [OperationContract]
        [WebGet(UriTemplate = "/Playmovie?url={url}", ResponseFormat = WebMessageFormat.Xml)]
        XmlDocument Playmovie(string url);


        [XmlSerializerFormat]
        [OperationContract]
        [WebGet(UriTemplate = "/Arrr", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Xml)]
        XmlDocument Arrr();

        [XmlSerializerFormat]
        [OperationContract]
        [WebGet(UriTemplate = "/FriendsSeason", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Xml)]
        XmlDocument FriendsSeason();

        [XmlSerializerFormat]
        [OperationContract]
        [WebGet(UriTemplate = "/FriendsShow?friendsSeason={friendsSeason}&imgUrl={imgUrl}", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Xml)]
        XmlDocument FriendsShow(string friendsSeason, string imgUrl);

        [XmlSerializerFormat]
        [OperationContract]
        [WebGet(UriTemplate = "/FriendsPlay?friendsPlayHref={friendsPlayHref}&imageHref={imageHref}", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Xml)]
        XmlDocument FriendsPlay(string friendsPlayHref, string imageHref);

        [XmlSerializerFormat]
        [OperationContract]
        [WebGet(UriTemplate = "/MultPoisk?ext={ext}", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Xml)]
        XmlDocument MultPoisk(string ext);

        [XmlSerializerFormat]
        [OperationContract]
        [WebGet(UriTemplate = "/MultPoiskSerial", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Xml)]
        XmlDocument MultPoiskSerial();

        [XmlSerializerFormat]
        [OperationContract]
        [WebGet(UriTemplate = "/MultPoiskNew", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Xml)]
        XmlDocument MultPoiskNew();

        [XmlSerializerFormat]
        [OperationContract]
        [WebGet(UriTemplate = "/MultPoiskList", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Xml)]
        XmlDocument MultPoiskList();

        [XmlSerializerFormat]
        [OperationContract]
        [WebGet(UriTemplate = "/multiDetails?movie={movie}&imgSource={imgSource}", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Xml)]
        XmlDocument multiDetails(string movie, string imgSource);

        [XmlSerializerFormat]
        [OperationContract]
        [WebGet(UriTemplate = "/multiSingle?multiPlayHref={multiPlayHref}&imgSource={imgSource}", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Xml)]
        XmlDocument multiSingle(string multiPlayHref, string imgSource);

        [XmlSerializerFormat]
        [OperationContract]
        [WebGet(UriTemplate = "/RussianCartoon", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Xml)]
        XmlDocument RussianCartoon();

        [XmlSerializerFormat]
        [OperationContract]
        [WebGet(UriTemplate = "/russianCartoonMoview?russianCartoonMoview={russianCartoonMoview}", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Xml)]
        XmlDocument RussianCartoonMoview(string russianCartoonMoview);

        [XmlSerializerFormat]
        [OperationContract]
        [WebGet(UriTemplate = "/GetRCartoonShow?russianCartoonMoview={russianCartoonMoview}&filmId={filmId}", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Xml)]
        XmlDocument GetRCartoonShow(string russianCartoonMoview, string filmId);


        [XmlSerializerFormat]
        [OperationContract]
        [WebGet(UriTemplate = "/getShows", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Xml)]
        XmlDocument GetShows();

        [XmlSerializerFormat]
        [OperationContract]
        [WebGet(UriTemplate = "/Show?show={show}", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Xml)]
        XmlDocument Shows(string show);

        [XmlSerializerFormat]
        [OperationContract]
        [WebGet(UriTemplate = "/ShowSeries?showSeries={showSeries}&name={name}", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Xml)]
        XmlDocument ShowSeries(string showSeries, string name);

        [XmlSerializerFormat]
        [OperationContract]
        [WebGet(UriTemplate = "/ShowSer?ser={ser}&title={title}&image={image}", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Xml)]
        XmlDocument ShowSer(string ser, string title, string image);


        [OperationContract]
        [WebGet(UriTemplate = "/appletv/us/js/application.js", BodyStyle = WebMessageBodyStyle.Bare)]
        Stream appletvjs();

        [OperationContract]
        [WebGet(UriTemplate = "/appletv/us/js/Trailers.Showtimes.js", BodyStyle = WebMessageBodyStyle.Bare)]
        Stream appletvShow();

        [OperationContract]
        [WebGet(UriTemplate = "/appletv/us/js/Trailers.js", BodyStyle = WebMessageBodyStyle.Bare)]
        Stream appletvTrailers();

        [OperationContract]
        [WebGet(UriTemplate = "/appletv/us/js/main.js", BodyStyle = WebMessageBodyStyle.Bare)]
        Stream appletvMain();

        [OperationContract]
        [WebGet(UriTemplate = "/appletv/us/js/showtimesconfig.js", BodyStyle = WebMessageBodyStyle.Bare)]
        Stream appletvConfig();

        [OperationContract]
        [WebGet(UriTemplate = "/appletv/us/js/Trailers.Log.js", BodyStyle = WebMessageBodyStyle.Bare)]
        Stream appletvLog();

        [OperationContract]
        [WebGet(UriTemplate = "/getstream", BodyStyle = WebMessageBodyStyle.Bare)]
        Stream GetStream();

        [OperationContract]
        [WebGet(UriTemplate = "/cert", BodyStyle = WebMessageBodyStyle.Bare)]
        Stream Cert();  
        
        
        [OperationContract]
        [WebGet(UriTemplate = "/getstreamts/*", BodyStyle = WebMessageBodyStyle.Bare)]
        Stream GetStreamTs();


        [OperationContract]
        [WebGet(UriTemplate = "/abc", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
        string GetAllRequests();


    }
}
