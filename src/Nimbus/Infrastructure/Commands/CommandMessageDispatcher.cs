using System;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Nimbus.DependencyResolution;
using Nimbus.Handlers;
using Nimbus.MessageContracts;

namespace Nimbus.Infrastructure.Commands
{
    internal class CommandMessageDispatcher : IMessageDispatcher
    {
        private readonly IDependencyResolver _dependencyResolver;
        private readonly IBrokeredMessageFactory _brokeredMessageFactory;
        private readonly Type _commandType;
        private readonly IClock _clock;
        private readonly Type _handlerType;

        public CommandMessageDispatcher(
            IDependencyResolver dependencyResolver,
            IBrokeredMessageFactory brokeredMessageFactory,
            Type commandType,
            IClock clock,
            Type handlerType)
        {
            _dependencyResolver = dependencyResolver;
            _brokeredMessageFactory = brokeredMessageFactory;
            _commandType = commandType;
            _clock = clock;
            _handlerType = handlerType;
        }

        public async Task Dispatch(BrokeredMessage message)
        {
            var busCommand = await _brokeredMessageFactory.GetBody(message, _commandType);
            await Dispatch((dynamic) busCommand, message);
        }

        private async Task Dispatch<TBusCommand>(TBusCommand busCommand, BrokeredMessage message) where TBusCommand : IBusCommand
        {
            using (var scope = _dependencyResolver.CreateChildScope())
            {
                var handler = scope.Resolve<IHandleCommand<TBusCommand>>(_handlerType.FullName);
                var handlerTask = handler.Handle(busCommand);
                var wrapper = new LongLivedTaskWrapper(handlerTask, handler as ILongRunningHandler, message, _clock);
                await wrapper.AwaitCompletion();
            }
        }
    }
}