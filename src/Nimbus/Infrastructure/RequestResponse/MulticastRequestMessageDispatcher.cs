using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Nimbus.DependencyResolution;
using Nimbus.Extensions;
using Nimbus.Handlers;
using Nimbus.Infrastructure.Filtering;
using Nimbus.Infrastructure.Logging;
using Nimbus.Infrastructure.PropertyInjection;
using Nimbus.Interceptors.Inbound;
using Nimbus.Interceptors.Outbound;
using Nimbus.MessageContracts;

namespace Nimbus.Infrastructure.RequestResponse
{
    internal class MulticastRequestMessageDispatcher : IMessageDispatcher
    {
        private readonly INimbusMessageFactory _nimbusMessageFactory;
        private readonly IDependencyResolver _dependencyResolver;
        private readonly IInboundInterceptorFactory _inboundInterceptorFactory;
        private readonly IOutboundInterceptorFactory _outboundInterceptorFactory;
        private readonly ILogger _logger;
        private readonly INimbusTransport _transport;
        private readonly IReadOnlyDictionary<Type, Type[]> _handlerMap;
        private readonly IPropertyInjector _propertyInjector;
        private readonly IFilterConditionProvider _filterConditionProvider;

        public MulticastRequestMessageDispatcher(INimbusMessageFactory nimbusMessageFactory,
                                                 IDependencyResolver dependencyResolver,
                                                 IInboundInterceptorFactory inboundInterceptorFactory,
                                                 ILogger logger,
                                                 INimbusTransport transport,
                                                 IOutboundInterceptorFactory outboundInterceptorFactory,
                                                 IReadOnlyDictionary<Type, Type[]> handlerMap,
                                                 IPropertyInjector propertyInjector,
                                                 IFilterConditionProvider filterConditionProvider)
        {
            _nimbusMessageFactory = nimbusMessageFactory;
            _dependencyResolver = dependencyResolver;
            _inboundInterceptorFactory = inboundInterceptorFactory;
            _logger = logger;
            _transport = transport;
            _handlerMap = handlerMap;
            _propertyInjector = propertyInjector;
            _filterConditionProvider = filterConditionProvider;
            _outboundInterceptorFactory = outboundInterceptorFactory;
        }

        public async Task Dispatch(NimbusMessage message)
        {
            var busRequest = message.Payload;
            var messageType = busRequest.GetType();

            // There should only ever be a single multicast request handler associated with this dispatcher
            var handlerType = _handlerMap.GetSingleHandlerTypeFor(messageType);
            var dispatchMethod = GetGenericDispatchMethodFor(busRequest);
            await (Task) dispatchMethod.Invoke(this, new[] {busRequest, message, handlerType});
        }

        // ReSharper disable UnusedMember.Local
        private async Task Dispatch<TBusRequest, TBusResponse>(TBusRequest busRequest, NimbusMessage nimbusMessage, Type handlerType)
            where TBusRequest : IBusMulticastRequest<TBusRequest, TBusResponse>
            where TBusResponse : IBusMulticastResponse
        {
            var subscriptionFilter = _filterConditionProvider.GetFilterConditionFor(handlerType);
            if (!nimbusMessage.MatchesFilter(subscriptionFilter))
            {
                _logger.Debug("Message {MessageId} does not match the subscription filter for {HandlerType}. Dropping it immediately.", nimbusMessage.MessageId, handlerType);
                return;
            }

            var replyQueueName = nimbusMessage.From;
            var replyQueueClient = _transport.GetQueueSender(replyQueueName);

            Exception exception = null;
            using (var scope = _dependencyResolver.CreateChildScope())
            {
                var handler = (IHandleMulticastRequest<TBusRequest, TBusResponse>) scope.Resolve(handlerType);
                _propertyInjector.Inject(handler, nimbusMessage);
                var inboundInterceptors = _inboundInterceptorFactory.CreateInterceptors(scope, handler, busRequest, nimbusMessage);

                foreach (var interceptor in inboundInterceptors)
                {
                    _logger.Debug("Executing OnRequestHandlerExecuting on {0} for message [MessageType:{1}, MessageId:{2}, CorrelationId:{3}]",
                                  interceptor.GetType().FullName,
                                  nimbusMessage.SafelyGetBodyTypeNameOrDefault(),
                                  nimbusMessage.MessageId,
                                  nimbusMessage.CorrelationId);
                    await interceptor.OnMulticastRequestHandlerExecuting(busRequest, nimbusMessage);
                    _logger.Debug("Executed OnRequestHandlerExecuting on {0} for message [MessageType:{1}, MessageId:{2}, CorrelationId:{3}]",
                                  interceptor.GetType().FullName,
                                  nimbusMessage.SafelyGetBodyTypeNameOrDefault(),
                                  nimbusMessage.MessageId,
                                  nimbusMessage.CorrelationId);
                }

                try
                {
                    var response = await handler.Handle(busRequest);

                    if (response != null)
                    {
                        var responseMessage = await _nimbusMessageFactory.CreateSuccessfulResponse(replyQueueName, response, nimbusMessage);
                        DispatchLoggingContext.NimbusMessage = responseMessage;

                        var outboundInterceptors = _outboundInterceptorFactory.CreateInterceptors(scope, nimbusMessage);
                        _logger.Debug("Sending successful response message {0} to {1} [MessageId:{2}, CorrelationId:{3}]",
                                      responseMessage.SafelyGetBodyTypeNameOrDefault(),
                                      replyQueueName,
                                      nimbusMessage.MessageId,
                                      nimbusMessage.CorrelationId);
                        foreach (var interceptor in outboundInterceptors)
                        {
                            await interceptor.OnMulticastResponseSending(response, nimbusMessage);
                        }

                        await replyQueueClient.Send(responseMessage);

                        foreach (var interceptor in outboundInterceptors.Reverse())
                        {
                            await interceptor.OnMulticastResponseSent(response, nimbusMessage);
                        }

                        _logger.Debug("Sent successful response message {0} to {1} [MessageId:{2}, CorrelationId:{3}]",
                                      nimbusMessage.SafelyGetBodyTypeNameOrDefault(),
                                      replyQueueName,
                                      nimbusMessage.MessageId,
                                      nimbusMessage.CorrelationId);
                    }
                    else
                    {
                        _logger.Debug("Handler declined to reply. [MessageId: {0}, CorrelationId: {1}]", nimbusMessage.MessageId, nimbusMessage.CorrelationId);
                    }
                }
                catch (Exception exc)
                {
                    // Capture any exception so we can send a failed response outside the catch block
                    exception = exc;
                }
                if (exception == null)
                {
                    foreach (var interceptor in inboundInterceptors.Reverse())
                    {
                        _logger.Debug("Executing OnRequestHandlerSuccess on {0} for message [MessageType:{1}, MessageId:{2}, CorrelationId:{3}]",
                                      interceptor.GetType().FullName,
                                      nimbusMessage.SafelyGetBodyTypeNameOrDefault(),
                                      nimbusMessage.MessageId,
                                      nimbusMessage.CorrelationId);

                        await interceptor.OnMulticastRequestHandlerSuccess(busRequest, nimbusMessage);

                        _logger.Debug("Executed OnRequestHandlerSuccess on {0} for message [MessageType:{1}, MessageId:{2}, CorrelationId:{3}]",
                                      interceptor.GetType().FullName,
                                      nimbusMessage.SafelyGetBodyTypeNameOrDefault(),
                                      nimbusMessage.MessageId,
                                      nimbusMessage.CorrelationId);
                    }
                }
                else
                {
                    foreach (var interceptor in inboundInterceptors.Reverse())
                    {
                        _logger.Debug("Executing OnRequestHandlerError on {0} for message [MessageType:{1}, MessageId:{2}, CorrelationId:{3}]",
                                      interceptor.GetType().FullName,
                                      nimbusMessage.SafelyGetBodyTypeNameOrDefault(),
                                      nimbusMessage.MessageId,
                                      nimbusMessage.CorrelationId);

                        await interceptor.OnMulticastRequestHandlerError(busRequest, nimbusMessage, exception);

                        _logger.Debug("Executed OnRequestHandlerError on {0} for message [MessageType:{1}, MessageId:{2}, CorrelationId:{3}]",
                                      interceptor.GetType().FullName,
                                      nimbusMessage.SafelyGetBodyTypeNameOrDefault(),
                                      nimbusMessage.MessageId,
                                      nimbusMessage.CorrelationId);
                    }

                    var failedResponseMessage =
                        await _nimbusMessageFactory.CreateFailedResponse(replyQueueName, nimbusMessage, exception);

                    _logger.Warn("Sending failed response message to {0} [MessageId:{1}, CorrelationId:{2}]",
                                 replyQueueName,
                                 exception.Message,
                                 nimbusMessage.MessageId,
                                 nimbusMessage.CorrelationId);
                    await replyQueueClient.Send(failedResponseMessage);
                    _logger.Debug("Sent failed response message to {0} [MessageId:{1}, CorrelationId:{2}]",
                                  replyQueueName,
                                  nimbusMessage.MessageId,
                                  nimbusMessage.CorrelationId);
                }
            }
        }

        // ReSharper restore UnusedMember.Local
        internal static MethodInfo GetGenericDispatchMethodFor(object request)
        {
            var closedGenericHandlerType =
                request.GetType()
                       .GetInterfaces().Where(t => t.IsClosedTypeOf(typeof(IBusMulticastRequest<,>)))
                       .Single();

            var genericArguments = closedGenericHandlerType.GetGenericArguments();
            var requestType = genericArguments[0];
            var responseType = genericArguments[1];

            var openGenericMethod = typeof(MulticastRequestMessageDispatcher).GetMethod("Dispatch", BindingFlags.NonPublic | BindingFlags.Instance);
            var closedGenericMethod = openGenericMethod.MakeGenericMethod(requestType, responseType);
            return closedGenericMethod;
        }
    }
}