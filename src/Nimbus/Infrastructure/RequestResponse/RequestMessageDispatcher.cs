using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Nimbus.DependencyResolution;
using Nimbus.Extensions;
using Nimbus.Handlers;
using Nimbus.Infrastructure.PropertyInjection;
using Nimbus.Interceptors.Inbound;
using Nimbus.Interceptors.Outbound;
using Nimbus.MessageContracts;

namespace Nimbus.Infrastructure.RequestResponse
{
    internal class RequestMessageDispatcher : IMessageDispatcher
    {
        private readonly INimbusMessageFactory _nimbusMessageFactory;
        private readonly IDependencyResolver _dependencyResolver;
        private readonly IInboundInterceptorFactory _inboundInterceptorFactory;
        private readonly IOutboundInterceptorFactory _outboundInterceptorFactory;
        private readonly ILogger _logger;
        private readonly INimbusTransport _transport;
        private readonly IReadOnlyDictionary<Type, Type[]> _handlerMap;
        private readonly IPropertyInjector _propertyInjector;

        public RequestMessageDispatcher(
            INimbusMessageFactory nimbusMessageFactory,
            IDependencyResolver dependencyResolver,
            IInboundInterceptorFactory inboundInterceptorFactory,
            IOutboundInterceptorFactory outboundInterceptorFactory,
            ILogger logger,
            INimbusTransport transport,
            IReadOnlyDictionary<Type, Type[]> handlerMap,
            IPropertyInjector propertyInjector)
        {
            _nimbusMessageFactory = nimbusMessageFactory;
            _dependencyResolver = dependencyResolver;
            _inboundInterceptorFactory = inboundInterceptorFactory;
            _outboundInterceptorFactory = outboundInterceptorFactory;
            _logger = logger;
            _transport = transport;
            _handlerMap = handlerMap;
            _propertyInjector = propertyInjector;
        }

        public async Task Dispatch(NimbusMessage message)
        {
            var busRequest = message.Payload;
            var messageType = busRequest.GetType();

            // There should only ever be a single request handler per message type
            var handlerType = _handlerMap.GetSingleHandlerTypeFor(messageType);
            var dispatchMethod = GetGenericDispatchMethodFor(busRequest);
            await (Task) dispatchMethod.Invoke(this, new[] {busRequest, message, handlerType});
        }

        // ReSharper disable UnusedMember.Local
        private async Task Dispatch<TBusRequest, TBusResponse>(TBusRequest busRequest, NimbusMessage nimbusMessage, Type handlerType)
            where TBusRequest : IBusRequest<TBusRequest, TBusResponse>
            where TBusResponse : IBusResponse
        {
            var replyQueueName = nimbusMessage.ReplyTo;
            var replyQueueClient = _transport.GetQueueSender(replyQueueName);

            Exception exception = null;
            using (var scope = _dependencyResolver.CreateChildScope())
            {
                var handler = (IHandleRequest<TBusRequest, TBusResponse>) scope.Resolve(handlerType);
                _propertyInjector.Inject(handler, nimbusMessage);
                var inboundInterceptors = _inboundInterceptorFactory.CreateInterceptors(scope, handler, busRequest, nimbusMessage);

                try
                {
                    foreach (var interceptor in inboundInterceptors)
                    {
                        _logger.Debug("Executing OnRequestHandlerExecuting on {0} for message [MessageType:{1}, MessageId:{2}, CorrelationId:{3}]",
                                      interceptor.GetType().FullName,
                                      nimbusMessage.SafelyGetBodyTypeNameOrDefault(),
                                      nimbusMessage.MessageId,
                                      nimbusMessage.CorrelationId);
                        await interceptor.OnRequestHandlerExecuting(busRequest, nimbusMessage);
                        _logger.Debug("Executed OnRequestHandlerExecuting on {0} for message [MessageType:{1}, MessageId:{2}, CorrelationId:{3}]",
                                      interceptor.GetType().FullName,
                                      nimbusMessage.SafelyGetBodyTypeNameOrDefault(),
                                      nimbusMessage.MessageId,
                                      nimbusMessage.CorrelationId);
                    }

                    var handlerTask = handler.Handle(busRequest);
                    var response = await handlerTask;

                    var responseMessage = (await _nimbusMessageFactory.CreateSuccessfulResponse(response, nimbusMessage))
                        .DestinedForQueue(replyQueueName);

                    var outboundInterceptors = _outboundInterceptorFactory.CreateInterceptors(scope, nimbusMessage);
                    foreach (var interceptor in outboundInterceptors.Reverse())
                    {
                        _logger.Debug("Executing OnResponseSending on {0} for message [MessageType:{1}, MessageId:{2}, CorrelationId:{3}]",
                                      interceptor.GetType().FullName,
                                      nimbusMessage.SafelyGetBodyTypeNameOrDefault(),
                                      nimbusMessage.MessageId,
                                      nimbusMessage.CorrelationId);

                        await interceptor.OnResponseSending(response, responseMessage);

                        _logger.Debug("Executed OnResponseSending on {0} for message [MessageType:{1}, MessageId:{2}, CorrelationId:{3}]",
                                      interceptor.GetType().FullName,
                                      nimbusMessage.SafelyGetBodyTypeNameOrDefault(),
                                      nimbusMessage.MessageId,
                                      nimbusMessage.CorrelationId);
                    }

                    _logger.Debug("Sending successful response message {0} to {1} [MessageId:{2}, CorrelationId:{3}]",
                                  responseMessage.SafelyGetBodyTypeNameOrDefault(),
                                  replyQueueName,
                                  nimbusMessage.MessageId,
                                  nimbusMessage.CorrelationId);
                    await replyQueueClient.Send(responseMessage);

                    foreach (var interceptor in outboundInterceptors.Reverse())
                    {
                        _logger.Debug("Executing OnResponseSent on {0} for message [MessageType:{1}, MessageId:{2}, CorrelationId:{3}]",
                                      interceptor.GetType().FullName,
                                      nimbusMessage.SafelyGetBodyTypeNameOrDefault(),
                                      nimbusMessage.MessageId,
                                      nimbusMessage.CorrelationId);

                        await interceptor.OnResponseSent(response, responseMessage);

                        _logger.Debug("Executed OnResponseSent on {0} for message [MessageType:{1}, MessageId:{2}, CorrelationId:{3}]",
                                      interceptor.GetType().FullName,
                                      nimbusMessage.SafelyGetBodyTypeNameOrDefault(),
                                      nimbusMessage.MessageId,
                                      nimbusMessage.CorrelationId);
                    }

                    _logger.Info("Sent successful response message {0} to {1} [MessageId:{2}, CorrelationId:{3}]",
                                 nimbusMessage.SafelyGetBodyTypeNameOrDefault(),
                                 replyQueueName,
                                 nimbusMessage.MessageId,
                                 nimbusMessage.CorrelationId);
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

                        await interceptor.OnRequestHandlerSuccess(busRequest, nimbusMessage);

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

                        await interceptor.OnRequestHandlerError(busRequest, nimbusMessage, exception);

                        _logger.Debug("Executed OnRequestHandlerError on {0} for message [MessageType:{1}, MessageId:{2}, CorrelationId:{3}]",
                                      interceptor.GetType().FullName,
                                      nimbusMessage.SafelyGetBodyTypeNameOrDefault(),
                                      nimbusMessage.MessageId,
                                      nimbusMessage.CorrelationId);
                    }

                    var failedResponseMessage = (await _nimbusMessageFactory.CreateFailedResponse(nimbusMessage, exception))
                        .DestinedForQueue(replyQueueName);

                    _logger.Warn("Sending failed response message to {0} [MessageId:{1}, CorrelationId:{2}]",
                                 replyQueueName,
                                 exception.Message,
                                 nimbusMessage.MessageId,
                                 nimbusMessage.CorrelationId);
                    await replyQueueClient.Send(failedResponseMessage);
                    _logger.Info("Sent failed response message to {0} [MessageId:{1}, CorrelationId:{2}]",
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
                       .GetInterfaces().Where(t => t.IsClosedTypeOf(typeof (IBusRequest<,>)))
                       .Single();

            var genericArguments = closedGenericHandlerType.GetGenericArguments();
            var requestType = genericArguments[0];
            var responseType = genericArguments[1];

            var openGenericMethod = typeof (RequestMessageDispatcher).GetMethod("Dispatch",
                                                                                BindingFlags.NonPublic | BindingFlags.Instance);
            var closedGenericMethod = openGenericMethod.MakeGenericMethod(requestType, responseType);
            return closedGenericMethod;
        }
    }
}