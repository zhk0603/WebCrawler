using System;
using System.Collections.Generic;
using System.Text;

namespace WebCrawler.Abstractions
{
    public interface ICrawlerBuilder
    {
        ICrawler Build();
    }
}
