using System;

namespace Crawler.Logger
{
    public class NLogger : ILogger
    {
        private readonly NLog.ILogger _logger;

        public NLogger(NLog.ILogger logger)
        {
            _logger = logger;
        }

        public bool IsTraceEnabled => _logger.IsTraceEnabled;
        public bool IsDebugEnabled => _logger.IsDebugEnabled;
        public bool IsInfoEnabled => _logger.IsInfoEnabled;
        public bool IsWarnEnabled => _logger.IsWarnEnabled;
        public bool IsErrorEnabled => _logger.IsErrorEnabled;
        public bool IsFatalEnabled { get; }

        protected virtual void Write(string message, Exception exception, LogLevel logLevel)
        {
            switch (logLevel)
            {
                case LogLevel.Trace:
                    _logger.Trace(exception, message);
                    break;
                case LogLevel.Debug:
                    _logger.Debug(exception, message);
                    break;
                case LogLevel.Info:
                    _logger.Info(exception, message);
                    break;
                case LogLevel.Warn:
                    _logger.Warn(exception, message);
                    break;
                case LogLevel.Error:
                    _logger.Error(exception, message);
                    break;
                case LogLevel.Fatal:
                    _logger.Fatal(exception, message);
                    break;
            }
        }

        public void Trace(string message)
        {
            Write(message, null, LogLevel.Trace);
        }

        public void Trace(Exception exception)
        {
            Write(null, exception, LogLevel.Trace);
        }

        public void Trace(string message, Exception exception)
        {
            Write(message, exception, LogLevel.Trace);
        }

        public void Debug(string message)
        {
            Write(message, null, LogLevel.Debug);
        }

        public void Debug(Exception exception)
        {
            Write(null, exception, LogLevel.Debug);
        }

        public void Debug(string message, Exception exception)
        {
            Write(message, exception, LogLevel.Debug);
        }

        public void Info(string message)
        {
            Write(message, null, LogLevel.Info);
        }

        public void Info(Exception exception)
        {
            Write(null, exception, LogLevel.Info);
        }

        public void Info(string message, Exception exception)
        {
            Write(message, exception, LogLevel.Info);
        }

        public void Warn(string message)
        {
            Write(message, null, LogLevel.Warn);
        }

        public void Warn(Exception exception)
        {
            Write(null, exception, LogLevel.Warn);
        }

        public void Warn(string message, Exception exception)
        {
            Write(message, exception, LogLevel.Warn);
        }

        public void Error(string message)
        {
            Write(message, null, LogLevel.Error);
        }

        public void Error(Exception exception)
        {
            Write(null, exception, LogLevel.Error);
        }

        public void Error(string message, Exception exception)
        {
            Write(message, exception, LogLevel.Error);
        }

        public void Fatal(string message)
        {
            Write(message, null, LogLevel.Fatal);
        }

        public void Fatal(Exception exception)
        {
            Write(null, exception, LogLevel.Fatal);
        }

        public void Fatal(string message, Exception exception)
        {
            Write(message, exception, LogLevel.Fatal);
        }
    }
}