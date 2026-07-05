using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Nimbus.Configuration.Settings;
using Nimbus.Infrastructure;
using Nimbus.Infrastructure.MessageSendersAndReceivers;
using Nimbus.InfrastructureContracts;
using Nimbus.Transports.RabbitMQ.ConnectionManagement;
using Nimbus.Transports.RabbitMQ.MessageConversion;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Nimbus.Transports.RabbitMQ.MessageSendersAndReceivers
{
    internal abstract class RabbitMqReceiverBase : ThrottlingMessageReceiver
    {
        protected readonly RabbitMqConnectionManager _connectionManager;
        protected readonly RabbitMqMessageConverter _messageConverter;
        protected readonly ILogger _logger;
        protected IChannel _channel;
        // Body is copied eagerly in the callback: ea.Body is backed by a pooled network buffer
        // in RabbitMQ.Client 7.x and is only valid for the duration of the consumer callback.
        private Channel<(ulong DeliveryTag, byte[] Body)> _inboundChannel;

        protected RabbitMqReceiverBase(ConcurrentHandlerLimitSetting concurrentHandlerLimit,
                                        IGlobalHandlerThrottle globalHandlerThrottle,
                                        RabbitMqConnectionManager connectionManager,
                                        RabbitMqMessageConverter messageConverter,
                                        ILogger logger)
            : base(concurrentHandlerLimit, globalHandlerThrottle, logger)
        {
            _connectionManager = connectionManager;
            _messageConverter = messageConverter;
            _logger = logger;
        }

        protected override async Task WarmUp()
        {
            // Tear down previous cycle's resources before creating new ones (bus stop/start safety).
            // Completing the .NET channel first makes any in-flight TryWrite a no-op, then closing
            // the AMQP channel stops new deliveries from the broker.
            _inboundChannel?.Writer.TryComplete();
            if (_channel != null)
            {
                try { await _channel.CloseAsync(); } catch { }
                try { _channel.Dispose(); } catch { }
                _channel = null;
            }

            _inboundChannel = Channel.CreateUnbounded<(ulong, byte[])>(
                new UnboundedChannelOptions { SingleWriter = false, SingleReader = true });

            _channel = await _connectionManager.CreateChannelAsync();
            await _connectionManager.EnsureDelayedExchangeDeclaredAsync(_channel);
            await DeclareTopologyAsync(_channel);

            await _channel.BasicQosAsync(prefetchSize: 0, prefetchCount: (ushort)(int)ConcurrentHandlerLimit, global: false);

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += OnMessageReceivedAsync;
            await _channel.BasicConsumeAsync(ConsumeQueue, autoAck: false, consumer: consumer);

            _logger.Info("{ReceiverDescription} is ready", ReceiverDescription);
        }

        protected abstract Task DeclareTopologyAsync(IChannel channel);
        protected abstract string ConsumeQueue { get; }
        protected abstract string ReceiverDescription { get; }

        private Task OnMessageReceivedAsync(object sender, BasicDeliverEventArgs ea)
        {
            _inboundChannel?.Writer.TryWrite((ea.DeliveryTag, ea.Body.ToArray()));
            return Task.CompletedTask;
        }

        protected override async Task<NimbusMessage> Fetch(CancellationToken cancellationToken)
        {
            ulong deliveryTag;
            byte[] body;
            try
            {
                (deliveryTag, body) = await _inboundChannel.Reader.ReadAsync(cancellationToken);
            }
            catch (OperationCanceledException) { return null; }
            catch (ChannelClosedException) { return null; }
            catch (Exception) when (cancellationToken.IsCancellationRequested) { return null; }

            _logger.Debug("Received message from {ReceiverDescription}", ReceiverDescription);

            try
            {
                var nimbusMessage = _messageConverter.FromRabbitMq(body);
                AfterDeserialize(nimbusMessage);
                await _channel.BasicAckAsync(deliveryTag, multiple: false);
                return nimbusMessage;
            }
            catch (Exception) when (cancellationToken.IsCancellationRequested)
            {
                return null;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error fetching message from {ReceiverDescription}", ReceiverDescription);
                try { await _channel.BasicNackAsync(deliveryTag, multiple: false, requeue: false); } catch { }
                throw;
            }
        }

        protected virtual void AfterDeserialize(NimbusMessage message) { }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _inboundChannel?.Writer.TryComplete();
                base.Dispose(disposing);
                try { _channel?.CloseAsync().GetAwaiter().GetResult(); } catch { }
                try { _channel?.Dispose(); } catch { }
                return;
            }

            base.Dispose(disposing);
        }
    }
}
