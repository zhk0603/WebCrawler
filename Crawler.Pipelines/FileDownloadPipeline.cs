using System.IO;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using Crawler.Downloader;

namespace Crawler.Pipelines
{
    public class FileDownloadPipeline : CrawlerPipeline<FileDownloadOptions>
    {
        private readonly string _path;

        public FileDownloadPipeline(FileDownloadOptions options) : base(options)
        {
            if (!string.IsNullOrWhiteSpace(options.DownloadDirectory))
                if (options.DownloadDirectory.StartsWith("~/") ||
                    options.DownloadDirectory.StartsWith("../") ||
                    options.DownloadDirectory.StartsWith("./"))
                {
                    _path = AbsolutelyPath(options.DownloadDirectory);
                }
                else
                {
                    _path = options.DownloadDirectory;
                }

            if (!System.IO.Directory.Exists(_path))
            {
                System.IO.Directory.CreateDirectory(_path);
            }
        }

        /// <summary>
        ///     相对路径转绝对路径。
        /// </summary>
        /// <param name="relativePath"></param>
        /// <returns></returns>
        protected virtual string AbsolutelyPath(string relativePath)
        {
            string[] absoluteDirectories =
                System.Environment.CurrentDirectory.Split(new[] {'\\'}, System.StringSplitOptions.RemoveEmptyEntries);
            string[] relativeDirectories =
                relativePath.Split(new char[] {'\\', '/'}, System.StringSplitOptions.RemoveEmptyEntries);

            var sb = new StringBuilder();
            if (relativePath.StartsWith("../"))
            {
                var index = 0;
                foreach (var dir in relativeDirectories)
                {
                    if (dir.Equals(".."))
                    {
                        ++index;
                    }
                }

                if (index == -1 || index == absoluteDirectories.Length - 1)
                {
                    throw new System.ArgumentException($"路径：{relativePath}，不是有效的相对路径。");
                }

                var pathDirs = absoluteDirectories.Take(absoluteDirectories.Length - index - 1)
                    .Concat(relativeDirectories.Skip(index));
                sb.Append(string.Join("\\", pathDirs));
            }
            else
            {
                var pathDirs = absoluteDirectories.Concat(relativeDirectories.Skip(1));
                sb.Append(string.Join("\\", pathDirs));
            }
            sb.Append("\\");
            return sb.ToString();
        }

        protected override async Task<bool> ExecuteAsync(PipelineContext context)
        {
            await SaveAsync(context.Page.ResultByte,context.Page.Cookie);
            return true;
        }

        protected virtual async Task SaveAsync(byte[] bytes, string fileName)
        {
            var savePath = _path + fileName;
            if (!File.Exists(savePath))
            {
                using (var fileStream = new FileStream(savePath, FileMode.Create))
                {
                    fileStream.Write(bytes, 0, bytes.Length);
                    await fileStream.FlushAsync();
                    fileStream.Close();
                }
            }
        }
    }

    public class FileDownloadOptions : PipelineOptions
    {
        /// <summary>
        ///     文件保存路径，支持绝对路径与相对路径，相对路径以当前程序运行目录为起点。
        /// </summary>
        public string DownloadDirectory { get; set; }

        public IDownloader Downloader { get; set; }
    }
}