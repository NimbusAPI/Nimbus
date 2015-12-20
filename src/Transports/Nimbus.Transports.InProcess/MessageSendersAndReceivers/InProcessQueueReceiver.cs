using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Nimbus.Infrastructure;
using Nimbus.Infrastructure.MessageSendersAndReceivers;

namespace Nimbus.Transports.InProcess.MessageSendersAndReceivers
{
    internal class InProcessQueueReceiver : INimbusMessageReceiver
    {
        private readonly string _queuePath;
        private readonly BlockingCollection<NimbusMessage> _messageQueue;
        private CancellationTokenSource _cancellationTokenSource;

        public InProcessQueueReceiver(string queuePath, BlockingCollection<NimbusMessage> messageQueue)
        {
            _queuePath = queuePath;
            _messageQueue = messageQueue;
        }

        public Task Start(Func<NimbusMessage, Task> callback)
        {
            return Task.Run(() =>
                            {
                                _cancellationTokenSource = new CancellationTokenSource();
                                Task.Run(() => ReceiveMessages(callback));
                            });
        }

        public Task Stop()
        {
            return Task.Run(() =>
                            {
                                if (_cancellationTokenSource == null) return;
                                _cancellationTokenSource.Cancel();
                                _cancellationTokenSource = null;
                            });
        }

        private async Task ReceiveMessages(Func<NimbusMessage, Task> callback)
        {
            var cancellationTokenSource = _cancellationTokenSource;
            if (cancellationTokenSource == null) return; // already cancelled

            while (!cancellationTokenSource.IsCancellationRequested)
            {
                try
                {
                    var nimbusMessage = _messageQueue.Take(cancellationTokenSource.Token);
                    nimbusMessage.ReceivedFromPath = _queuePath;
                    await callback(nimbusMessage);
                }
                catch (OperationCanceledException)
                {
                }
            }
        }
    }
}