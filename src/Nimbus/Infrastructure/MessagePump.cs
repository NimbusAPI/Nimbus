using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Nimbus.Extensions;
using Nimbus.Infrastructure.Dispatching;
using Nimbus.Infrastructure.MessageSendersAndReceivers;
using Nimbus.Infrastructure.TaskScheduling;

namespace Nimbus.Infrastructure
{
    [DebuggerDisplay("{_receiver}")]
    internal class MessagePump : IMessagePump
    {
        private readonly IClock _clock;
        private readonly IDispatchContextManager _dispatchContextManager;
        private readonly ILogger _logger;
        private readonly IMessageDispatcher _messageDispatcher;
        private readonly INimbusMessageReceiver _receiver;
        private readonly INimbusTaskFactory _taskFactory;

        private bool _started;
        private readonly SemaphoreSlim _startStopSemaphore = new SemaphoreSlim(1, 1);

        public MessagePump(
            IClock clock,
            IDispatchContextManager dispatchContextManager,
            ILogger logger,
            IMessageDispatcher messageDispatcher,
            INimbusMessageReceiver receiver,
            INimbusTaskFactory taskFactory)
        {
            _clock = clock;
            _dispatchContextManager = dispatchContextManager;
            _logger = logger;
            _messageDispatcher = messageDispatcher;
            _receiver = receiver;
            _taskFactory = taskFactory;
        }

        public async Task Start()
        {
            await _startStopSemaphore.WaitAsync();

            try
            {
                if (_started) return;
                _started = true;

                _logger.Debug("Message pump for {Receiver} starting...", _receiver);
                await _receiver.Start(Dispatch);
                _logger.Debug("Message pump for {Receiver} started", _receiver);
            }
            finally
            {
                _startStopSemaphore.Release();
            }
        }

        public async Task Stop()
        {
            await _startStopSemaphore.WaitAsync();

            try
            {
                if (!_started) return;
                _started = false;

                _logger.Debug("Message pump for {Receiver} stopping...", _receiver);
                await _receiver.Stop();
                _logger.Debug("Message pump for {Receiver} stopped.", _receiver);
            }
            finally
            {
                _startStopSemaphore.Release();
            }
        }

        private async Task Dispatch(BrokeredMessage message)
        {
            // Early exit: have we pre-fetched this message and had our lock already expire? If so, just
            // bail - it will already have been picked up by someone else.
            if (message.LockedUntilUtc <= _clock.UtcNow) return;

            try
            {
                Exception exception = null;

                try
                {
                    LogInfo("Dispatching", message);
                    using (_dispatchContextManager.StartNewDispatchContext(new SubsequentDispatchContext(message)))
                    {
                        await _taskFactory.StartNew(() => _messageDispatcher.Dispatch(message), TaskContext.Dispatch).Unwrap();
                    }
                    LogDebug("Dispatched", message);

                    LogDebug("Completing", message);
                    message.Properties[MessagePropertyKeys.DispatchComplete] = true;
                    await _taskFactory.StartNew(() => message.CompleteAsync(), TaskContext.CompleteOrAbandon).Unwrap();
                    LogInfo("Completed", message);

                    return;
                }

                catch (Exception exc)
                {
                    if (exc is MessageLockLostException || (exc.InnerException is MessageLockLostException))
                    {
                        _logger.Error(exc,
                                      "Message completion failed for {Type} from {QueuePath} [MessageId:{MessageId}, CorrelationId:{CorrelationId}]",
                                      message.SafelyGetBodyTypeNameOrDefault(),
                                      message.ReplyTo,
                                      message.MessageId,
                                      message.CorrelationId);
                        return;
                    }

                    _logger.Error(exc,
                                  "Message dispatch failed for {Type} from {QueuePath} [MessageId:{MessageId}, CorrelationId:{CorrelationId}]",
                                  message.SafelyGetBodyTypeNameOrDefault(),
                                  message.ReplyTo,
                                  message.MessageId,
                                  message.CorrelationId);

                    exception = exc;
                }

                try
                {
                    LogDebug("Abandoning", message);
                    await message.AbandonAsync(exception.ExceptionDetailsAsProperties(_clock.UtcNow));
                    LogDebug("Abandoned", message);
                }
                catch (Exception exc)
                {
                    _logger.Error(exc,
                                  "Could not call Abandon() on message {Type} from {QueuePath} [MessageId:{MessageId}, CorrelationId:{CorrelationId}].",
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
            _logger.Debug("{MessagePumpAction} message {Type} from {QueuePath} [MessageId:{MessageId}, CorrelationId:{CorrelationId}]",
                          activity,
                          message.SafelyGetBodyTypeNameOrDefault(),
                          message.ReplyTo,
                          message.MessageId,
                          message.CorrelationId);
        }

        private void LogInfo(string activity, BrokeredMessage message)
        {
            _logger.Info("{MessagePumpAction} message {Type} from {QueuePath} [MessageId:{MessageId}, CorrelationId:{CorrelationId}]",
                         activity,
                         message.SafelyGetBodyTypeNameOrDefault(),
                         message.ReplyTo,
                         message.MessageId,
                         message.CorrelationId);
        }

        public void Dispose()
        {
            // ReSharper disable CSharpWarnings::CS4014
#pragma warning disable 4014
            Stop();
#pragma warning restore 4014
            // ReSharper restore CSharpWarnings::CS4014
        }
    }
}