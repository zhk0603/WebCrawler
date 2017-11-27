using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace Crawler
{
    public class RedisBloomFilter : IUrlFilter
    {
        private static readonly string RedisFilterKey = "Crawler.RedisFilter";
        private readonly IDatabase _database;
        private int _hashFunctionCount = 7;

        public RedisBloomFilter(string connectionString)
        {
            _database = ConnectionMultiplexer.Connect(connectionString).GetDatabase();
        }

        public void Add(string url)
        {
            int primaryHash = url.GetHashCode();
            int secondaryHash = HashString(url);

            for (int i = 0; i < _hashFunctionCount; i++)
            {
                int hash = ComputeHash(primaryHash, secondaryHash, i);
                _database.StringSetBit(RedisFilterKey, hash, true);
            }
        }

        public bool Contains(string url)
        {
            int primaryHash = url.GetHashCode();
            int secondaryHash = HashString(url);
            for (int i = 0; i < _hashFunctionCount; i++)
            {
                int hash = ComputeHash(primaryHash, secondaryHash, i);
                if (_database.StringGetBit(RedisFilterKey, hash) == false)
                    return false;
            }
            return true;
        }

        private int ComputeHash(int primaryHash, int secondaryHash, int i)
        {
            int resultingHash = (primaryHash + (i * secondaryHash)) % int.MaxValue;
            return Math.Abs(resultingHash);
        }

        private static int HashString(string input)
        {
            string s = input;
            int hash = 0;

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
