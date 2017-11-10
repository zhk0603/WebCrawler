using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Crawler.Logger;
using Crawler.Pipeline;

namespace Crawler
{
    public class Crawler
    {
        public CrawlerState CrawlerState { get; private set; }
        public ILogger Logger { get; }

        public List<IPipeline> PipelineCollection { get; set; }

        public void Run()
        {
            throw new NotImplementedException();
        }

        public static Crawler Create()
        {
            return new Crawler();
        }
    }
}
