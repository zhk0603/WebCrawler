using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Crawler.Pipeline;

namespace Crawler
{
    public class CrawlerConfiguration
    {
        public IEnumerable<Site> Sites { get; set; }
        public int ThreadNum { get; set; }
        public IPipeline Pipeline { get; set; }
    }
}
