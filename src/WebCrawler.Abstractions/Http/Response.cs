using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace WebCrawler.Abstractions.Http
{
    public class Response
    {
        public Uri Uri { get; set; }
        public CookieCollection CookieCollection { get; set; }
        public WebHeaderCollection Header { get; set; }
        public int StatusCode { get; set; }
        public string StatusDescription { get; set; }
        public DataContext DataContext { get; set; }
        public  Stream Body { get; set; }
        public long? ContentLength { get; set; }
        public string ContentType { get; set; }
    }
}
