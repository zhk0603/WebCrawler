using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Crawler.Scheduler;

namespace Crawler.Pipeline
{
    public class PipelineOptions
    {
        public string Name { get; set; }
        public IScheduler Scheduler { get; set; }
        public ICrawler Crawler { get; set; }
    }
}
