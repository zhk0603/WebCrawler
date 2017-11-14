using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crawler.Logger
{
    public static class LogManager
    {
        private static ILoggerFactory _loggerFactory;

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

            return _loggerFactory.Create(name);
        }

        public static ILogger GetLogger(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }
            return _loggerFactory.Create(type);
        }

        public static ILogger GetLogger<T>()
        {
            return _loggerFactory.Create<T>();
        }
    }
}
