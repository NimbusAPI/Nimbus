using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Nimbus.DependencyResolution;
using Nimbus.Extensions;
using Nimbus.Handlers;
using Nimbus.Interceptors.Inbound;
using Nimbus.MessageContracts;

namespace Nimbus.Infrastructure.Commands
{
    internal class CommandMessageDispatcher : IMessageDispatcher
    {
        private readonly IDependencyResolver _dependencyResolver;
        private readonly IInboundInterceptorFactory _inboundInterceptorFactory;
        private readonly IBrokeredMessageFactory _brokeredMessageFactory;
        private readonly Type _commandType;
        private readonly IClock _clock;
        private readonly Type _handlerType;
        private readonly ILogger _logger;

        public CommandMessageDispatcher(
            IDependencyResolver dependencyResolver,
            IInboundInterceptorFactory inboundInterceptorFactory,
            IBrokeredMessageFactory brokeredMessageFactory,
            Type commandType,
            IClock clock,
            Type handlerType,
            ILogger logger)
        {
            _dependencyResolver = dependencyResolver;
            _inboundInterceptorFactory = inboundInterceptorFactory;
            _brokeredMessageFactory = brokeredMessageFactory;
            _commandType = commandType;
            _clock = clock;
            _handlerType = handlerType;
            _logger = logger;
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
                var interceptors = _inboundInterceptorFactory.CreateInterceptors(scope, handler, busCommand);

                foreach (var interceptor in interceptors)
                {
                    
                    _logger.Debug("Executing OnCommandHandlerExecuting on {0} for message [MessageType:{1}, MessageId:{2}, CorrelationId{3}]",
                        interceptor.GetType().FullName,
                        message.SafelyGetBodyTypeNameOrDefault(),
                        message.MessageId,
                        message.CorrelationId);
                    await interceptor.OnCommandHandlerExecuting(busCommand, message);
                    _logger.Debug("Executed OnCommandHandlerExecuting on {0} for message [MessageType:{1}, MessageId:{2}, CorrelationId{3}]",
                        interceptor.GetType().FullName,
                        message.SafelyGetBodyTypeNameOrDefault(),
                        message.MessageId,
                        message.CorrelationId);
                }

                Exception exception;
                try
                {
                    var handlerTask = handler.Handle(busCommand);
                    var wrapper = new LongLivedTaskWrapper(handlerTask, handler as ILongRunningTask, message, _clock);
                    await wrapper.AwaitCompletion();

                    foreach (var interceptor in interceptors.Reverse())
                    {
                        _logger.Debug("Executing OnCommandHandlerSuccess on {0} for message [MessageType:{1}, MessageId:{2}, CorrelationId{3}]",
                        interceptor.GetType().FullName,
                        message.SafelyGetBodyTypeNameOrDefault(),
                        message.MessageId,
                        message.CorrelationId);

                        await interceptor.OnCommandHandlerSuccess(busCommand, message);

                        _logger.Debug("Executed OnCommandHandlerSuccess on {0} for message [MessageType:{1}, MessageId:{2}, CorrelationId{3}]",
                        interceptor.GetType().FullName,
                        message.SafelyGetBodyTypeNameOrDefault(),
                        message.MessageId,
                        message.CorrelationId);
                    }
                    return;
                }
                catch (Exception exc)
                {
                    exception = exc;
                }

                foreach (var interceptor in interceptors.Reverse())
                {
                    _logger.Debug("Executing OnCommandHandlerError on {0} for message [MessageType:{1}, MessageId:{2}, CorrelationId{3}]",
                        interceptor.GetType().FullName,
                        message.SafelyGetBodyTypeNameOrDefault(),
                        message.MessageId,
                        message.CorrelationId);

                    await interceptor.OnCommandHandlerError(busCommand, message, exception);

                    _logger.Debug("Executed OnCommandHandlerError on {0} for message [MessageType:{1}, MessageId:{2}, CorrelationId{3}]",
                        interceptor.GetType().FullName,
                        message.SafelyGetBodyTypeNameOrDefault(),
                        message.MessageId,
                        message.CorrelationId);

                }
                throw exception;
            }
        }
    }
}