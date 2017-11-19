using Crawler.Downloader;
using Crawler.Schedulers;

namespace Crawler.Pipelines
{
    public class PipelineOptions
    {
        public PipelineOptions()
        {
            Sleep = 100;
            WaitForComplete = 5 * 60 * 1000; // 等待五分钟。
        }

        public string Name { get; set; }
        public IScheduler Scheduler { get; set; }
        public IDownloader Downloader { get; set; }
        public int Sleep { get; set; }
        /// <summary>
        ///     无法从调度器中获取资源时等待指定时间退出管道。（单位毫秒）
        /// </summary>
        public int WaitForComplete { get; set; }
    }
}