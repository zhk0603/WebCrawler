using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Crawler.Downloader;
using Crawler.Logger;
using Crawler.Pipelines;

namespace Crawler.Simple
{
    class SimpleCrawler
    {
        public static Crawler CnBlog()
        {
            var builder = new CrawlerBuilder();

            var sites = new List<Site>();
            for (var i = 1; i <= 5; i++)
                sites.Add(new Site
                {
                    Url = $"https://news.cnblogs.com/n/page/{i}/"
                });

            builder
                .AddSiteRange(sites)
                .SetLogFactory(new NLoggerFactory())
                .UsePipeline(typeof(Pipeline1), new PipelineOptions())
                .UsePipeline<Pipeline2>(new PipelineOptions())
                .UsePipeline<Pipeline3>()
                .UseMultiThread(5)
                .UseNamed("Simple Crawler");

            return builder.Builder();
        }

        public static Crawler CnieltsSpider()
        {
            var builder = new CrawlerBuilder();
            builder
                .AddSite("http://www.cnielts.com/topic/list_19_1.html")
                .AddSite("http://www.cnielts.com/topic/list_19_2.html")
                .AddSite("http://www.cnielts.com/topic/list_19_3.html")
                .AddSite("http://www.cnielts.com/topic/list_19_4.html")
                .UsePipeline<CnieltsPipeline1>()
                .UsePipeline<CnielstPipeline2>(new CnielstPipeline2Options(new HttpDownloader()))
                .UsePipeline<CnielstPipeline3>(new FileDownloadOptions()
                {
                    DownloadDirectory = @"E:\学习资料\English\新概念第二册\",
                    Downloader = new HttpDownloader()
                })
                .UseMultiThread(3)
                .SetLogFactory(new NLoggerFactory())
                .UseNamed("CnieltsSpider");
           return builder.Builder();
        }

        public static ICrawler UrlFinderPipeline()
        {
            var builder = new CrawlerBuilder();
            builder
                .UsePipeline<UrlFinderPipeline>(new UrlFinderOptons())
                .UseMultiThread(1)
                .SetLogFactory(new NLoggerFactory())
                .UseNamed("UrlFinderPipeline");
            return builder.Builder();
        }

        public static ICrawler ParallelSpider()
        {
            var builder = new CrawlerBuilder();
            builder
                .AddSite("http://www.cnielts.com/topic/list_19_1.html")
                .AddSite("http://www.cnielts.com/topic/list_19_2.html")
                .AddSite("http://www.cnielts.com/topic/list_19_3.html")
                .AddSite("http://www.cnielts.com/topic/list_19_4.html")
                .UsePipeline<CnieltsPipeline1>()
                .UsePipeline<CnielstPipeline2>(new CnielstPipeline2Options(new HttpDownloader()))
                .UsePipeline<CnielstPipeline3>(new FileDownloadOptions()
                {
                    DownloadDirectory = @"E:\学习资料\English\新概念第二册\",
                    Downloader = new HttpDownloader()
                })
                .UseMultiThread(3)
                .SetLogFactory(new NLoggerFactory())
                .UseNamed("ParallelSpider")
                .UseParallelMode();
            return builder.Builder();
        }
    }
}
