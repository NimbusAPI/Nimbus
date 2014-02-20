using System;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Nimbus.Extensions;
using Nimbus.InfrastructureContracts;
using Nimbus.MessageContracts;

namespace Nimbus.Infrastructure.Commands
{
    internal class CommandMessageDispatcher : IMessageDispatcher
    {
        private readonly ICommandHandlerFactory _commandHandlerFactory;
        private readonly Type _commandType;

        public CommandMessageDispatcher(ICommandHandlerFactory commandHandlerFactory, Type commandType)
        {
            _commandHandlerFactory = commandHandlerFactory;
            _commandType = commandType;
        }

        public async Task Dispatch(BrokeredMessage message)
        {
            var busCommand = message.GetBody(_commandType);

            using (var handler = GetHandler((dynamic)busCommand))
            {
                await handler.Value.Handle(busCommand);
            }
        }

        // ReSharper disable UnusedParameter.Local
        // The parameter is used for dynamic construction of the correct generic type.
        private OwnedComponent<IHandleCommand<TBusCommand>> GetHandler<TBusCommand>(TBusCommand busCommand) where TBusCommand : IBusCommand
        // ReSharper restore UnusedParameter.Local
        {
            return _commandHandlerFactory.GetHandler<TBusCommand>();
        }
    }
}