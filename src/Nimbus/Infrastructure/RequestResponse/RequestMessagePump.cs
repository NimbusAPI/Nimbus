using System;
using System.Linq;
using System.Reflection;
using Microsoft.ServiceBus.Messaging;
using Nimbus.Extensions;
using Nimbus.InfrastructureContracts;
using Nimbus.MessageContracts;

namespace Nimbus.Infrastructure.RequestResponse
{
    public class RequestMessagePump : MessagePump
    {
        private readonly MessagingFactory _messagingFactory;
        private readonly IRequestBroker _requestBroker;
        private readonly Type _messageType;
        private MessageReceiver _reciever;

        public RequestMessagePump(MessagingFactory messagingFactory, IRequestBroker requestBroker, Type messageType, ILogger logger) : base(logger)
        {
            _messagingFactory = messagingFactory;
            _requestBroker = requestBroker;
            _messageType = messageType;
        }

        public override void Start()
        {
            var queuePath = PathFactory.QueuePathFor(_messageType);
            _reciever = _messagingFactory.CreateMessageReceiver(queuePath);
            base.Start();
        }

        public override void Stop()
        {
            if (_reciever != null) _reciever.Close();
            base.Stop();
        }

        protected override BrokeredMessage[] ReceiveMessages()
        {
            return _reciever.ReceiveBatch(int.MaxValue, BatchTimeout).ToArray();
        }

        protected override void PumpMessage(BrokeredMessage message)
        {
            HandleRequest(message);
        }

        private void HandleRequest(BrokeredMessage requestMessage)
        {
            var replyQueueName = requestMessage.ReplyTo;
            var replyQueueClient = _messagingFactory.CreateQueueClient(replyQueueName);

            var request = requestMessage.GetBody(_messageType);

            BrokeredMessage responseMessage;
            try
            {
                var response = InvokeGenericHandleMethod(_requestBroker, request);
                responseMessage = new BrokeredMessage(response);
                responseMessage.Properties.Add(MessagePropertyKeys.RequestSuccessfulKey, true);
            }
            catch (Exception exc)
            {
                responseMessage = new BrokeredMessage();
                responseMessage.Properties.Add(MessagePropertyKeys.RequestSuccessfulKey, false);
                foreach (var prop in ExceptionDetailsAsProperties(exc)) responseMessage.Properties.Add(prop.Key, prop.Value);
            }

            responseMessage.CorrelationId = requestMessage.CorrelationId;
            replyQueueClient.Send(responseMessage);
        }

        private static object InvokeGenericHandleMethod(IRequestBroker requestBroker, object request)
        {
            var handleMethod = ExtractHandlerMethodInfo(request);
            var response = handleMethod.Invoke(requestBroker, new[] {request});
            return response;
        }

        internal static MethodInfo ExtractHandlerMethodInfo(object request)
        {
            var closedGenericTypeOfIBusRequest = request.GetType()
                                                        .GetInterfaces()
                                                        .Where(t => t.IsClosedTypeOf(typeof (IBusRequest<,>)))
                                                        .Single();

            var genericArguments = closedGenericTypeOfIBusRequest.GetGenericArguments();
            var requestType = genericArguments[0];
            var responseType = genericArguments[1];

            var genericHandleMethod = typeof (IRequestBroker).GetMethod("Handle");
            var handleMethod = genericHandleMethod.MakeGenericMethod(new[] {requestType, responseType});
            return handleMethod;
        }
    }
}