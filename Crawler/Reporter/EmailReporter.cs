using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Crawler;

namespace Crawler.Reporter
{
    public class EmailReporter : IReporter
    {
        public EmailReporter()
        {
        }

        public int ReportStatusInterval { get; set; }
        public void ReportStatus()
        {
            throw new NotImplementedException();
        }
    }
}
