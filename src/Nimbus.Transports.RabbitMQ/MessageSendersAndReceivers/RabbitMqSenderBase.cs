using System;
using System.Threading;
using System.Threading.Tasks;
using Nimbus.Infrastructure;
using Nimbus.Infrastructure.MessageSendersAndReceivers;
using Nimbus.InfrastructureContracts;
using Nimbus.Transports.RabbitMQ.ConnectionManagement;
using Nimbus.Transports.RabbitMQ.MessageConversion;
using RabbitMQ.Client;

namespace Nimbus.Transports.RabbitMQ.MessageSendersAndReceivers
{
    internal abstract class RabbitMqSenderBase : INimbusMessageSender, IAsyncDisposable
    {
        protected readonly RabbitMqConnectionManager _connectionManager;
        protected readonly RabbitMqMessageConverter _messageConverter;
        protected readonly IClock _clock;
        protected readonly ILogger _logger;
        private readonly SemaphoreSlim _lock = new(1, 1);
        protected IChannel _channel;

        protected RabbitMqSenderBase(RabbitMqConnectionManager connectionManager,
                                      RabbitMqMessageConverter messageConverter,
                                      IClock clock,
                                      ILogger logger)
        {
            _connectionManager = connectionManager;
            _messageConverter = messageConverter;
            _clock = clock;
            _logger = logger;
        }

        public async Task Send(NimbusMessage message)
        {
            await _lock.WaitAsync();
            try
            {
                await EnsureChannelAsync();
                var (body, props) = _messageConverter.ToRabbitMq(message, _clock);
                await PublishAsync(message, body, props);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to send message to {SenderDescription}", SenderDescription);
                await ResetChannelAsync();
                throw;
            }
            finally
            {
                _lock.Release();
            }
        }

        protected abstract string SenderDescription { get; }
        protected abstract Task EnsureChannelAsync();
        protected abstract Task PublishAsync(NimbusMessage message, byte[] body, BasicProperties props);

        protected async Task ResetChannelAsync()
        {
            try { if (_channel != null) await _channel.CloseAsync(); } catch { }
            try { _channel?.Dispose(); } catch { }
            _channel = null;
        }

        public async ValueTask DisposeAsync()
        {
            await _lock.WaitAsync();
            try { await ResetChannelAsync(); }
            finally { _lock.Release(); }
            _lock.Dispose();
        }
    }
}
