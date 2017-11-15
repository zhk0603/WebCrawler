using System.Threading.Tasks;

namespace Crawler.Pipelines
{
    public interface IPipeline
    {
        string Name { get; set; }
        bool IsComplete { get; set; }
        bool IsSkip { get; set; }
        IPipeline Next { get; set; }
        Task ExecuteAsync(PipelineContext context);
    }
}