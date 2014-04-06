using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Nimbus.Extensions;

namespace Nimbus.Infrastructure.MessageSendersAndReceivers
{
    internal abstract class BatchingMessageSender : INimbusMessageSender
    {
        private readonly List<BrokeredMessage> _outboundQueue = new List<BrokeredMessage>();
        private readonly object _sendingMutex = new object();

        protected abstract void SendBatch(BrokeredMessage[] messages);

        public Task Send(BrokeredMessage message)
        {
            return Task.Run(() =>
                            {
                                lock (_outboundQueue)
                                {
                                    _outboundQueue.Add(message);
                                    TriggerMessageFlush();
                                }
                            });
        }

        private void TriggerMessageFlush()
        {
            Task.Run((Action) (FlushMessages));
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

            lock (_sendingMutex)
            {
                try
                {
                    SendBatch(toSend);
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

        public abstract void Dispose();
    }
}