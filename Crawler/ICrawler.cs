using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crawler
{
    public interface ICrawler
    {
        void Pause();
        void Continue();
        void Exit();
        void Run();
        Task RunAsync();
    }
}
