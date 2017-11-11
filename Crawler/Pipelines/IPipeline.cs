using System.Threading.Tasks;

namespace Crawler.Pipelines
{
    public interface IPipeline
    {
        bool IsComplete { get; set; }
        IPipeline Next { get; set; }
        void Initialize();
        Task ExecuteAsync(PipelineContext context);
    }
}