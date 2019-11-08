using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WebCrawler
{
    public interface ICrawler
    {
        void Pause();
        void Continue();
        void Exit();
        Task ExitAsync(CancellationToken cancellationToken = default);

        void Run();
        Task RunAsync(CancellationToken cancellationToken = default);
    }
}