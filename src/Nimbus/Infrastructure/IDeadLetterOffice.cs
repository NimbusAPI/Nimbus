using System.Threading.Tasks;

namespace Nimbus.Infrastructure
{
    internal interface IDeadLetterOffice
    {
        Task Post(NimbusMessage message);
    }
}