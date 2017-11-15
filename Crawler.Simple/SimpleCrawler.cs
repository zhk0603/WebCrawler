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
                .AddSite("http://www.cnielts.com/topic/list_18_1.html")
                .AddSite("http://www.cnielts.com/topic/list_18_2.html")
                .AddSite("http://www.cnielts.com/topic/list_18_3.html")
                .UsePipeline<CnieltsPipeline1>()
                .UsePipeline<CnielstPipeline2>(new CnielstPipeline2Options(new HttpDownloader()))
                .UsePipeline<CnielstPipeline3>(new FileDownloadOptions()
                {
                    DownloadDirectory = "~/",
                    Downloader = new HttpDownloader()
                })
                .UseMultiThread(3)
                .SetLogFactory(new NLoggerFactory())
                .UseNamed("CnieltsSpider");
           return builder.Builder();
        }
    }
}
