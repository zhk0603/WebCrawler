using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WebCrawler
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
