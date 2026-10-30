using System.Threading.Tasks;
using Apache.NMS;

namespace Nimbus.Transports.AMQP.QueueManagement
{
    internal interface IQueueManager
    {
        Task<IQueue> GetQueue(ISession session, string queuePath);
        Task<ITopic> GetTopic(ISession session, string topicPath);
        Task<ISession> CreateSession(AcknowledgementMode acknowledgementMode);
        Task<IMessageConsumer> CreateDurableConsumer(ISession session, ITopic topic, string subscriptionName, string selector);
        void CloseDurableConsumer(IMessageConsumer consumer);
    }
}
