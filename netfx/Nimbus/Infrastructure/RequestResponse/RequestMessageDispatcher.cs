using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Nimbus.DependencyResolution;
using Nimbus.Extensions;
using Nimbus.Handlers;
using Nimbus.Infrastructure.Logging;
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
            var replyQueueName = nimbusMessage.From;
            var replyQueueClient = _transport.GetQueueSender(replyQueueName);

            using (var scope = _dependencyResolver.CreateChildScope())
            {
                var handler = (IHandleRequest<TBusRequest, TBusResponse>) scope.Resolve(handlerType);
                _propertyInjector.Inject(handler, nimbusMessage);
                var inboundInterceptors = _inboundInterceptorFactory.CreateInterceptors(scope, handler, busRequest, nimbusMessage);

                Exception exception = null;
                try
                {
                    foreach (var interceptor in inboundInterceptors)
                    {
                        _logger.Debug("Executing OnRequestHandlerExecuting for {InterceptorType}", interceptor.GetType().Name);
                        await interceptor.OnRequestHandlerExecuting(busRequest, nimbusMessage);
                        _logger.Debug("Executed OnRequestHandlerExecuting for {InterceptorType}", interceptor.GetType().Name);
                    }

                    _logger.Debug("Dispatching to {HandlerType}", handler.GetType().Name);
                    var handlerTask = handler.Handle(busRequest);
                    var response = await handlerTask;
                    _logger.Debug("Dispatched to {HandlerType}", handler.GetType().Name);

                    var responseMessage = await _nimbusMessageFactory.CreateSuccessfulResponse(replyQueueName, response, nimbusMessage);
                    DispatchLoggingContext.NimbusMessage = responseMessage;

                    var outboundInterceptors = _outboundInterceptorFactory.CreateInterceptors(scope, nimbusMessage);
                    foreach (var interceptor in outboundInterceptors.Reverse())
                    {
                        _logger.Debug("Executing OnResponseSending for {InterceptorType}", interceptor.GetType().Name);
                        await interceptor.OnResponseSending(response, responseMessage);
                        _logger.Debug("Executed OnResponseSending for {InterceptorType}", interceptor.GetType().Name);
                    }

                    _logger.Debug("Sending successful response message");
                    await replyQueueClient.Send(responseMessage);
                    _logger.Debug("Sent successful response message");

                    foreach (var interceptor in outboundInterceptors.Reverse())
                    {
                        _logger.Debug("Executing OnResponseSent for {InterceptorType}", interceptor.GetType().Name);
                        await interceptor.OnResponseSent(response, responseMessage);
                        _logger.Debug("Executed OnResponseSent for {InterceptorType}", interceptor.GetType().Name);
                    }
                }
                catch (Exception exc)
                {
                    // Capture any exception so we can send a failed response outside the catch block
                    exception = exc;
                    _logger.Warn(exc, "Request message dispatch failed.");
                }
                if (exception == null)
                {
                    foreach (var interceptor in inboundInterceptors.Reverse())
                    {
                        _logger.Debug("Executing OnRequestHandlerSuccess for {InterceptorType}", interceptor.GetType().Name);
                        await interceptor.OnRequestHandlerSuccess(busRequest, nimbusMessage);
                        _logger.Debug("Executed OnRequestHandlerSuccess for {InterceptorType}", interceptor.GetType().Name);
                    }
                }
                else
                {
                    foreach (var interceptor in inboundInterceptors.Reverse())
                    {
                        _logger.Debug("Executing OnRequestHandlerError for {InterceptorType}", interceptor.GetType().Name);
                        await interceptor.OnRequestHandlerError(busRequest, nimbusMessage, exception);
                        _logger.Debug("Executed OnRequestHandlerError for {InterceptorType}", interceptor.GetType().Name);
                    }

                    var failedResponseMessage = (await _nimbusMessageFactory.CreateFailedResponse(replyQueueName, nimbusMessage, exception));

                    _logger.Debug("Sending failed response message");
                    await replyQueueClient.Send(failedResponseMessage);
                    _logger.Debug("Sent failed response message");
                }
            }
        }

        // ReSharper restore UnusedMember.Local

        internal static MethodInfo GetGenericDispatchMethodFor(object request)
        {
            var closedGenericHandlerType = request.GetType()
                                                  .GetInterfaces().Where(t => t.IsClosedTypeOf(typeof (IBusRequest<,>)))
                                                  .Single();

            var genericArguments = closedGenericHandlerType.GetGenericArguments();
            var requestType = genericArguments[0];
            var responseType = genericArguments[1];

            var openGenericMethod = typeof (RequestMessageDispatcher).GetMethod("Dispatch", BindingFlags.NonPublic | BindingFlags.Instance);
            var closedGenericMethod = openGenericMethod.MakeGenericMethod(requestType, responseType);
            return closedGenericMethod;
        }
    }
}