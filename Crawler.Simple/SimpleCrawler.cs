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
        // 多个入口链接，管道链模式
        public static Crawler CnBlog()
        {
            var sites = new List<Site>();
            for (var i = 1; i <= 5; i++)
                sites.Add(new Site
                {
                    Url = $"https://news.cnblogs.com/n/page/{i}/"
                });
            CrawlerBuilder.Current.ClearPipelines();
            CrawlerBuilder.Current.ClearSites();
            CrawlerBuilder.Current
                .AddSiteRange(sites)
                .SetLogFactory(new NLoggerFactory())
                .UsePipeline(typeof(Pipeline1), new PipelineOptions()
                {
                    WaitForComplete = 1000
                })
                .UsePipeline<Pipeline2>(new PipelineOptions())
                .UsePipeline<Pipeline3>()
                .UseMultiThread(5)
                .UseNamed("Simple Crawler");

            return CrawlerBuilder.Current.Builder();
        }

        public static Crawler CnieltsSpider()
        {
            CrawlerBuilder.Current.ClearPipelines();
            CrawlerBuilder.Current.ClearSites();
            CrawlerBuilder.Current
                .AddSite("http://www.cnielts.com/topic/list_19_1.html")
                .AddSite("http://www.cnielts.com/topic/list_19_2.html")
                .AddSite("http://www.cnielts.com/topic/list_19_3.html")
                .AddSite("http://www.cnielts.com/topic/list_19_4.html")
                .UsePipeline<CnieltsPipeline1>()
                .UsePipeline<CnieltsPipeline2>(new PipelineOptions())
                .UsePipeline<CnieltsPipeline3>(new FileDownloadOptions()
                {
                    DownloadDirectory = @"E:\学习资料\English\新概念第二册\"
                })
                .UseMultiThread(1)
                .SetLogFactory(new NLoggerFactory())
                .UseNamed("CnieltsSpider");
            return CrawlerBuilder.Current.Builder();
        }

        // 多管道 并行模式下载资料。
        public static Crawler CnieltsV2Spider()
        {
            CrawlerBuilder.Current.ClearPipelines();
            CrawlerBuilder.Current.ClearSites();
            CrawlerBuilder.Current
                .AddSite("http://www.cnielts.com/topic/list_19_1.html")
                .AddSite("http://www.cnielts.com/topic/list_19_2.html")
                .AddSite("http://www.cnielts.com/topic/list_19_3.html")
                .AddSite("http://www.cnielts.com/topic/list_19_4.html")
                .UsePipeline<Cnielts_V2.CnieltsPipeline1>(new PipelineOptions())
                .UsePipeline<Cnielts_V2.CnieltsPipeline2>(new PipelineOptions())
                .UsePipeline<Cnielts_V2.CnieltsPipeline3>(new FileDownloadOptions()
                {
                    DownloadDirectory = @"~/CnieltsV2Spider/",
                    Downloader = new HttpDownloader()
                })
                .UseMultiThread(3)
                .SetLogFactory(new NLoggerFactory())
                .UseParallelMode()
                .UseNamed("CnieltsV2Spider");
            return CrawlerBuilder.Current.Builder();
        }

        // 爬取整站页面。
        public static ICrawler UrlFinderPipeline()
        {
            CrawlerBuilder.Current.ClearPipelines();
            CrawlerBuilder.Current.ClearSites();
            CrawlerBuilder.Current
                .AddSite("https://www.yezismile.com")
                .UsePipeline<UrlFinderPipeline>(new UrlFinderOptons()
                {
                    WaitForComplete = 5000,
                    UrlValidator = url => url.Contains("www.yezismile.com"),
                    Sleep = 200
                })
                .UseMultiThread(10)
                .SetLogFactory(new NLoggerFactory())
                .UseBloomFilter(int.MaxValue, int.MaxValue / 21, 8)
                .UseNamed("UrlFinderPipeline");
            return CrawlerBuilder.Current.Builder();
        }

        // 爬取整站页面，并保存。
        public static ICrawler CrawlerFullSite()
        {
            CrawlerBuilder.Current.ClearPipelines();
            CrawlerBuilder.Current.ClearSites();
            CrawlerBuilder.Current
                .AddSite("https://www.yezismile.com")
                .UsePipeline<Yezismile.YezismileUrlFinderPipeline>(new UrlFinderOptons()
                {
                    WaitForComplete = 10000,
                    UrlValidator = url => url.Contains("www.yezismile.com"),
                    Sleep = 200
                })
                .UsePipeline<FileDownloadPipeline>(new FileDownloadOptions("~/Yezismile/2"))
                .UseMultiThread(8)
                .SetLogFactory(new NLoggerFactory())
                .UseBloomFilter(int.MaxValue, int.MaxValue / 21, 8)
                .UseNamed("CrawlerFullSite");
            return CrawlerBuilder.Current.Builder();
        }
    }
}
