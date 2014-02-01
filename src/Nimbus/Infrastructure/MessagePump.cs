using System;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Nimbus.InfrastructureContracts;

namespace Nimbus.Infrastructure
{
    public class MessagePump : IMessagePump
    {
        private bool _haveBeenToldToStop;

        private readonly INimbusMessageReceiver _receiver;
        private readonly IMessageDispatcher _dispatcher;
        private readonly ILogger _logger;

        protected readonly TimeSpan BatchTimeout = TimeSpan.FromMinutes(5);

        public MessagePump(INimbusMessageReceiver receiver, IMessageDispatcher dispatcher, ILogger logger)
        {
            _receiver = receiver;
            _dispatcher = dispatcher;
            _logger = logger;
        }

        public void Start()
        {
            Task.Run(() => InternalMessagePump());
        }

        public void Stop()
        {
            _haveBeenToldToStop = true;
        }

        private async Task InternalMessagePump()
        {
            while (!_haveBeenToldToStop)
            {
                BrokeredMessage message;
                Exception exception = null;

                try
                {
                    message = await _receiver.Receive();
                    if (message == null) continue;
                }
                catch (TimeoutException)
                {
                    continue;
                }

                try
                {
                    _logger.Debug("Dispatching message: {0} from {1}", message, message.ReplyTo);
                    await _dispatcher.Dispatch(message);
                    _logger.Debug("Dispatched message: {0} from {1}", message, message.ReplyTo);

                    _logger.Debug("Completing message {0}", message);
                    await message.CompleteAsync();
                    _logger.Debug("Completed message {0}", message);

                    continue;
                }
                catch (Exception exc)
                {
                    exception = exc;
                }

                _logger.Error(exception, "Message dispatch failed");

                _logger.Debug("Abandoning message {0} from {1}", message, message.ReplyTo);
                await message.AbandonAsync(exception.ExceptionDetailsAsProperties());
                _logger.Debug("Abandoned message {0} from {1}", message, message.ReplyTo);
            }
        }
    }
}