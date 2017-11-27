using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Crawler.Logger;
using Crawler.Pipelines;

namespace Crawler
{
    public class CrawlerBuilder
    {
        private static readonly CrawlerBuilder _builder = new CrawlerBuilder();
        private readonly IList<IPipeline> _pipelines;
        private readonly IList<Tuple<Delegate, object[]>> _pipelineTuples;
        private readonly IList<Site> _sites;
        private string _named;
        private int _threadNum = 1;
        private PipelineRunMode _runMode;

        public CrawlerBuilder()
        {
            _sites = new List<Site>();
            _pipelines = new List<IPipeline>();
            _pipelineTuples = new List<Tuple<Delegate, object[]>>();
            Properties = new Dictionary<string, object>();

            Properties[Constants.CrawlerVersionKey] = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        public static CrawlerBuilder Current => _builder;

        public Dictionary<string, object> Properties { get; }

        public CrawlerBuilder AddSite(string url)
        {
            var site = new Site(url);
            return AddSite(site);
        }
        public CrawlerBuilder AddSite(Site site)
        {
            if (site == null)
                throw new ArgumentNullException(nameof(site));

            _sites.Add(site);

            return this;
        }

        public CrawlerBuilder AddSiteRange(IEnumerable<Site> sites)
        {
            if (sites == null)
                throw new ArgumentNullException(nameof(sites));

            foreach (var site in sites)
                _sites.Add(site);

            return this;
        }

        public CrawlerBuilder SetLogFactory(ILoggerFactory loggerFactory)
        {
            if (loggerFactory == null)
            {
                throw new ArgumentNullException(nameof(loggerFactory));
            }

            LoggerManager.SetLogFactory(loggerFactory);
            return this;
        }

        public CrawlerBuilder UsePipeline(Type pipelineType, params object[] options)
        {
            _pipelineTuples.Add(PipelineFactory(pipelineType, options));

            return this;
        }

        public CrawlerBuilder UsePipeline<T>(params object[] options)
            where T : IPipeline
        {
            return UsePipeline(typeof(T), options);
        }

        public CrawlerBuilder ClearSites()
        {
            _sites.Clear();
            return this;
        }
        public CrawlerBuilder ClearPipelines()
        {
            _pipelines.Clear();
            _pipelineTuples.Clear();
            return this;
        }

        private Tuple<Delegate, object[]> PipelineFactory(Type pipelineType, params object[] args)
        {
            if (!pipelineType.IsAbstract && typeof(IPipeline).IsAssignableFrom(pipelineType))
            {
                var constructors = pipelineType.GetConstructors();
                foreach (var constructor in constructors)
                {
                    var parameters = constructor.GetParameters();
                    var parameterTypes = parameters.Select(p => p.ParameterType).ToArray();
                    if (parameterTypes.Length != args.Length)
                        continue;
                    if (!parameterTypes
                        .Zip(args, TestArgForParameter)
                        .All(x => x))
                        continue;
                    var parameterExpressions =
                        parameters.Select(p => Expression.Parameter(p.ParameterType, p.Name)).ToArray();
                    var callConstructor = Expression.New(constructor, parameterExpressions);
                    var pipelineDelegate = Expression.Lambda(callConstructor, parameterExpressions).Compile();
                    return Tuple.Create(pipelineDelegate, args);
                }
            }

            throw new NotSupportedException($"类型：{pipelineType.FullName}，不是有效的管道。");
        }

        private static bool TestArgForParameter(Type parameterType, object arg)
        {
            return arg == null && !parameterType.IsValueType ||
                   parameterType.IsInstanceOfType(arg);
        }

        public CrawlerBuilder UseMultiThread(int threadNum)
        {
            if (threadNum < 1)
                throw new ArgumentException("爬虫线程数不能小于0。");

            _threadNum = threadNum;
            return this;
        }

        public CrawlerBuilder UseNamed(string named)
        {
            if (string.IsNullOrWhiteSpace(named))
            {
                throw new ArgumentNullException(nameof(named));
            }

            _named = named;
            return this;
        }

        public CrawlerBuilder UseParallelMode()
        {
            _runMode = PipelineRunMode.Parallel;
            return this;
        }

        public CrawlerBuilder UseBloomFilter(int capacity)
        {
            UrlFilterManager.SetUrlFilter(new BloomFilter(capacity));
            return this;
        }
        public CrawlerBuilder UseBloomFilter(int capacity, float errorRate)
        {
            UrlFilterManager.SetUrlFilter(new BloomFilter(capacity, errorRate));
            return this;
        }

        public CrawlerBuilder UseBloomFilter(int capacity, float errorRate, BloomFilter.HashFunction hashFunction)
        {
            UrlFilterManager.SetUrlFilter(new BloomFilter(capacity, errorRate, hashFunction));
            return this;
        }

        public Crawler Builder()
        {
            return BuilderInternal();
        }

        internal Crawler BuilderInternal()
        {
            ConvertPipeline();
            var crawler = new Crawler(_sites, StitchingPipeline()) {ThreadNum = _threadNum};
            if (!string.IsNullOrWhiteSpace(_named))
                crawler.Name = _named;
            crawler.RunMode = _runMode;

            return crawler;
        }

        private IEnumerable<IPipeline> StitchingPipeline()
        {
            if (_runMode == PipelineRunMode.Parallel)
            {
                return _pipelines;
            }

            IPipeline root = null;
            IPipeline next = null;
            foreach (var pipeline in _pipelines)
                if (root == null)
                {
                    root = pipeline;
                    next = pipeline;
                }
                else
                {
                    next.Next = pipeline;
                    next = pipeline;
                }

            return new List<IPipeline>
            {
                root ?? (root = new EmptyPipeline())
            };
        }

        private void ConvertPipeline()
        {
            foreach (var p in _pipelineTuples)
            {
                var pipelineDelegate = p.Item1;
                var pipelineArgs = p.Item2;

                var pipeline = (IPipeline) pipelineDelegate.DynamicInvoke(pipelineArgs);

                _pipelines.Add(pipeline);
            }
        }
    }

    public class EmptyPipeline : IPipeline
    {
        public EmptyPipeline()
        {
            Name = "Empty Pipeline";
        }

        public string Name { get; set; }
        public bool IsComplete { get; set; }
        public bool IsSkip { get; set; }

        Task IPipeline.ExecuteAsync(PipelineContext context)
        {
            Console.WriteLine("使用了一个空的管道。");
            return Task.FromResult<object>(null);
        }

        public IPipeline Next { get; set; }
    }
}