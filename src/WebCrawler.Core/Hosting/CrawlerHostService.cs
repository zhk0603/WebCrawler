using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace WebCrawler.Core.Hosting
{
    public class CrawlerHostService : IHostedService
    {
        private readonly ICrawlerConfigure _crawlerConfigure;
        private ICrawler _crawler;

        public CrawlerHostService(ICrawlerConfigure crawlerConfigure)
        {
            _crawlerConfigure = crawlerConfigure;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
        }
    }
}