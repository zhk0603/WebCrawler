using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace WebCrawler.Core.Hosting
{
    public static class GenericHostBuilderExtensions
    {
        public static IHostBuilder ConfigureCrawler(this IHostBuilder builder, Action<CrawlerConfigure> configure)
        {
            var crawlerBuilder = new CrawlerConfigure(builder);
            configure(crawlerBuilder);
            builder.ConfigureServices((context, services) =>
            {
                services.AddHostedService<CrawlerHostService>();
            });
            return builder;
        }
    }
}
