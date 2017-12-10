using System;

namespace Crawler.Filter
{
    public class UrlFilterManager
    {
        private static IUrlFilter _urlFilter;

        public static void SetUrlFilter(IUrlFilter urlFilter)
        {
            _urlFilter = urlFilter;
        }

        public static void SetUrlFilter(Func<IUrlFilter> func)
        {
            _urlFilter = func();
        }

        public static IUrlFilter Current => _urlFilter ?? (_urlFilter = new BloomFilter(int.MaxValue / 21, 0.001F));
    }
}