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
    internal class RequestMessageDispatcher : IMessageDispatcher
    {
        private readonly INimbusMessagingFactory _messagingFactory;
        private readonly Type _messageType;
        private readonly IRequestHandlerFactory _requestHandlerFactory;
        private readonly IClock _clock;

        public RequestMessageDispatcher(INimbusMessagingFactory messagingFactory, Type messageType, IRequestHandlerFactory requestHandlerFactory, IClock clock)
        {
            _messagingFactory = messagingFactory;
            _messageType = messageType;
            _requestHandlerFactory = requestHandlerFactory;
            _clock = clock;
        }

        public async Task Dispatch(BrokeredMessage message)
        {
            var replyQueueName = message.ReplyTo;
            var replyQueueClient = _messagingFactory.GetQueueSender(replyQueueName);

            var request = message.GetBody(_messageType);

            BrokeredMessage responseMessage;
            try
            {
                var response = InvokeGenericHandleMethod(_requestHandlerFactory, request);
                responseMessage = new BrokeredMessage(response);
                responseMessage.Properties.Add(MessagePropertyKeys.RequestSuccessful, true);
                responseMessage.Properties.Add(MessagePropertyKeys.MessageType, _messageType.FullName);
            }
            catch (Exception exc)
            {
                responseMessage = new BrokeredMessage();
                responseMessage.Properties.Add(MessagePropertyKeys.RequestSuccessful, false);
                foreach (var prop in exc.ExceptionDetailsAsProperties(_clock.UtcNow)) responseMessage.Properties.Add(prop.Key, prop.Value);
            }

            responseMessage.CorrelationId = message.CorrelationId;
            await replyQueueClient.Send(responseMessage);
        }

        private static object InvokeGenericHandleMethod(IRequestHandlerFactory requestHandlerFactory, object request)
        {
            // We can't use dynamic dispatch here as the DLR isn't so great at figuring things out when we have
            // multiple generic parameters.  -andrewh 19/01/2014
            var handleMethod = ExtractHandlerMethodInfo(request);
            var response = handleMethod.Invoke(requestHandlerFactory, new[] {request});
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

            var genericHandleMethod = typeof (IRequestHandlerFactory).GetMethod("Handle");
            var handleMethod = genericHandleMethod.MakeGenericMethod(new[] {requestType, responseType});
            return handleMethod;
        }
    }
}