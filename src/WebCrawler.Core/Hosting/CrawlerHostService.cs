using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace WebCrawler.Core.Hosting
{
    public class CrawlerHostService : IHostedService
    {
        private readonly ICrawlerBuilder _builder;
        private ICrawler _crawler;

        public CrawlerHostService(ICrawlerBuilder builder)
        {
            _builder = builder;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _crawler = _builder.Build();

            await _crawler.RunAsync(cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _crawler.ExitAsync(cancellationToken);
        }
    }
}