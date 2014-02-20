using System.Threading.Tasks;
using Nimbus.Infrastructure;
using Nimbus.InfrastructureContracts;

namespace Nimbus.UnitTests.MessageBrokerTests.TestInfrastructure.BrokerFactories
{
    public class DefaultMessageBrokerFactory : ICreateMessageBroker<ICommandBroker>, ICreateMessageBroker<IMulticastEventBroker>
    {
        async Task<ICommandBroker> ICreateMessageBroker<ICommandBroker>.Create(ITypeProvider typeProvider)
        {
            return new DefaultMessageBroker(typeProvider);
        }

        async Task<IMulticastEventBroker> ICreateMessageBroker<IMulticastEventBroker>.Create(ITypeProvider typeProvider)
        {
            return new DefaultMessageBroker(typeProvider);
        }

        public void Dispose()
        {
        }
    }
}