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
                .AddSite("http://cuiqingcai.com/")
                .UsePipeline<Yezismile.YezismileUrlFinderPipeline>(new UrlFinderOptons()
                {
                    WaitForComplete = 10000,
                    UrlValidator = url => url.Contains("cuiqingcai.com"),
                    Sleep = 500
                })
                .UsePipeline<FileDownloadPipeline>(new FileDownloadOptions("~/Cuiqingcai/"))
                .UseMultiThread(5)
                .SetLogFactory(new NLoggerFactory())
                .UseBloomFilter(int.MaxValue, int.MaxValue / 21, 8)
                .UseNamed("CrawlerFullSite");
            return CrawlerBuilder.Current.Builder();
        }

        public static ICrawler CnBlogsCrawler()
        {
            CrawlerBuilder.Current.ClearPipelines();
            CrawlerBuilder.Current.ClearSites();

            var cookie =
                "UM_distinctid=15fd7aea40f104-0faa19baa1a89f-5b4a2c1d-100200-15fd7aea41033f; .CNBlogsCookie=7DCE52A39EEF9BFBFD03285E1C5F450136D1B76B140607C3FDACDD9EA6A5BFE3BCB02A769920C4A2F63BC2857757876A0726EC8B6D59F3039F4FBB573DA23B0645323061077804092F31FB7C776C6B2A3E6B33C6; .Cnblogs.AspNetCore.Cookies=CfDJ8BMYgQprmCpNu7uffp6PrYY_K-64pplxZZ8bw-7p3XTJJdlaNLZyFgZb2peEhUzinW7S5bRbXITCLcZCsbif_4TZVwdduO1t8qv7hjJ9STctL8Uwt5TOdF_k0Vy7HghXRArOb3fIF5jyd73XuvGt9jmran2od20egcgoRdlq3_gWB5OsR2h5AXFRQGqNUQueGmNh9nwoaKUQ9Sy8Zas1eIZGGJyPGpnjtMgXIY5gt3sOrbZrZk0FanUF2dPfhH6HwuVMSxIDDhkNiF9jN9_gSN3c0PIzFcV3LReUAJc31mmY; .AspNetCore.Antiforgery.b8-pDmTq1XM=CfDJ8BMYgQprmCpNu7uffp6PrYZwkiQVIRV01gbQ3QP3NxpfdtbDHl3XmzrhkMZhV3zyBp-XUMpusUAxoYOCgLe4XfxSUEaZwLMpuF9csFQxzRBPDI1mfroDhWa1PommGwyADOtywoNVehoqgZHAlCgGd6Q;";
            var waitForComplete = 60 * 1000;
            var sleep = 500;

            CrawlerBuilder.Current
                .AddSite("https://home.cnblogs.com/u/artech")
                .UsePipeline(typeof(CnBlogs.UserInfoPipeline), new CnBlogs.CnBlogsOptions{ Sleep = sleep, WaitForComplete = waitForComplete, Cookie = cookie})
                .UsePipeline<CnBlogs.FollowFansPipeline>(new CnBlogs.CnBlogsOptions { Sleep = sleep, WaitForComplete = waitForComplete, Cookie = cookie })
                .UsePipeline<CnBlogs.PostUserListPipeline>(new CnBlogs.CnBlogsOptions { Sleep = sleep, WaitForComplete = waitForComplete, Cookie = cookie})
                .SetLogFactory(new NLoggerFactory())
                .UseBloomFilter(int.MaxValue, int.MaxValue / 21, 8)
                .UseMultiThread(5)
                .UseParallelMode();

            return CrawlerBuilder.Current.Builder();
        }
    }
}