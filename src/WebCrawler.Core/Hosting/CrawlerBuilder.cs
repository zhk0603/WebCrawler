using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace WebCrawler.Core.Hosting
{
    public class CrawlerBuilder : ICrawlerBuilder
    {
        private readonly IHostBuilder _builder;
        private readonly IConfiguration _config;

        public CrawlerBuilder(IHostBuilder builder)
        {
            _builder = builder;

            _config = new ConfigurationBuilder()
                .Build();

            _builder.ConfigureHostConfiguration(config => { config.AddConfiguration(_config); });

            _builder.ConfigureServices((builderContext, services) =>
            {
                // 注册爬虫依赖服务
            });
        }

        public ICrawler Build()
        {
            throw new NotImplementedException();
        }

        public ICrawlerBuilder ConfigureUrl(params string[] urls)
        {
            throw new NotImplementedException();
        }

        public ICrawlerBuilder ConfigureAppConfiguration(Action<CrawlerBuilderContext, IConfigurationBuilder> configureDelegate)
        {
            throw new NotImplementedException();
        }

        public ICrawlerBuilder ConfigureServices(Action<IServiceCollection> configureDelegate)
        {
            _builder.ConfigureServices(configureDelegate);
            return this;
        }

        public ICrawlerBuilder ConfigureServices(Action<CrawlerBuilderContext, IServiceCollection> configureServices)
        {
            throw new NotImplementedException();
        }

        public ICrawlerBuilder Configure(Action<IServiceCollection> configureDelegate)
        {
            throw new NotImplementedException();
        }
    }
}
