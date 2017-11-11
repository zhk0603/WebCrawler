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

        public virtual bool IsComplete { get; set; }

        async Task IPipeline.ExecuteAsync(PipelineContext context)
        {
            BaseInitialize();

            if (await ExecuteAsync(context))
                if (Next != null) await Next?.ExecuteAsync(context);

            await AfterExceute(context);
        }

        public IPipeline Next { get; set; }

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
        protected CrawlerPipeline(TOptions options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            if (string.IsNullOrEmpty(options.Name))
                options.Name = GetType().Name;
            if (options.Scheduler == null)
                options.Scheduler = new SiteScheduler();

            Options = options;
        }

        public TOptions Options { get; set; }
    }
}