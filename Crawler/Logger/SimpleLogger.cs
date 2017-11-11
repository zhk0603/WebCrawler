using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crawler.Logger
{
    public class SimpleLogger : ILogger
    {
        public void Write(string message)
        {
            System.Console.WriteLine(message);
        }
    }
}
