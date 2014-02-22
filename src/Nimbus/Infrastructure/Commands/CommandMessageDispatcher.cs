using System;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Nimbus.Extensions;
using Nimbus.HandlerFactories;
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
            await Dispatch((dynamic) busCommand, message);
        }

        private async Task Dispatch<TBusCommand>(TBusCommand busCommand, BrokeredMessage message) where TBusCommand : IBusCommand
        {
            using (var handler = _commandHandlerFactory.GetHandler<TBusCommand>())
            {
                await handler.Component.Handle(busCommand);
            }
        }
    }
}