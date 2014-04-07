using System;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Nimbus.HandlerFactories;
using Nimbus.Handlers;
using Nimbus.MessageContracts;

namespace Nimbus.Infrastructure.Commands
{
    internal class CommandMessageDispatcher : IMessageDispatcher
    {
        private readonly ICommandHandlerFactory _commandHandlerFactory;
        private readonly IBrokeredMessageFactory _brokeredMessageFactory;
        private readonly Type _commandType;
        private readonly IClock _clock;

        public CommandMessageDispatcher(
            ICommandHandlerFactory commandHandlerFactory,
            IBrokeredMessageFactory brokeredMessageFactory,
            Type commandType,
            IClock clock)
        {
            _commandHandlerFactory = commandHandlerFactory;
            _brokeredMessageFactory = brokeredMessageFactory;
            _commandType = commandType;
            _clock = clock;
        }

        public async Task Dispatch(BrokeredMessage message)
        {
            var busCommand = await _brokeredMessageFactory.GetBody(message, _commandType);
            await Dispatch((dynamic) busCommand, message);
        }

        private async Task Dispatch<TBusCommand>(TBusCommand busCommand, BrokeredMessage message) where TBusCommand : IBusCommand
        {
            using (var handler = _commandHandlerFactory.GetHandler<TBusCommand>())
            {
                var handlerTask = handler.Component.Handle(busCommand);
                var wrapper = new LongLivedTaskWrapper(handlerTask, handler.Component as ILongRunningHandler, message, _clock);
                await wrapper.AwaitCompletion();
            }
        }
    }
}