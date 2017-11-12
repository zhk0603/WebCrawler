using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crawler.Downloader
{
    public interface IDownloader
    {
        Page GetPage(Site requestSite);
        Page GetPage(string requestUrl);
    }
}
