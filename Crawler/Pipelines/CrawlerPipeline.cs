using System;
using System.Threading.Tasks;
using Crawler.Logger;
using Crawler.Scheduler;

namespace Crawler.Pipelines
{
    public class CrawlerPipeline : IPipeline
    {
        private readonly ILogger _logger;

        void IPipeline.Initialize()
        {
            BaseInitialize();
        }

        public virtual string Name { get; set; }
        public virtual bool IsComplete { get; set; }
        public virtual bool IsSkip { get; set; }
        public IPipeline Next { get; set; }

        async Task IPipeline.ExecuteAsync(PipelineContext context)
        {
            if (this.IsComplete || this.IsSkip)
            {
                Console.WriteLine($"管道：【{this.Name}】已完成，直接进入下一管道。");
                if (Next != null) await Next?.ExecuteAsync(context);
                return;
            }

            BaseInitialize();

            if (await ExecuteAsync(context))
                if (Next != null) await Next?.ExecuteAsync(context);

            await AfterExceute(context);
        }

        protected virtual void BaseInitialize()
        {
        }

        public virtual Task AfterExceute(PipelineContext context)
        {
            return Task.FromResult<object>(null);
        }

        protected virtual Task<bool> ExecuteAsync(PipelineContext context)
        {
            return Task.FromResult(true);
        }
    }

    public class CrawlerPipeline<TOptions> : CrawlerPipeline
        where TOptions : PipelineOptions
    {
        public CrawlerPipeline(TOptions options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            if (string.IsNullOrEmpty(options.Name))
                options.Name = GetType().Name;
            base.Name = options.Name;

            if (options.Scheduler == null)
                options.Scheduler = new SiteScheduler();

            Options = options;
        }

        public TOptions Options { get; set; }
    }
}