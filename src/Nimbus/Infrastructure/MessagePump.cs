using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nimbus.Configuration.Settings;
using Nimbus.Extensions;
using Nimbus.Infrastructure.Dispatching;
using Nimbus.Infrastructure.MessageSendersAndReceivers;

namespace Nimbus.Infrastructure
{
    [DebuggerDisplay("{_receiver}")]
    internal class MessagePump : IMessagePump
    {
        private readonly MaxDeliveryAttemptSetting _maxDeliveryAttempts;
        private readonly IClock _clock;
        private readonly IDispatchContextManager _dispatchContextManager;
        private readonly ILogger _logger;
        private readonly IMessageDispatcher _messageDispatcher;
        private readonly INimbusMessageReceiver _receiver;
        private readonly IDeadLetterOffice _deadLetterOffice;
        private readonly IDelayedDeliveryService _delayedDeliveryService;
        private readonly IDeliveryRetryStrategy _deliveryRetryStrategy;

        private bool _started;
        private readonly SemaphoreSlim _startStopSemaphore = new SemaphoreSlim(1, 1);

        public MessagePump(MaxDeliveryAttemptSetting maxDeliveryAttempts,
                           IClock clock,
                           IDeadLetterOffice deadLetterOffice,
                           IDelayedDeliveryService delayedDeliveryService,
                           IDeliveryRetryStrategy deliveryRetryStrategy,
                           IDispatchContextManager dispatchContextManager,
                           ILogger logger,
                           IMessageDispatcher messageDispatcher,
                           INimbusMessageReceiver receiver)
        {
            _maxDeliveryAttempts = maxDeliveryAttempts;
            _clock = clock;
            _dispatchContextManager = dispatchContextManager;
            _logger = logger;
            _messageDispatcher = messageDispatcher;
            _receiver = receiver;
            _deadLetterOffice = deadLetterOffice;
            _delayedDeliveryService = delayedDeliveryService;
            _deliveryRetryStrategy = deliveryRetryStrategy;
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

        private async Task Dispatch(NimbusMessage message)
        {
            // Early exit: have we pre-fetched this message and had our lock already expire? If so, just
            // bail - it will already have been picked up by someone else.
            var now = _clock.UtcNow;
            if (message.ExpiresAfter <= now)
            {
                _logger.Debug(
                    "Message {MessageId} appears to have already expired (expires after {ExpiresAfter} and it is now {Now}) so we're not dispatching it. Watch out for clock drift between your hosts!",
                    message.MessageId,
                    message.ExpiresAfter,
                    now);
                await _deadLetterOffice.Post(message);
                return;
            }

            try
            {
                try
                {
                    LogInfo("Dispatching", message);
                    message.RecordDeliveryAttempt(now);
                    using (_dispatchContextManager.StartNewDispatchContext(new SubsequentDispatchContext(message)))
                    {
                        await _messageDispatcher.Dispatch(message);
                    }
                    LogDebug("Dispatched", message);
                    return;
                }

                catch (Exception exc)
                {
                    _logger.Error(exc,
                                  "Message dispatch failed for {Type} from {QueuePath} [MessageId:{MessageId}, CorrelationId:{CorrelationId}]",
                                  message.SafelyGetBodyTypeNameOrDefault(),
                                  message.From,
                                  message.MessageId,
                                  message.CorrelationId);
                }

                var numDeliveryAttempts = message.DeliveryAttempts.Count();
                if (numDeliveryAttempts >= _maxDeliveryAttempts)
                {
                    _logger.Error("Too many delivery attempts ({DeliveryAttempts}) for message {MessageId}. Posting it to the dead letter office.",
                                  numDeliveryAttempts,
                                  message.MessageId);

                    try
                    {
                        await _deadLetterOffice.Post(message);
                    }
                    catch (Exception exc)
                    {
                        _logger.Error(exc,
                                      "Failed to post message {Type} from {QueuePath} [MessageId:{MessageId}, CorrelationId:{CorrelationId}] to dead letter office.",
                                      message.SafelyGetBodyTypeNameOrDefault(),
                                      message.From,
                                      message.MessageId,
                                      message.CorrelationId);
                    }
                }
                else
                {
                    try
                    {
                        var nextDeliveryTime = _deliveryRetryStrategy.CalculateNextRetryTime(message);
                        _logger.Info("Re-enqueuing message {MessageId} for attempt {DeliveryAttempts} at delivery at {DeliveryTime}",
                                     message.MessageId,
                                     numDeliveryAttempts + 1,
                                     nextDeliveryTime);
                        await _delayedDeliveryService.DeliverAt(message, nextDeliveryTime);
                    }
                    catch (Exception exc)
                    {
                        _logger.Error(exc,
                                      "Failed to re-enqueue message {Type} from {QueuePath} [MessageId:{MessageId}, CorrelationId:{CorrelationId}] for re-delivery.",
                                      message.SafelyGetBodyTypeNameOrDefault(),
                                      message.From,
                                      message.MessageId,
                                      message.CorrelationId);
                    }
                }
            }
            catch (Exception exc)
            {
                _logger.Error(exc, "Unhandled exception in message pump");
            }
        }

        private void LogDebug(string activity, NimbusMessage message)
        {
            _logger.Debug("{MessagePumpAction} message {Type} from {QueuePath} [MessageId:{MessageId}, CorrelationId:{CorrelationId}]",
                          activity,
                          message.SafelyGetBodyTypeNameOrDefault(),
                          message.From,
                          message.MessageId,
                          message.CorrelationId);
        }

        private void LogInfo(string activity, NimbusMessage message)
        {
            _logger.Info("{MessagePumpAction} message {Type} from {QueuePath} [MessageId:{MessageId}, CorrelationId:{CorrelationId}]",
                         activity,
                         message.SafelyGetBodyTypeNameOrDefault(),
                         message.From,
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