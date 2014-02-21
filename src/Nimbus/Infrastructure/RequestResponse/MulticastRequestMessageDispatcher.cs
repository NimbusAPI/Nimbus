using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Nimbus.Extensions;
using Nimbus.InfrastructureContracts;
using Nimbus.MessageContracts;

namespace Nimbus.Infrastructure.RequestResponse
{
    internal class MulticastRequestMessageDispatcher : IMessageDispatcher
    {
        private readonly INimbusMessagingFactory _messagingFactory;
        private readonly IMulticastRequestHandlerFactory _multicastRequestHandlerFactory;
        private readonly Type _requestType;

        public MulticastRequestMessageDispatcher(INimbusMessagingFactory messagingFactory, IMulticastRequestHandlerFactory multicastRequestHandlerFactory, Type requestType)
        {
            _messagingFactory = messagingFactory;
            _multicastRequestHandlerFactory = multicastRequestHandlerFactory;
            _requestType = requestType;
        }

        public async Task Dispatch(BrokeredMessage message)
        {
            var request = message.GetBody(_requestType);
            var dispatchMethod = GetGenericDispatchMethodFor(request);
            await (Task) dispatchMethod.Invoke(this, new[] {request, message});
        }

        private async Task Dispatch<TBusRequest, TBusResponse>(TBusRequest busRequest, BrokeredMessage message)
            where TBusRequest : IBusRequest<TBusRequest, TBusResponse>
            where TBusResponse : IBusResponse
        {
            var replyQueueName = message.ReplyTo;
            var replyQueueClient = _messagingFactory.GetQueueSender(replyQueueName);

            var requestTimeoutInMilliseconds = (int) message.Properties[MessagePropertyKeys.RequestTimeoutInMilliseconds];
            var timeout = TimeSpan.FromMilliseconds(requestTimeoutInMilliseconds);

            using (var handlers = _multicastRequestHandlerFactory.GetHandlers<TBusRequest, TBusResponse>())
            {
                var tasks = handlers.Component
                                    .Select(h => h.Handle(busRequest)
                                                  .ContinueWith(async t =>
                                                                      {
                                                                          var responseMessage = new BrokeredMessage(t.Result);
                                                                          responseMessage.Properties.Add(MessagePropertyKeys.RequestSuccessful, true);
                                                                          responseMessage.CorrelationId = message.CorrelationId;
                                                                          await replyQueueClient.Send(responseMessage);
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