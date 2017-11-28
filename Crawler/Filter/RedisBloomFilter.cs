using System;
using StackExchange.Redis;

namespace Crawler.Filter
{
    public class RedisBloomFilter : IUrlFilter
    {
        private static readonly string RedisFilterKey = "Crawler.RedisFilter";
        private readonly IDatabase _database;
        private readonly int _hashFunctionCount;

        public RedisBloomFilter() : this(System.Configuration.ConfigurationManager.AppSettings["redisConnectionString"], 7)
        {
        }

        public RedisBloomFilter(string connectionString, int hashFunctionCount)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

            _database = ConnectionMultiplexer.Connect(connectionString).GetDatabase();
            _hashFunctionCount = hashFunctionCount;
        }

        public void Add(string url)
        {
            var primaryHash = url.GetHashCode();
            var secondaryHash = HashString(url);

            for (int i = 0; i < _hashFunctionCount; i++)
            {
                var hash = ComputeHash(primaryHash, secondaryHash, i);
                _database.StringSetBit(RedisFilterKey, hash, true);
            }
        }

        public bool Contains(string url)
        {
            int primaryHash = url.GetHashCode();
            var secondaryHash = HashString(url);
            for (int i = 0; i < _hashFunctionCount; i++)
            {
                var hash = ComputeHash(primaryHash, secondaryHash, i);
                if (_database.StringGetBit(RedisFilterKey, hash) == false)
                    return false;
            }
            return true;
        }

        private long ComputeHash(long primaryHash, long secondaryHash, int i)
        {
            var resultingHash = (primaryHash + (i * secondaryHash)) % int.MaxValue;
            return Math.Abs(resultingHash);
        }

        private static long HashString(string input)
        {
            string s = input;
            long hash = 0;

            for (int i = 0; i < s.Length; i++)
            {
                hash += s[i];
                hash += (hash << 10);
                hash ^= (hash >> 6);
            }
            hash += (hash << 3);
            hash ^= (hash >> 11);
            hash += (hash << 15);
            return hash;
        }
    }
}
