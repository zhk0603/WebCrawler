using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace WebCrawler.Abstractions.Http
{
    public class CrawlerResponse
    {
        public Uri Uri { get; set; }
        public string Cookie { get; set; }
        public CookieCollection CookieCollection { get; set; }
        public byte[] ResultByte { get; set; }
        public string HtmlSource { get; set; }
        public WebHeaderCollection Header { get; set; }
        public int HttpStatusCode { get; set; }
        public string StatusDescription { get; set; }
    }
}
