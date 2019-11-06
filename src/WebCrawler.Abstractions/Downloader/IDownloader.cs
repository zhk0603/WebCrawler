using WebCrawler.Abstractions.Http;

namespace WebCrawler.Abstractions.Downloader
{
    public interface IDownloader
    {
        CrawlerResponse Download(CrawlerRequest request);
    }
}
