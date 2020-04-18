using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;

namespace Nimbus.Transports.AzureServiceBus.ConnectionManagement
{
    public class IConnectionManager
    {
        public async Task<IMessageSender> CreateMessageSenderAsync(string queuePath)
        {
            throw new System.NotImplementedException();
        }

        public async Task<IMessageReceiver> CreateMessageReceiverAsync(string queuePath, ReceiveMode receiveAndDelete)
        {
            throw new System.NotImplementedException();
        }

        public async Task<ITopicClient> CreateTopicClient(string topicPath)
        {
            throw new System.NotImplementedException();
        }

        public async Task<ISubscriptionClient> CreateSubscriptionClient(string topicPath, string subscriptionName, ReceiveMode receiveAndDelete)
        {
            throw new System.NotImplementedException();
        }
    }
}