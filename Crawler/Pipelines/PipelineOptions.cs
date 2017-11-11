using Crawler.Scheduler;

namespace Crawler.Pipelines
{
    public class PipelineOptions
    {
        public string Name { get; set; }
        public IScheduler Scheduler { get; set; }
        public ICrawler Crawler { get; set; }
    }
}