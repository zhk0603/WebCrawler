using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace WebCrawler.Core.Hosting
{
    public interface ICrawlerConfigure
    {
        ICrawlerConfigure ConfigureUrl(params string[] urls);
        ICrawlerConfigure ConfigureAppConfiguration(Action<CrawlerBuilderContext, IConfigurationBuilder> configureDelegate);
        ICrawlerConfigure ConfigureServices(Action<IServiceCollection> configureDelegate);
        ICrawlerConfigure ConfigureServices(Action<CrawlerBuilderContext, IServiceCollection> configureServices);
        ICrawlerBuilder Configure(Action<IServiceCollection> configureDelegate);
    }
}