using System;
using System.Linq;
using System.Threading.Tasks;
using Nimbus.Transports.AzureServiceBus.BrokeredMessages;
using Nimbus.Transports.AzureServiceBus.QueueManagement;

namespace Nimbus.Transports.AzureServiceBus.DeadLetterOffice
{
    internal class AzureServiceBusDeadLetterOffice : IDeadLetterOffice
    {
        readonly IQueueManager _queueManager;
        private readonly IBrokeredMessageFactory _brokeredMessageFactory;

        public AzureServiceBusDeadLetterOffice(IQueueManager queueManager, IBrokeredMessageFactory brokeredMessageFactory)
        {
            _queueManager = queueManager;
            _brokeredMessageFactory = brokeredMessageFactory;
        }

        public async Task<NimbusMessage> Peek()
        {
            var messageReceiver = await _queueManager.CreateDeadQueueMessageReceiver();

            var brokeredMessage = await messageReceiver.PeekAsync();
            if (brokeredMessage == null) return null;

            var nimbusMessage = await _brokeredMessageFactory.BuildNimbusMessage(brokeredMessage);
            return nimbusMessage;
        }

        public async Task<NimbusMessage> Pop()
        {
            var messageReceiver = await _queueManager.CreateDeadQueueMessageReceiver();

            var brokeredMessage = await messageReceiver.ReceiveAsync(TimeSpan.Zero);
            if (brokeredMessage == null) return null;

            var nimbusMessage = await _brokeredMessageFactory.BuildNimbusMessage(brokeredMessage);
            return nimbusMessage;
        }

        public async Task Post(NimbusMessage message)
        {
            var messageSender = await _queueManager.CreateDeadQueueMessageSender();
            var brokeredMessage = await _brokeredMessageFactory.BuildBrokeredMessage(message);
            await messageSender.SendAsync(brokeredMessage);
        }

        public async Task<int> Count()
        {
            var messageReceiver = await _queueManager.CreateDeadQueueMessageReceiver();

            var brokeredMessages = await messageReceiver.PeekBatchAsync(int.MaxValue);
            return brokeredMessages.Count();
        }
    }
}