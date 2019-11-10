using System.Collections.Generic;

namespace WebCrawler.Abstractions.Http
{
    public class DataContext
    {
        public Request Request { get; set; }
        public Response Response { get; set; }
        public IDictionary<object, object> Items { get; set; }
    }
}