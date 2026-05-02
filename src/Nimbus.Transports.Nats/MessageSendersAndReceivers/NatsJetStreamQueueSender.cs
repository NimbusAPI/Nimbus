using System.Text;
using NATS.Client.JetStream.Models;
using Nimbus.Infrastructure.MessageSendersAndReceivers;
using Nimbus.InfrastructureContracts;
using Nimbus.Transports.Nats.ConnectionManagement;

namespace Nimbus.Transports.Nats.MessageSendersAndReceivers
{
    internal class NatsJetStreamQueueSender : INimbusMessageSender
    {
        private readonly string _queuePath;
        private readonly string _streamName;
        private readonly NatsJetStreamContextFactory _jsContextFactory;
        private readonly ISerializer _serializer;

        public NatsJetStreamQueueSender(string queuePath,
                                        NatsJetStreamContextFactory jsContextFactory,
                                        ISerializer serializer)
        {
            _queuePath = queuePath;
            _streamName = SanitiseName(queuePath);
            _jsContextFactory = jsContextFactory;
            _serializer = serializer;
        }

        public async Task Send(NimbusMessage message)
        {
            await _jsContextFactory.EnsureStreamAsync(_streamName, _queuePath, StreamConfigRetention.Workqueue);
            var bytes = Encoding.UTF8.GetBytes(_serializer.Serialize(message));
            await _jsContextFactory.PublishAsync(_queuePath, bytes);
        }

        private static string SanitiseName(string path) => NatsNameSanitiser.Sanitise(path);
    }
}
