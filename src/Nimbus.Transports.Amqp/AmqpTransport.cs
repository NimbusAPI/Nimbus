using System;using System.Threading.Tasks;
using Nimbus.Configuration.PoorMansIocContainer;
using Nimbus.Configuration.Settings;
using Nimbus.Infrastructure;
using Nimbus.Infrastructure.Logging;
using Nimbus.Infrastructure.MessageSendersAndReceivers;
using Nimbus.InfrastructureContracts.Filtering.Conditions;
using Nimbus.Serializers.Json;
using Nimbus.Transports.Amqp.MessageSendersAndRecievers;


namespace Nimbus.Transports.Amqp
{
    internal class AmqpTransport : INimbusTransport
    {
        public AmqpTransport()
        {
        }

        public Task TestConnection()
        {
            return Task.CompletedTask;
        }

        public INimbusMessageSender GetQueueSender(string queuePath)
        {
            return new AmqpMessageSender(queuePath, new JsonSerializer());
        }

        public INimbusMessageReceiver GetQueueReceiver(string queuePath)
        {
            return new AmqpMessageReciever(queuePath, new JsonSerializer(),
                new ConcurrentHandlerLimitSetting {Value = 10},
                new GlobalHandlerThrottle(new GlobalConcurrentHandlerLimitSetting
                    {Value = 10}), new ConsoleLogger()   );
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