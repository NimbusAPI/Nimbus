using System.Threading.Tasks;
using Nimbus.Infrastructure;
using Nimbus.InfrastructureContracts;

namespace Nimbus.UnitTests.MessageBrokerTests.TestInfrastructure.BrokerFactories
{
    public class DefaultMessageBrokerFactory : ICreateMessageHandlerFactory<ICommandHandlerFactory>,
                                               ICreateMessageHandlerFactory<IMulticastEventHandlerFactory>,
                                               ICreateMessageHandlerFactory<ICompetingEventHandlerFactory>,
                                               ICreateMessageHandlerFactory<IRequestBroker>,
                                               ICreateMessageHandlerFactory<IMulticastRequestHandlerFactory>
    {
        async Task<ICommandHandlerFactory> ICreateMessageHandlerFactory<ICommandHandlerFactory>.Create(ITypeProvider typeProvider)
        {
            return new DefaultMessageHandlerFactory(typeProvider);
        }

        async Task<IMulticastEventHandlerFactory> ICreateMessageHandlerFactory<IMulticastEventHandlerFactory>.Create(ITypeProvider typeProvider)
        {
            return new DefaultMessageHandlerFactory(typeProvider);
        }

        async Task<ICompetingEventHandlerFactory> ICreateMessageHandlerFactory<ICompetingEventHandlerFactory>.Create(ITypeProvider typeProvider)
        {
            return new DefaultMessageHandlerFactory(typeProvider);
        }

        async Task<IRequestBroker> ICreateMessageHandlerFactory<IRequestBroker>.Create(ITypeProvider typeProvider)
        {
            return new DefaultMessageHandlerFactory(typeProvider);
        }

        async Task<IMulticastRequestHandlerFactory> ICreateMessageHandlerFactory<IMulticastRequestHandlerFactory>.Create(ITypeProvider typeProvider)
        {
            return new DefaultMessageHandlerFactory(typeProvider);
        }

        public void Dispose()
        {
        }
    }
}