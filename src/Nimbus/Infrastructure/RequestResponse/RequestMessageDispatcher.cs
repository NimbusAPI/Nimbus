﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Nimbus.Configuration.Settings;
using Nimbus.DependencyResolution;
using Nimbus.Extensions;
using Nimbus.Handlers;
using Nimbus.Infrastructure.LongRunningTasks;
using Nimbus.Infrastructure.PropertyInjection;
using Nimbus.Infrastructure.TaskScheduling;
using Nimbus.Interceptors.Inbound;
using Nimbus.Interceptors.Outbound;
using Nimbus.MessageContracts;

namespace Nimbus.Infrastructure.RequestResponse
{
    internal class RequestMessageDispatcher : IMessageDispatcher
    {
        private readonly IBrokeredMessageFactory _brokeredMessageFactory;
        private readonly IClock _clock;
        private readonly IDependencyResolver _dependencyResolver;
        private readonly IInboundInterceptorFactory _inboundInterceptorFactory;
        private readonly IOutboundInterceptorFactory _outboundInterceptorFactory;
        private readonly ILogger _logger;
        private readonly INimbusMessagingFactory _messagingFactory;
        private readonly IReadOnlyDictionary<Type, Type[]> _handlerMap;
        private readonly DefaultMessageLockDurationSetting _defaultMessageLockDuration;
        private readonly INimbusTaskFactory _taskFactory;
        private readonly IPropertyInjector _propertyInjector;

        public RequestMessageDispatcher(
            IBrokeredMessageFactory brokeredMessageFactory,
            IClock clock,
            IDependencyResolver dependencyResolver,
            IInboundInterceptorFactory inboundInterceptorFactory,
            IOutboundInterceptorFactory outboundInterceptorFactory,
            ILogger logger,
            INimbusMessagingFactory messagingFactory,
            IReadOnlyDictionary<Type, Type[]> handlerMap,
            DefaultMessageLockDurationSetting defaultMessageLockDuration,
            INimbusTaskFactory taskFactory,
            IPropertyInjector propertyInjector)
        {
            _brokeredMessageFactory = brokeredMessageFactory;
            _clock = clock;
            _dependencyResolver = dependencyResolver;
            _inboundInterceptorFactory = inboundInterceptorFactory;
            _outboundInterceptorFactory = outboundInterceptorFactory;
            _logger = logger;
            _messagingFactory = messagingFactory;
            _handlerMap = handlerMap;
            _defaultMessageLockDuration = defaultMessageLockDuration;
            _taskFactory = taskFactory;
            _propertyInjector = propertyInjector;
        }

        public async Task Dispatch(BrokeredMessage message)
        {
            var busRequest = await _brokeredMessageFactory.GetBody(message);
            var messageType = busRequest.GetType();

            // There should only ever be a single request handler per message type
            var handlerType = _handlerMap.GetSingleHandlerTypeFor(messageType);
            var dispatchMethod = GetGenericDispatchMethodFor(busRequest);
            await (Task) dispatchMethod.Invoke(this, new[] {busRequest, message, handlerType});
        }

        // ReSharper disable UnusedMember.Local
        private async Task Dispatch<TBusRequest, TBusResponse>(TBusRequest busRequest, BrokeredMessage brokeredMessage, Type handlerType)
            where TBusRequest : IBusRequest<TBusRequest, TBusResponse>
            where TBusResponse : IBusResponse
        {
            var replyQueueName = brokeredMessage.ReplyTo;
            var replyQueueClient = _messagingFactory.GetQueueSender(replyQueueName);

            Exception exception = null;
            using (var scope = _dependencyResolver.CreateChildScope())
            {
                var handler = (IHandleRequest<TBusRequest, TBusResponse>)scope.Resolve(handlerType);
                _propertyInjector.Inject(handler, brokeredMessage);
                var inboundInterceptors = _inboundInterceptorFactory.CreateInterceptors(scope, handler, busRequest, brokeredMessage);

                try
                {
                    foreach (var interceptor in inboundInterceptors)
                    {
                        _logger.Debug("Executing OnRequestHandlerExecuting on {0} for message [MessageType:{1}, MessageId:{2}, CorrelationId:{3}]",
                                      interceptor.GetType().FullName,
                                      brokeredMessage.SafelyGetBodyTypeNameOrDefault(),
                                      brokeredMessage.MessageId,
                                      brokeredMessage.CorrelationId);
                        await interceptor.OnRequestHandlerExecuting(busRequest, brokeredMessage);
                        _logger.Debug("Executed OnRequestHandlerExecuting on {0} for message [MessageType:{1}, MessageId:{2}, CorrelationId:{3}]",
                                      interceptor.GetType().FullName,
                                      brokeredMessage.SafelyGetBodyTypeNameOrDefault(),
                                      brokeredMessage.MessageId,
                                      brokeredMessage.CorrelationId);
                    }

                    var handlerTask = handler.Handle(busRequest);
                    var longRunningTask = handler as ILongRunningTask;
                    TBusResponse response;
                    if (longRunningTask != null)
                    {
                        var wrapperTask = new LongRunningTaskWrapper<TBusResponse>(handlerTask, longRunningTask, brokeredMessage, _clock, _logger, _defaultMessageLockDuration, _taskFactory);
                        response = await wrapperTask.AwaitCompletion();
                    }
                    else
                    {
                        response = await handlerTask;
                    }

                    var responseMessage = (await _brokeredMessageFactory.CreateSuccessfulResponse(response, brokeredMessage))
                        .DestinedForQueue(replyQueueName);

                    var outboundInterceptors = _outboundInterceptorFactory.CreateInterceptors(scope, brokeredMessage);
                    foreach (var interceptor in outboundInterceptors.Reverse())
                    {
                        _logger.Debug("Executing OnResponseSending on {0} for message [MessageType:{1}, MessageId:{2}, CorrelationId:{3}]",
                                      interceptor.GetType().FullName,
                                      brokeredMessage.SafelyGetBodyTypeNameOrDefault(),
                                      brokeredMessage.MessageId,
                                      brokeredMessage.CorrelationId);

                        await interceptor.OnResponseSending(response, responseMessage);

                        _logger.Debug("Executed OnResponseSending on {0} for message [MessageType:{1}, MessageId:{2}, CorrelationId:{3}]",
                                      interceptor.GetType().FullName,
                                      brokeredMessage.SafelyGetBodyTypeNameOrDefault(),
                                      brokeredMessage.MessageId,
                                      brokeredMessage.CorrelationId);
                    }

                    _logger.Debug("Sending successful response message {0} to {1} [MessageId:{2}, CorrelationId:{3}]",
                                  responseMessage.SafelyGetBodyTypeNameOrDefault(),
                                  replyQueueName,
                                  brokeredMessage.MessageId,
                                  brokeredMessage.CorrelationId);
                    await replyQueueClient.Send(responseMessage);

                    foreach (var interceptor in outboundInterceptors.Reverse())
                    {
                        _logger.Debug("Executing OnResponseSent on {0} for message [MessageType:{1}, MessageId:{2}, CorrelationId:{3}]",
                                      interceptor.GetType().FullName,
                                      brokeredMessage.SafelyGetBodyTypeNameOrDefault(),
                                      brokeredMessage.MessageId,
                                      brokeredMessage.CorrelationId);

                        await interceptor.OnResponseSent(response, responseMessage);

                        _logger.Debug("Executed OnResponseSent on {0} for message [MessageType:{1}, MessageId:{2}, CorrelationId:{3}]",
                                      interceptor.GetType().FullName,
                                      brokeredMessage.SafelyGetBodyTypeNameOrDefault(),
                                      brokeredMessage.MessageId,
                                      brokeredMessage.CorrelationId);
                    }

                    _logger.Info("Sent successful response message {0} to {1} [MessageId:{2}, CorrelationId:{3}]",
                                 brokeredMessage.SafelyGetBodyTypeNameOrDefault(),
                                 replyQueueName,
                                 brokeredMessage.MessageId,
                                 brokeredMessage.CorrelationId);
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
                                      brokeredMessage.SafelyGetBodyTypeNameOrDefault(),
                                      brokeredMessage.MessageId,
                                      brokeredMessage.CorrelationId);

                        await interceptor.OnRequestHandlerSuccess(busRequest, brokeredMessage);

                        _logger.Debug("Executed OnRequestHandlerSuccess on {0} for message [MessageType:{1}, MessageId:{2}, CorrelationId:{3}]",
                                      interceptor.GetType().FullName,
                                      brokeredMessage.SafelyGetBodyTypeNameOrDefault(),
                                      brokeredMessage.MessageId,
                                      brokeredMessage.CorrelationId);
                    }
                }
                else
                {
                    foreach (var interceptor in inboundInterceptors.Reverse())
                    {
                        _logger.Debug("Executing OnRequestHandlerError on {0} for message [MessageType:{1}, MessageId:{2}, CorrelationId:{3}]",
                                      interceptor.GetType().FullName,
                                      brokeredMessage.SafelyGetBodyTypeNameOrDefault(),
                                      brokeredMessage.MessageId,
                                      brokeredMessage.CorrelationId);

                        await interceptor.OnRequestHandlerError(busRequest, brokeredMessage, exception);

                        _logger.Debug("Executed OnRequestHandlerError on {0} for message [MessageType:{1}, MessageId:{2}, CorrelationId:{3}]",
                                      interceptor.GetType().FullName,
                                      brokeredMessage.SafelyGetBodyTypeNameOrDefault(),
                                      brokeredMessage.MessageId,
                                      brokeredMessage.CorrelationId);
                    }

                    var failedResponseMessage = (await _brokeredMessageFactory.CreateFailedResponse(brokeredMessage, exception))
                        .DestinedForQueue(replyQueueName);

                    _logger.Warn("Sending failed response message to {0} [MessageId:{1}, CorrelationId:{2}]",
                                 replyQueueName,
                                 exception.Message,
                                 brokeredMessage.MessageId,
                                 brokeredMessage.CorrelationId);
                    await replyQueueClient.Send(failedResponseMessage);
                    _logger.Info("Sent failed response message to {0} [MessageId:{1}, CorrelationId:{2}]",
                                 replyQueueName,
                                 brokeredMessage.MessageId,
                                 brokeredMessage.CorrelationId);
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
            var closedGenericMethod = openGenericMethod.MakeGenericMethod(new[] {requestType, responseType});
            return closedGenericMethod;
        }
    }
}