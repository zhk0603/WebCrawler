using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crawler.Scheduler
{
    public class SchedulerFactory
    {


        public IScheduler Create()
        {
            return new SiteScheduler();
        }
    }
}