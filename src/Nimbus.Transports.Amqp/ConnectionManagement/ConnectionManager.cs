using Amqp;
using Nimbus.Configuration.Settings;

namespace Nimbus.Transports.Amqp.ConnectionManagement
{
    public interface IConnectionManager
    {
        ISenderLink CreateQueueSender(string queuePath);

        IReceiverLink CreateQueueReceiver(string queuePath);

        ISenderLink CreateTopicSender(string topicPath);

        IReceiverLink CreateTopicReceiver(string topicPath);

    }

    public class ConnectionManager : IConnectionManager
    {
        private readonly ConnectionStringSetting _connectionStringSetting;
        private readonly ApplicationNameSetting _applicationName;
        private Connection _connection;

        public ConnectionManager(ConnectionStringSetting connectionStringSetting, 
                                 ApplicationNameSetting applicationName)
        {
            _connectionStringSetting = connectionStringSetting;
            _applicationName = applicationName;
        }


        private Connection GetConnection()
        {
            if (_connection == null)
            {
                var address = new Address(_connectionStringSetting.Value);
                _connection = new Connection(address);
            }

            return _connection;
        }

        public ISenderLink CreateQueueSender(string queuePath)
        {
            var session = new Session(GetConnection());
            var link = new SenderLink(session, _applicationName, queuePath);
            return link;
        }

        public IReceiverLink CreateQueueReceiver(string queuePath)
        {
            var session = new Session(GetConnection());
            var link = new ReceiverLink(session, _applicationName, queuePath);
            return link;
        }

        public ISenderLink CreateTopicSender(string topicPath)
        {
            var session = new Session(GetConnection());
            var link = new SenderLink(session, _applicationName, topicPath);
            return link;
        }

        public IReceiverLink CreateTopicReceiver(string topicPath)
        {
            var session = new Session(GetConnection());
            var link = new ReceiverLink(session, _applicationName, topicPath);
            return link;
        }
    }
}