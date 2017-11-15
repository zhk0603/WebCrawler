using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crawler.Scheduler
{
    public class SchedulerManager
    {
        private static readonly Dictionary<string, IScheduler> Schedules;
        private static readonly object Lock = new object();

        static SchedulerManager()
        {
            Schedules = new Dictionary<string, IScheduler>();
        }

        public static IScheduler GetScheduler(string key)
        {
            if (!Schedules.TryGetValue(key, out var scheduler))
            {
                lock (Lock)
                {
                    if (!Schedules.TryGetValue(key, out scheduler))
                    {
                        scheduler = new SiteScheduler();
                        Schedules.Add(key, scheduler);
                    }
                }
            }
            return scheduler;
        }
    }
}