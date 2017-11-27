using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using System.Threading.Tasks;
using Nimbus.Configuration.Settings;
using Nimbus.Infrastructure.Dispatching;
using Nimbus.Infrastructure.Logging;
using Nimbus.Infrastructure.MessageSendersAndReceivers;

namespace Nimbus.Infrastructure
{
    [DebuggerDisplay("{_receiver}")]
    internal class MessagePump : IMessagePump
    {
        private readonly EnableDeadLetteringOnMessageExpirationSetting _enableDeadLetteringOnMessageExpiration;
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

        public MessagePump(EnableDeadLetteringOnMessageExpirationSetting enableDeadLetteringOnMessageExpiration,
                           MaxDeliveryAttemptSetting maxDeliveryAttempts,
                           IClock clock,
                           IDeadLetterOffice deadLetterOffice,
                           IDelayedDeliveryService delayedDeliveryService,
                           IDeliveryRetryStrategy deliveryRetryStrategy,
                           IDispatchContextManager dispatchContextManager,
                           ILogger logger,
                           IMessageDispatcher messageDispatcher,
                           INimbusMessageReceiver receiver)
        {
            _enableDeadLetteringOnMessageExpiration = enableDeadLetteringOnMessageExpiration;
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

                var sw = Stopwatch.StartNew();
                _logger.Debug("Message pump for {Receiver} starting...", _receiver);
                await _receiver.Start(Dispatch);
                _logger.Debug("Message pump for {Receiver} started in {Elapsed}", _receiver, sw.Elapsed);
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

                var sw = Stopwatch.StartNew();
                _logger.Debug("Message pump for {Receiver} stopping...", _receiver);
                await _receiver.Stop();
                _logger.Debug("Message pump for {Receiver} stopped in {Elapsed}.", _receiver, sw.Elapsed);
            }
            finally
            {
                _startStopSemaphore.Release();
            }
        }

        private async Task Dispatch(NimbusMessage message)
        {
            DispatchLoggingContext.NimbusMessage = message;

            // Early exit: have we pre-fetched this message and had our lock already expire? If so, just
            // bail - it will already have been picked up by someone else.
            var now = _clock.UtcNow;
            if (message.ExpiresAfter <= now)
            {
                _logger.Warn(
                    "Message {MessageId} appears to have already expired (expires after {ExpiresAfter} and it is now {Now}) so we're not dispatching it. Watch out for clock drift between your hosts!",
                    message.MessageId,
                    message.ExpiresAfter,
                    now);

                await PostToDeadLetterOffice(message);

                return;
            }

            try
            {
                try
                {
                    _logger.Debug("Dispatching message {MessageId}", message.MessageId);
                    message.RecordDeliveryAttempt(now);
                    using (_dispatchContextManager.StartNewDispatchContext(new SubsequentDispatchContext(message)))
                    {
                        await _messageDispatcher.Dispatch(message);
                    }
                    _logger.Debug("Dispatched message {MessageId}", message.MessageId);
                    return;
                }
                catch (Exception exc)
                {
                    _logger.Warn(exc, "Dispatch failed for message {MessageId}", message.MessageId);
                }

                var numDeliveryAttempts = message.DeliveryAttempts.Count();
                if (numDeliveryAttempts >= _maxDeliveryAttempts)
                {
                    _logger.Error("Too many delivery attempts ({DeliveryAttempts}) for message {MessageId}.", numDeliveryAttempts, message.MessageId);
                    await PostToDeadLetterOffice(message);
                }
                else
                {
                    try
                    {
                        var nextDeliveryTime = _deliveryRetryStrategy.CalculateNextRetryTime(message);
                        _logger.Debug("Re-enqueuing message {MessageId} for attempt {DeliveryAttempts} at delivery at {DeliveryTime}",
                                     message.MessageId,
                                     numDeliveryAttempts + 1,
                                     nextDeliveryTime);
                        await _delayedDeliveryService.DeliverAfter(message, nextDeliveryTime);
                    }
                    catch (Exception exc)
                    {
                        _logger.Error(exc, "Failed to re-enqueue message {MessageId} for re-delivery.", message.MessageId);
                    }
                }
            }
            catch (Exception exc)
            {
                _logger.Error(exc, "Unhandled exception in message pump");
            }
        }

        private async Task PostToDeadLetterOffice(NimbusMessage message)
        {
            if (!_enableDeadLetteringOnMessageExpiration) return;

            _logger.Debug("Posting message to dead letter office.");

            try
            {
                await _deadLetterOffice.Post(message);
            }
            catch (Exception exc)
            {
                _logger.Error(exc, "Failed to post message to dead letter office.");
            }
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