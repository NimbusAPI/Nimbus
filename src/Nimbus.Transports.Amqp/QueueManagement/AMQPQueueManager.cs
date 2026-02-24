using System.Threading.Tasks;
using Apache.NMS;
using Nimbus.InfrastructureContracts;
using Nimbus.Transports.AMQP.ConnectionManagement;

namespace Nimbus.Transports.AMQP.QueueManagement
{
    internal class AMQPQueueManager : IQueueManager
    {
        private readonly NmsConnectionManager _connectionManager;
        private readonly ILogger _logger;

        public AMQPQueueManager(NmsConnectionManager connectionManager, ILogger logger)
        {
            _connectionManager = connectionManager;
            _logger = logger;
        }

        public Task<IQueue> GetQueue(ISession session, string queuePath)
        {
            return Task.Run(() =>
            {
                _logger.Debug("Getting queue: {QueuePath}", queuePath);
                var queue = session.GetQueue(queuePath);
                return queue;
            });
        }

        public Task<ITopic> GetTopic(ISession session, string topicPath)
        {
            return Task.Run(() =>
            {
                _logger.Debug("Getting topic: {TopicPath}", topicPath);
                var topic = session.GetTopic(topicPath);
                return topic;
            });
        }

        public Task<ISession> CreateSession(AcknowledgementMode acknowledgementMode)
        {
            return _connectionManager.CreateSession(acknowledgementMode);
        }
    }
}
