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
        private readonly INimbusMessageSenderFactory _messageSenderFactory;
        private readonly IMulticastRequestBroker _multicastRequestBroker;
        private readonly Type _requestType;

        public MulticastRequestMessageDispatcher(INimbusMessageSenderFactory messageSenderFactory, IMulticastRequestBroker multicastRequestBroker, Type requestType)
        {
            _messageSenderFactory = messageSenderFactory;
            _multicastRequestBroker = multicastRequestBroker;
            _requestType = requestType;
        }

        public async Task Dispatch(BrokeredMessage message)
        {
            var requestMessage = message;
            var replyQueueName = requestMessage.ReplyTo;
            var replyQueueClient = _messageSenderFactory.GetQueueSender(replyQueueName);

            var busRequest = requestMessage.GetBody(_requestType);

            var requestTimeoutInMilliseconds = (int) requestMessage.Properties[MessagePropertyKeys.RequestTimeoutInMillisecondsKey];
            var timeout = TimeSpan.FromMilliseconds(requestTimeoutInMilliseconds);

            var responses = InvokeGenericHandleMethod(_multicastRequestBroker, busRequest, timeout);
            foreach (var response in responses)
            {
                var responseMessage = new BrokeredMessage(response);
                responseMessage.Properties.Add(MessagePropertyKeys.RequestSuccessfulKey, true);
                responseMessage.CorrelationId = requestMessage.CorrelationId;
                await replyQueueClient.Send(responseMessage);
            }
        }

        private static IEnumerable<object> InvokeGenericHandleMethod(IMulticastRequestBroker requestBroker, object request, TimeSpan timeout)
        {
            var handleMethod = ExtractHandleMulticastMethodInfo(request);
            var response = handleMethod.Invoke(requestBroker, new[] {request, timeout});
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

            var genericHandleMethod = typeof (IMulticastRequestBroker).GetMethod("HandleMulticast");
            var handleMethod = genericHandleMethod.MakeGenericMethod(new[] {requestType, responseType});
            return handleMethod;
        }
    }
}