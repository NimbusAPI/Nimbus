using System.Threading.Tasks;

namespace Nimbus.Configuration
{
    internal interface IMessagePumpsManager
    {
        Task Start(MessagePumpTypes messagePumpTypes);
        Task Stop(MessagePumpTypes messagePumpTypes);
    }
}