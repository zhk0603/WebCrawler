/*
 * 爬取 cnblogs 的用户信息，导出到excel。
 * 2017年11月28日 13:55:00
 *     将调度器更换为Redis调度器。
 */

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Crawler.Pipelines;
using HtmlAgilityPack;

namespace Crawler.Simple
{
    public class CnBlogs
    {
        private static readonly string _relationUserApi = "https://home.cnblogs.com/relation_users";
        private static readonly string _userInfoUrl = "https://home.cnblogs.com/u/";
        private static readonly string _method = "POST";
        private static readonly string _accept = "application/json, text/javascript, */*; q=0.01";
        private static readonly string _contentType = "application/json; charset=UTF-8";

        public class SaveUserInfoPipeline : CrawlerPipeline<CnBlogsOptions>
        {
            private readonly Schedulers.IScheduler _followFansScheduler;

            public SaveUserInfoPipeline(CnBlogsOptions options) : base(options)
            {
                Options.Scheduler =
                    Schedulers.SchedulerManager.GetScheduler<Schedulers.RedisScheduler<Site>>("CnBlogs");
                _followFansScheduler =
                    Schedulers.SchedulerManager.GetScheduler<Schedulers.RedisScheduler<string>>("followFansScheduler");

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
                site.Cookie = Options.Cookie;
                site.UserAgent =
                    "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/62.0.3202.62 Safari/537.36";
                return site;
            }

            protected override Task<bool> ExecuteAsync(PipelineContext context)
            {
                if (context.Site != null)
                {
                    var page = Options.Downloader.GetPage(context.Site);
                    if (page.HttpStatusCode == 200 && page.DocumentNode != null)
                    {
                        var reg = new Regex("[a-fA-F0-9]{8}-([a-fA-F0-9]{4}-){3}[a-fA-Z0-9]{12}",
                            RegexOptions.IgnoreCase);
                        var match = reg.Match(page.HtmlSource);

                        var user = new UserInfo();
                        user.UserId = Guid.Parse(match.Value);

                        // 将用户guid推入调度器。
                        _followFansScheduler.Push(match.Value);

                        #region 正则匹配信息。

                        var userProfileNode = page.DocumentNode.SelectSingleNode("//div[@id='user_profile_block']");
                        var htmlSource = userProfileNode.InnerHtml;
                        match = Regex.Match(htmlSource, "<h1[^>]* class=\"display_name\">([^<]*)</h1>");
                        if (match.Success)
                        {
                            user.NickName = match.Groups[1].Value.Trim();
                        }
                        var matches = Regex.Matches(htmlSource,
                            "<li[^>]*><span[^>]* class=\'text_gray\'>([^<]*)</span>([^<]*)</li>");
                        if (matches.Count > 0)
                        {
                            foreach (Match item in matches)
                            {
                                if (item.Success)
                                {
                                    var value1 = item.Groups[1].Value;
                                    var value2 = item.Groups[2].Value;
                                    if (value1.Contains("姓名"))
                                    {
                                        user.UserName = value2;
                                    }
                                    else if (value1.Contains("性别"))
                                    {
                                        user.Sex = value2.Contains("男");
                                    }
                                    else if (value1.Contains("出生日期"))
                                    {
                                        user.Birthday = DateTime.Parse(value2.Trim());
                                    }
                                    else if (value1.Contains("家乡"))
                                    {
                                        var tmp = value2.Split('-');
                                        user.Province = tmp[0];
                                        user.District = tmp[1];
                                    }
                                    else if (value1.Contains("现居住地"))
                                    {
                                        var tmp = value2.Split('-');
                                        user.CurProvince = tmp[0];
                                        user.CurDistrict = tmp[1];
                                    }
                                    else if (value1.Contains("婚姻"))
                                    {
                                        user.MarriageState = value2;
                                    }
                                    else if (value1.Contains("职位"))
                                    {
                                        user.JobTitle = value2;
                                    }
                                    else if (value1.Contains("单位"))
                                    {
                                        user.WorkUnit = value2;
                                    }
                                    else if (value1.Contains("工作状况"))
                                    {
                                        user.JobState = value2;
                                    }
                                    else if (value1.Contains("感兴趣的技术"))
                                    {
                                        user.Interest = value2;
                                    }
                                    else if (value1.Contains("最近目标"))
                                    {
                                        user.RecentGoals = value2;
                                    }
                                    else if (value1.Contains("座右铭"))
                                    {
                                        user.Motto = value2;
                                    }
                                    else if (value1.Contains("自我介绍"))
                                    {
                                        user.SelfIntroduction = value2;
                                    }
                                }
                            }
                        }
                        match = Regex.Match(htmlSource,
                            "<span class='text_gray'>园龄：</span><span title='入园时间：([^']*)'>");
                        if (match.Success)
                        {
                            user.JoinTime = DateTime.Parse(match.Groups[1].Value);
                        }
                        match = Regex.Match(htmlSource, "<span class='text_gray'>博客：</span><a href='([^']*)'");
                        if (match.Success)
                        {
                            user.BlogUrl = match.Groups[1].Value;
                        }
                        user.FollowingCount =
                            int.Parse(userProfileNode.SelectSingleNode("//a[@id='following_count']").InnerText);
                        user.FollowerCount =
                            int.Parse(userProfileNode.SelectSingleNode("//a[@id='follower_count']").InnerText);
                        user.AvatarUrl = userProfileNode.SelectSingleNode("//img[@class='img_avatar']")
                            .GetAttributeValue("src", "");

                        #endregion

                        SaveToDb(user);
                    }
                }

                return base.ExecuteAsync(context);
            }

            void SaveToDb(UserInfo userInfo)
            {
                var sql = @"INSERT INTO t_cnblogs.dbo.[UserInfo]
 (
     UserId,
     NickName,
     UserName,
     Sex,
     Birthday,
     Province,
     District,
     CurProvince,
     CurDistrict,
     MarriageState,
     JobTitle,
     WorkUnit,
     JobState,
     Interest,
     RecentGoals,
     Motto,
     SelfIntroduction,
     JoinTime,
     BlogUrl,
     FollowingCount,
     FollowerCount,
     AvatarUrl
 )
 VALUES
 (
     @UserId,
     @NickName,
     @UserName,
     @Sex,
     @Birthday,
     @Province,
     @District,
     @CurProvince,
     @CurDistrict,
     @MarriageState,
     @JobTitle,
     @WorkUnit,
     @JobState,
     @Interest,
     @RecentGoals,
     @Motto,
     @SelfIntroduction,
     @JoinTime,
     @BlogUrl,
     @FollowingCount,
     @FollowerCount,
     @AvatarUrl
 );";
                using (var conn = new SqlConnection(Options.ConnStr))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        AddParameters(cmd, "@UserId", userInfo.UserId);
                        AddParameters(cmd, "@NickName", userInfo.NickName);
                        AddParameters(cmd, "@UserName", userInfo.UserName);
                        AddParameters(cmd, "@Sex", userInfo.Sex);
                        AddParameters(cmd, "@Birthday", userInfo.Birthday);
                        AddParameters(cmd, "@Province", userInfo.Province);
                        AddParameters(cmd, "@District", userInfo.District);
                        AddParameters(cmd, "@CurProvince", userInfo.CurProvince);
                        AddParameters(cmd, "@CurDistrict", userInfo.CurDistrict);
                        AddParameters(cmd, "@MarriageState", userInfo.MarriageState);
                        AddParameters(cmd, "@JobTitle", userInfo.JobTitle);
                        AddParameters(cmd, "@WorkUnit", userInfo.WorkUnit);
                        AddParameters(cmd, "@JobState", userInfo.JobState);
                        AddParameters(cmd, "@Interest", userInfo.Interest);
                        AddParameters(cmd, "@RecentGoals", userInfo.RecentGoals);
                        AddParameters(cmd, "@Motto", userInfo.Motto);
                        AddParameters(cmd, "@SelfIntroduction", userInfo.SelfIntroduction);
                        AddParameters(cmd, "@JoinTime", userInfo.JoinTime);
                        AddParameters(cmd, "@BlogUrl", userInfo.BlogUrl);
                        AddParameters(cmd, "@FollowingCount", userInfo.FollowingCount);
                        AddParameters(cmd, "@FollowerCount", userInfo.FollowerCount);
                        AddParameters(cmd, "@AvatarUrl", userInfo.AvatarUrl);

                        try
                        {
                            cmd.ExecuteNonQuery();
                            Logger.Trace($"{userInfo.NickName}[{userInfo.UserId}] success.");
                        }
                        catch (Exception e)
                        {
                            Logger.Error(e.Message, e);
                        }
                    }
                }
            }
        }

        static void AddParameters(SqlCommand cmd, string parameterName, object value)
        {
            cmd.Parameters.AddWithValue(parameterName, value ?? DBNull.Value);
        }

        // 从用户关注的人/粉丝中获取种子
        public class AnalysisFollowPipeline : CrawlerPipeline<CnBlogsOptions>
        {
            private readonly Schedulers.IScheduler _requestItemScheduler;

            public AnalysisFollowPipeline(CnBlogsOptions options) : base(options)
            {
                Options.Scheduler =
                    Schedulers.SchedulerManager.GetScheduler<Schedulers.RedisScheduler<string>>("followFansScheduler");
                _requestItemScheduler =
                    Schedulers.SchedulerManager.GetScheduler<Schedulers.RedisScheduler<RequestItem>>(
                        "requestItemScheduler");
            }

            protected override Task<bool> ExecuteAsync(PipelineContext context)
            {
                if (context.Site != null)
                {
                    var userId = Guid.Parse(context.Site.Url);
                    Analysis(userId, true);
                    Analysis(userId, false);
                }
                return base.ExecuteAsync(context);
            }

            void Analysis(Guid userId, bool isFollowes)
            {
                var site = new Site(_relationUserApi)
                {
                    Cookie = Options.Cookie,
                    Postdata =
                        $"{{\"uid\":\"{userId}\",\"groupId\":\"00000000-0000-0000-0000-000000000000\",\"page\":1,\"isFollowes\":{isFollowes.ToString().ToLower()}}}",
                    Method = _method,
                    Accept = _accept,
                    ContentType = _contentType
                };

                var page = Options.Downloader.GetPage(site);
                if (page.HttpStatusCode == 200)
                {

                    var result = Newtonsoft.Json.JsonConvert.DeserializeObject<PagerObj>(page.HtmlSource);
                    if (!string.IsNullOrEmpty(result.Pager))
                    {
                        var doc = new HtmlDocument();
                        doc.LoadHtml(result.Pager);
                        var pageNode = doc.DocumentNode;
                        var aNodes = pageNode.SelectNodes("//a");

                        if (aNodes != null)
                        {
                            // 倒数第二个a标签为总页数。
                            var totalPageCountNode = aNodes[aNodes.Count - 2];
                            var totalPageCount = int.Parse(totalPageCountNode.InnerText);
                            for (var i = totalPageCount; i > 0; i--)
                            {
                                // 交给 CrawlerUserPipeline 去获取用户的粉丝列表数据。
                                _requestItemScheduler.Push(new RequestItem
                                {
                                    UserId = userId,
                                    Page = i,
                                    IsFollowes = isFollowes
                                });
                            }
                        }
                    }
                    else
                    {
                        _requestItemScheduler.Push(new RequestItem
                        {
                            UserId = userId,
                            Page = 1,
                            IsFollowes = false
                        });
                    }

                }
            }
        }

        public class CrawlerUserPipeline : CrawlerPipeline<CnBlogsOptions>
        {
            private readonly Schedulers.IScheduler _cnBlogsScheduler;

            public CrawlerUserPipeline(CnBlogsOptions options) : base(options)
            {
                Options.Scheduler =
                    Schedulers.SchedulerManager.GetScheduler<Schedulers.RedisScheduler<RequestItem>>(
                        "requestItemScheduler");
                _cnBlogsScheduler =
                    Schedulers.SchedulerManager.GetScheduler<Schedulers.RedisScheduler<Site>>("CnBlogs");
            }

            protected override Task<bool> ExecuteAsync(PipelineContext context)
            {
                if (context.Site != null)
                {
                    var page = Options.Downloader.GetPage(context.Site);
                    if (page.HttpStatusCode == 200)
                    {
                        var usersObj = Newtonsoft.Json.JsonConvert.DeserializeObject<UsersObj>(page.HtmlSource);
                        foreach (var user in usersObj.Users)
                        {
                            _cnBlogsScheduler.Push(new Site(_userInfoUrl + user.Alias));
                        }
                    }
                }
                return base.ExecuteAsync(context);
            }

            protected override Site OnParseSite(object item)
            {
                var reqItem = (RequestItem) item;
                var site = new Site(_relationUserApi)
                {
                    Cookie = Options.Cookie,
                    Postdata =
                        $"{{\"uid\":\"{reqItem.UserId}\",\"groupId\":\"00000000-0000-0000-0000-000000000000\",\"page\":{reqItem.Page},\"isFollowes\":{reqItem.IsFollowes.ToString().ToLower()}}}",
                    Method = _method,
                    Accept = _accept,
                    ContentType = _contentType
                };

                return site;
            }
        }

        public class CnBlogsOptions : PipelineOptions
        {
            public string Cookie { get; set; }
            public string ConnStr { get; set; }
        }

        // 用户基本信息实体。
        public class UserInfo
        {
            public Guid UserId { get; set; }
            public string NickName { get; set; }
            public string UserName { get; set; }
            public bool? Sex { get; set; }
            public DateTime? Birthday { get; set; }
            public string Province { get; set; } // 省份
            public string District { get; set; } // 市区
            public string CurProvince { get; set; }
            public string CurDistrict { get; set; }
            public string MarriageState { get; set; }
            public string JobTitle { get; set; }
            public string WorkUnit { get; set; }
            public string JobState { get; set; }
            public string Interest { get; set; } // 兴趣
            public string RecentGoals { get; set; }
            public string Motto { get; set; } // 座右铭
            public string SelfIntroduction { get; set; } // 自我介绍
            public DateTime? JoinTime { get; set; }
            public string BlogUrl { get; set; }
            public int FollowingCount { get; set; } // 关注数量
            public int FollowerCount { get; set; } // 粉丝数量
            public string AvatarUrl { get; set; } // 头像地址

        }

        public class UsersObj
        {
            public User[] Users { get; set; }
        }

        public class PagerObj
        {
            public string Pager { get; set; }
        }

        public class User
        {
            public string Alias { get; set; }
        }

        [Serializable]
        public class RequestItem : IIdentity
        {
            public Guid UserId { get; set; }
            public bool IsFollowes { get; set; }
            public int Page { get; set; }

            public string Name => UserId.ToString("N") + "_" + Page + "_" + IsFollowes;
        }
    }
}
