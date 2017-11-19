using System.Collections.Generic;

namespace Crawler.Pipelines
{
    public class PipelineContext
    {
        public PipelineContext()
        {
            PipelineData = new PipelineData();
        }

        public ICrawler Crawler { get; set; }
        public CrawlerConfiguration Configuration { get; set; }
        public Page Page { get; set; }
        public Site Site { get; set; }
        public PipelineData PipelineData { get; set; }
    }
}