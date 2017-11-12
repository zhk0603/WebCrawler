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
            // 进入可升级读模式，因为在获取一个 site 后需要从 list 中移除。
            _lock.EnterUpgradeableReadLock();
            try
            {
                return Pop();
            }
            finally
            {
                _lock.ExitUpgradeableReadLock();
            }
        }

        void IScheduler.Push(object @object)
        {
            _lock.EnterWriteLock();
            try
            {
                if (@object is Site requestSite)
                    Push(requestSite);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public virtual Site Pop()
        {
            if (_siteStack.Count == 0)
                return null;

            var site = _siteStack.FirstOrDefault();

            _lock.EnterWriteLock();
            try
            {
                _siteStack.RemoveAt(0);
            }
            finally
            {
                _lock.ExitWriteLock();
            }

            return site;
        }

        public virtual void Push(Site requestSite)
        {
            _siteStack.Add(requestSite);
        }

        ~SiteScheduler()
        {
            _lock?.Dispose();
        }
    }
}