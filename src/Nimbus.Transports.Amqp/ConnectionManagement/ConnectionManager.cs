using System.Runtime.InteropServices;
using Amqp;
using Nimbus.Configuration.Settings;

namespace Nimbus.Transports.Amqp.ConnectionManagement
{
    public interface IConnectionManager
    {
        ISenderLink CreateMessageSender(string queuePath);
        IReceiverLink CreateMessageReceiver(string queuePath);
        
        //IMessageReceiver CreateMessageReceiver(string queuePath, ReceiveMode receiveMode)
    }
    public class ConnectionManager : IConnectionManager
    {
        private readonly ConnectionStringSetting _connectionStringSetting;
        private Connection _connection;

        public ConnectionManager(ConnectionStringSetting connectionStringSetting)
        {
            _connectionStringSetting = connectionStringSetting;
        }

        public ISenderLink CreateMessageSender(string queuePath)
        {
            var session = new Session(GetConnection());
            var link = new SenderLink(session, queuePath, queuePath);
            return link;
        }

        public IReceiverLink CreateMessageReceiver(string queuePath)
        {
            var session = new Session(GetConnection());
            var link = new ReceiverLink(session, queuePath, queuePath);
            return link;
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
    }
}