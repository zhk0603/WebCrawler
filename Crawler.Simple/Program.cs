using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using Crawler.Filter;
using Crawler.Pipelines;
using HtmlAgilityPack;

namespace Crawler.Simple
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            //SimpleCrawler.CnBlog().Run();
            //Console.ReadKey();

            //SimpleCrawler.CnieltsSpider().Run();
            //Console.ReadKey();

            //SimpleCrawler.CnieltsV2Spider().Run();
            //Console.ReadKey();

            //SimpleCrawler.UrlFinderPipeline().Run();
            //Console.ReadKey();

            //SimpleCrawler.CrawlerFullSite().Run();
            //Console.ReadKey();

            //SimpleCrawler.CnBlogsCrawler().Run();
            //Console.ReadKey();

            SimpleCrawler.RedisCnblogsCrawler().Run();
            Console.ReadKey();

        }
    }

    internal class Pipeline1 : CrawlerPipeline<PipelineOptions>
    {
        public Pipeline1(PipelineOptions options) : base(options)
        {
            Options.Scheduler = Schedulers.SchedulerManager.GetSiteScheduler("Pipeline1");
        }

        protected override void Initialize(PipelineContext context)
        {
            Console.WriteLine("处理管道1-开始");

            foreach (var site in context.Configuration.StartSites)
            {
                Options.Scheduler.Push(site);
            }

            base.Initialize(context);
        }

        protected override Task<bool> ExecuteAsync(PipelineContext context)
        {
            return Task.Factory.StartNew(() =>
            {
                context.Page = Options.Downloader.GetPage(context.Site);
                var node = context.Page.DocumentNode;
                var titleColl = node?.SelectNodes("//div[@id='news_list']/div[@class='news_block']/div[2]/h2/a");
                if (titleColl != null)
                    foreach (var title in titleColl)
                        Console.WriteLine("标题：" + title.InnerText);

                return false;
            });
        }

        protected override Task AfterExceute(PipelineContext context)
        {
            Console.WriteLine("处理管道1-结束" + context.Page.Uri);
            return base.AfterExceute(context);
        }
    }

    internal class Pipeline2 : CrawlerPipeline<PipelineOptions>
    {
        public Pipeline2(PipelineOptions options) : base(options)
        {
        }

        protected override void Initialize(PipelineContext context)
        {
            Console.WriteLine("\t处理管道2-开始");
        }

        protected override Task<bool> ExecuteAsync(PipelineContext context)
        {
            // 返回 false 将不执行 管道3
            return Task.FromResult(false);
        }

        protected override Task AfterExceute(PipelineContext context)
        {
            Console.WriteLine("\t处理管道2-结束");
            return Task.FromResult(0);
        }
    }

    internal class Pipeline3 : CrawlerPipeline
    {
        protected override void Initialize(PipelineContext context)
        {
            Console.WriteLine("\t\tI'm Pipeline3");
        }
    }
}