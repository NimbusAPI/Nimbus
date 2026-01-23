using System.Threading.Tasks;
using Apache.NMS;
using Nimbus.Transports.AMQP.ConnectionManagement;

namespace Nimbus.Transports.AMQP.QueueManagement
{
    internal interface IQueueManager
    {
        Task<IQueue> GetQueue(ISession session, string queuePath);
        Task<ITopic> GetTopic(ISession session, string topicPath);
        Task<PooledConnection> GetConnection();
    }
}
