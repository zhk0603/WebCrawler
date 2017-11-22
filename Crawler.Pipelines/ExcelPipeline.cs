using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crawler.Pipelines
{
    public class ExcelPipeline : CrawlerPipeline<ExcelPipelineOptions>
    {
        protected ExcelPipeline(ExcelPipelineOptions options) : base(options)
        {
        }
    }

    public class ExcelPipelineOptions : PipelineOptions
    {

    }
}