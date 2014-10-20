using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Nimbus.Extensions;

namespace Nimbus.Infrastructure.MessageSendersAndReceivers
{
    internal abstract class BatchingMessageSender : INimbusMessageSender
    {
        private const int _maxConcurrentFlushTasks = 10;
        private const int _maximumBatchSize = 100;

        private readonly List<BrokeredMessage> _outboundQueue = new List<BrokeredMessage>();
        private bool _disposed;

        private readonly object _mutex = new object();
        private readonly SemaphoreSlim _sendingSemaphore = new SemaphoreSlim(_maxConcurrentFlushTasks, _maxConcurrentFlushTasks);

        private Task _lazyFlushTask;

        protected abstract Task SendBatch(BrokeredMessage[] messages);

        public Task Send(BrokeredMessage message)
        {
            lock (_mutex)
            {
                _outboundQueue.Add(message);

                if (_outboundQueue.Count >= _maximumBatchSize)
                {
                    return DoBatchSendNow();
                }

                if (_lazyFlushTask == null)
                {
                    _lazyFlushTask = FlushMessagesLazily();
                }
                return _lazyFlushTask;
            }
        }

        private async Task FlushMessagesLazily()
        {
            if (_disposed) return;

            await Task.Delay(TimeSpan.FromMilliseconds(100)); // sleep *after* we grab a semaphore to allow messages to be batched
            _lazyFlushTask = null;
            await DoBatchSendNow();
        }

        private async Task DoBatchSendNow()
        {
            await _sendingSemaphore.WaitAsync();
            try
            {
                BrokeredMessage[] toSend;
                lock (_mutex)
                {
                    toSend = _outboundQueue.Take(_maximumBatchSize).ToArray();
                    _outboundQueue.RemoveRange(0, toSend.Length);
                }
                if (toSend.None()) return;

                await SendBatch(toSend);
            }
            finally
            {
                _sendingSemaphore.Release();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            _disposed = true;
        }
    }
}