namespace Nimbus.Transports.AzureServiceBus2.DeadLetterOffice
{
    using System;
    using System.Threading.Tasks;
    using Nimbus.InfrastructureContracts;
    using Nimbus.Transports.AzureServiceBus2.BrokeredMessages;
    using Nimbus.Transports.AzureServiceBus2.QueueManagement;

    internal class AzureServiceBusDeadLetterOffice : IDeadLetterOffice
    {
        readonly IQueueManager _queueManager;
        private readonly IBrokeredMessageFactory _brokeredMessageFactory;

        public AzureServiceBusDeadLetterOffice(IQueueManager queueManager, IBrokeredMessageFactory brokeredMessageFactory)
        {
            this._queueManager = queueManager;
            this._brokeredMessageFactory = brokeredMessageFactory;
        }

        public async Task<NimbusMessage> Peek()
        {
            var messageReceiver = await this._queueManager.CreateDeadQueueMessageReceiver();

            var message = await messageReceiver.PeekMessageAsync();
            if (message == null) return null;

            var nimbusMessage = await this._brokeredMessageFactory.BuildNimbusMessage(message);
            return nimbusMessage;
        }

        public async Task<NimbusMessage> Pop()
        {
            var messageReceiver = await this._queueManager.CreateDeadQueueMessageReceiver();

            var message = await messageReceiver.ReceiveMessageAsync(TimeSpan.FromMilliseconds(10));
            if (message == null) return null;
            
            var nimbusMessage = await this._brokeredMessageFactory.BuildNimbusMessage(message);
            return nimbusMessage;
        }

        public async Task Post(NimbusMessage nimbusMessage)
        {
            var messageSender = await this._queueManager.CreateDeadQueueMessageSender();
            var message = await this._brokeredMessageFactory.BuildMessage(nimbusMessage);
            await messageSender.SendMessageAsync(message);
        }

        public async Task<int> Count()
        {
            var messageReceiver = await this._queueManager.CreateDeadQueueMessageReceiver();
            var messages = await messageReceiver.PeekMessagesAsync(int.MaxValue);
            return messages.Count;
        }
    }
}