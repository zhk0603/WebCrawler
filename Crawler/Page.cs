using System;
using System.Net;
using HtmlAgilityPack;

namespace Crawler
{
    public class Page
    {
        public Uri Uri { get; set; }
        public string Cookie { get; set; }
        public CookieCollection CookieCollection { get; set; }
        public byte[] ResultByte { get; set; }
        public string HtmlSource { get; set; }
        public WebHeaderCollection Header { get; set; }
        public HtmlNode DocumentNode { get; set; }
        public int HttpStatusCode { get; set; }
        public string StatusDescription { get; set; }
    }
}