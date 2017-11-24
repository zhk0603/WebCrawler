using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Crawler.Pipelines;
using Crawler.Schedulers;
using NLog;

namespace Crawler
{
    public class NLoggerReporter : IReporter
    {
        private readonly List<IPipeline> _pipelines = new List<IPipeline>();
        private readonly List<Dictionary<string, IScheduler>> _schedulerDic;
        private readonly PipelineRunMode _runMode;

        public NLoggerReporter(PipelineRunMode runMode,
            IEnumerable<IPipeline> pipelines,
            List<Dictionary<string, IScheduler>> schedulerDic,
            ICrawler crawler) : this(runMode, pipelines)
        {
            _runMode = runMode;
            _schedulerDic = schedulerDic;
        }

        private NLoggerReporter(PipelineRunMode runMode, IEnumerable<IPipeline> pipelines)
        {
            if (runMode == PipelineRunMode.Chain)
            {
                AppendPipelines(pipelines.FirstOrDefault());
            }
            else
            {
                _pipelines = pipelines.ToList();
            }
        }

        void AppendPipelines(IPipeline nextPipeline)
        {
            if (nextPipeline != null)
            {
                _pipelines.Add(nextPipeline);
                AppendPipelines(nextPipeline.Next);
            }
        }

        public int ReportStatusInterval { get; set; } = 3000;

        public void ReportStatus()
        {
            ReportStatusCore();
        }

        protected virtual void ReportStatusCore()
        {
            var sb = new StringBuilder();
            sb.Append(
                $"Pipeline Mode:{_runMode}, Pipelines:{_pipelines.Count}, Completed Pipeline:{_pipelines.Count(x => x.IsComplete)}");

            foreach (var dic in _schedulerDic)
            {
                foreach (var pair in dic)
                {
                    sb.Append("\r\n");
                    sb.Append($"Scheduler:{pair.Key}, Count:{pair.Value.Count}, TotalCount:{pair.Value.TotalCount}");
                }
            }
            Logger.LoggerManager.GetLogger("Reporter").Info(sb.ToString());
        }
    }
}
