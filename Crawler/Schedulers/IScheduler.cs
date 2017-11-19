namespace Crawler.Schedulers
{
    // 资源调度器。
    public interface IScheduler
    {
        object Pop();
        void Push(object @object);
        long Count { get; }
    }
}