using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nimbus.DependencyResolution;
using Nimbus.Exceptions;
using Nimbus.Extensions;
using Nimbus.Handlers;
using Nimbus.Infrastructure.PropertyInjection;
using Nimbus.Interceptors.Inbound;
using Nimbus.MessageContracts;

namespace Nimbus.Infrastructure.Commands
{
    internal class CommandMessageDispatcher : IMessageDispatcher
    {
        private readonly IDependencyResolver _dependencyResolver;
        private readonly IInboundInterceptorFactory _inboundInterceptorFactory;
        private readonly ILogger _logger;
        private readonly IReadOnlyDictionary<Type, Type[]> _handlerMap;
        private readonly IPropertyInjector _propertyInjector;

        public CommandMessageDispatcher(IDependencyResolver dependencyResolver,
                                        IInboundInterceptorFactory inboundInterceptorFactory,
                                        ILogger logger,
                                        IReadOnlyDictionary<Type, Type[]> handlerMap,
                                        IPropertyInjector propertyInjector)
        {
            _dependencyResolver = dependencyResolver;
            _inboundInterceptorFactory = inboundInterceptorFactory;
            _logger = logger;
            _handlerMap = handlerMap;
            _propertyInjector = propertyInjector;
        }

        public async Task Dispatch(NimbusMessage message)
        {
            var busCommand = message.Payload;
            var messageType = busCommand.GetType();

            // There should only ever be a single command handler
            var handlerType = _handlerMap.GetSingleHandlerTypeFor(messageType);
            await Dispatch((dynamic) busCommand, message, handlerType);
        }

        private async Task Dispatch<TBusCommand>(TBusCommand busCommand, NimbusMessage nimbusMessage, Type handlerType) where TBusCommand : IBusCommand
        {
            using (var scope = _dependencyResolver.CreateChildScope())
            {
                var handler = (IHandleCommand<TBusCommand>) scope.Resolve(handlerType);
                _propertyInjector.Inject(handler, nimbusMessage);
                var interceptors = _inboundInterceptorFactory.CreateInterceptors(scope, handler, busCommand, nimbusMessage);

                Exception exception;
                try
                {
                    foreach (var interceptor in interceptors)
                    {
                        _logger.Debug(
                            "Executing OnCommandHandlerExecuting on {InterceptorType} for message [MessageType:{MessageType}, MessageId:{MessageId}, CorrelationId:{CorrelationId}]",
                            interceptor.GetType().FullName,
                            nimbusMessage.SafelyGetBodyTypeNameOrDefault(),
                            nimbusMessage.MessageId,
                            nimbusMessage.CorrelationId);
                        await interceptor.OnCommandHandlerExecuting(busCommand, nimbusMessage);
                        _logger.Debug(
                            "Executed OnCommandHandlerExecuting on {InterceptorType} for message [MessageType:{MessageType}, MessageId:{MessageId}, CorrelationId:{CorrelationId}]",
                            interceptor.GetType().FullName,
                            nimbusMessage.SafelyGetBodyTypeNameOrDefault(),
                            nimbusMessage.MessageId,
                            nimbusMessage.CorrelationId);
                    }

                    await handler.Handle(busCommand);

                    foreach (var interceptor in interceptors.Reverse())
                    {
                        _logger.Debug("Executing OnCommandHandlerSuccess on {0} for message [MessageType:{1}, MessageId:{2}, CorrelationId:{3}]",
                                      interceptor.GetType().FullName,
                                      nimbusMessage.SafelyGetBodyTypeNameOrDefault(),
                                      nimbusMessage.MessageId,
                                      nimbusMessage.CorrelationId);
                        await interceptor.OnCommandHandlerSuccess(busCommand, nimbusMessage);
                        _logger.Debug("Executed OnCommandHandlerSuccess on {0} for message [MessageType:{1}, MessageId:{2}, CorrelationId:{3}]",
                                      interceptor.GetType().FullName,
                                      nimbusMessage.SafelyGetBodyTypeNameOrDefault(),
                                      nimbusMessage.MessageId,
                                      nimbusMessage.CorrelationId);
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
                                  nimbusMessage.SafelyGetBodyTypeNameOrDefault(),
                                  nimbusMessage.MessageId,
                                  nimbusMessage.CorrelationId);
                    await interceptor.OnCommandHandlerError(busCommand, nimbusMessage, exception);
                    _logger.Debug("Executed OnCommandHandlerError on {0} for message [MessageType:{1}, MessageId:{2}, CorrelationId:{3}]",
                                  interceptor.GetType().FullName,
                                  nimbusMessage.SafelyGetBodyTypeNameOrDefault(),
                                  nimbusMessage.MessageId,
                                  nimbusMessage.CorrelationId);
                }

                _logger.Debug("Failed to Dispatch CommandMessage for message [MessageType:{0}, MessageId:{1}, CorrelationId:{2}]",
                              nimbusMessage.SafelyGetBodyTypeNameOrDefault(),
                              nimbusMessage.MessageId,
                              nimbusMessage.CorrelationId);

                throw new DispatchFailedException("Failed to Dispatch CommandMessage", exception);
            }
        }
    }
}