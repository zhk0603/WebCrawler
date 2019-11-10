using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace WebCrawler.Core.Hosting
{
    public class CrawlerConfigure : ICrawlerConfigure
    {
        private readonly IHostBuilder _builder;
        private readonly IConfiguration _config;
        private readonly List<string> _urls;

        public CrawlerConfigure(IHostBuilder builder)
        {
            _builder = builder;

            _urls = new List<string>();

            _config = new ConfigurationBuilder()
                .AddEnvironmentVariables(prefix: "CRAWLER_")
                .Build();

            builder.Properties.Add("crawler:CrawlerConfigure", this);

            _builder.ConfigureHostConfiguration(config =>
            {
                config.AddConfiguration(_config);
            });

            _builder.ConfigureServices((builderContext, services) =>
            {
                // 注册爬虫依赖服务
                services.AddSingleton(this);
            });
        }

        public ICrawlerConfigure ConfigureUrl(params string[] urls)
        {
            _urls.AddRange(urls);
            return this;
        }

        public ICrawlerConfigure ConfigureAppConfiguration(Action<CrawlerBuilderContext, IConfigurationBuilder> configureDelegate)
        {
            _builder.ConfigureAppConfiguration((context, builder) =>
            {
                var crawlerBuilderContext = GetCrawlerBuilderContext(context);
                configureDelegate(crawlerBuilderContext, builder);
            });

            return this;
        }

        public ICrawlerConfigure ConfigureServices(Action<IServiceCollection> configureDelegate)
        {
            _builder.ConfigureServices(configureDelegate);
            return this;
        }

        public ICrawlerConfigure ConfigureServices(Action<CrawlerBuilderContext, IServiceCollection> configureServices)
        {
            _builder.ConfigureServices((context, builder) =>
            {
                var crawlerBuilderContext = GetCrawlerBuilderContext(context);
                configureServices(crawlerBuilderContext, builder);
            });

            return this;
        }

        public ICrawlerBuilder Configure(Action<IServiceCollection> configureDelegate)
        {
            throw new NotImplementedException();
        }

        private CrawlerBuilderContext GetCrawlerBuilderContext(HostBuilderContext context)
        {
            if (!context.Properties.TryGetValue(typeof(CrawlerBuilderContext), out var contextVal))
            {
                var webHostBuilderContext = new CrawlerBuilderContext
                {
                    Configuration = context.Configuration,
                    HostingEnvironment = context.HostingEnvironment,
                };
                context.Properties[typeof(CrawlerBuilderContext)] = webHostBuilderContext;
                return webHostBuilderContext;
            }

            var webHostContext = (CrawlerBuilderContext)contextVal;
            webHostContext.Configuration = context.Configuration;
            return webHostContext;
        }
    }
}
