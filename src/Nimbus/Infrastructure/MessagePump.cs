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
                try
                {
                    var messages = ReceiveMessages();
                    var completedMessages = new List<BrokeredMessage>();
                    var abandonedMessages = new List<AbandonedMessage>();

                    foreach (var message in messages)
                    {
                        try
                        {
                            PumpMessage(message);
                            completedMessages.Add(message);
                        }
                        catch (Exception exc)
                        {
                            abandonedMessages.Add(new AbandonedMessage(message, exc));
                            _logger.Error(exc, "Message dispatch failed.");
                        }
                    }

                    completedMessages
                        .Select(m => m.CompleteAsync())
                        .WaitAll();

                    abandonedMessages
                        .Select(am => am.Message.AbandonAsync(ExceptionDetailsAsProperties(am.Exception)))
                        .WaitAll();
                }
                catch (Exception exc)
                {
                    _logger.Error(exc, "Message dispatch failed.");
                }
            }
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

        protected class AbandonedMessage
        {
            public AbandonedMessage(BrokeredMessage message, Exception exception)
            {
                Message = message;
                Exception = exception;
            }

            public BrokeredMessage Message { get; set; }
            public Exception Exception { get; set; }
        }
    }
}