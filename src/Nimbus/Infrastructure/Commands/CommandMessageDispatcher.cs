using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Nimbus.Configuration.Settings;
using Nimbus.DependencyResolution;
using Nimbus.Exceptions;
using Nimbus.Extensions;
using Nimbus.Handlers;
using Nimbus.Infrastructure.LongRunningTasks;
using Nimbus.Infrastructure.PropertyInjection;
using Nimbus.Infrastructure.TaskScheduling;
using Nimbus.Interceptors.Inbound;
using Nimbus.MessageContracts;

namespace Nimbus.Infrastructure.Commands
{
    internal class CommandMessageDispatcher : IMessageDispatcher
    {
        private readonly IDependencyResolver _dependencyResolver;
        private readonly IInboundInterceptorFactory _inboundInterceptorFactory;
        private readonly IBrokeredMessageFactory _brokeredMessageFactory;
        private readonly IClock _clock;
        private readonly ILogger _logger;
        private readonly IReadOnlyDictionary<Type, Type[]> _handlerMap;
        private readonly DefaultMessageLockDurationSetting _defaultMessageLockDuration;
        private readonly INimbusTaskFactory _taskFactory;
        private readonly IPropertyInjector _propertyInjector;

        public CommandMessageDispatcher(
            IBrokeredMessageFactory brokeredMessageFactory,
            IClock clock,
            IDependencyResolver dependencyResolver,
            IInboundInterceptorFactory inboundInterceptorFactory,
            ILogger logger,
            IReadOnlyDictionary<Type, Type[]> handlerMap,
            DefaultMessageLockDurationSetting defaultMessageLockDuration,
            INimbusTaskFactory taskFactory,
            IPropertyInjector propertyInjector)
        {
            _brokeredMessageFactory = brokeredMessageFactory;
            _clock = clock;
            _dependencyResolver = dependencyResolver;
            _inboundInterceptorFactory = inboundInterceptorFactory;
            _logger = logger;
            _handlerMap = handlerMap;
            _defaultMessageLockDuration = defaultMessageLockDuration;
            _taskFactory = taskFactory;
            _propertyInjector = propertyInjector;
        }

        public async Task Dispatch(BrokeredMessage message)
        {
            var busCommand = await _brokeredMessageFactory.GetBody(message);
            var messageType = busCommand.GetType();

            // There should only ever be a single command handler
            var handlerType = _handlerMap.GetSingleHandlerTypeFor(messageType);
            await Dispatch((dynamic) busCommand, message, handlerType);
        }

        private async Task Dispatch<TBusCommand>(TBusCommand busCommand, BrokeredMessage message, Type handlerType) where TBusCommand : IBusCommand
        {
            using (var scope = _dependencyResolver.CreateChildScope())
            {
                var handler = scope.Resolve<IHandleCommand<TBusCommand>>(handlerType.FullName);
                _propertyInjector.Inject(handler);
                var interceptors = _inboundInterceptorFactory.CreateInterceptors(scope, handler, busCommand);

                Exception exception;
                try
                {
                    foreach (var interceptor in interceptors)
                    {
                        _logger.Debug(
                            "Executing OnCommandHandlerExecuting on {InterceptorType} for message [MessageType:{MessageType}, MessageId:{MessageId}, CorrelationId:{CorrelationId}]",
                            interceptor.GetType().FullName,
                            message.SafelyGetBodyTypeNameOrDefault(),
                            message.MessageId,
                            message.CorrelationId);
                        await interceptor.OnCommandHandlerExecuting(busCommand, message);
                        _logger.Debug(
                            "Executed OnCommandHandlerExecuting on {InterceptorType} for message [MessageType:{MessageType}, MessageId:{MessageId}, CorrelationId:{CorrelationId}]",
                            interceptor.GetType().FullName,
                            message.SafelyGetBodyTypeNameOrDefault(),
                            message.MessageId,
                            message.CorrelationId);
                    }

                    var handlerTask = _taskFactory.StartNew(async () => await handler.Handle(busCommand), TaskContext.Handle).Unwrap();
                    var longRunningTask = handler as ILongRunningTask;
                    if (longRunningTask != null)
                    {
                        var wrapper = new LongLivedTaskWrapper(handlerTask, longRunningTask, message, _clock, _logger, _defaultMessageLockDuration, _taskFactory);
                        await wrapper.AwaitCompletion();
                    }
                    else
                    {
                        await handlerTask;
                    }

                    foreach (var interceptor in interceptors.Reverse())
                    {
                        _logger.Debug("Executing OnCommandHandlerSuccess on {0} for message [MessageType:{1}, MessageId:{2}, CorrelationId:{3}]",
                                      interceptor.GetType().FullName,
                                      message.SafelyGetBodyTypeNameOrDefault(),
                                      message.MessageId,
                                      message.CorrelationId);
                        await interceptor.OnCommandHandlerSuccess(busCommand, message);
                        _logger.Debug("Executed OnCommandHandlerSuccess on {0} for message [MessageType:{1}, MessageId:{2}, CorrelationId:{3}]",
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
                    _logger.Debug("Executing OnCommandHandlerError on {0} for message [MessageType:{1}, MessageId:{2}, CorrelationId:{3}]",
                                  interceptor.GetType().FullName,
                                  message.SafelyGetBodyTypeNameOrDefault(),
                                  message.MessageId,
                                  message.CorrelationId);
                    await interceptor.OnCommandHandlerError(busCommand, message, exception);
                    _logger.Debug("Executed OnCommandHandlerError on {0} for message [MessageType:{1}, MessageId:{2}, CorrelationId:{3}]",
                                  interceptor.GetType().FullName,
                                  message.SafelyGetBodyTypeNameOrDefault(),
                                  message.MessageId,
                                  message.CorrelationId);
                }

                _logger.Debug("Failed to Dispatch CommandMessage for message [MessageType:{0}, MessageId:{1}, CorrelationId:{2}]",
                              message.SafelyGetBodyTypeNameOrDefault(),
                              message.MessageId,
                              message.CorrelationId);

                throw new DispatchFailedException("Failed to Dispatch CommandMessage", exception);
            }
        }
    }
}