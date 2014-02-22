using Castle.MicroKernel;
using Castle.MicroKernel.Lifestyle;
using Nimbus.HandlerFactories;
using Nimbus.Handlers;
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
            var scope = _container.BeginScope();
            var handler = _container.Resolve<IHandleCommand<TBusCommand>>();
            return new OwnedComponent<IHandleCommand<TBusCommand>>(handler, scope);
        }
    }
}