using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
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
        public HtmlNode HtmlNode { get; set; }
        public int HttpStatusCode { get; set; }
        public string StatusDescription { get; set; }
    }
}
