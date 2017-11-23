using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crawler.Pipelines
{
    public class ExcelPipeline<TEntity> : CrawlerPipeline<ExcelPipelineOptions<TEntity>>
    {
        private readonly List<TEntity> _cacheList1;
        private readonly List<TEntity> _cacheList2;

        protected ExcelPipeline(ExcelPipelineOptions<TEntity> options) : base(options)
        {
            _cacheList1 = new List<TEntity>();
            _cacheList2 = new List<TEntity>();
        }
    }

    public class ExcelPipelineOptions<TEntity> : PipelineOptions
    {
        /// <summary>
        ///     数据块大小。分块保存到excel文件中。
        /// </summary>
        public int BlockSite { get; set; }

        /// <summary>
        ///     每个文件最大大小。
        /// </summary>
        public int MaximumFileSize { get; set; }
    }
}