using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Crawler.Filter;
using StackExchange.Redis;

namespace Crawler.Schedulers
{
    public class RedisScheduler<T> : IScheduler
    {
        private readonly string _redisSchedulerKey;
        private readonly IUrlFilter _urlFilter;
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
        private readonly IDatabase _database;
        private long _totalCount;

        public RedisScheduler(string name, string connectionString)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

            _redisSchedulerKey = $"Crawler.Schedulers.RedisScheduler.{name}";
            _database = ConnectionMultiplexer.Connect(connectionString).GetDatabase();
            _urlFilter = UrlFilterManager.Current;
        }

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
            _lock.EnterWriteLock();
            try
            {
                var item = _database.ListRightPop(_redisSchedulerKey);
                if (TryDeserialize(item, out var value))
                {
                    return value;
                }
                return default(T);
            }
            catch(Exception e)
            {
                return default(T);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
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

        public virtual void Push(T requestSite)
        {
            if (_urlFilter == null || !_urlFilter.Contains(_redisSchedulerKey + requestSite))
            {
                _database.ListLeftPush(_redisSchedulerKey, Serialize(requestSite));
                _urlFilter?.Add(_redisSchedulerKey + requestSite);
                _totalCount++;
            }
        }

        public long Count => _database.ListLength(_redisSchedulerKey);

        public long TotalCount => _totalCount;

        private byte[] Serialize(object obj)
        {
            if (obj == null)
            {
                return null;
            }
            var binaryFormatter = new BinaryFormatter();
            using (var memoryStream = new MemoryStream())
            {
                binaryFormatter.Serialize(memoryStream, obj);
                var data = memoryStream.ToArray();
                return data;
            }
        }

        private bool TryDeserialize(byte[] data, out T @object)
        {
            @object = default(T);
            if (data == null)
            {
                return false;
            }
            try
            {
                var binaryFormatter = new BinaryFormatter();
                using (var memoryStream = new MemoryStream(data))
                {
                    var result = binaryFormatter.Deserialize(memoryStream);
                    @object = (T) result;
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}