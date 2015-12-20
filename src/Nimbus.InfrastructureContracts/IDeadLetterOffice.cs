using System.Threading.Tasks;

namespace Nimbus
{
    public interface IDeadLetterOffice
    {
        Task<NimbusMessage> Peek();
        Task<NimbusMessage> Pop();
        Task Post(NimbusMessage message);
        Task<int> Count();
    }
}