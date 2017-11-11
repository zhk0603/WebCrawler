using System.Threading.Tasks;

namespace Crawler
{
    public interface ICrawler
    {
        void Pause();
        void Continue();
        void Exit();
        void Run();
        Task RunAsync();
    }
}