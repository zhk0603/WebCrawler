/*
 下载 中国雅思网 的 新概念第一册 flash。
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Crawler.Pipelines;

namespace Crawler.Simple
{
    public class CnieltsPipeline1 : CrawlerPipeline
    {
        protected override Task<bool> ExecuteAsync(PipelineContext context)
        {
            return Task.Factory.StartNew(() =>
            {
                if (context.Page.HttpStatusCode == 200 && context.Page.HtmlNode != null)
                {
                    var liNodes = context.Page.HtmlNode.SelectNodes("//div[@id='middlebar']/div/ul/li");
                    if (liNodes != null && liNodes.Count > 0)
                    {
                        var courseColl = new List<Course>();
                        foreach (var node in liNodes)
                        {
                            var aNode = node.SelectSingleNode("/a");
                            var course = new Course
                            {
                                Title = aNode.InnerText,
                                Url = aNode.GetAttributeValue("href",""),
                                Time = DateTime.Parse(node.InnerText)
                            };
                            courseColl.Add(course);
                            Console.WriteLine($"标题：{course.Time}\tLink:{course.Url}");
                        }
                        context.PipelineData.Add("CourseData", courseColl);
                    }
                }
                return true;
            });
        }
    }

    public class CnielstPipeline2 : CrawlerPipeline<CnielstPipeline2Options>
    {
        public CnielstPipeline2(CnielstPipeline2Options options) : base(options)
        {
        }

        protected override Task<bool> ExecuteAsync(PipelineContext context)
        {
            return Task.Factory.StartNew(() =>
            {
                if (context.PipelineData["CourseData"] is List<Course> courseColl && courseColl.Count > 0)
                {
                    foreach (var course in courseColl)
                    {
                        var page = Options.Downloader.GetPage(course.Url);

                    }
                }
                return true;
            });
        }
    }

    public class CnielstPipeline2Options : PipelineOptions
    {
        public CnielstPipeline2Options(Downloader.IDownloader downloader)
        {
            Downloader = downloader;
        }
        public Downloader.IDownloader Downloader { get; set; }
    }

    public class Course
    {
        public string Title { get; set; }
        public string Url { get; set; }
        public DateTime Time { get; set; }
    }
}
