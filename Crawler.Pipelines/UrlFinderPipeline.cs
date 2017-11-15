using System;
using System.Threading.Tasks;
using Crawler.Downloader;

namespace Crawler.Pipelines
{
    public class UrlFinderPipeline : CrawlerPipeline<PageFinderOptons>
    {
        protected UrlFinderPipeline(PageFinderOptons options) : base(options)
        {
            Options.Scheduler = Scheduler.SchedulerManager.GetScheduler(nameof(UrlFinderPipeline));
            if (Options.Downloader == null)
            {
                Options.Downloader = new HttpDownloader();
            }
        }

        protected override void Initialize(PipelineContext context)
        {
            foreach (var site in context.Configuration.StartSites)
            {
                Options.Scheduler.Push(site);
            }
        }

        protected override async Task<bool> ExecuteAsync(PipelineContext context)
        {
            if (IsComplete || IsSkip)
            {
                return true;
            }

            var site = (Site)Options.Scheduler.Pop();
            if (site == null)
            {
                return true;
            }
            var page = Options.Downloader.GetPage(site);
            if (page.HttpStatusCode == 200 && page.HtmlNode != null)
            {
                var aNodes = page.HtmlNode.SelectNodes("//a");
                if (aNodes != null && aNodes.Count > 0)
                {
                    foreach (var item in aNodes)
                    {
                        var href = item.GetAttributeValue("href", "");
                        if (!string.IsNullOrWhiteSpace(href))
                        {
                            var url = UrlHelper.Content(site.Url, href);

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

            return true;
        }
    }

    public class PageFinderOptons : PipelineOptions
    {
        public PageFinderOptons()
        {
            UrlValidator = url => true;
        }

        public Func<string, bool> UrlValidator { get; set; }
        public IDownloader Downloader { get; set; }
    }
}