namespace Crawler.Filter
{
    public interface IUrlFilter
    {
        void Add(string url);
        bool Contains(string url);
    }
}
