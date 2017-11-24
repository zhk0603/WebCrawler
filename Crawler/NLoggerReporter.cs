using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Crawler.Pipelines;
using Crawler.Schedulers;

namespace Crawler
{
    public class NLoggerReporter : IReporter
    {
        private IEnumerable<IPipeline> _pipelines;
        private List<Dictionary<string, IScheduler>> _schedulerDic;
        public NLoggerReporter(IEnumerable<IPipeline> pipelines, List<Dictionary<string, IScheduler>> schedulerDic)
        {
            _pipelines = pipelines;
            _schedulerDic = schedulerDic;
        }

        public int ReportStatusInterval { get; set; } = 3000;

        public void ReportStatus()
        {
            ReportStatusCore();
            Thread.Sleep(ReportStatusInterval);
        }

        protected virtual void ReportStatusCore()
        {
            Console.WriteLine("ReportStatusCore");
        }
    }
}
