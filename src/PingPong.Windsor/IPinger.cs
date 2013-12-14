using System.Threading.Tasks;

namespace PingPong.Windsor
{
    public interface IPinger
    {
        Task<string> Ping(string message);
    }
}