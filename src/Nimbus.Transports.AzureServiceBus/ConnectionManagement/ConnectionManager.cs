using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Nimbus.Configuration.Settings;

namespace Nimbus.Transports.AzureServiceBus.ConnectionManagement
{
    public class ConnectionManager : IConnectionManager
    {
        private readonly ConnectionStringSetting _connectionStringSetting;

        public ConnectionManager(ConnectionStringSetting connectionStringSetting)
        {
            _connectionStringSetting = connectionStringSetting;
        }

        public IMessageSender CreateMessageSender(string queuePath)
        {
            return new MessageSender(_connectionStringSetting, queuePath);
        }

        public IMessageReceiver CreateMessageReceiver(string queuePath, ReceiveMode receiveMode)
        {
            return new MessageReceiver(_connectionStringSetting, queuePath, receiveMode);
        }

        public ITopicClient CreateTopicClient(string topicPath)
        {
            return new TopicClient(_connectionStringSetting, topicPath);
        }

        public ISubscriptionClient CreateSubscriptionClient(string topicPath, string subscriptionName, ReceiveMode receiveMode)
        {
            return new SubscriptionClient(_connectionStringSetting, topicPath, subscriptionName, receiveMode);
        }
    }
}