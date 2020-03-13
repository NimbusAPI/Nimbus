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
        private readonly IMessageFactory _MessageFactory;

        public AzureServiceBusDeadLetterOffice(IQueueManager queueManager, IMessageFactory MessageFactory)
        {
            _queueManager = queueManager;
            _MessageFactory = MessageFactory;
        }

        public async Task<NimbusMessage> Peek()
        {
            var messageReceiver = await _queueManager.CreateDeadQueueMessageReceiver();

            var Message = await messageReceiver.PeekAsync();
            if (Message == null) return null;

            var nimbusMessage = await _MessageFactory.BuildNimbusMessage(Message);
            return nimbusMessage;
        }

        public async Task<NimbusMessage> Pop()
        {
            var messageReceiver = await _queueManager.CreateDeadQueueMessageReceiver();

            var Message = await messageReceiver.ReceiveAsync(TimeSpan.Zero);
            if (Message == null) return null;

            var nimbusMessage = await _MessageFactory.BuildNimbusMessage(Message);
            return nimbusMessage;
        }

        public async Task Post(NimbusMessage message)
        {
            var messageSender = await _queueManager.CreateDeadQueueMessageSender();
            var Message = await _MessageFactory.BuildMessage(message);
            await messageSender.SendAsync(Message);
        }

        public async Task<int> Count()
        {
            var messageReceiver = await _queueManager.CreateDeadQueueMessageReceiver();

            var Messages = await messageReceiver.PeekBatchAsync(int.MaxValue);
            return Messages.Count();
        }
    }
}