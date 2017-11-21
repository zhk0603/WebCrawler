using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Crawler.Pipelines;

namespace Crawler.Simple
{
    class Yezismile
    {
        public class YezismileUrlFinderPipeline : UrlFinderPipeline
        {
            public YezismileUrlFinderPipeline(UrlFinderOptons options) : base(options)
            {
            }

            protected override async Task<bool> ExecuteAsync(PipelineContext context)
            {
                if (context.Site != null) context.Site.ResultType = Downloader.ResultType.Byte;
                await base.ExecuteAsync(context);
                return context.Page != null && context.Page.HttpStatusCode == 200;
            }
        }
    }
}
