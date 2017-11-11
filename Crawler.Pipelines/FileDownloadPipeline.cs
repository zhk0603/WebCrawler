using System.Threading.Tasks;

namespace Crawler.Pipelines
{
    public class FileDownloadPipeline : CrawlerPipeline<FileDownloadOptions>
    {
        private readonly string _path;

        protected FileDownloadPipeline(FileDownloadOptions options) : base(options)
        {
            if (!string.IsNullOrWhiteSpace(options.DownloadDirectory))
                if (options.DownloadDirectory.StartsWith("~/"))
                {
                    //TODO
                }
                else
                {
                    _path = options.DownloadDirectory;
                }
        }

        protected override async Task<bool> ExecuteAsync(PipelineContext context)
        {
            await InternalExecuteAsync(context.Page.ResultByte);
            return true;
        }

        internal Task InternalExecuteAsync(byte[] bytes)
        {
            return Task.FromResult(1);
        }
    }

    public class FileDownloadOptions : PipelineOptions
    {
        /// <summary>
        ///     文件保存路径，支持绝对路径与相对路径，相对路径以当前程序目录为起点。
        /// </summary>
        public string DownloadDirectory { get; set; }
    }
}