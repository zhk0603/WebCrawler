using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Crawler.Downloader;
using Crawler.Logger;
using Crawler.Schedulers;

namespace Crawler.Pipelines
{
    public class CrawlerPipeline : IPipeline
    {
        private bool _initializen;
        private readonly object _initLock = new object();

        public CrawlerPipeline()
        {
            Logger = LoggerManager.GetLogger(GetType());
        }

        private IPipeline _next;

        protected ILogger Logger { get; }
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
                if (_next != null)
                {
                    await _next.ExecuteAsync(context);
                }

                return;
            }

            if (!_initializen)
            {
                lock (_initLock)
                {
                    if (!_initializen)
                    {
                        Initialize(context);
                        _initializen = true;
                    }
                }
            }

            await BeforeExceute(context);

            if (await ExecuteAsync(context))
            {
                if (_next != null) await _next?.ExecuteAsync(context);
            }

            await AfterExceute(context);
        }

        /// <summary>
        ///     只会执行一次初始化操作。
        /// </summary>
        /// <param name="context"></param>
        protected virtual void Initialize(PipelineContext context)
        {
        }

        protected virtual Task BeforeExceute(PipelineContext context)
        {
            return Task.FromResult<object>(null);
        }

        protected virtual Task AfterExceute(PipelineContext context)
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
        private readonly Stopwatch _stopwatch;
        private readonly object _swLock = new object();

        public CrawlerPipeline(TOptions options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            if (string.IsNullOrEmpty(options.Name))
                options.Name = GetType().Name;
            base.Name = options.Name;

            if (options.Downloader == null)
                options.Downloader = new HttpDownloader();

            Options = options;
            _stopwatch = new Stopwatch();
        }

        protected TOptions Options { get; }

        protected virtual Site OnParseSite(object item)
        {
            if (item is Site site)
            {
                return site;
            }
            if (item is string s)
            {
                return new Site(s);
            }
            return new Site();
        }

        protected override async Task BeforeExceute(PipelineContext context)
        {
            if (Options.Scheduler != null)
            {
                var item = Options.Scheduler.Pop();
                if (item == null)
                {
                    lock (_swLock)
                    {
                        Logger.Trace("等待获取资源中……");
                        _stopwatch.Start();
                        Thread.Sleep(200);
                        _stopwatch.Stop();
                        return;
                    }
                }
                context.Site = OnParseSite(item);
            }

            await base.BeforeExceute(context);
        }

        protected override Task AfterExceute(PipelineContext context)
        {
            if (Options.WaitForComplete > 0 && _stopwatch.ElapsedMilliseconds >= Options.WaitForComplete)
            {
                this.IsComplete = true;
            }

            Thread.Sleep(Options.Sleep);
            return base.AfterExceute(context);
        }
    }
}