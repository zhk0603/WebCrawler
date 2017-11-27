using System;
using System.Threading.Tasks;
using Crawler.Downloader;
using Crawler.Schedulers;

namespace Crawler.Pipelines
{
    public class UrlFinderPipeline : CrawlerPipeline<UrlFinderOptons>
    {
        public UrlFinderPipeline(UrlFinderOptons options) : base(options)
        {
            if(Options.Scheduler == null)
                Options.Scheduler = SchedulerManager.GetSiteScheduler(nameof(UrlFinderPipeline));
        }

        protected override void Initialize(PipelineContext context)
        {
            foreach (var site in context.Configuration.StartSites)
            {
                Options.Scheduler.Push(site);
            }
        }

        protected override Task<bool> ExecuteAsync(PipelineContext context)
        {
            if (IsComplete || IsSkip)
            {
                return Task.FromResult(true);
            }

            var site = context.Site;
            if (site == null)
            {
                return Task.FromResult(true);
            }
            var page = Options.Downloader.GetPage(site);
            if (page.HttpStatusCode == 200 && page.HtmlNode != null)
            {
                context.Page = page;
                var aNodes = page.HtmlNode.SelectNodes("//a");
                if (aNodes != null && aNodes.Count > 0)
                {
                    foreach (var item in aNodes)
                    {
                        var href = item.GetAttributeValue("href", "");
                        if (!string.IsNullOrWhiteSpace(href))
                        {
                            var url = UrlHelper.Combine(site.Url, href);
                            if (Options.UrlValidator(url))
                            {
                                var obj = new Site(url);
                                Options.Scheduler.Push(obj);
                            }
                        }
                    }
                }
            }
            else
            {
                Logger.Error(site.Url + "\t" + page.HtmlSource);
            }
            Logger.Trace("待爬取页面数量：" + Options.Scheduler.Count);
            Logger.Trace("页面总数：" + Options.Scheduler.TotalCount);
            return Task.FromResult(true);
        }
    }

    public class UrlFinderOptons : PipelineOptions
    {
        public UrlFinderOptons()
        {
            UrlValidator = url => true;
        }

        public Func<string, bool> UrlValidator { get; set; }
    }
}