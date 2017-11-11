using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Crawler.Scheduler
{
    public class SiteScheduler : IScheduler
    {
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
        private readonly List<Site> _siteStack = new List<Site>();

        object IScheduler.Pop()
        {
            return Pop();
        }

        void IScheduler.Push(object @object)
        {
            if (@object is Site requestSite)
                Push(requestSite);
        }

        public virtual Site Pop()
        {
            try
            {
                _lock.EnterReadLock();

                if (_siteStack.Count == 0)
                    return null;

                var site = _siteStack.FirstOrDefault();
                _siteStack.RemoveAt(0);
                return site;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public virtual void Push(Site requestSite)
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