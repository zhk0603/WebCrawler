using System;

namespace Crawler.Logger
{
    public interface ILogger
    {
        bool IsTraceEnabled { get; }
        bool IsDebugEnabled { get; }
        bool IsInfoEnabled { get; }
        bool IsWarnEnabled { get; }
        bool IsErrorEnabled { get; }

        void Trace(string message);
        void Trace(Exception exception);
        void Trace(string message, Exception exception);
        void Debug(string message);
        void Debug(Exception exception);
        void Debug(string message, Exception exception);
        void Info(string message);
        void Info(Exception exception);
        void Info(string message, Exception exception);
        void Warn(string message);
        void Warn(Exception exception);
        void Warn(string message, Exception exception);
        void Error(string message);
        void Error(Exception exception);
        void Error(string message, Exception exception);
    }
}