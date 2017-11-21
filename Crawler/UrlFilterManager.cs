using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crawler
{
    public class UrlFilterManager
    {
        private static IUrlFilter _urlFilter;

        public static void SetUrlFilter(IUrlFilter urlFilter)
        {
            _urlFilter = urlFilter;
        }

        public static IUrlFilter Current => _urlFilter;
    }
}