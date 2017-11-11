using System;

namespace Crawler.Logger
{
    public class SimpleLogger : ILogger
    {
        public void Write(string message, Exception exception, LogLevel logLevel)
        {
            Console.WriteLine(message);
        }
    }
}