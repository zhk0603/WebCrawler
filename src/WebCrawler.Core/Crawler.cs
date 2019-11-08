using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebCrawler.Abstractions;

namespace WebCrawler.Core
{
    public class Crawler : ICrawler
    {
        public void Continue()
        {
            throw new NotImplementedException();
        }

        public void Exit()
        {
            throw new NotImplementedException();
        }

        public Task ExitAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public void Pause()
        {
            throw new NotImplementedException();
        }

        public void Run()
        {
            throw new NotImplementedException();
        }

        public Task RunAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
