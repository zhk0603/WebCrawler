namespace Crawler.Reporter
{
    public interface IReporter
    {
        int ReportStatusInterval { get; set; }
        void ReportStatus();
    }
}
