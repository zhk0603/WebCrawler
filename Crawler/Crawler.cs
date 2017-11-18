using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Crawler.Downloader;
using Crawler.Logger;
using Crawler.Pipelines;
using Crawler.Scheduler;

namespace Crawler
{
    public class Crawler : ICrawler
    {
        private readonly IScheduler _scheduler;
        private readonly IEnumerable<Site> _sites;
        private readonly IDownloader _downloader;
        private DateTime _beginTime;
        private DateTime _endTime;
        private IEnumerable<IPipeline> _pipelines;
        private int _threadNum;
        private string _named;
        private PipelineRunMode _runMode;

        public Crawler()
        {
            _scheduler = new SiteScheduler();
            _downloader = new HttpDownloader();
        }

        public Crawler(string name, IEnumerable<Site> sites, IEnumerable<IPipeline> pipelines) : this()
        {
            Name = name;
            _sites = sites ?? throw new ArgumentNullException(nameof(sites));
            _pipelines = pipelines ?? throw new ArgumentNullException(nameof(pipelines));
        }

        public Crawler(IEnumerable<Site> sites, IEnumerable<IPipeline> pipelines) : this(Guid.NewGuid().ToString("N"), sites,
            pipelines)
        {
        }

        public string Name
        {
            get => _named;
            set
            {
                _named = value;
                Logger = LoggerManager.GetLogger(_named);
            }
        }

        public int ThreadNum
        {
            get => _threadNum;
            set
            {
                if (CheckState(CrawlerState.Running))
                    throw new InvalidOperationException("爬虫正在运行。");

                if (value < 0)
                    throw new ArgumentException("爬虫线程数量不能小于0。");
                _threadNum = value;
            }
        }

        public PipelineRunMode RunMode
        {
            get => _runMode;
            set
            {
                if (CheckState(CrawlerState.Running))
                    throw new InvalidOperationException("爬虫正在运行。");
                _runMode = value;
            }
        }

        public CrawlerState CrawlerState { get; protected set; }

        public ILogger Logger { get; protected set; }

        public IEnumerable<IPipeline> Pipelines
        {
            get => _pipelines;
            set
            {
                if (CheckState(CrawlerState.Running))
                    throw new InvalidOperationException("爬虫正在运行。");
                _pipelines = value;
            }
        }

        public void Pause()
        {
            if (CrawlerState == CrawlerState.Running)
                CrawlerState = CrawlerState.Stopped;
        }

        public void Continue()
        {
            if (CrawlerState == CrawlerState.Stopped)
                CrawlerState = CrawlerState.Running;
        }

        public void Exit()
        {
            CrawlerState = CrawlerState.Exited;
        }

        public void Run()
        {
            if (CrawlerState == CrawlerState.Running)
                return;

            foreach (var site in _sites)
                _scheduler.Push(site);

            CrawlerState = CrawlerState.Running;
            _beginTime = DateTime.Now;

            while (CrawlerState == CrawlerState.Running || CrawlerState == CrawlerState.Stopped)
            {
                if (CrawlerState == CrawlerState.Stopped)
                {
                    Thread.Sleep(500);
                    continue;
                }

                Parallel.For(0, ThreadNum, new ParallelOptions
                {
                    MaxDegreeOfParallelism = ThreadNum
                }, i =>
                {
                    while (CrawlerState == CrawlerState.Running)
                    {
                        Page page = null;


                        if (_scheduler is SiteScheduler)
                        {
                            var site = (Site) _scheduler.Pop();
                            if (site == null)
                            {
                                CrawlerState = CrawlerState.Finished;
                                break;
                            }
                            page = _downloader.GetPage(site);
                        }

                        var context = new PipelineContext
                        {
                            Crawler = this,
                            Page = page,
                            Configuration = new CrawlerConfiguration
                            {
                                Crawler = this,
                                Pipelines = Pipelines,
                                StartSites = _sites,
                                ThreadNum = _threadNum
                            }
                        };

                        try
                        {
                            if (RunMode == PipelineRunMode.Parallel)
                            {
                                Task.WaitAll(Pipelines.Select(pipeline => pipeline.ExecuteAsync(context)).ToArray());
                            }
                            else
                            {
                                Pipelines.FirstOrDefault()?.ExecuteAsync(context).GetAwaiter().GetResult();
                            }
                        }
                        catch (Exception exception)
                        {
                            Logger.Error(exception);
                        }
                    }
                });
            }

            _endTime = DateTime.Now;
            Logger?.Info("总耗时（s）：" + (_endTime - _beginTime).TotalSeconds);
        }

        public Task RunAsync()
        {
            return Task.Factory.StartNew(Run);
        }

        private bool CheckState(CrawlerState state)
        {
            return CrawlerState == state;
        }
    }
}