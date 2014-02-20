using System.Threading.Tasks;
using Autofac;
using Nimbus.Configuration;
using Nimbus.InfrastructureContracts;

namespace Nimbus.UnitTests.MessageBrokerTests.TestInfrastructure.BrokerFactories
{
    public class AutofacCommandBrokerFactory : ICreateMessageBroker<ICommandBroker>
    {
        private IContainer _container;

        public async Task<ICommandBroker> Create(ITypeProvider typeProvider)
        {
            var builder = new ContainerBuilder();
            builder.RegisterNimbus(typeProvider);
            _container = builder.Build();

            return _container.Resolve<ICommandBroker>();
        }

        public void Dispose()
        {
            var container = _container;
            if (container == null) return;
            container.Dispose();
        }
    }
}