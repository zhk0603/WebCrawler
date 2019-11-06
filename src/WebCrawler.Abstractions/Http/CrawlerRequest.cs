using System;
using System.Net;
using HtmlAgilityPack;

namespace WebCrawler.Abstractions.Http
{
    public class CrawlerRequest
    {
        /// <summary>
        ///     请求URL必须填写
        /// </summary>
        public string Url { get; set; }
    }
}
