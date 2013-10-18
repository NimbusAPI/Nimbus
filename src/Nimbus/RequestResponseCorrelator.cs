using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;

namespace Nimbus
{
    public class RequestResponseCorrelator : IRequestResponseCorrelator
    {
        private readonly MessagingFactory _messagingFactory;
        private readonly string _replyQueueName;

        private readonly IDictionary<Guid, IRequestResponseWrapper> _requestWrappers = new Dictionary<Guid, IRequestResponseWrapper>();

        private bool _haveBeenToldToShutUp;

        public RequestResponseCorrelator(MessagingFactory messagingFactory, string replyQueueName)
        {
            _messagingFactory = messagingFactory;
            _replyQueueName = replyQueueName;
        }

        public async Task<TResponse> MakeCorrelatedRequest<TRequest, TResponse>(BusRequest<TRequest, TResponse> busRequest)
        {
            var queueName = busRequest.GetType().FullName;
            var sender = _messagingFactory.CreateMessageSender(queueName);

            var correlationId = Guid.NewGuid();
            var message = new BrokeredMessage(busRequest)
            {
                CorrelationId = correlationId.ToString(),
                ReplyTo = _replyQueueName,
            };

            var wrapper = new RequestResponseWrapper<TResponse>();
            _requestWrappers[correlationId] = wrapper;

            await sender.SendAsync(message);
            wrapper.Semaphore.WaitOne();

            return wrapper.Response;
        }

        public void Start()
        {
            Task.Run(() => InternalMessagePump());
        }

        public void Stop()
        {
            _haveBeenToldToShutUp = true;
        }

        private void InternalMessagePump()
        {
            var receiver = _messagingFactory.CreateMessageReceiver(_replyQueueName);

            while (! _haveBeenToldToShutUp)
            {
                var message = receiver.Receive(TimeSpan.FromSeconds(1));
                if (message == null) continue;

                var correlationId = Guid.Parse(message.CorrelationId);
                IRequestResponseWrapper wrapper;
                if (! _requestWrappers.TryGetValue(correlationId, out wrapper))
                    continue;



                var responseType = wrapper.ResponseType;
                var response = message.GetBody(responseType);

                wrapper.SetResponse(response);
                wrapper.Semaphore.Release();
            }
        }
    }
}