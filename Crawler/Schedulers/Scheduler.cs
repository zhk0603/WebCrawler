using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Crawler.Filter;

namespace Crawler.Schedulers
{
    public class Scheduler<T> : IScheduler
    {
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
        private readonly List<T> _stack = new List<T>();
        private long _totalCount = 0;

        object IScheduler.Pop()
        {
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

        public virtual T Pop()
        {
            if (_stack.Count == 0)
                return default(T);

            var site = _stack.FirstOrDefault();

            _lock.EnterWriteLock();
            try
            {
                _stack.RemoveAt(0);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
            return site;
        }

        void IScheduler.Push(object @object)
        {
            _lock.EnterWriteLock();
            try
            {
                if (@object is T requestObject)
                    Push(requestObject);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public long Count
        {
            get
            {
                try
                {
                    _lock.EnterReadLock();
                    return _stack.Count;
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
        }

        public long TotalCount => _totalCount;

        public virtual void Push(T requestSite)
        {
            if (UrlFilterManager.Current != null)
            {
                var identity = requestSite.ToString();
                if (requestSite is IIdentity requestIdentity)
                {
                    identity = requestIdentity.Name;
                }

                if (UrlFilterManager.Current.Contains(identity))
                {
                    return;
                }

                UrlFilterManager.Current.Add(identity);
            }
            _stack.Add(requestSite);
            _totalCount++;
        }

        ~Scheduler()
        {
            _lock?.Dispose();
        }
    }
}
