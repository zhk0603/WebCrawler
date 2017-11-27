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
        public delegate int HashFunction(string item);

        private readonly int _hashFunctionCount;
        private readonly BitArray _bitArray;
        private readonly HashFunction _getHashSecondary;

        #region ctor

        /// <summary>
        /// Creates a new Bloom filter, specifying an error rate of 1/capacity, using the optimal size for the underlying data structure based on the desired capacity and error rate, as well as the optimal number of hash functions.
        /// A secondary hash function will be provided for you if your type T is either string or int. Otherwise an exception will be thrown. If you are not using these types please use the overload that supports custom hash functions.
        /// </summary>
        /// <param name="capacity">The anticipated number of items to be added to the filter. More than this number of items can be added, but the error rate will exceed what is expected.</param>
        public BloomFilter(int capacity) : this(capacity, null)
        {
        }

        /// <summary>
        /// Creates a new Bloom filter, using the optimal size for the underlying data structure based on the desired capacity and error rate, as well as the optimal number of hash functions.
        /// A secondary hash function will be provided for you if your type T is either string or int. Otherwise an exception will be thrown. If you are not using these types please use the overload that supports custom hash functions.
        /// </summary>
        /// <param name="capacity">The anticipated number of items to be added to the filter. More than this number of items can be added, but the error rate will exceed what is expected.</param>
        /// <param name="errorRate">The accepable false-positive rate (e.g., 0.01F = 1%)</param>
        public BloomFilter(int capacity, float errorRate) : this(capacity, errorRate, null)
        {
        }

        /// <summary>
        /// Creates a new Bloom filter, specifying an error rate of 1/capacity, using the optimal size for the underlying data structure based on the desired capacity and error rate, as well as the optimal number of hash functions.
        /// </summary>
        /// <param name="capacity">The anticipated number of items to be added to the filter. More than this number of items can be added, but the error rate will exceed what is expected.</param>
        /// <param name="hashFunction">The function to hash the input values. Do not use GetHashCode(). If it is null, and T is string or int a hash function will be provided for you.</param>
        public BloomFilter(int capacity, HashFunction hashFunction) : this(capacity, BestErrorRate(capacity),
            hashFunction)
        {
        }

        /// <summary>
        /// Creates a new Bloom filter, using the optimal size for the underlying data structure based on the desired capacity and error rate, as well as the optimal number of hash functions.
        /// </summary>
        /// <param name="capacity">The anticipated number of items to be added to the filter. More than this number of items can be added, but the error rate will exceed what is expected.</param>
        /// <param name="errorRate">The accepable false-positive rate (e.g., 0.01F = 1%)</param>
        /// <param name="hashFunction">The function to hash the input values. Do not use GetHashCode(). If it is null, and T is string or int a hash function will be provided for you.</param>
        public BloomFilter(int capacity, float errorRate, HashFunction hashFunction) : this(capacity, errorRate,
            hashFunction, BestM(capacity, errorRate), BestK(capacity, errorRate))
        {
        }

        /// <summary>
        /// Creates a new Bloom filter.
        /// </summary>
        /// <param name="capacity">The anticipated number of items to be added to the filter. More than this number of items can be added, but the error rate will exceed what is expected.</param>
        /// <param name="errorRate">The accepable false-positive rate (e.g., 0.01F = 1%)</param>
        /// <param name="hashFunction">The function to hash the input values. Do not use GetHashCode(). If it is null, and T is string or int a hash function will be provided for you.</param>
        /// <param name="m">The number of elements in the BitArray.</param>
        /// <param name="k">The number of hash functions to use.</param>
        public BloomFilter(int capacity, float errorRate, HashFunction hashFunction, int m, int k)
        {
            // validate the params are in range
            if (capacity < 1)
                throw new ArgumentOutOfRangeException(nameof(capacity), capacity, "capacity must be > 0");
            if (errorRate >= 1 || errorRate <= 0)
                throw new ArgumentOutOfRangeException(nameof(errorRate), errorRate,
                    $"errorRate must be between 0 and 1, exclusive. Was {errorRate}");
            if (m < 1) // from overflow in bestM calculation
                throw new ArgumentOutOfRangeException(
                    $"The provided capacity and errorRate values would result in an array of length > int.MaxValue. Please reduce either of these values. Capacity: {capacity}, Error rate: {errorRate}");

            // set the secondary hash function
            _getHashSecondary = hashFunction ?? HashString;

            _hashFunctionCount = k;
            _bitArray = new BitArray(m);
        }

        #endregion

        #region public member

        public virtual void Add(string url)
        {
            // start flipping bits for each hash of item
            int primaryHash = url.GetHashCode();
            int secondaryHash = _getHashSecondary(url);
            for (int i = 0; i < _hashFunctionCount; i++)
            {
                int hash = ComputeHash(primaryHash, secondaryHash, i);
                _bitArray[hash] = true;
            }
        }

        public virtual bool Contains(string url)
        {
            int primaryHash = url.GetHashCode();
            int secondaryHash = _getHashSecondary(url);
            for (int i = 0; i < _hashFunctionCount; i++)
            {
                int hash = ComputeHash(primaryHash, secondaryHash, i);
                if (_bitArray[hash] == false)
                    return false;
            }
            return true;
        }

        #endregion

        private static int BestK(int capacity, float errorRate)
        {
            return (int) Math.Round(Math.Log(2.0) * BestM(capacity, errorRate) / capacity);
        }

        private static int BestM(int capacity, float errorRate)
        {
            return (int) Math.Ceiling(capacity * Math.Log(errorRate, (1.0 / Math.Pow(2, Math.Log(2.0)))));
        }

        private static float BestErrorRate(int capacity)
        {
            float c = (float) (1.0 / capacity);
            if (c != 0)
                return c;
            else
                return
                    (float) Math.Pow(0.6185,
                        int.MaxValue / capacity); // http://www.cs.princeton.edu/courses/archive/spring02/cs493/lec7.pdf
        }

        /// <summary>
        /// Performs Dillinger and Manolios double hashing. 
        /// </summary>
        private int ComputeHash(int primaryHash, int secondaryHash, int i)
        {
            int resultingHash = (primaryHash + (i * secondaryHash)) % _bitArray.Count;
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
