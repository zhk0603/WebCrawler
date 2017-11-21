using System;
using System.Collections.Generic;

namespace Crawler.Schedulers
{
    public class SchedulerManager
    {
        private static readonly Dictionary<string, Dictionary<string, IScheduler>> ScheduleContainer;
        private static readonly object Lock = new object();

        static SchedulerManager()
        {
            ScheduleContainer = new Dictionary<string, Dictionary<string, IScheduler>>();
        }

        public static IScheduler GetSiteScheduler(string key)
        {
            return InternalGetScheduler(typeof(SiteScheduler), key);
        }

        public static IScheduler GetScheduler<TScheduler>(string key)
            where TScheduler : IScheduler
        {
            return InternalGetScheduler(typeof(TScheduler), key);
        }

        internal static IScheduler InternalGetScheduler(Type schedulerType, string key)
        {
            if (schedulerType == null)
            {
                throw new ArgumentNullException(nameof(schedulerType));
            }

            if (!typeof(IScheduler).IsAssignableFrom(schedulerType))
            {
                throw new ArgumentException($"类型：{schedulerType.FullName}，没实现 IScheduler 接口。");
            }

            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (!ScheduleContainer.TryGetValue(schedulerType.FullName, out var schedules))
            {
                lock (Lock)
                {
                    if (!ScheduleContainer.TryGetValue(schedulerType.FullName, out schedules))
                    {
                        schedules = new Dictionary<string, IScheduler>();
                        ScheduleContainer.Add(schedulerType.FullName, schedules);
                    }
                }
            }

            if (!schedules.TryGetValue(key, out var scheduler))
            {
                lock (Lock)
                {
                    if (!schedules.TryGetValue(key, out scheduler))
                    {
                        scheduler = (IScheduler) Activator.CreateInstance(schedulerType);
                        schedules.Add(key, scheduler);
                    }
                }
            }
            return scheduler;
        }

    }
}