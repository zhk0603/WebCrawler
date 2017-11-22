/*
 * 爬取cnblogs 的用户信息。
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Crawler.Pipelines;

namespace Crawler.Simple
{
    public class CnBlogs
    {
        public class UserInfoPipeline : Pipelines.CrawlerPipeline<Pipelines.PipelineOptions>
        {
            public UserInfoPipeline(PipelineOptions options) : base(options)
            {
                Options.Scheduler = Schedulers.SchedulerManager.GetSiteScheduler("CnBlogs");
            }

            protected override void Initialize(PipelineContext context)
            {
                foreach (var site in context.Configuration.StartSites)
                {
                    Options.Scheduler.Push(site);
                }

                base.Initialize(context);
            }

            protected override Site OnParseSite(object item)
            {
                var site = item as Site;
                // 设置你登录后的cookie.
                site.Cookie = "UM_distinctid=15fd7aea40f104-0faa19baa1a89f-5b4a2c1d-100200-15fd7aea41033f; .CNBlogsCookie=8FCF3E2C8698D5322FCFE4A454D73ED31C54DE202E939782FD34F631F16F9A167A48E56B9B181953EE3064DA1C41CE4AC247E5AB83CF54AA1CAFFE41DC7D9917CE31FE5EFE7BF3AC71791521B7D67D4657B5A1B5; .Cnblogs.AspNetCore.Cookies=CfDJ8BMYgQprmCpNu7uffp6PrYaWOy0is5FzvvaAWTs3T0-t3XxY1zKkSSNG7eBxpFeJf8YwynB6GD5mMPY-K956UGmcnGdWho2VxEsBzV8h3-21BIdE0Mi_PnpjPhf5b_PMlwLJvvnAPi1gDvaUtUrBHDImGjfeFSeyvbF3QT1id1XO5rJsy5fOS5X1gpbkp1t-G4cgX7AqnM3KkaMMBwweAlCIHQZw1sMY3XKIY4lXB0vDpJDnKqOH-9-K5WDGDMC9G9qPzQ2aDwA-_VptarBr_PwQuPvtKa1MeMg9RriGBjmW; _ga=GA1.2.940203622.1510883896; _gid=GA1.2.500418132.1511152068";
                site.UserAgent =
                    "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/62.0.3202.62 Safari/537.36";
                return site;
            }

            protected override Task<bool> ExecuteAsync(PipelineContext context)
            {
                if (context.Site != null)
                {
                    var page = Options.Downloader.GetPage(context.Site);
                    if (page.HttpStatusCode == 200 && page.HtmlNode != null)
                    {
                        var nickNameNode = page.HtmlNode.SelectSingleNode("//h1[@class='display_name']");
                        Console.WriteLine("昵称：" + nickNameNode.InnerText.Trim());
                        var userProfileNodes = page.HtmlNode.SelectNodes("//ul[@id='user_profile']/li");
                        if (userProfileNodes != null)
                        {
                            foreach (var liNode in userProfileNodes)
                            {
                                Console.WriteLine(liNode.InnerText);
                            }
                        }
                    }
                }

                return base.ExecuteAsync(context);
            }
        }
    }
}
