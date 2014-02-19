using System;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;

namespace Nimbus.Infrastructure.MessageSendersAndReceivers
{
    internal class NimbusQueueMessageReceiver : INimbusMessageReceiver
    {
        private readonly IQueueManager _queueManager;
        private readonly string _queuePath;

        private MessageReceiver _messageReceiver;
        private readonly object _mutex = new object();

        public NimbusQueueMessageReceiver(IQueueManager queueManager, string queuePath)
        {
            _queueManager = queueManager;
            _queuePath = queuePath;
        }

        public void Start(Func<BrokeredMessage, Task> callback)
        {
            lock (_mutex)
            {
                if (_messageReceiver != null) throw new InvalidOperationException("Already started!");

                _messageReceiver = _queueManager.CreateMessageReceiver(_queuePath);
                _messageReceiver.OnMessageAsync(callback,
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
                var messageReceiver = _messageReceiver;
                if (messageReceiver == null) return;

                messageReceiver.Close();
                _messageReceiver = null;
            }
        }

        public override string ToString()
        {
            return _queuePath;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;

            Stop();
        }
    }
}