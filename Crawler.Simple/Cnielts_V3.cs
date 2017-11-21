using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Crawler.Pipelines;

namespace Crawler.Simple
{
    class Cnielts_V3
    {
        public class CnieltsPipeline1 : Cnielts_V2.CnieltsPipeline1
        {
            public CnieltsPipeline1(PipelineOptions options) : base(options)
            {
            }

            protected override Task AfterExceute(PipelineContext context)
            {


                return base.AfterExceute(context);
            }
        }
    }
}
