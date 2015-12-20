using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Nimbus.Configuration.Settings;
using Nimbus.Extensions;
using Nimbus.Infrastructure;
using Nimbus.Infrastructure.MessageSendersAndReceivers;
using Nimbus.Transports.WindowsServiceBus.BrokeredMessages;
using Nimbus.Transports.WindowsServiceBus.Extensions;

namespace Nimbus.Transports.WindowsServiceBus.SendersAndRecievers
{
    internal class WindowsServiceBusQueueMessageReceiver : ThrottlingMessageReceiver
    {
        private readonly IBrokeredMessageFactory _brokeredMessageFactory;
        private readonly IQueueManager _queueManager;
        private readonly string _queuePath;

        private volatile MessageReceiver _messageReceiver;
        private readonly object _mutex = new object();

        public WindowsServiceBusQueueMessageReceiver(IBrokeredMessageFactory brokeredMessageFactory,
                                          IQueueManager queueManager,
                                          string queuePath,
                                          ConcurrentHandlerLimitSetting concurrentHandlerLimit,
                                          ILogger logger)
            : base(concurrentHandlerLimit, logger)
        {
            _queueManager = queueManager;
            _queuePath = queuePath;
            _brokeredMessageFactory = brokeredMessageFactory;
        }

        public override string ToString()
        {
            return _queuePath;
        }

        protected override async Task WarmUp()
        {
            await GetMessageReceiver();
        }

        protected override async Task<NimbusMessage[]> FetchBatch(int batchSize, Task cancellationTask)
        {
            if (batchSize < 1) return new NimbusMessage[0];

            try
            {
                var messageReceiver = await GetMessageReceiver();

                var receiveTask = messageReceiver.ReceiveBatchAsync(batchSize, TimeSpan.FromSeconds(300));
                await Task.WhenAny(receiveTask, cancellationTask);
                if (cancellationTask.IsCompleted) return new NimbusMessage[0];

                var brokeredMessages = await receiveTask;

                var mappingTasks = brokeredMessages.Select(_brokeredMessageFactory.BuildNimbusMessage).ToArray();
                await mappingTasks.WhenAll();

                var nimbusMessages = mappingTasks.Select(t => t.Result).ToArray();
                return nimbusMessages;
            }
            catch (Exception exc)
            {
                if (exc.IsTransientFault()) throw;
                DiscardMessageReceiver();
                throw;
            }
        }

        private async Task<MessageReceiver> GetMessageReceiver()
        {
            if (_messageReceiver != null) return _messageReceiver;

            _messageReceiver = await _queueManager.CreateMessageReceiver(_queuePath);
            _messageReceiver.PrefetchCount = ConcurrentHandlerLimit;
            return _messageReceiver;
        }

        private void DiscardMessageReceiver()
        {
            var messageReceiver = _messageReceiver;
            _messageReceiver = null;

            if (messageReceiver == null) return;
            if (messageReceiver.IsClosed) return;

            messageReceiver.Close();
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (!disposing) return;

                DiscardMessageReceiver();
            }
            catch (MessagingEntityNotFoundException)
            {
            }
            catch (ObjectDisposedException)
            {
            }
            finally
            {
                base.Dispose(disposing);
            }
        }
    }
}