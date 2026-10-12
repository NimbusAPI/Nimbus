using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nimbus.InfrastructureContracts;
using Nimbus.Transports.RabbitMQ.ConnectionManagement;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace Nimbus.Transports.RabbitMQ.MessageSendersAndReceivers
{
    internal static class RabbitMqSubscriptionQueue
    {
        // x-expires makes the broker delete the queue (and its bindings) once it has had no consumers for the
        // idle window, which cleans up the per-instance queues orphaned by every restart.
        // Every declaration of a subscription queue must use these same arguments, or the broker rejects it with 406.
        public static async Task<IChannel> DeclareAsync(RabbitMqConnectionManager connectionManager,
                                                        IChannel channel,
                                                        string subscriptionName,
                                                        TimeSpan autoDeleteOnIdle,
                                                        ILogger logger)
        {
            var arguments = new Dictionary<string, object> {{"x-expires", (int) autoDeleteOnIdle.TotalMilliseconds}};

            try
            {
                await channel.QueueDeclareAsync(subscriptionName, durable: true, exclusive: false, autoDelete: false, arguments: arguments);
                return channel;
            }
            catch (OperationInterruptedException exc) when (exc.ShutdownReason?.ReplyCode == 406)
            {
                // The queue predates idle cleanup and was declared without x-expires. The broker closed the channel,
                // so carry on over a new one and attach to the existing queue as-is.
                logger.Warn("Queue {Queue} already exists without x-expires, so it will not be removed when idle. Delete it, or apply an 'expires' policy, to enable idle cleanup.", subscriptionName);

                try { channel.Dispose(); } catch { }
                channel = await connectionManager.CreateChannelAsync();
                await channel.QueueDeclarePassiveAsync(subscriptionName);
                return channel;
            }
        }
    }
}
