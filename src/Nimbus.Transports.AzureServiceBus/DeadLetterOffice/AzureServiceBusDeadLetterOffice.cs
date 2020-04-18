using System;
using System.Threading.Tasks;
using Nimbus.InfrastructureContracts;
using Nimbus.Transports.AzureServiceBus.Messages;
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

            var Message = await messageReceiver.PeekAsync();
            if (Message == null) return null;

            var nimbusMessage = await _brokeredMessageFactory.BuildNimbusMessage(Message);
            return nimbusMessage;
        }

        public async Task<NimbusMessage> Pop()
        {
            var messageReceiver = await _queueManager.CreateDeadQueueMessageReceiver();

            var Message = await messageReceiver.ReceiveAsync(TimeSpan.Zero);
            if (Message == null) return null;

            var nimbusMessage = await _brokeredMessageFactory.BuildNimbusMessage(Message);
            return nimbusMessage;
        }

        public async Task Post(NimbusMessage message)
        {
            var messageSender = await _queueManager.CreateDeadQueueMessageSender();
            var Message = await _brokeredMessageFactory.BuildMessage(message);
            await messageSender.SendAsync(Message);
        }

        public async Task<int> Count()
        {
            var messageReceiver = await _queueManager.CreateDeadQueueMessageReceiver();

            var messages = await messageReceiver.PeekAsync(int.MaxValue);
            return messages.Count;
        }
    }
}