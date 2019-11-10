using WebCrawler.Abstractions.Http;

namespace WebCrawler.Downloader
{
    public interface IDownloader
    {
        Response Download(Request request);
    }
}
