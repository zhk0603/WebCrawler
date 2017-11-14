using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crawler.Logger
{
    public interface ILoggerFactory
    {
        ILogger Create<T>();
        ILogger Create(Type type);
        ILogger Create(string name);
    }
}
