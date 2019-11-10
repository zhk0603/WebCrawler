using System;
using System.Collections.Generic;
using System.Text;
using WebCrawler.Abstractions.Http;

namespace WebCrawler.Abstractions
{
    public interface IDataProcessor
    {
        void Handle(DataContext dataContext);
    }
}
