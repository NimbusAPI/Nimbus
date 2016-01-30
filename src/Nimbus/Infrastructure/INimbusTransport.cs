using System.Threading.Tasks;
using Nimbus.Infrastructure.MessageSendersAndReceivers;

namespace Nimbus.Infrastructure
{
    internal interface INimbusTransport
    {
        Task TestConnection();

        INimbusMessageSender GetQueueSender(string queuePath);
        INimbusMessageReceiver GetQueueReceiver(string queuePath);

        INimbusMessageSender GetTopicSender(string topicPath);
        INimbusMessageReceiver GetTopicReceiver(string topicPath, string subscriptionName);

        //FIXME add remaining transport-level functionality here and stop exposing it via the container
    }
}