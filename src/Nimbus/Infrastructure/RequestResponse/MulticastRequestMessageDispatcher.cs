using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Nimbus.Extensions;
using Nimbus.HandlerFactories;
using Nimbus.Handlers;
using Nimbus.MessageContracts;

namespace Nimbus.Infrastructure.RequestResponse
{
    internal class MulticastRequestMessageDispatcher : IMessageDispatcher
    {
        private readonly INimbusMessagingFactory _messagingFactory;
        private readonly IBrokeredMessageFactory _brokeredMessageFactory;
        private readonly IMulticastRequestHandlerFactory _multicastRequestHandlerFactory;
        private readonly Type _requestType;
        private readonly IClock _clock;
        private readonly ILogger _logger;

        public MulticastRequestMessageDispatcher(INimbusMessagingFactory messagingFactory,
                                                 IBrokeredMessageFactory brokeredMessageFactory,
                                                 IMulticastRequestHandlerFactory multicastRequestHandlerFactory,
                                                 Type requestType,
                                                 IClock clock,
                                                 ILogger logger)
        {
            _messagingFactory = messagingFactory;
            _brokeredMessageFactory = brokeredMessageFactory;
            _multicastRequestHandlerFactory = multicastRequestHandlerFactory;
            _requestType = requestType;
            _clock = clock;
            _logger = logger;
        }

        public async Task Dispatch(BrokeredMessage message)
        {
            var request = await _brokeredMessageFactory.GetBody(message, _requestType);
            var dispatchMethod = GetGenericDispatchMethodFor(request);
            await (Task) dispatchMethod.Invoke(this, new[] {request, message});
        }

        private async Task Dispatch<TBusRequest, TBusResponse>(TBusRequest busRequest, BrokeredMessage message)
            where TBusRequest : IBusRequest<TBusRequest, TBusResponse>
            where TBusResponse : IBusResponse
        {
            var replyQueueName = message.ReplyTo;
            var replyQueueClient = _messagingFactory.GetQueueSender(replyQueueName);

            // NOTE: This doesn't appear to be used anywhere (yet?)
            var timeout = message.GetRequestTimeout();

            using (var handlers = _multicastRequestHandlerFactory.GetHandlers<TBusRequest, TBusResponse>())
            {
                var tasks = handlers.Component
                                    .Select(h => new LongLivedTaskWrapper<TBusResponse>(h.Handle(busRequest), h as ILongRunningHandler, message, _clock)
                                                .AwaitCompletion()
                                                .ContinueWith(async t =>
                                                                    {
                                                                        var responseMessage = await _brokeredMessageFactory.CreateSuccessfulResponse(t.Result, message);
                                                                        _logger.Debug("Sending successful multicast response message {0} to {1} [MessageId:{2}, CorrelationId:{3}]",
                                                                                      message.SafelyGetBodyTypeNameOrDefault(),
                                                                                      replyQueueName,
                                                                                      message.MessageId,
                                                                                      message.CorrelationId);
                                                                        await replyQueueClient.Send(responseMessage);
                                                                        _logger.Info("Sent successful multicast response message {0} to {1} [MessageId:{2}, CorrelationId:{3}]",
                                                                                     message.SafelyGetBodyTypeNameOrDefault(),
                                                                                     replyQueueName,
                                                                                     message.MessageId,
                                                                                     message.CorrelationId);
                                                                    }))
                                    .ToArray();

                await Task.WhenAll(tasks);
            }
        }

        internal static MethodInfo GetGenericDispatchMethodFor(object request)
        {
            var closedGenericTypeOfIBusRequest = request.GetType()
                                                        .GetInterfaces()
                                                        .Where(t => t.IsClosedTypeOf(typeof (IBusRequest<,>)))
                                                        .Single();

            var genericArguments = closedGenericTypeOfIBusRequest.GetGenericArguments();
            var requestType = genericArguments[0];
            var responseType = genericArguments[1];

            var openGenericMethod = typeof (MulticastRequestMessageDispatcher).GetMethod("Dispatch", BindingFlags.NonPublic | BindingFlags.Instance);
            var closedGenericMethod = openGenericMethod.MakeGenericMethod(new[] {requestType, responseType});
            return closedGenericMethod;
        }
    }
}