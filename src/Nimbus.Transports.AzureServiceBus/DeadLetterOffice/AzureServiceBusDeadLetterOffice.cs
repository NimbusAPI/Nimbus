using System;
using System.Threading.Tasks;
using Nimbus.InfrastructureContracts;
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

            var Message = await messageReceiver.PeekAsync();
            if (Message == null) return null;

            var nimbusMessage = await _brokeredMessageFactory.BuildNimbusMessage(Message);
            return nimbusMessage;
        }

        public async Task<NimbusMessage> Pop()
        {
            var messageReceiver = await _queueManager.CreateDeadQueueMessageReceiver();

            var message = await messageReceiver.ReceiveAsync(TimeSpan.FromMilliseconds(10));
            if (message == null) return null;
            
            var nimbusMessage = await _brokeredMessageFactory.BuildNimbusMessage(message);
            return nimbusMessage;
        }

        public async Task Post(NimbusMessage nimbusMessage)
        {
            var messageSender = await _queueManager.CreateDeadQueueMessageSender();
            var message = await _brokeredMessageFactory.BuildMessage(nimbusMessage);
            await messageSender.SendAsync(message);
        }

        public async Task<int> Count()
        {
            var messageReceiver = await _queueManager.CreateDeadQueueMessageReceiver();
            var messages = await messageReceiver.PeekAsync(int.MaxValue);
            return messages.Count;
        }
    }
}