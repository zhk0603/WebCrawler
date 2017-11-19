/*
 下载 中国雅思网 的 新概念第一册 flash。
 此版的下载方式为深度优先。
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Crawler.Downloader;
using Crawler.Pipelines;
using Crawler.Schedulers;

namespace Crawler.Simple
{
    public class CnieltsPipeline1 : CrawlerPipeline
    {
        private readonly IScheduler _scheduler;
        private readonly IDownloader _downloader;

        public CnieltsPipeline1()
        {
            _scheduler = SchedulerManager.GetSiteScheduler("CnieltsPipeline1");
            _downloader = new HttpDownloader();
        }

        protected override void Initialize(PipelineContext context)
        {
            foreach (var site in context.Configuration.StartSites)
            {
                _scheduler.Push(site);
            }
            base.Initialize(context);
        }

        protected override Task<bool> ExecuteAsync(PipelineContext context)
        {
            return Task.Factory.StartNew(() =>
            {
                var site = (Site) _scheduler.Pop();
                if (site != null)
                {
                    var page = _downloader.GetPage(site);
                    if (page.HttpStatusCode == 200 && page.HtmlNode != null)
                    {
                        var liNodes = page.HtmlNode.SelectNodes("//div[@id='middlebar']/div/ul/li");
                        if (liNodes != null && liNodes.Count > 0)
                        {
                            var courseColl = new List<Course>();
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

                                courseColl.Add(course);
                                Console.WriteLine($"标题：{course.Title}\tLink:{course.Url}");
                            }
                            context.PipelineData.Add("CourseData", courseColl);
                        }
                    }
                }

                return true;
            });
        }
    }

    public class CnielstPipeline2 : CrawlerPipeline<PipelineOptions>
    {
        public CnielstPipeline2(PipelineOptions options) : base(options)
        {
        }

        protected override Task<bool> ExecuteAsync(PipelineContext context)
        {
            return Task.Factory.StartNew(() =>
            {
                if (context.PipelineData.TryGetValue("CourseData", out var courseColl))
                {
                    var downloadUrls = new List<string>();
                    foreach (var course in (List<Course>)courseColl)
                    {
                        var page = Options.Downloader.GetPage("http://www.cnielts.com/topic/" + course.Url);

                        if (page.HttpStatusCode == 200 && page.HtmlNode != null)
                        {
                            var downALabelNode = page.HtmlNode.SelectSingleNode("//div[@id='DownTips']/a");
                            var url = downALabelNode?.GetAttributeValue("href", "");
                            if (!string.IsNullOrEmpty(url))
                            {
                                downloadUrls.Add(url);
                            }
                        }
                    }

                    context.PipelineData["DownloadUrls"] = downloadUrls;
                }
                return true;
            });
        }
    }


    public class CnielstPipeline3 : FileDownloadPipeline
    {
        public CnielstPipeline3(FileDownloadOptions options) : base(options)
        {
        }

        protected override async Task<bool> ExecuteAsync(PipelineContext context)
        {
            if (context.PipelineData.TryGetValue("DownloadUrls", out var downloadUrls))
            {
                foreach (var url in (List<string>)downloadUrls)
                {
                    var site = new Site(url) { ResultType = Downloader.ResultType.Byte };
                    var page = Options.Downloader.GetPage(site);
                    await SaveAsync(page.ResultByte, url.Substring(url.LastIndexOf('/') + 1));
                }
            }
            return false;
        }
    }

    public class Course
    {
        public string Title { get; set; }
        public string Url { get; set; }
        public DateTime Time { get; set; }
    }
}