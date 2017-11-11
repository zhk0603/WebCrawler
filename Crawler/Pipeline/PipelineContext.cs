using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crawler.Pipeline
{
    public class PipelineContext
    {
        public PipelineContext()
        {
            PipelineData = new Dictionary<string, object>();
        }

        public ICrawler Crawler { get; set; }
        public CrawlerConfiguration Configuration { get; set; }
        public Page Page { get; set; }
        public IDictionary<string, object> PipelineData { get; set; }
    }
}