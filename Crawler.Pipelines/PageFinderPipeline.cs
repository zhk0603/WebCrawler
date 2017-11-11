using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crawler.Pipelines
{
    public class PageFinderPipeline : CrawlerPipeline<PageFinderOptons>
    {
        protected PageFinderPipeline(PageFinderOptons options) : base(options)
        {
        }
    }

    public class PageFinderOptons : PipelineOptions
    {
    }
}
