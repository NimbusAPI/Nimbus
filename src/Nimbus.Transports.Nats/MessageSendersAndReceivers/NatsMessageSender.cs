using System.Text;
using Nimbus.Infrastructure.MessageSendersAndReceivers;
using Nimbus.InfrastructureContracts;
using Nimbus.Transports.Nats.ConnectionManagement;

namespace Nimbus.Transports.Nats.MessageSendersAndReceivers;

internal class NatsMessageSender : INimbusMessageSender
{
    private readonly string _topicPath;
    private readonly NatsConnectionFactory _connectionFactory;
    private readonly ISerializer _serializer;

    public NatsMessageSender(string topicPath, NatsConnectionFactory connectionFactory, ISerializer serializer)
    {
        _topicPath = topicPath;
        _connectionFactory = connectionFactory;
        _serializer = serializer;
    }

    public async Task Send(NimbusMessage message)
    {
        var serialized = _serializer.Serialize(message);
        var bytes = Encoding.UTF8.GetBytes(serialized);
        var connection = _connectionFactory.GetConnection();
        await connection.PublishAsync(_topicPath, bytes);
    }
}