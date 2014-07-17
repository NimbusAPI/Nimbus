using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Nimbus.Extensions;
using Nimbus.Handlers;
using Nimbus.MessageContracts.Exceptions;

namespace Nimbus.Infrastructure
{
    internal class LongLivedTaskWrapper<T> : LongLivedTaskWrapperBase
    {
        public LongLivedTaskWrapper(ILogger logger, Task<T> handlerTask, ILongRunningTask longRunningHandler, BrokeredMessage message, IClock clock)
            : base(logger, handlerTask, longRunningHandler, message, clock)
        {
        }

        public async Task<T> AwaitCompletion()
        {
            var firstTaskToComplete = await AwaitCompletionInternal(HandlerTask);
            return await ((Task<T>) firstTaskToComplete);
        }
    }

    internal class LongLivedTaskWrapper : LongLivedTaskWrapperBase
    {
        public LongLivedTaskWrapper(ILogger logger, Task handlerTask, ILongRunningTask longRunningHandler, BrokeredMessage message, IClock clock)
            : base(logger, handlerTask, longRunningHandler, message, clock)
        {
        }

        public async Task AwaitCompletion()
        {
            var firstTaskToComplete = await AwaitCompletionInternal(HandlerTask);
            await firstTaskToComplete;
        }
    }

    internal abstract class LongLivedTaskWrapperBase
    {
        private readonly ILogger _logger;
        protected readonly Task HandlerTask;
        private readonly ILongRunningTask _longRunningHandler;
        private readonly BrokeredMessage _message;
        private readonly IClock _clock;

        private bool _completed;
        private readonly object _mutex = new object();

        // BrokeredMessage is sealed and can't easily be mocked so we sub our our
        // invocation strategies for its properties/methods instead.  -andrewh 12/3/2014
        internal static Func<BrokeredMessage, DateTimeOffset> LockedUntilUtcStrategy = m => m.LockedUntilUtc;
        internal static Action<BrokeredMessage> RenewLockStrategy;

        protected LongLivedTaskWrapperBase(ILogger logger, Task handlerTask, ILongRunningTask longRunningHandler, BrokeredMessage message, IClock clock)
        {
            _logger = logger;
            HandlerTask = handlerTask;
            _longRunningHandler = longRunningHandler;
            _message = message;
            _clock = clock;

            // Use the default method unless another has been defined for testing
            if (RenewLockStrategy == null) RenewLockStrategy = RenewLock;
        }

        private void RenewLock(BrokeredMessage message)
        {
            // http://msdn.microsoft.com/en-us/library/microsoft.servicebus.messaging.brokeredmessage.renewlock.aspx
            // RenewLock updates the message lock by resetting the service side timer, and updates the LockedUntilUtc property once RenewLock completes.
            // You can renew locks for the same duration as the entity lock timeout, and there is no maximum duration for a lock renewal.
            _logger.Debug("Renewing message lock for message [MessageType:{0}, MessageId:{1}, CorrelationId:{2}]",
                          message.SafelyGetBodyTypeNameOrDefault(),
                          message.MessageId,
                          message.CorrelationId);
            message.RenewLock();
            _logger.Info("Renewed lock until {0} for message [MessageType:{1}, MessageId:{2}, CorrelationId:{3}]",
                         message.LockedUntilUtc,
                         message.SafelyGetBodyTypeNameOrDefault(),
                         message.MessageId,
                         message.CorrelationId);
        }

        protected async Task<Task> AwaitCompletionInternal(Task handlerTask)
        {
            var tasks = new List<Task> {handlerTask};

#pragma warning disable 4014
            handlerTask.ContinueWith(t =>
#pragma warning restore 4014
            {
                lock (_mutex)
                {
                    _completed = true;
                }
            });

            if (_longRunningHandler != null)
            {
                _logger.Debug("Configuring watcher for long running handler {0} for message [MessageType:{1}, MessageId:{2}, CorrelationId:{3}]",
                              _longRunningHandler.GetType().FullName,
                              _message.SafelyGetBodyTypeNameOrDefault(),
                              _message.MessageId,
                              _message.CorrelationId);
                var watcherTask = Watch(_longRunningHandler, _message);
                tasks.Add(watcherTask);
            }

            var firstTaskToComplete = await Task.WhenAny(tasks);

            if (firstTaskToComplete.IsFaulted)
            {
                ExceptionDispatchInfo.Capture(firstTaskToComplete.Exception.InnerException).Throw();
            }

            return firstTaskToComplete;
        }

        private async Task Watch(ILongRunningTask longRunningHandler, BrokeredMessage message)
        {
            _logger.Debug("Starting to watch long running handler {0} for message [MessageType:{1}, MessageId:{2}, CorrelationId:{3}]",
              _longRunningHandler.GetType().FullName,
              _message.SafelyGetBodyTypeNameOrDefault(),
              _message.MessageId,
              _message.CorrelationId);

            while (true)
            {
                var lockedUntil = LockedUntilUtcStrategy(message);
                var remainingLockTime = lockedUntil.Subtract(_clock.UtcNow);
                var halfOfRemainingLockTime = TimeSpan.FromMilliseconds(remainingLockTime.TotalMilliseconds/2);
                var timeToDelay = halfOfRemainingLockTime > TimeSpan.Zero ? halfOfRemainingLockTime : TimeSpan.Zero;
                await Task.Delay(timeToDelay);

                if (_completed)
                {
                    _logger.Debug("Long running handler has completed {0} for message [MessageType:{1}, MessageId:{2}, CorrelationId:{3}]",
                                  _longRunningHandler.GetType().FullName,
                                  _message.SafelyGetBodyTypeNameOrDefault(),
                                  _message.MessageId,
                                  _message.CorrelationId);
                    return;
                }
                lock (_mutex)
                {
                    if (_completed)
                    {
                        _logger.Debug("Long running handler has completed {0} for message [MessageType:{1}, MessageId:{2}, CorrelationId:{3}]",
                                      _longRunningHandler.GetType().FullName,
                                      _message.SafelyGetBodyTypeNameOrDefault(),
                                      _message.MessageId,
                                      _message.CorrelationId);
                        return;
                    }

                    if (!longRunningHandler.IsAlive)
                    {
                        throw new BusException(
                            "Long running handler is reporting that it is no longer alive! {0} for message [MessageType:{1}, MessageId:{2}, CorrelationId:{3}]"
                                .FormatWith(_longRunningHandler.GetType().FullName,
                                            _message.SafelyGetBodyTypeNameOrDefault(),
                                            _message.MessageId,
                                            _message.CorrelationId));
                    }

                    RenewLockStrategy(message);
                }
            }
        }
    }
}
