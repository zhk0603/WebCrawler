using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crawler
{
    public interface IReporter
    {
        int ReportStatusInterval { get; set; }
        void ReportStatus();
    }
}
