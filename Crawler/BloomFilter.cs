using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crawler
{
    public class BloomFilter : IUrlFilter
    {
        private Random _random;
        private int _bitSize, _numberOfHashes, _setSize, _setCount;
        private readonly BitArray _bitArray;

        #region ctor

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bitSize">布隆过滤器的大小(m</param>
        /// <param name="setSize">集合的大小 (n)</param>
        public BloomFilter(int bitSize, int setSize)
        {
            _bitSize = bitSize;
            _bitArray = new BitArray(bitSize);
            _setSize = setSize;
            _numberOfHashes = OptimalNumberOfHashes(_bitSize, _setSize);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bitSize">布隆过滤器的大小(m)</param>
        /// <param name="setSize">集合的大小 (n)</param>
        /// <param name="numberOfHashes">hash散列函数的数量(k)</param>
        public BloomFilter(int bitSize, int setSize, int numberOfHashes)
        {
            _bitSize = bitSize;
            _bitArray = new BitArray(bitSize);
            _setSize = setSize;
            _numberOfHashes = numberOfHashes;
        }

        #endregion

        #region Properties

        public int NumberOfHashes
        {
            set { _numberOfHashes = value; }
            get { return _numberOfHashes; }
        }

        public int SetSize
        {
            set { _setSize = value; }
            get { return _setSize; }
        }

        public int BitSize
        {
            set { _bitSize = value; }
            get { return _bitSize; }
        }

        #endregion

        #region public member

        public void Add(string url)
        {
            _random = new Random(Hash(url));

            for (int i = 0; i < _numberOfHashes; i++)
                _bitArray[_random.Next(_bitSize)] = true;
            _setCount++;
        }

        public bool Contains(string url)
        {
            _random = new Random(Hash(url));

            for (int i = 0; i < _numberOfHashes; i++)
            {
                if (!_bitArray[_random.Next(_bitSize)])
                    return false;
            }

            return true;
        }

        public int Count => _setCount;

        public double FalsePositiveProbability()
        {
            return Math.Pow((1 - Math.Exp(-_numberOfHashes * _setSize / (double) _bitSize)), _numberOfHashes);
        }

        #endregion

        private int Hash(string item)
        {
            return item.GetHashCode();
        }

        private int OptimalNumberOfHashes(int bitSize, int setSize)
        {
            return (int) Math.Ceiling((bitSize / setSize) * Math.Log(2.0));
        }
    }
}
