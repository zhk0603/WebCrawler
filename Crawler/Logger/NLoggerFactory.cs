using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog.Config;

namespace Crawler.Logger
{
    public class NLoggerFactory : ILoggerFactory
    {
        public NLoggerFactory(string configFileName)
        {
            var fileName = System.IO.Path.Combine(Environment.CurrentDirectory, configFileName);
            NLog.LogManager.Configuration = new XmlLoggingConfiguration(fileName);
        }

        public NLoggerFactory() : this("NLog.config")
        {

        }

        public ILogger Create<T>()
        {
            return new NLogger(NLog.LogManager.GetLogger(typeof(T).FullName));
        }

        public ILogger Create(Type type)
        {
            return new NLogger(NLog.LogManager.GetLogger(type.FullName));
        }

        public ILogger Create(string name)
        {
            return new NLogger(NLog.LogManager.GetLogger(name));
        }
    }
}