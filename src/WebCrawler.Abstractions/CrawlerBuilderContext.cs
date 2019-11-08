using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace WebCrawler
{
    public class CrawlerBuilderContext
    {
        public IHostEnvironment HostingEnvironment { get; set; }

        public IConfiguration Configuration { get; set; }
    }
}