using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Crawler.Scheduler;

namespace Crawler.Pipeline
{
    public class CrawlerPipeline : IPipeline
    {
        void IPipeline.Initialize()
        {
            BaseInitialize();
        }

        protected virtual void BaseInitialize()
        {
        }

        public virtual bool IsComplete { get; set; }

        async Task IPipeline.ExecuteAsync(PipelineContext context)
        {
            BaseInitialize();

            await BeforeExceute(context);

            if (await ExecuteAsync())
            {
                if (Next != null) await Next?.ExecuteAsync(context);
            }

            await AfterExceute(context);
        }


        public virtual Task BeforeExceute(PipelineContext context)
        {
            return Task.FromResult<object>(null);
        }

        public virtual Task AfterExceute(PipelineContext context)
        {
            return Task.FromResult<object>(null);
        }

        protected virtual Task<bool> ExecuteAsync()
        {
            return Task.FromResult(true);
        }

        public IPipeline Next { get; set; }
    }

    public class CrawlerPipeline<TOptions> : CrawlerPipeline
        where TOptions : PipelineOptions
    {
        protected CrawlerPipeline(TOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (string.IsNullOrEmpty(options.Name))
            {
                options.Name = GetType().Name;
            }
            if (options.Scheduler == null)
            {
                options.Scheduler = new DefaultScheduler();
            }

            Options = options;
        }
        public TOptions Options { get; set; }
    }
}
