using System;
using System.Threading.Tasks;
using Crawler.Logger;
using Crawler.Scheduler;

namespace Crawler.Pipelines
{
    public class CrawlerPipeline : IPipeline
    {
        private IPipeline _next;
        void IPipeline.Initialize()
        {
            BaseInitialize();
        }

        public virtual string Name { get; set; }
        public virtual bool IsComplete { get; set; }
        public virtual bool IsSkip { get; set; }

        IPipeline IPipeline.Next
        {
            get => _next;
            set => _next = value;
        }

        async Task IPipeline.ExecuteAsync(PipelineContext context)
        {
            if (this.IsComplete || this.IsSkip)
            {
                Console.WriteLine($"管道：【{this.Name}】已完成，直接进入下一管道。");
                if (_next != null) await _next?.ExecuteAsync(context);
                return;
            }

            BaseInitialize();

            if (await ExecuteAsync(context))
            {
                if (_next != null) await _next?.ExecuteAsync(context);
            }

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