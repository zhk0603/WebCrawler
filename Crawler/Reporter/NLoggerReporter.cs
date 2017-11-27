using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crawler.Pipelines;
using Crawler.Schedulers;

namespace Crawler.Reporter
{
    public class NLoggerReporter : IReporter
    {
        private readonly List<IPipeline> _pipelines = new List<IPipeline>();
        private readonly List<Dictionary<string, IScheduler>> _schedulerDic;
        private readonly PipelineRunMode _runMode;

        public NLoggerReporter(PipelineRunMode runMode,
            IEnumerable<IPipeline> pipelines,
            List<Dictionary<string, IScheduler>> schedulerDic) : this(runMode, pipelines)
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

        public int ReportStatusInterval { get; set; } = 5000;

        public void ReportStatus()
        {
            ReportStatusCore();
        }

        protected virtual void ReportStatusCore()
        {
            var sb = new StringBuilder();
            var pipelineStatus =
                $"Pipeline Mode:{_runMode}, Pipelines:{_pipelines.Count}, Completed Pipeline:{_pipelines.Count(x => x.IsComplete)}";
            var maxLength = pipelineStatus.Length;
            sb.Append(pipelineStatus);
            foreach (var dic in _schedulerDic)
            {
                foreach (var pair in dic)
                {
                    sb.Append("\r\n");
                    var tmpStr = $"Scheduler:{pair.Key}, Count:{pair.Value.Count}, TotalCount:{pair.Value.TotalCount}";
                    sb.Append(tmpStr);
                    if (tmpStr.Length > maxLength)
                    {
                        maxLength = tmpStr.Length;
                    }
                }
            }
            sb.Insert(0, "*", maxLength);
            sb.Insert(maxLength, "\r\n");
            sb.Append("\r\n");
            sb.Append('*', maxLength);

            Logger.LoggerManager.GetLogger("Reporter").Info(sb.ToString());
        }
    }
}
