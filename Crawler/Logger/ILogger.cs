using System;

namespace Crawler.Logger
{
    public interface ILogger
    {
        void Write(string message, Exception exception, LogLevel logLevel);
    }
}