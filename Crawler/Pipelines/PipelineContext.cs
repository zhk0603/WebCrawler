using System;
using System.Collections.Generic;

namespace Crawler.Pipelines
{
    public class PipelineContext : ICloneable
    {
        public PipelineContext()
        {
            PipelineData = new PipelineData();
        }

        public PipelineContext(PipelineData pipelineData)
        {
            this.PipelineData = pipelineData;
        }

        public ICrawler Crawler { get; set; }
        public CrawlerConfiguration Configuration { get; set; }
        public Page Page { get; set; }
        public Site Site { get; set; }
        public PipelineData PipelineData { get; }

        public object Clone()
        {
            return new PipelineContext(this.PipelineData)
            {
                Crawler = this.Crawler,
                Configuration = this.Configuration,
                Page = this.Page,
                Site = this.Site
            };
        }
    }
}