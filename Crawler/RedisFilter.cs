using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace Crawler
{
    public class RedisFilter : IUrlFilter
    {
        private ConnectionMultiplexer _connectionMultiplexer;
        private IDatabase _database;
        public RedisFilter(string connectionString)
        {
            _connectionMultiplexer = ConnectionMultiplexer.Connect(connectionString);
            _database = _connectionMultiplexer.GetDatabase();
        }

        public int Count => throw new NotImplementedException();

        public void Add(string url)
        {
            throw new NotImplementedException();
        }

        public bool Contains(string url)
        {
            _database.HashExists(url.GetHashCode().ToString(), null);
            throw new NotImplementedException();
        }
    }
}
