using System.Text;
using Nimbus.Infrastructure.MessageSendersAndReceivers;
using Nimbus.InfrastructureContracts;
using Nimbus.Transports.Nats.ConnectionManagement;

namespace Nimbus.Transports.Nats.MessageSendersAndReceivers
{
    internal class NatsJetStreamTopicSender : INimbusMessageSender
    {
        private readonly string _topicPath;
        private readonly string _streamName;
        private readonly NatsJetStreamContextFactory _jsContextFactory;
        private readonly ISerializer _serializer;

        public NatsJetStreamTopicSender(string topicPath,
                                        NatsJetStreamContextFactory jsContextFactory,
                                        ISerializer serializer)
        {
            _topicPath = topicPath;
            _streamName = $"T_{SanitiseName(topicPath)}";
            _jsContextFactory = jsContextFactory;
            _serializer = serializer;
        }

        public async Task Send(NimbusMessage message)
        {
            await _jsContextFactory.EnsureStreamAsync(_streamName, _topicPath);
            var bytes = Encoding.UTF8.GetBytes(_serializer.Serialize(message));
            await _jsContextFactory.GetConnection().PublishAsync(_topicPath, bytes);
        }

        private static string SanitiseName(string path)
        {
            var safe = System.Text.RegularExpressions.Regex.Replace(path, @"[^a-zA-Z0-9_-]", "_");
            return safe.Length > 240 ? safe[..240] : safe;
        }
    }
}
