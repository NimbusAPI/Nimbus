using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Nimbus.Extensions;
using Nimbus.InfrastructureContracts;

namespace Nimbus.Infrastructure
{
    public abstract class MessagePump : IMessagePump
    {
        private bool _haveBeenToldToStop;
        private readonly ILogger _logger;

        protected readonly TimeSpan BatchTimeout = TimeSpan.FromMinutes(5);

        protected MessagePump(ILogger logger)
        {
            _logger = logger;
        }

        public virtual void Start()
        {
            Task.Run((() => InternalMessagePump()));
        }

        public virtual void Stop()
        {
            _haveBeenToldToStop = true;
        }

        private void InternalMessagePump()
        {
            while (!_haveBeenToldToStop)
            {
                BrokeredMessage[] messages;

                try
                {
                    messages = ReceiveMessages();
                }
                catch (TimeoutException)
                {
                    continue;
                }

                try
                {
                    messages
                        .Select(m => Task.Run(delegate
                                              {
                                                  _logger.Debug("Dispatching message: {0} from {1}", m, m.ReplyTo);
                                                  PumpMessage(m);
                                              })
                                         .ContinueWith(t => HandleDispatchCompletion(t, m)))
                        .WaitAll();
                }
                catch (Exception exc)
                {
                    _logger.Error(exc, "Overall message dispatch failed.");
                }
            }
        }

        private async Task HandleDispatchCompletion(Task t, BrokeredMessage m)
        {
            if (t.IsFaulted)
            {
                var exception = t.Exception;
                _logger.Error(exception, "Message dispatch failed");
                await m.AbandonAsync(ExceptionDetailsAsProperties(exception));
                return;
            }

            _logger.Debug("Dispatched message: {0} from {1}", m, m.ReplyTo);
            await m.CompleteAsync();
        }

        protected static Dictionary<string, object> ExceptionDetailsAsProperties(Exception exception)
        {
            if (exception is TargetInvocationException || exception is AggregateException) return ExceptionDetailsAsProperties(exception.InnerException);

            return new Dictionary<string, object>
                   {
                       {MessagePropertyKeys.ExceptionTypeKey, exception.GetType().FullName},
                       {MessagePropertyKeys.ExceptionMessageKey, exception.Message},
                       {MessagePropertyKeys.ExceptionStackTraceKey, exception.StackTrace},
                   };
        }

        protected abstract BrokeredMessage[] ReceiveMessages();
        protected abstract void PumpMessage(BrokeredMessage message);
    }
}