using System;
using System.IO;
using System.Net;

namespace WebCrawler.Abstractions.Http
{
    public class Request
    {
        /// <summary>
        ///     请求URL必须填写
        /// </summary>
        public string Url { get; set; }
        public DataContext DataContext { get; set; }
        public Stream Body { get; set; }

        // cookie header ……
    }
}
