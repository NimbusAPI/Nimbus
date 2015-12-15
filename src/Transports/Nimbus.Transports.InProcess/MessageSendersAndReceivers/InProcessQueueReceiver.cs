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
        private readonly BlockingCollection<NimbusMessage> _queue;
        private CancellationTokenSource _cancellationTokenSource;

        public InProcessQueueReceiver(BlockingCollection<NimbusMessage> queue)
        {
            _queue = queue;
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

        private void ReceiveMessages(Func<NimbusMessage, Task> callback)
        {
            var cancellationTokenSource = _cancellationTokenSource;
            if (cancellationTokenSource == null) return;    // already cancelled

            while (!cancellationTokenSource.IsCancellationRequested)
            {
                try
                {
                    var nimbusMessage = _queue.Take(cancellationTokenSource.Token);
                    callback(nimbusMessage);
                }
                catch (OperationCanceledException)
                {
                }
            }
        }
    }
}