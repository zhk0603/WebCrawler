using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using WebCrawler.Core.Hosting;

namespace WebCrawler.Simple
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureCrawler(configure =>
                {
                    configure.ConfigureServices(services =>
                    {
                        //services.add
                    });

                    configure.ConfigureUrl("");
                })
                .Build();

            await host.RunAsync();
        }
    }
}
