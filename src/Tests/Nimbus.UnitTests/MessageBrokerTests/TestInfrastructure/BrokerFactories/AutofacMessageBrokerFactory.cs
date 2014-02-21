using System;
using System.Threading.Tasks;
using Autofac;
using Nimbus.Configuration;
using Nimbus.InfrastructureContracts;

namespace Nimbus.UnitTests.MessageBrokerTests.TestInfrastructure.BrokerFactories
{
    public class AutofacMessageBrokerFactory : ICreateMessageHandlerFactory<ICommandHandlerFactory>,
                                               ICreateMessageHandlerFactory<IMulticastEventHandlerFactory>,
                                               ICreateMessageHandlerFactory<ICompetingEventHandlerFactory>,
                                               ICreateMessageHandlerFactory<IRequestBroker>,
                                               ICreateMessageHandlerFactory<IMulticastRequestHandlerFactory>
    {
        private IContainer _container;

        async Task<IMulticastEventHandlerFactory> ICreateMessageHandlerFactory<IMulticastEventHandlerFactory>.Create(ITypeProvider typeProvider)
        {
            BuildContainer(typeProvider);

            return _container.Resolve<IMulticastEventHandlerFactory>();
        }

        async Task<ICommandHandlerFactory> ICreateMessageHandlerFactory<ICommandHandlerFactory>.Create(ITypeProvider typeProvider)
        {
            BuildContainer(typeProvider);

            return _container.Resolve<ICommandHandlerFactory>();
        }

        async Task<ICompetingEventHandlerFactory> ICreateMessageHandlerFactory<ICompetingEventHandlerFactory>.Create(ITypeProvider typeProvider)
        {
            BuildContainer(typeProvider);

            return _container.Resolve<ICompetingEventHandlerFactory>();
        }

        async Task<IRequestBroker> ICreateMessageHandlerFactory<IRequestBroker>.Create(ITypeProvider typeProvider)
        {
            BuildContainer(typeProvider);

            return _container.Resolve<IRequestBroker>();
        }

        async Task<IMulticastRequestHandlerFactory> ICreateMessageHandlerFactory<IMulticastRequestHandlerFactory>.Create(ITypeProvider typeProvider)
        {
            BuildContainer(typeProvider);

            return _container.Resolve<IMulticastRequestHandlerFactory>();
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