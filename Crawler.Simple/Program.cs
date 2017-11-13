using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Crawler.Downloader;
using Crawler.Pipelines;

namespace Crawler.Simple
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var builder = new CrawlerBuilder();

            //var sites = new List<Site>();
            //for (var i = 1; i <= 5; i++)
            //    sites.Add(new Site
            //    {
            //        Url = $"https://news.cnblogs.com/n/page/{i}/"
            //    });
            //
            //builder
            //    .AddSiteRange(sites)
            //    .UsePipeline(typeof(Pipeline1), new PipelineOptions())
            //    .UsePipeline<Pipeline2>(new PipelineOptions())
            //    .UsePipeline<Pipeline3>()
            //    .UseMultiThread(1)
            //    .UseNamed("Simple Crawler");
            //
            //var crawler = builder.Builder();
            //crawler.Run();
            //Console.ReadKey();

            builder.ClearPipelines()
                .ClearSites()
                .AddSite("http://www.cnielts.com/topic/list_18_1.html")
                .UsePipeline<CnieltsPipeline1>()
                .UsePipeline<CnielstPipeline2>(new CnielstPipeline2Options(new HttpDownloader()))
                .UseNamed("CnieltsSpider");
            var crawler = builder.Builder();
            crawler.Run();
            Console.ReadKey();

        }
    }

    internal class Pipeline1 : CrawlerPipeline<PipelineOptions>
    {
        public Pipeline1(PipelineOptions options) : base(options)
        {
        }

        protected override void BaseInitialize()
        {
            Console.WriteLine("处理管道1-开始");
        }

        protected override Task<bool> ExecuteAsync(PipelineContext context)
        {
            return Task.Factory.StartNew(() =>
            {
                var node = context.Page.HtmlNode;
                var titleColl = node?.SelectNodes("//div[@id='news_list']/div[@class='news_block']/div[2]/h2/a");
                if (titleColl != null)
                    foreach (var title in titleColl)
                        Console.WriteLine("标题：" + title.InnerText);

                return false;
            });
        }

        public override Task AfterExceute(PipelineContext context)
        {
            Console.WriteLine("处理管道1-结束");
            return Task.FromResult(0);
        }
    }

    internal class Pipeline2 : CrawlerPipeline<PipelineOptions>
    {
        public Pipeline2(PipelineOptions options) : base(options)
        {
        }

        protected override void BaseInitialize()
        {
            Console.WriteLine("\t处理管道2-开始");
        }

        protected override Task<bool> ExecuteAsync(PipelineContext context)
        {
            // 返回 false 将不执行 管道3
            return Task.FromResult(false);
        }

        public override Task AfterExceute(PipelineContext context)
        {
            Console.WriteLine("\t处理管道2-结束");
            return Task.FromResult(0);
        }
    }

    internal class Pipeline3 : CrawlerPipeline
    {
        protected override void BaseInitialize()
        {
            Console.WriteLine("\t\tI'm Pipeline3");
        }
    }
}