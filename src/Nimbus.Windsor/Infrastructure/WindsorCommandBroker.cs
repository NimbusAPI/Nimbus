using Castle.MicroKernel;
using Castle.MicroKernel.Lifestyle;
using Castle.Windsor;
using Nimbus.InfrastructureContracts;
using Nimbus.MessageContracts;

namespace Nimbus.Windsor.Infrastructure
{
    public class WindsorCommandBroker : ICommandBroker
    {
        private readonly IKernel _container;

        public WindsorCommandBroker(IKernel container)
        {
            _container = container;
        }

        public void Dispatch<TBusCommand>(TBusCommand busCommand) where TBusCommand : IBusCommand
        {
            using (var scope = _container.BeginScope())
            {
                var type = typeof (IHandleCommand<TBusCommand>);

                var handler = (IHandleCommand<TBusCommand>) _container.Resolve(type);
                handler.Handle(busCommand);
            }
        }
    }
}