using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Nimbus.Extensions;
using Nimbus.MessageContracts.Exceptions;

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

            try
            {
                await Task.Delay(TimeSpan.FromMilliseconds(100));
                await _sendingSemaphore.WaitAsync();

                BrokeredMessage[] toSend;
                lock (_enqueuingMutex)
                {
                    toSend = _outboundQueue.ToArray();
                    _outboundQueue.Clear();
                    _pendingFlushTask = null;
                }
                if (toSend.None()) return;

                for (var retries = 0; retries < 3; retries++)
                {
                    try
                    {
                        await SendBatch(toSend);
                        return;
                    }
                    catch (Exception ex)
                    {
                        if (ex.IsTransientFault())
                        {
                            _logger.Warn("Going to retry after {0} was thrown sending batch: {1}, {2}", ex.GetType().Name, ex.Message, ex.ToString());
                        }
                        else
                        {
                            throw;
                        }
                    }

                    toSend = toSend
                        .Select(m => m.Clone())
                        .ToArray();

                    await Task.Delay(TimeSpan.FromSeconds(retries*2));
                }

                throw new BusException("Retry count exceeded while sending message batch.");
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