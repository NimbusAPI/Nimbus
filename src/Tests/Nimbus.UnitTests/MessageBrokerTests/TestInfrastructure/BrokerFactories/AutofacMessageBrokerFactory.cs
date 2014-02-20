using System;
using System.Threading.Tasks;
using Autofac;
using Nimbus.Configuration;
using Nimbus.InfrastructureContracts;

namespace Nimbus.UnitTests.MessageBrokerTests.TestInfrastructure.BrokerFactories
{
    public class AutofacMessageBrokerFactory : ICreateMessageHandlerFactory<ICommandHandlerFactory>,
                                               ICreateMessageHandlerFactory<IMulticastEventBroker>,
                                               ICreateMessageHandlerFactory<ICompetingEventBroker>,
                                               ICreateMessageHandlerFactory<IRequestBroker>,
                                               ICreateMessageHandlerFactory<IMulticastRequestBroker>
    {
        private IContainer _container;

        async Task<IMulticastEventBroker> ICreateMessageHandlerFactory<IMulticastEventBroker>.Create(ITypeProvider typeProvider)
        {
            BuildContainer(typeProvider);

            return _container.Resolve<IMulticastEventBroker>();
        }

        async Task<ICommandHandlerFactory> ICreateMessageHandlerFactory<ICommandHandlerFactory>.Create(ITypeProvider typeProvider)
        {
            BuildContainer(typeProvider);

            return _container.Resolve<ICommandHandlerFactory>();
        }

        async Task<ICompetingEventBroker> ICreateMessageHandlerFactory<ICompetingEventBroker>.Create(ITypeProvider typeProvider)
        {
            BuildContainer(typeProvider);

            return _container.Resolve<ICompetingEventBroker>();
        }

        async Task<IRequestBroker> ICreateMessageHandlerFactory<IRequestBroker>.Create(ITypeProvider typeProvider)
        {
            BuildContainer(typeProvider);

            return _container.Resolve<IRequestBroker>();
        }

        async Task<IMulticastRequestBroker> ICreateMessageHandlerFactory<IMulticastRequestBroker>.Create(ITypeProvider typeProvider)
        {
            BuildContainer(typeProvider);

            return _container.Resolve<IMulticastRequestBroker>();
        }

        private void BuildContainer(ITypeProvider typeProvider)
        {
            if (_container != null) throw new InvalidOperationException("This factory is only supposed to be used to construct one test subject.");
            var builder = new ContainerBuilder();
            builder.RegisterNimbus(typeProvider);
            _container = builder.Build();
        }

        public void Dispose()
        {
            var container = _container;
            if (container == null) return;
            container.Dispose();
        }
    }
}