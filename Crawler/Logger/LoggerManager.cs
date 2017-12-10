using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crawler.Logger
{
    public static class LoggerManager
    {
        private static ILoggerFactory _loggerFactory;

        public static ILoggerFactory CurrentFactory => _loggerFactory ?? (_loggerFactory = new NLoggerFactory());

        public static void SetLogFactory(ILoggerFactory factory)
        {
            _loggerFactory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        public static ILogger GetLogger(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            return CurrentFactory.Create(name);
        }

        public static ILogger GetLogger(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }
            return CurrentFactory.Create(type);
        }

        public static ILogger GetLogger<T>()
        {
            return CurrentFactory.Create<T>();
        }
    }
}
