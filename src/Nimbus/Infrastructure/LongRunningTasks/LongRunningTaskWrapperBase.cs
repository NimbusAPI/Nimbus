using System;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Nimbus.Handlers;
using Nimbus.Infrastructure.TaskScheduling;
using Nimbus.MessageContracts.Exceptions;

namespace Nimbus.Infrastructure.LongRunningTasks
{
    internal abstract class LongRunningTaskWrapperBase
    {
        protected readonly Task HandlerTask;
        private readonly ILongRunningTask _longRunningHandler;
        private readonly BrokeredMessage _message;
        private readonly IClock _clock;
        private readonly ILogger _logger;
        private readonly TimeSpan _messageLockDuration;
        private readonly INimbusTaskFactory _taskFactory;

        private const double _acceptableRemainingLockProportion = (double)2/3;

        // BrokeredMessage is sealed and can't easily be mocked so we sub our our
        // invocation strategies for its properties/methods instead.  -andrewh 12/3/2014
        internal static Func<BrokeredMessage, DateTimeOffset> LockedUntilUtcStrategy = m => m.LockedUntilUtc;
        internal static Func<BrokeredMessage, Task> RenewLockStrategy = m => m.RenewLockAsync();

        protected LongRunningTaskWrapperBase(Task handlerTask,
                                             ILongRunningTask longRunningHandler,
                                             BrokeredMessage message,
                                             IClock clock,
                                             ILogger logger,
                                             INimbusTaskFactory taskFactory,
                                             TimeSpan messageLockDuration)
        {
            HandlerTask = handlerTask;
            _longRunningHandler = longRunningHandler;
            _message = message;
            _clock = clock;
            _logger = logger;
            _messageLockDuration = messageLockDuration;
            _taskFactory = taskFactory;

            _logger.Debug("Long-lived task wrapper created for message {MessageId}", message.MessageId);
        }

        protected async Task<Task> AwaitCompletionInternal(Task handlerTask)
        {
            _logger.Debug("{0}.{1} for message ID {2}", GetType().Name, "AwaitCompletionInternal", _message.MessageId);

            var watcherTask = Watch(_longRunningHandler, _message);
            var tasks = new[] {handlerTask, watcherTask};
            var firstTaskToComplete = await Task.WhenAny(tasks);

            if (firstTaskToComplete.IsFaulted)
            {
                ExceptionDispatchInfo.Capture(firstTaskToComplete.Exception.InnerException).Throw();
            }

            return firstTaskToComplete;
        }

        private Task Watch(ILongRunningTask longRunningHandler, BrokeredMessage message)
        {
            _logger.Debug("Starting long-running task wrapper for message {MessageId}", message.MessageId);
            var task = _taskFactory.StartNew(() => WatchHandlerTask(longRunningHandler, message), TaskContext.LongRunningTaskWatcher).Unwrap();
            return task;
        }

        private async Task WatchHandlerTask(ILongRunningTask longRunningHandler, BrokeredMessage message)
        {
            _logger.Debug("Started long-running task wrapper for message {MessageId}", message.MessageId);

            while (true)
            {
                var now = _clock.UtcNow;
                var lockedUntil = LockedUntilUtcStrategy(message);
                var remainingLockTime = lockedUntil.Subtract(now);
                if (remainingLockTime < TimeSpan.Zero)
                {
                    // oops. Missed that boat :|
                    _logger.Warn(
                        "Long-running task wrapper {HandlerType} for message {MessageId} woke up too late (had {LockTimeRemaining} seconds remaining when it woke up).",
                        longRunningHandler.GetType().FullName,
                        message.MessageId,
                        remainingLockTime);
                    return;
                }

                var acceptableRemainingLockDuration = TimeSpan.FromMilliseconds(_messageLockDuration.TotalMilliseconds*_acceptableRemainingLockProportion);
                var remainingTimeBeforeRenewalRequired = remainingLockTime - acceptableRemainingLockDuration;
                var timeToDelay = remainingTimeBeforeRenewalRequired <= TimeSpan.Zero
                                      ? TimeSpan.Zero
                                      : remainingTimeBeforeRenewalRequired;

                if (timeToDelay > TimeSpan.Zero)
                {
                    _logger.Debug(
                        "Sleeping for {SleepDuration} before checking whether lock for message {MessageId} requires renewal (currently has {LockTimeRemaining} remaining seconds).",
                        timeToDelay,
                        message.MessageId,
                        remainingLockTime);
                    await Task.Delay(timeToDelay);
                    //Thread.Sleep(timeToDelay);
                }

                object dispatchComplete;
                if (message.Properties.TryGetValue(MessagePropertyKeys.DispatchComplete, out dispatchComplete) && dispatchComplete as bool? == true)
                {
                    //_logger.Debug("Long-running task wrapper awoke after message {0} had already been dispatched. Nothing to see here.", message.MessageId);
                    return;
                }

                object dispatchAbandoned;
                if (message.Properties.TryGetValue(MessagePropertyKeys.DispatchAbandoned, out dispatchAbandoned) && dispatchAbandoned as bool? == true)
                {
                    return;
                }

                _logger.Info(
                    "Long-running handler {HandlerType} for message {MessageId} requires a lock renewal ({LockTimeRemaining} seconds remaining; {LockTimeRequired} required).",
                    longRunningHandler.GetType().FullName,
                    message.MessageId,
                    lockedUntil.Subtract(now),
                    acceptableRemainingLockDuration);

                if (!longRunningHandler.IsAlive) throw new BusException("Long-running handler died or stopped responding.");
                try
                {
                    await RenewLockStrategy(message);

                    _logger.Debug("Long-running handler {HandlerType} for message {MessageId} renewed its lock (now has {LockTimeRemaining} seconds remaining).",
                                  longRunningHandler.GetType().FullName,
                                  message.MessageId,
                                  LockedUntilUtcStrategy(message).Subtract(now));
                }
                catch (Exception exc)
                {
                    _logger.Error(exc,
                                  "Long-running handler {HandlerType} for message {MessageId} failed to renew its lock (had {LockTimeRemaining} seconds remaining when it attempted to).",
                                  longRunningHandler.GetType().FullName,
                                  message.MessageId,
                                  remainingLockTime);

                    throw;
                }
            }
        }
    }
}
