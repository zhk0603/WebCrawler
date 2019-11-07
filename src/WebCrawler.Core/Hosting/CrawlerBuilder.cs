using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace WebCrawler.Core.Hosting
{
    public class CrawlerBuilder : ICrawlerBuilder
    {
        private readonly IHostBuilder _builder;

        public CrawlerBuilder(IHostBuilder builder)
        {
            _builder = builder;
        }

        public ICrawler Build()
        {
            throw new NotImplementedException();
        }

        public ICrawlerBuilder ConfigureUrl(params string[] urls)
        {
            throw new NotImplementedException();
        }

        public ICrawlerBuilder ConfigureServices(Action<IServiceCollection> configureDelegate)
        {
            throw new NotImplementedException();
        }

        public ICrawlerBuilder Configure(Action<IServiceCollection> configureDelegate)
        {
            throw new NotImplementedException();
        }
    }
}
