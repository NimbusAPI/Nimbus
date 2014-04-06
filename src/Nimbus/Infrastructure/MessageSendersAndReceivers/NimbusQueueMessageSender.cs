using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Nimbus.Extensions;

namespace Nimbus.Infrastructure.MessageSendersAndReceivers
{
    internal class NimbusQueueMessageSender : INimbusMessageSender
    {
        private readonly IQueueManager _queueManager;
        private readonly string _queuePath;

        private readonly Lazy<MessageSender> _queueClient;
        private readonly List<BrokeredMessage> _outboundQueue = new List<BrokeredMessage>();

        public NimbusQueueMessageSender(IQueueManager queueManager, string queuePath)
        {
            _queueManager = queueManager;
            _queuePath = queuePath;

            _queueClient = new Lazy<MessageSender>(() => _queueManager.CreateMessageSender(_queuePath));
        }

        public Task Send(BrokeredMessage message)
        {
            return Task.Run(() =>
                            {
                                lock (_outboundQueue)
                                {
                                    _outboundQueue.Add(message);
                                    Console.WriteLine("{0} pending messages in outbound queue for {1}", _outboundQueue.Count, _queuePath);
                                    TriggerMessageFlush();
                                }
                            });
        }

        private void TriggerMessageFlush()
        {
            Task.Run(() => FlushMessages());
        }

        private void FlushMessages()
        {
            BrokeredMessage[] toSend;

            lock (_outboundQueue)
            {
                toSend = _outboundQueue.Take(100).ToArray();
                _outboundQueue.RemoveRange(0, toSend.Length);
                if (_outboundQueue.Any()) TriggerMessageFlush();
            }

            if (toSend.None()) return;

            lock (_queueClient.Value)
            {
                Console.WriteLine("Flushing outbound message queue {1} ({0} messages)", toSend.Length, _queuePath);
                try
                {
                    _queueClient.Value.SendBatch(toSend);
                }
                catch (Exception)
                {
                    lock (_outboundQueue)
                    {
                        _outboundQueue.AddRange(toSend);
                    }
                }
            }
        }

        public void Dispose()
        {
            if (!_queueClient.IsValueCreated) return;
            _queueClient.Value.Close();
        }
    }
}