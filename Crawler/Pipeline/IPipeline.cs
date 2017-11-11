using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace Crawler.Pipeline
{
    public interface IPipeline
    {
        void Initialize();
        bool IsComplete { get; set; }
        Task ExecuteAsync(PipelineContext context);
        IPipeline Next { get; set; }
    }
}
