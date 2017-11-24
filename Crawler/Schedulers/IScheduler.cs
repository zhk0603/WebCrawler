namespace Crawler.Schedulers
{
    // 资源调度器。
    public interface IScheduler : IMonitorable
    {
        object Pop();
        void Push(object @object);
    }

    public interface IMonitorable
    {
        long Count { get; }
        long TotalCount { get; }
    }
}