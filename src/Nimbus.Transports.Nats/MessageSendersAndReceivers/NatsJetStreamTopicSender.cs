using System.Text;
using Nimbus.Configuration.Settings;
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
        private readonly AutoDeleteOnIdleSetting _autoDeleteOnIdle;

        public NatsJetStreamTopicSender(string topicPath,
                                        NatsJetStreamContextFactory jsContextFactory,
                                        ISerializer serializer,
                                        AutoDeleteOnIdleSetting autoDeleteOnIdle)
        {
            _topicPath = topicPath;
            _streamName = SanitiseName(topicPath);
            _jsContextFactory = jsContextFactory;
            _serializer = serializer;
            _autoDeleteOnIdle = autoDeleteOnIdle;
        }

        public async Task Send(NimbusMessage message)
        {
            await _jsContextFactory.EnsureStreamAsync(_streamName, _topicPath, maxAge: _autoDeleteOnIdle.Value);
            var bytes = Encoding.UTF8.GetBytes(_serializer.Serialize(message));
            await _jsContextFactory.PublishAsync(_topicPath, bytes);
        }

        private static string SanitiseName(string path) => NatsNameSanitiser.Sanitise(path);
    }
}
