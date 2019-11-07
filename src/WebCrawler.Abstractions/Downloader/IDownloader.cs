using WebCrawler.Abstractions.Http;

namespace WebCrawler.Downloader
{
    public interface IDownloader
    {
        CrawlerResponse Download(CrawlerRequest request);
    }
}
