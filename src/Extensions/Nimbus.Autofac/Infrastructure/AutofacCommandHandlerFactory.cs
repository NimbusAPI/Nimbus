using Autofac;
using Autofac.Features.OwnedInstances;
using Nimbus.InfrastructureContracts;
using Nimbus.MessageContracts;

namespace Nimbus.Autofac.Infrastructure
{
    public class AutofacCommandHandlerFactory : ICommandHandlerFactory
    {
        private readonly ILifetimeScope _lifetimeScope;

        public AutofacCommandHandlerFactory(ILifetimeScope lifetimeScope)
        {
            _lifetimeScope = lifetimeScope;
        }

        public OwnedComponent<IHandleCommand<TBusCommand>> GetHandler<TBusCommand>() where TBusCommand : IBusCommand
        {
            var handler = _lifetimeScope.Resolve<Owned<IHandleCommand<TBusCommand>>>();
            return new OwnedComponent<IHandleCommand<TBusCommand>>(handler.Value, handler);
        }
    }
}