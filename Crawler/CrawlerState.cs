using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crawler
{
    public enum CrawlerState
    {
        Init,
        Running,
        Stopped,
        Finished,
        Exited
    }
}
