using System;
using System.Threading.Tasks;
using Nimbus.Configuration.Settings;
using Nimbus.Infrastructure;
using Nimbus.Infrastructure.MessageSendersAndReceivers;
using Nimbus.InfrastructureContracts;
using Nimbus.InfrastructureContracts.Filtering.Conditions;
using Nimbus.Transports.Amqp.SendersAndReceivers;

namespace Nimbus.Transports.Amqp
{
    internal class AmqpTransport : INimbusTransport
    {
        private readonly ISerializer _serializer;
        private readonly ILogger _logger;

        public AmqpTransport(ISerializer serializer, ILogger logger)
        {
            _serializer = serializer;
            _logger = logger;
        }

        public Task TestConnection()
        {
            return Task.CompletedTask;
        }

        public INimbusMessageSender GetQueueSender(string queuePath)
        {
            return new AmqpMessageSender(queuePath, _serializer);
        }

        public INimbusMessageReceiver GetQueueReceiver(string queuePath)
        {
            return new AmqpMessageReceiver(queuePath, _serializer,
                                           new ConcurrentHandlerLimitSetting {Value = 10},
                                           new GlobalHandlerThrottle(new GlobalConcurrentHandlerLimitSetting
                                                                     {Value = 10}), _logger   );
        }

        public INimbusMessageSender GetTopicSender(string topicPath)
        {
            throw new NotImplementedException();
        }

        public INimbusMessageReceiver GetTopicReceiver(string topicPath, string subscriptionName, IFilterCondition filter)
        {
            throw new NotImplementedException();
        }
    }
}