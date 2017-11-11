namespace Crawler.Scheduler
{
    // 资源调度器。
    public interface IScheduler
    {
        object Pop();
        void Push(object @object);
    }
}