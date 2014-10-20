using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Nimbus.Extensions;

namespace Nimbus.Infrastructure.MessageSendersAndReceivers
{
    internal abstract class BatchingMessageSender : INimbusMessageSender
    {
        private const int _maxConcurrentFlushTasks = 10;

        private readonly ILogger _logger;
        private readonly List<BrokeredMessage> _outboundQueue = new List<BrokeredMessage>();
        private bool _disposed;

        private readonly object _enqueuingMutex = new object();
        private readonly SemaphoreSlim _sendingSemaphore = new SemaphoreSlim(_maxConcurrentFlushTasks, _maxConcurrentFlushTasks);

        private Task _pendingFlushTask;

        protected BatchingMessageSender(ILogger logger)
        {
            _logger = logger;
        }

        protected abstract Task SendBatch(BrokeredMessage[] messages);

        public async Task Send(BrokeredMessage message)
        {
            var clone = message.Clone();

            Task taskToAwait;
            lock (_enqueuingMutex)
            {
                _outboundQueue.Add(message);
                taskToAwait = GetOrCreateNextFlushTask();
            }

            await taskToAwait;
        }

        private Task GetOrCreateNextFlushTask()
        {
            lock (_enqueuingMutex)
            {
                if (_pendingFlushTask == null)
                {
                    _pendingFlushTask = FlushMessages();
                }

                return _pendingFlushTask;
            }
        }

        private async Task FlushMessages()
        {
            if (_disposed) return;

            await _sendingSemaphore.WaitAsync();
            await Task.Delay(TimeSpan.FromMilliseconds(100));   // sleep *after* we grab a semaphore to allow messages to be batched
            try
            {
                BrokeredMessage[] toSend;
                lock (_enqueuingMutex)
                {
                    toSend = _outboundQueue.ToArray();
                    _outboundQueue.Clear();
                    _pendingFlushTask = null;
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