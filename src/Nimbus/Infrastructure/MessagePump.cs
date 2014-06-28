using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Nimbus.Extensions;
using Nimbus.Infrastructure.MessageSendersAndReceivers;

namespace Nimbus.Infrastructure
{
    [DebuggerDisplay("{_receiver}")]
    internal class MessagePump : IMessagePump
    {
        private readonly IClock _clock;
        private readonly ILogger _logger;
        private readonly IMessageDispatcher _messageDispatcher;
        private readonly INimbusMessageReceiver _receiver;

        private bool _started;
        private readonly SemaphoreSlim _startStopSemaphore = new SemaphoreSlim(1, 1);

        public MessagePump(
            IClock clock,
            ILogger logger,
            IMessageDispatcher messageDispatcher,
            INimbusMessageReceiver receiver)
        {
            _clock = clock;
            _logger = logger;
            _messageDispatcher = messageDispatcher;
            _receiver = receiver;
        }

        public async Task Start()
        {
            try
            {
                await _startStopSemaphore.WaitAsync();

                if (_started) return;
                _started = true;

                _logger.Debug("Message pump for {0} starting...", _receiver);
                await _receiver.Start(Dispatch);
                _logger.Debug("Message pump for {0} started", _receiver);
            }
            finally
            {
                _startStopSemaphore.Release();
            }
        }

        public async Task Stop()
        {
            try
            {
                await _startStopSemaphore.WaitAsync();

                if (!_started) return;
                _started = false;

                _logger.Debug("Message pump for {0} stopping...", _receiver);
                await _receiver.Stop();
                _logger.Debug("Message pump for {0} stopped.", _receiver);
            }
            finally
            {
                _startStopSemaphore.Release();
            }
        }

        private async Task Dispatch(BrokeredMessage message)
        {
            try
            {
                Exception exception = null;

                try
                {
                    LogInfo("Dispatching", message);
                    await _messageDispatcher.Dispatch(message);
                    LogDebug("Dispatched", message);

                    LogDebug("Completing", message);
                    await message.CompleteAsync();
                    LogInfo("Completed", message);

                    return;
                }
                catch (Exception exc)
                {
                    exception = exc;
                }

                _logger.Error(exception,
                              "Message dispatch failed for {0} from {1} [MessageId:{2}, CorrelationId:{3}]",
                              message.SafelyGetBodyTypeNameOrDefault(),
                              message.ReplyTo,
                              message.MessageId,
                              message.CorrelationId);

                try
                {
                    LogDebug("Abandoning", message);
                    await message.AbandonAsync(exception.ExceptionDetailsAsProperties(_clock.UtcNow));
                    LogDebug("Abandoned", message);
                }
                catch (Exception exc)
                {
                    _logger.Error(exc,
                                  "Could not call Abandon() on message {0} from {1} [MessageId:{2}, CorrelationId:{3}]. Possible lock expiry?",
                                  message.SafelyGetBodyTypeNameOrDefault(),
                                  message.MessageId,
                                  message.CorrelationId,
                                  message.ReplyTo);
                }
            }
            catch (Exception exc)
            {
                _logger.Error(exc, "Unhandled exception in message pump");
            }
        }

        private void LogDebug(string activity, BrokeredMessage message)
        {
            _logger.Debug("{0} message {1} from {2} [MessageId:{3}, CorrelationId:{4}]",
                          activity,
                          message.SafelyGetBodyTypeNameOrDefault(),
                          message.ReplyTo,
                          message.MessageId,
                          message.CorrelationId);
        }

        private void LogInfo(string activity, BrokeredMessage message)
        {
            _logger.Info("{0} message {1} from {2} [MessageId:{3}, CorrelationId:{4}]",
                         activity,
                         message.SafelyGetBodyTypeNameOrDefault(),
                         message.ReplyTo,
                         message.MessageId,
                         message.CorrelationId);
        }

        public void Dispose()
        {
            // ReSharper disable CSharpWarnings::CS4014
            Stop(); // don't await
            // ReSharper restore CSharpWarnings::CS4014
        }
    }
}