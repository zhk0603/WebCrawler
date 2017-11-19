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

        public ILogger Logger { get; set; }
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
            Logger.Trace("initialize");
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
        private readonly Stopwatch _stopwatch;
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

        public TOptions Options { get; set; }

        protected override void Initialize(PipelineContext context)
        {
            _stopwatch.Start();
            base.Initialize(context);
        }

        protected override async Task<bool> ExecuteAsync(PipelineContext context)
        {
            var result = await base.ExecuteAsync(context);
            Thread.Sleep(Options.Sleep);
            return result;
        }

        public override Task AfterExceute(PipelineContext context)
        {
            _stopwatch.Stop();
            if (_stopwatch.ElapsedMilliseconds >= Options.WaitForComplete)
            {
                this.IsComplete = true;
            }
            else
            {
                _stopwatch.Start();
            }

            return base.AfterExceute(context);
        }
    }
}