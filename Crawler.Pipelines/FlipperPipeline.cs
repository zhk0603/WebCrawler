using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crawler.Pipelines
{
    /// <summary>
    ///     翻页器
    /// </summary>
    public class FlipperPipeline : UrlFinderPipeline
    {
        public FlipperPipeline(FlipperPipelineOptions options) : base(options)
        {
        }
    }

    public class FlipperPipelineOptions : UrlFinderOptons
    {
    }
}
