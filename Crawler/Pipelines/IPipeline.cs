using System.Threading.Tasks;

namespace Crawler.Pipelines
{
    public interface IPipeline
    {
        void Initialize();
        bool IsComplete { get; set; }
        Task ExecuteAsync(PipelineContext context);
        IPipeline Next { get; set; }
    }
}
