using System.Collections.Concurrent;
using NATS.Client.JetStream;
using NATS.Client.JetStream.Models;
using NATS.Client.Core;

namespace Nimbus.Transports.Nats.ConnectionManagement
{
    internal class NatsJetStreamContextFactory
    {
        private readonly NatsConnectionFactory _connectionFactory;
        private readonly SemaphoreSlim _contextLock = new(1, 1);
        private readonly ConcurrentDictionary<string, byte> _ensuredStreams = new();
        private INatsJSContext? _jsContext;

        public NatsJetStreamContextFactory(NatsConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public NatsConnection GetConnection() => _connectionFactory.GetConnection();

        public async Task<INatsJSContext> GetContextAsync(CancellationToken ct = default)
        {
            if (_jsContext != null) return _jsContext;
            await _contextLock.WaitAsync(ct);
            try
            {
                if (_jsContext != null) return _jsContext;
                var conn = _connectionFactory.GetConnection();
                await conn.ConnectAsync();
                _jsContext = new NatsJSContext(conn);
                return _jsContext;
            }
            finally
            {
                _contextLock.Release();
            }
        }

        public async Task EnsureStreamAsync(string streamName, string subject, CancellationToken ct = default)
        {
            if (_ensuredStreams.ContainsKey(streamName)) return;
            var ctx = await GetContextAsync(ct);
            await ctx.CreateOrUpdateStreamAsync(new StreamConfig { Name = streamName, Subjects = [subject] }, ct);
            _ensuredStreams.TryAdd(streamName, 0);
        }

        public async Task<INatsJSConsumer> EnsureConsumerAsync(string streamName, ConsumerConfig config, CancellationToken ct = default)
        {
            var ctx = await GetContextAsync(ct);
            return await ctx.CreateOrUpdateConsumerAsync(streamName, config, ct);
        }
    }
}
