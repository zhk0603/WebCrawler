using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crawler
{
    public interface IUrlFilter
    {
        void Add(string url);
        bool Contains(string url);
    }
}
