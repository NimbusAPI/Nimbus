using System.Threading.Tasks;

namespace PingPong.PathGenerator
{
    public interface IPinger
    {
        Task<string> Ping(string message);
    }
}