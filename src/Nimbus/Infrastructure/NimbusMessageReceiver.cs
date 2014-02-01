using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;

namespace Nimbus.Infrastructure
{
    /// <remarks>
    ///     We *could* have just taken a dependeny on the IMessageReceiver interface but it's internal so no can do :(
    /// </remarks>
    internal class NimbusMessageReceiver : INimbusMessageReceiver
    {
        private readonly SubscriptionClient _subscriptionClient;
        private readonly MessageReceiver _messageReceiver;

        public NimbusMessageReceiver(MessageReceiver messageReceiver)
        {
            _messageReceiver = messageReceiver;
        }

        public NimbusMessageReceiver(SubscriptionClient subscriptionClient)
        {
            _subscriptionClient = subscriptionClient;
        }

        public async Task<BrokeredMessage> Receive()
        {
            return _messageReceiver != null
                ? await _messageReceiver.ReceiveAsync()
                : await _subscriptionClient.ReceiveAsync();
        }
    }
}