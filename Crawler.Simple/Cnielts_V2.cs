using Crawler.Pipelines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Crawler.Schedulers;

namespace Crawler.Simple
{
    class Cnielts_V2
    {
        public class CnieltsPipeline1 : CrawlerPipeline<PipelineOptions>
        {
            private readonly IScheduler _downloadPageScheduler;

            public CnieltsPipeline1(PipelineOptions options) : base(options)
            {
                _downloadPageScheduler = SchedulerManager.GetScheduler<Scheduler<string>>("downloadPageScheduler");
                Options.Scheduler = SchedulerManager.GetSiteScheduler("CnieltsPipeline1");
            }

            protected override void Initialize(PipelineContext context)
            {
                foreach (var site in context.Configuration.StartSites)
                {
                    this.Options.Scheduler.Push(site);
                }
                base.Initialize(context);
            }

            protected override Task<bool> ExecuteAsync(PipelineContext context)
            {
                return Task.Factory.StartNew(() =>
                {
                    if (context.Site != null)
                    {
                        var page = Options.Downloader.GetPage(context.Site);
                        if (page.HttpStatusCode == 200 && page.HtmlNode != null)
                        {
                            var liNodes = page.HtmlNode.SelectNodes("//div[@id='middlebar']/div/ul/li");
                            if (liNodes != null && liNodes.Count > 0)
                            {
                                foreach (var node in liNodes)
                                {
                                    var aNode = node.SelectSingleNode("a");

                                    var reg = new Regex(@"(?<time>\(.*\))");
                                    Match match = reg.Match(node.InnerText);
                                    var tiem = match.Groups["time"].Value;
                                    var time = tiem.Substring(1, tiem.Length - 2);

                                    var course = new Course
                                    {
                                        Title = aNode.InnerText,
                                        Url = aNode.GetAttributeValue("href", ""),
                                        Time = DateTime.Parse(time)
                                    };
                                    var downloadUrl = UrlHelper.Combine(context.Site.Url, course.Url);
                                    _downloadPageScheduler.Push(downloadUrl);
                                }
                                Logger.Trace(context.Site.Url + "成功");
                            }
                        }
                    }
                    return false;
                });
            }
        }

        public class CnielstPipeline2 : CrawlerPipeline<PipelineOptions>
        {
            private readonly IScheduler _downloadUrlScheduler;

            public CnielstPipeline2(PipelineOptions options) : base(options)
            {
                _downloadUrlScheduler = SchedulerManager.GetScheduler<Scheduler<string>>("downloadUrlScheduler");
                Options.Scheduler = SchedulerManager.GetScheduler<Scheduler<string>>("downloadPageScheduler");
            }

            protected override Task<bool> ExecuteAsync(PipelineContext context)
            {
                return Task.Factory.StartNew(() =>
                {
                    if (context.Site != null)
                    {
                        var page = Options.Downloader.GetPage(context.Site);

                        if (page.HttpStatusCode == 200 && page.HtmlNode != null)
                        {
                            var downALabelNode = page.HtmlNode.SelectSingleNode("//div[@id='DownTips']/a");
                            var downloadUrl = downALabelNode?.GetAttributeValue("href", "");
                            if (!string.IsNullOrEmpty(downloadUrl))
                            {
                                Logger.Trace(context.Site.Url + "成功");
                                _downloadUrlScheduler.Push(downloadUrl);
                            }
                        }
                    }
                    return false;
                });
            }
        }

        public class CnielstPipeline3 : FileDownloadPipeline
        {
            public CnielstPipeline3(FileDownloadOptions options) : base(options)
            {
                Options.Scheduler = SchedulerManager.GetScheduler<Scheduler<string>>("downloadUrlScheduler");
            }

            protected override async Task<bool> ExecuteAsync(PipelineContext context)
            {
                if (context.Site !=null)
                {
                    var url = context.Site.Url;
                    var page = Options.Downloader.GetPage(context.Site);
                    await SaveAsync(page.ResultByte, url.Substring(url.LastIndexOf('/') + 1));
                    Logger.Trace(url + "成功");
                }
                return false;
            }
        }
    }
}
