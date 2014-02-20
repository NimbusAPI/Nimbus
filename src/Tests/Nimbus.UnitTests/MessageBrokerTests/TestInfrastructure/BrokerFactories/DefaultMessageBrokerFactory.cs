using System.Threading.Tasks;
using Nimbus.Infrastructure;
using Nimbus.InfrastructureContracts;

namespace Nimbus.UnitTests.MessageBrokerTests.TestInfrastructure.BrokerFactories
{
    public class DefaultMessageBrokerFactory : ICreateMessageHandlerFactory<ICommandHandlerFactory>,
                                               ICreateMessageHandlerFactory<IMulticastEventBroker>,
                                               ICreateMessageHandlerFactory<ICompetingEventBroker>,
                                               ICreateMessageHandlerFactory<IRequestBroker>,
                                               ICreateMessageHandlerFactory<IMulticastRequestBroker>
    {
        async Task<ICommandHandlerFactory> ICreateMessageHandlerFactory<ICommandHandlerFactory>.Create(ITypeProvider typeProvider)
        {
            return new DefaultMessageHandlerFactory(typeProvider);
        }

        async Task<IMulticastEventBroker> ICreateMessageHandlerFactory<IMulticastEventBroker>.Create(ITypeProvider typeProvider)
        {
            return new DefaultMessageHandlerFactory(typeProvider);
        }

        async Task<ICompetingEventBroker> ICreateMessageHandlerFactory<ICompetingEventBroker>.Create(ITypeProvider typeProvider)
        {
            return new DefaultMessageHandlerFactory(typeProvider);
        }

        async Task<IRequestBroker> ICreateMessageHandlerFactory<IRequestBroker>.Create(ITypeProvider typeProvider)
        {
            return new DefaultMessageHandlerFactory(typeProvider);
        }

        async Task<IMulticastRequestBroker> ICreateMessageHandlerFactory<IMulticastRequestBroker>.Create(ITypeProvider typeProvider)
        {
            return new DefaultMessageHandlerFactory(typeProvider);
        }

        public void Dispose()
        {
        }
    }
}