using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Nimbus.Configuration.Settings;
using Nimbus.Extensions;
using Nimbus.Infrastructure.MessageSendersAndReceivers;
using Nimbus.InfrastructureContracts;

namespace Nimbus.Infrastructure
{
    internal class MessagePump : IMessagePump
    {
        private bool _haveBeenToldToStop;

        private readonly INimbusMessageReceiver _receiver;
        private readonly IMessageDispatcher _dispatcher;
        private readonly ILogger _logger;
        private readonly DefaultBatchSizeSetting _defaultBatchSize;
        private readonly IClock _clock;

        private Task _internalMessagePump;

        public MessagePump(INimbusMessageReceiver receiver, IMessageDispatcher dispatcher, ILogger logger, DefaultBatchSizeSetting defaultBatchSize, IClock clock)
        {
            _receiver = receiver;
            _dispatcher = dispatcher;
            _logger = logger;
            _defaultBatchSize = defaultBatchSize;
            _clock = clock;
        }

        public async Task Start()
        {
            if (_internalMessagePump != null)
                throw new InvalidOperationException("Message pump either is already running or was previously running and has not completed shutting down.");

            _logger.Debug("Message pump for {0} starting...", _receiver);
            _haveBeenToldToStop = false;
            await _receiver.WaitUntilReady();
            _internalMessagePump = Task.Run(() => InternalMessagePump());
            _logger.Debug("Message pump for {0} started", _receiver);
        }

        public async Task Stop()
        {
            var internalMessagePump = _internalMessagePump;

            // not running?
            if (internalMessagePump == null) return;

            if (_haveBeenToldToStop)
            {
                await internalMessagePump;
                return;
            }

            // actually stop
            _logger.Debug("Message pump for {0} stopping...", _receiver);
            _haveBeenToldToStop = true;

            await internalMessagePump;
            _internalMessagePump = null;
            _logger.Debug("Message pump for {0} stopped.", _receiver);
        }

        private async Task InternalMessagePump()
        {
            while (!_haveBeenToldToStop)
            {
                try
                {
                    BrokeredMessage[] messages;

                    try
                    {
                        messages = (await _receiver.Receive(_defaultBatchSize)).ToArray();
                        if (messages.None()) continue;
                    }
                    catch (TimeoutException)
                    {
                        continue;
                    }
                    catch (MessagingException exc)
                    {
                        _logger.Error(exc.Message, exc);
                        Thread.Sleep(TimeSpan.FromSeconds(1));
                        continue;
                    }

                    var dispatchTasks = messages.Select(Dispatch).ToArray();
                    await Task.WhenAll(dispatchTasks);
                }
                catch (Exception exc)
                {
                    _logger.Error(exc, "Unhandled exception in message pump");
                }
            }
        }

        private async Task Dispatch(BrokeredMessage message)
        {
            Exception exception = null;

            try
            {
                _logger.Debug("Dispatching message: {0} from {1}", message, message.ReplyTo);
                await _dispatcher.Dispatch(message);
                _logger.Debug("Dispatched message: {0} from {1}", message, message.ReplyTo);

                _logger.Debug("Completing message {0}", message);
                await message.CompleteAsync();
                _logger.Debug("Completed message {0}", message);

                return;
            }
            catch (Exception exc)
            {
                exception = exc;
            }

            _logger.Error(exception, "Message dispatch failed");

            _logger.Debug("Abandoning message {0} from {1}", message, message.ReplyTo);
            await message.AbandonAsync(exception.ExceptionDetailsAsProperties(_clock.UtcNow));
            _logger.Debug("Abandoned message {0} from {1}", message, message.ReplyTo);
        }

        public void Dispose()
        {
            Stop().Wait();
        }
    }
}