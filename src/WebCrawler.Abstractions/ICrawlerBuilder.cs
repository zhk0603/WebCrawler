using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;

namespace WebCrawler
{
    public interface ICrawlerBuilder
    {
        /// <summary>
        ///     构建一个 <see cref="ICrawler"/>。
        /// </summary>
        /// <returns></returns>
        ICrawler Build();

        /// <summary>
        ///     配置请求url.
        /// </summary>
        /// <param name="urls"></param>
        /// <returns></returns>
        ICrawlerBuilder ConfigureUrl(params string[] urls);

        /// <summary>
        ///     添加服务到容器。
        /// </summary>
        /// <param name="configureDelegate"></param>
        /// <returns></returns>
        ICrawlerBuilder ConfigureServices(Action<IServiceCollection> configureDelegate);

        ICrawlerBuilder Configure(Action<IServiceCollection> configureDelegate);
    }
}
