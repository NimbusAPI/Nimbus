using Microsoft.ServiceBus.Messaging;
using Nimbus.Configuration.Settings;
using Nimbus.InfrastructureContracts;

namespace Nimbus.Infrastructure.RequestResponse
{
    public class ResponseMessagePumpFactory
    {
        private readonly MessagingFactory _messagingFactory;
        private readonly ILogger _logger;
        private readonly ReplyQueueNameSetting _replyQueueName;
        private readonly ResponseMessagePumpDispatcher _dispatcher;

        internal ResponseMessagePumpFactory(MessagingFactory messagingFactory, ReplyQueueNameSetting replyQueueName, ResponseMessagePumpDispatcher dispatcher, ILogger logger)
        {
            _messagingFactory = messagingFactory;
            _logger = logger;
            _replyQueueName = replyQueueName;
            _dispatcher = dispatcher;
        }

        public IMessagePump Create()
        {
            var messageReceiver = _messagingFactory.CreateMessageReceiver(_replyQueueName);
            var receiver = new NimbusMessageReceiver(messageReceiver);
            var pump = new MessagePump(receiver, _dispatcher, _logger);
            return pump;
        }
    }

    //FIXME work in progress.
    //public class NimbusMessageReceiverFactory
    //{
    //    public NimbusMessageReceiver CreateQueueReceiver(string queuePath)
    //    {
    //    }

    //    public NimbusMessageReceiver CreateSubscriptionReceiver(string topicPath, string subscriptionName)
    //    {
    //    }
    //}
}