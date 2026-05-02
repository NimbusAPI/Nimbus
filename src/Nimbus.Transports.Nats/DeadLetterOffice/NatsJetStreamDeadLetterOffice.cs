using System.Text;
using NATS.Client.JetStream;
using NATS.Client.JetStream.Models;
using Nimbus.InfrastructureContracts;
using Nimbus.Transports.Nats.ConnectionManagement;

namespace Nimbus.Transports.Nats.DeadLetterOffice
{
    internal class NatsJetStreamDeadLetterOffice : IDeadLetterOffice
    {
        private const string StreamName = "NIMBUS_DEADLETTER";
        private const string Subject = "nimbus.deadletter";
        private const string ConsumerName = "nimbus-deadletter";

        private readonly NatsJetStreamContextFactory _jsContextFactory;
        private readonly ISerializer _serializer;
        private readonly SemaphoreSlim _initLock = new(1, 1);
        private INatsJSConsumer? _consumer;

        public NatsJetStreamDeadLetterOffice(NatsJetStreamContextFactory jsContextFactory, ISerializer serializer)
        {
            _jsContextFactory = jsContextFactory;
            _serializer = serializer;
        }

        public async Task Post(NimbusMessage message)
        {
            await _jsContextFactory.EnsureDeadLetterStreamAsync(StreamName, Subject);
            var bytes = Encoding.UTF8.GetBytes(_serializer.Serialize(message));
            await _jsContextFactory.PublishAsync(Subject, bytes);
        }

        public async Task<NimbusMessage> Pop()
        {
            await EnsureConsumerAsync();
            await foreach (var msg in _consumer!.FetchAsync<byte[]>(new NatsJSFetchOpts { MaxMsgs = 1, Expires = TimeSpan.FromSeconds(1) }))
            {
                await msg.AckAsync();
                if (msg.Data == null) return null!;
                return (NimbusMessage)_serializer.Deserialize(Encoding.UTF8.GetString(msg.Data), typeof(NimbusMessage));
            }
            return null!;
        }

        public async Task<NimbusMessage> Peek()
        {
            await _jsContextFactory.EnsureDeadLetterStreamAsync(StreamName, Subject);
            var stream = await _jsContextFactory.GetStreamAsync(StreamName);
            await stream.RefreshAsync();
            if (stream.Info.State.Messages == 0) return null!;

            var consumer = await stream.CreateOrderedConsumerAsync(new NatsJSOrderedConsumerOpts
            {
                FilterSubjects = [Subject],
                DeliverPolicy = ConsumerConfigDeliverPolicy.All,
            });
            try
            {
                await foreach (var msg in consumer.FetchAsync<byte[]>(new NatsJSFetchOpts { MaxMsgs = 1, Expires = TimeSpan.FromSeconds(2) }))
                {
                    if (msg.Data == null) return null!;
                    return (NimbusMessage)_serializer.Deserialize(Encoding.UTF8.GetString(msg.Data), typeof(NimbusMessage));
                }
                return null!;
            }
            finally
            {
                if (consumer is IAsyncDisposable d) await d.DisposeAsync();
            }
        }

        public async Task Purge()
        {
            await _jsContextFactory.EnsureDeadLetterStreamAsync(StreamName, Subject);
            var stream = await _jsContextFactory.GetStreamAsync(StreamName);
            await stream.PurgeAsync(new StreamPurgeRequest());
        }

        public async Task<int> Count()
        {
            await _jsContextFactory.EnsureDeadLetterStreamAsync(StreamName, Subject);
            var stream = await _jsContextFactory.GetStreamAsync(StreamName);
            await stream.RefreshAsync();
            return (int)stream.Info.State.Messages;
        }

        private async Task EnsureConsumerAsync()
        {
            if (_consumer != null) return;
            await _initLock.WaitAsync();
            try
            {
                if (_consumer != null) return;
                await _jsContextFactory.EnsureDeadLetterStreamAsync(StreamName, Subject);
                _consumer = await _jsContextFactory.EnsureConsumerAsync(StreamName, new ConsumerConfig
                {
                    Name = ConsumerName,
                    DurableName = ConsumerName,
                    FilterSubject = Subject,
                    AckPolicy = ConsumerConfigAckPolicy.Explicit,
                    DeliverPolicy = ConsumerConfigDeliverPolicy.All,
                });
            }
            finally
            {
                _initLock.Release();
            }
        }
    }
}
