using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crawler.Logger
{
    public enum LogLevel
    {
        /// <summary>
        ///     表示调试的日志级别
        /// </summary>
        Debug = 1,

        /// <summary>
        ///     表示消息的日志级别
        /// </summary>
        Info = 2,

        /// <summary>
        ///     表示警告的日志级别
        /// </summary>
        Warn = 3,

        /// <summary>
        ///     表示错误的日志级别
        /// </summary>
        Error = 4,
    }
}
