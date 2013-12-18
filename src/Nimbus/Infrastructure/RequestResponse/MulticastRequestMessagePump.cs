using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.ServiceBus.Messaging;
using Nimbus.Extensions;
using Nimbus.InfrastructureContracts;
using Nimbus.MessageContracts;

namespace Nimbus.Infrastructure.RequestResponse
{
    internal class MulticastRequestMessagePump : MessagePump
    {
        private readonly MessagingFactory _messagingFactory;
        private readonly IMulticastRequestBroker _multicastRequestBroker;
        private readonly Type _requestType;
        private readonly string _applicationSharedSubscriptionName;
        private SubscriptionClient _client;

        public MulticastRequestMessagePump(MessagingFactory messagingFactory,
                                           IMulticastRequestBroker multicastRequestBroker,
                                           Type requestType,
                                           string applicationSharedSubscriptionName,
                                           ILogger logger) : base(logger)
        {
            _messagingFactory = messagingFactory;
            _multicastRequestBroker = multicastRequestBroker;
            _requestType = requestType;
            _applicationSharedSubscriptionName = applicationSharedSubscriptionName;
        }

        public override void Start()
        {
            var topicPath = PathFactory.TopicPathFor(_requestType);
            _client = _messagingFactory.CreateSubscriptionClient(topicPath, _applicationSharedSubscriptionName);
            base.Start();
        }

        public override void Stop()
        {
            if (_client != null) _client.Close();
            base.Stop();
        }

        protected override BrokeredMessage[] ReceiveMessages()
        {
            return _client.ReceiveBatch(int.MaxValue, BatchTimeout).ToArray();
        }

        protected override void PumpMessage(BrokeredMessage requestMessage)
        {
            var replyQueueName = requestMessage.ReplyTo;
            var replyQueueClient = _messagingFactory.CreateQueueClient(replyQueueName);

            var busRequest = requestMessage.GetBody(_requestType);

            var requestTimeoutInMilliseconds = (int) requestMessage.Properties[MessagePropertyKeys.RequestTimeoutInMillisecondsKey];
            var timeout = TimeSpan.FromMilliseconds(requestTimeoutInMilliseconds);

            var responses = InvokeGenericHandleMethod(_multicastRequestBroker, busRequest, timeout);
            foreach (var response in responses)
            {
                var responseMessage = new BrokeredMessage(response);
                responseMessage.Properties.Add(MessagePropertyKeys.RequestSuccessfulKey, true);
                responseMessage.CorrelationId = requestMessage.CorrelationId;
                replyQueueClient.Send(responseMessage);
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
                                            .Where(t => t.IsClosedTypeOf(typeof(IBusRequest<,>)))
                                            .Single();

            var genericArguments = closedGenericTypeOfIBusRequest.GetGenericArguments();
            var requestType = genericArguments[0];
            var responseType = genericArguments[1];

            var genericHandleMethod = typeof(IMulticastRequestBroker).GetMethod("HandleMulticast");
            var handleMethod = genericHandleMethod.MakeGenericMethod(new[] { requestType, responseType });
            return handleMethod;
        }
    }
}