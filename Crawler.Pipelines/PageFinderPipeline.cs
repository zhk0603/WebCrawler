namespace Crawler.Pipelines
{
    public class PageFinderPipeline : CrawlerPipeline<PageFinderOptons>
    {
        protected PageFinderPipeline(PageFinderOptons options) : base(options)
        {
        }
    }

    public class PageFinderOptons : PipelineOptions
    {
    }
}