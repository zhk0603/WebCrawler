using System;
using System.Linq;
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

        public static List<Dictionary<string, IScheduler>> GetAllScheduler()
        {
            return ScheduleContainer.Select(x => x.Value).ToList();
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
                        scheduler = RedisSchedulerFactory(schedulerType, key) ??
                                    (IScheduler) Activator.CreateInstance(schedulerType);
                        schedules.Add(key, scheduler);
                    }
                }
            }
            return scheduler;
        }

        private static IScheduler RedisSchedulerFactory(Type schedulerType, string key)
        {
            if (schedulerType.IsGenericType &&
                schedulerType.GetGenericTypeDefinition() == typeof(RedisScheduler<>))
            {
                var constructors = schedulerType.GetConstructors();

                var connectionString = System.Configuration.ConfigurationManager.AppSettings["redisConnectionString"];
                var schedulerKey = (schedulerType.FullName + key).GetHashCode().ToString();
                foreach (var constructor in constructors)
                {
                    var parameters = constructor.GetParameters();
                    var parameterTypes = parameters.Select(p => p.ParameterType).ToArray();
                    if (parameterTypes.Length != 2) continue;
                    if (parameterTypes.Any(x => x != typeof(string))) continue;
                    return (IScheduler) constructor.Invoke(new object[] {schedulerKey, connectionString});
                }
            }
            return null;
        }
    }
}