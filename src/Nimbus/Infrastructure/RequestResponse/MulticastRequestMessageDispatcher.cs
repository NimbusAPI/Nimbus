using System;
using System.Collections.Generic;
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
            var requestMessage = message;
            var replyQueueName = requestMessage.ReplyTo;
            var replyQueueClient = _messagingFactory.GetQueueSender(replyQueueName);

            var busRequest = requestMessage.GetBody(_requestType);

            var requestTimeoutInMilliseconds = (int) requestMessage.Properties[MessagePropertyKeys.RequestTimeoutInMilliseconds];
            var timeout = TimeSpan.FromMilliseconds(requestTimeoutInMilliseconds);

            var responses = InvokeGenericHandleMethod(_multicastRequestHandlerFactory, busRequest, timeout);
            foreach (var response in responses)
            {
                var responseMessage = new BrokeredMessage(response);
                responseMessage.Properties.Add(MessagePropertyKeys.RequestSuccessful, true);
                responseMessage.CorrelationId = requestMessage.CorrelationId;
                await replyQueueClient.Send(responseMessage);
            }
        }

        private static IEnumerable<object> InvokeGenericHandleMethod(IMulticastRequestHandlerFactory requestHandlerFactory, object request, TimeSpan timeout)
        {
            var handleMethod = ExtractHandleMulticastMethodInfo(request);
            var response = handleMethod.Invoke(requestHandlerFactory, new[] {request, timeout});
            return (IEnumerable<object>) response;
        }

        internal static MethodInfo ExtractHandleMulticastMethodInfo(object request)
        {
            var closedGenericTypeOfIBusRequest = request.GetType()
                                                        .GetInterfaces()
                                                        .Where(t => t.IsClosedTypeOf(typeof (IBusRequest<,>)))
                                                        .Single();

            var genericArguments = closedGenericTypeOfIBusRequest.GetGenericArguments();
            var requestType = genericArguments[0];
            var responseType = genericArguments[1];

            var genericHandleMethod = typeof (IMulticastRequestHandlerFactory).GetMethod("HandleMulticast");
            var handleMethod = genericHandleMethod.MakeGenericMethod(new[] {requestType, responseType});
            return handleMethod;
        }
    }
}