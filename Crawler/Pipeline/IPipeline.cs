using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crawler.Pipeline
{
    public interface IPipeline
    {
        void Execute();
    }
}
