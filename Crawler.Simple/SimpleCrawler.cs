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
                .UseMultiThread(1)
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
                    DownloadDirectory = @"~/CnieltsV2Spider/2017-12-10/",
                    Downloader = new HttpDownloader()
                })
                .UseMultiThread(3)
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
                    Sleep = 200,
                    Scheduler =
                        Schedulers.SchedulerManager.GetScheduler<Schedulers.RedisScheduler<Site>>("UrlFinderPipeline")
                })
                .UseMultiThread(10)
                //.UseBloomFilter(int.MaxValue, 0.001F)
                .UseRedisBloomFilter()
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
                    UrlValidator = url => url.Contains("yezismile.com"),
                    Sleep = 500
                })
                .UsePipeline<FileDownloadPipeline>(new FileDownloadOptions("~/Yezismile/HtmlSources/"))
                .UseMultiThread(8)
                .UseNamed("CrawlerFullSite");
            return CrawlerBuilder.Current.Builder();
        }

        public static ICrawler CnBlogsCrawler()
        {
            CrawlerBuilder.Current.ClearPipelines();
            CrawlerBuilder.Current.ClearSites();

            var cookie =
                "UM_distinctid=15fd7aea40f104-0faa19baa1a89f-5b4a2c1d-100200-15fd7aea41033f; .AspNetCore.Antiforgery.b8-pDmTq1XM=CfDJ8BMYgQprmCpNu7uffp6PrYZwkiQVIRV01gbQ3QP3NxpfdtbDHl3XmzrhkMZhV3zyBp-XUMpusUAxoYOCgLe4XfxSUEaZwLMpuF9csFQxzRBPDI1mfroDhWa1PommGwyADOtywoNVehoqgZHAlCgGd6Q; __utma=226521935.940203622.1510883896.1511340298.1511400020.2;.CNBlogsCookie=21F214BA965AD78A4B0668129C6D0F61DC03622B866C6F582007CB680D65ABB3B5C9B2C5135AF06CCEC079D674E6F77112E69D258F3E49159F5A62C270A934F737B34F6C1BE88F2A94D2D8483835482A2D69BDA3D8C7865D3C1F31C9456DFA87ABF84792; .Cnblogs.AspNetCore.Cookies=CfDJ8BMYgQprmCpNu7uffp6PrYZl07eg1xkLN9aBbgKGLXJez8caqpvgeam7-VtYQzrAWXnrYOIgzuBTZsorAdQPSh6MmR2ymUNdLOIHtui9cmIClnj3h4bgVHksFeKCtw25aezLtHCSsdn994FToPdBphzu0HiHTdPODta7IqtCghP6yUHpzIdqphHAL0Uovhj0BACtOY2TNYUSE-mJIE0t2MMSiXDvtkpoRTTndl8Z-G0tHd2Uq9AWkIWsi534ByWwUnYZpYo490crqL00myP6pdA_xYQ_wqwc3nrG8LaVcGYdzCtY_RcnpLRL7T_ompunYQ;";
            var waitForComplete = 60 * 60 * 1000;
            var sleep = 300;
            var connStr = "server=.;database=t_cnblogs;user=sa;password=123@Abc;";

            CrawlerBuilder.Current
                .AddSite("https://home.cnblogs.com/u/daxnet")
                .UsePipeline(typeof(CnBlogs.SaveUserInfoPipeline),
                    new CnBlogs.CnBlogsOptions
                    {
                        Sleep = sleep,
                        WaitForComplete = waitForComplete,
                        Cookie = cookie,
                        ConnStr = connStr
                    })
                .UsePipeline<CnBlogs.AnalysisFollowPipeline>(new CnBlogs.CnBlogsOptions
                {
                    Sleep = sleep,
                    WaitForComplete = waitForComplete,
                    Cookie = cookie
                })
                .UsePipeline<CnBlogs.CrawlerUserPipeline>(new CnBlogs.CnBlogsOptions
                {
                    Sleep = sleep,
                    WaitForComplete = waitForComplete,
                    Cookie = cookie
                })
                .UseBloomFilter(int.MaxValue / 21, 0.001F)
                .UseMultiThread(5)
                .UseParallelMode();

            return CrawlerBuilder.Current.Builder();
        }

        public static ICrawler RedisCnblogsCrawler()
        {
            CrawlerBuilder.Current.ClearPipelines();
            CrawlerBuilder.Current.ClearSites();

            var cookie =
                ".CNBlogsCookie=BFE40C6CB5A313065FB04E41881EEB8CC982F0878CDB4408E274E1457F2DB633F593B274F4ED6C95BE79F682613997DACF4E2A87249B3300BEAB18539659614921192B14BA47A32D58E971F489222EAC50CE62E9; .Cnblogs.AspNetCore.Cookies=CfDJ8N7AeFYNSk1Put6Iydpme2ZQ94Ef2Y--c-UzsxyWDAzfo-WG4rbFZnX5GUasyI3_zQKx6lv63yEl8zWoNQN_ZzVuvuOOxOC-Ju1Ju9r5EzOn0ZLxBWOTpvAw3AtX_PrME32ZOxXdaiIkxsaE6AJgqvytPpamSCBmsMdu73aYksi0GmuFpisb-ItFS1uW0t4RvJMqcFt6kI1HOvnM1JuN6FMGAz9Ptk_lFO9NL3Cup7Jbf7w1OKz3WAONnS2NAIPMbr_aUiMgIND-z7mA1_N1LBPqEMotF7waZwWGuQiB97si;";
            var waitForComplete = 60 * 60 * 1000;
            var sleep = 300;
            var connStr = "server=.;database=t_cnblogs;user=sa;password=123@Abc;";

            CrawlerBuilder.Current
                .AddSite("https://home.cnblogs.com/u/daxnet")
                .UsePipeline(typeof(CnBlogs.SaveUserInfoPipeline),
                    new CnBlogs.CnBlogsOptions
                    {
                        Sleep = sleep,
                        WaitForComplete = waitForComplete,
                        Cookie = cookie,
                        ConnStr = connStr
                    })
                .UsePipeline<CnBlogs.AnalysisFollowPipeline>(new CnBlogs.CnBlogsOptions
                {
                    Sleep = sleep,
                    WaitForComplete = waitForComplete,
                    Cookie = cookie
                })
                .UsePipeline<CnBlogs.CrawlerUserPipeline>(new CnBlogs.CnBlogsOptions
                {
                    Sleep = sleep,
                    WaitForComplete = waitForComplete,
                    Cookie = cookie
                })
                .UseMultiThread(5)
                .UseRedisBloomFilter()
                .UseParallelMode();

            return CrawlerBuilder.Current.Builder();
        }
    }
}