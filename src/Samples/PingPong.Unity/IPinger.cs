using System.Threading.Tasks;

namespace PingPong.Unity
{
    public interface IPinger
    {
        Task<string> Ping(string message);
    }
}