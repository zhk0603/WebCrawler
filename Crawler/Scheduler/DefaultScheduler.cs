using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Crawler.Scheduler
{
    public class DefaultScheduler : IScheduler
    {
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
        private readonly List<Site> _siteStack = new List<Site>();

        public Site Pop()
        {
            try
            {
                _lock.EnterReadLock();

                if (_siteStack.Count == 0)
                {
                    return null;
                }

                var site = _siteStack.FirstOrDefault();
                _siteStack.RemoveAt(0);
                return site;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public void Push(Site requestSite)
        {
            try
            {
                _lock.EnterWriteLock();
                _siteStack.Add(requestSite);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
    }
}
