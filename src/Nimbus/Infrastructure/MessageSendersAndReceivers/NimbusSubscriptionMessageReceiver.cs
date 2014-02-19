using System;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Nimbus.Extensions;

namespace Nimbus.Infrastructure.MessageSendersAndReceivers
{
    internal class NimbusSubscriptionMessageReceiver : INimbusMessageReceiver
    {
        private readonly IQueueManager _queueManager;
        private readonly string _topicPath;
        private readonly string _subscriptionName;

        private SubscriptionClient _subscriptionClient;
        private readonly object _mutex = new object();

        public NimbusSubscriptionMessageReceiver(IQueueManager queueManager, string topicPath, string subscriptionName)
        {
            _queueManager = queueManager;
            _topicPath = topicPath;
            _subscriptionName = subscriptionName;
        }

        public void Start(Func<BrokeredMessage, Task> callback)
        {
            lock (_mutex)
            {
                if (_subscriptionClient != null) throw new InvalidOperationException("Already started!");
                _subscriptionClient = _queueManager.CreateSubscriptionReceiver(_topicPath, _subscriptionName);

                _subscriptionClient.OnMessageAsync(callback,
                                                   new OnMessageOptions
                                                   {
                                                       AutoComplete = false,
                                                       MaxConcurrentCalls = Environment.ProcessorCount,
                                                   });
            }
        }

        public void Stop()
        {
            lock (_mutex)
            {
                var subscriptionClient = _subscriptionClient;
                if (subscriptionClient == null) return;

                subscriptionClient.Close();
                _subscriptionClient = null;
            }
        }

        public override string ToString()
        {
            return "{0}/{1}".FormatWith(_topicPath, _subscriptionName);
        }

        public void Dispose()
        {
            Stop();
        }
    }
}