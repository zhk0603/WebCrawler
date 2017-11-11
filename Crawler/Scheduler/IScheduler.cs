namespace Crawler.Scheduler
{
    // 资源调度器。
    public interface IScheduler
    {
        Site Pop();
        void Push(Site requestSite);
    }
}
