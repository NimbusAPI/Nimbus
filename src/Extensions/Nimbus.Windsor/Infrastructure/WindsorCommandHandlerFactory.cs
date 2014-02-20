using Castle.MicroKernel;
using Castle.MicroKernel.Lifestyle;
using Nimbus.InfrastructureContracts;
using Nimbus.MessageContracts;

namespace Nimbus.Windsor.Infrastructure
{
    public class WindsorCommandHandlerFactory : ICommandHandlerFactory
    {
        private readonly IKernel _container;

        public WindsorCommandHandlerFactory(IKernel container)
        {
            _container = container;
        }

        public OwnedComponent<IHandleCommand<TBusCommand>> GetHandler<TBusCommand>() where TBusCommand : IBusCommand
        {
            var type = typeof (IHandleCommand<TBusCommand>);
            var scope = _container.BeginScope();
            var handler = (IHandleCommand<TBusCommand>) _container.Resolve(type);

            return new OwnedComponent<IHandleCommand<TBusCommand>>(handler, scope);
        }
    }
}