using Nimbus.MessageContracts;

namespace Nimbus.Interceptors
{
    public interface ICommandInterceptor<in TBusCommand> : IMessageInterceptor<TBusCommand> where TBusCommand : IBusCommand
    {
    }
}