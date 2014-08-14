using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Nimbus.Handlers;
using Nimbus.Infrastructure.TaskScheduling;
using Nimbus.MessageContracts.Exceptions;

namespace Nimbus.Infrastructure
{
    internal class LongLivedTaskWrapper<T> : LongLivedTaskWrapperBase
    {
        public LongLivedTaskWrapper(Task<T> handlerTask, ILongRunningTask longRunningHandler, BrokeredMessage message, IClock clock, ILogger logger, TimeSpan messageLockDuration)
            : base(handlerTask, longRunningHandler, message, clock, logger, messageLockDuration)
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
        public LongLivedTaskWrapper(Task handlerTask, ILongRunningTask longRunningHandler, BrokeredMessage message, IClock clock, ILogger logger, TimeSpan messageLockDuration)
            : base(handlerTask, longRunningHandler, message, clock, logger, messageLockDuration)
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
        protected readonly Task HandlerTask;
        private readonly ILongRunningTask _longRunningHandler;
        private readonly BrokeredMessage _message;
        private readonly IClock _clock;
        private readonly ILogger _logger;
        private readonly TimeSpan _messageLockDuration;

        private bool _completed;

        // BrokeredMessage is sealed and can't easily be mocked so we sub our our
        // invocation strategies for its properties/methods instead.  -andrewh 12/3/2014
        internal static Func<BrokeredMessage, DateTimeOffset> LockedUntilUtcStrategy = m => m.LockedUntilUtc;
        internal static Func<BrokeredMessage, Task> RenewLockStrategy = m => m.RenewLockAsync();

        protected LongLivedTaskWrapperBase(Task handlerTask,
                                           ILongRunningTask longRunningHandler,
                                           BrokeredMessage message,
                                           IClock clock,
                                           ILogger logger,
                                           TimeSpan messageLockDuration)
        {
            HandlerTask = handlerTask;
            _longRunningHandler = longRunningHandler;
            _message = message;
            _clock = clock;
            _logger = logger;
            _messageLockDuration = messageLockDuration;
        }

        protected async Task<Task> AwaitCompletionInternal(Task handlerTask)
        {
            var tasks = new List<Task> {handlerTask};

#pragma warning disable 4014
            handlerTask.ContinueWith(t => _completed = true, TaskContinuationOptions.ExecuteSynchronously);
#pragma warning restore 4014

            if (_longRunningHandler != null)
            {
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

        private Task Watch(ILongRunningTask longRunningHandler, BrokeredMessage message)
        {
            var task = Task.Factory.StartNew(async () =>
                                                   {
                                                       while (true)
                                                       {
                                                           var lockedUntil = LockedUntilUtcStrategy(message);
                                                           var remainingLockTime = lockedUntil.Subtract(_clock.UtcNow);
                                                           if (remainingLockTime < TimeSpan.Zero)
                                                           {
                                                               // oops. Missed that boat :|
                                                               _logger.Warn(
                                                                   "Long-running handler {0} for message {1} attempted to renew too late (had {2} seconds remaining when it attempted to).",
                                                                   longRunningHandler.GetType().FullName,
                                                                   message.MessageId,
                                                                   remainingLockTime);
                                                               //return;    //FIXME disabled for debugging
                                                           }

                                                           var acceptableRemainingLockDuration = TimeSpan.FromMilliseconds(_messageLockDuration.TotalMilliseconds*2/3);
                                                           var remainingTimeBeforeRenewalRequired = remainingLockTime - acceptableRemainingLockDuration;
                                                           var timeToDelay = remainingTimeBeforeRenewalRequired <= TimeSpan.Zero
                                                                                 ? TimeSpan.Zero
                                                                                 : remainingTimeBeforeRenewalRequired;
                                                           await Task.Delay(timeToDelay);

                                                           if (_completed) return;
                                                           if (message.Properties[MessagePropertyKeys.DispatchComplete] as bool? == true) return;

                                                           _logger.Info("Long-running handler {0} for message {1} requires a lock renewal ({2} seconds remaining; {3} required).",
                                                                        longRunningHandler.GetType().FullName,
                                                                        message.MessageId,
                                                                        lockedUntil.Subtract(_clock.UtcNow),
                                                                        acceptableRemainingLockDuration);

                                                           if (!longRunningHandler.IsAlive) throw new BusException("Long-running handler died or stopped responding.");
                                                           try
                                                           {
                                                               await RenewLockStrategy(message);

                                                               _logger.Debug("Long-running handler {0} for message {1} renewed its lock (now has {2} seconds remaining).",
                                                                             longRunningHandler.GetType().FullName,
                                                                             message.MessageId,
                                                                             LockedUntilUtcStrategy(message).Subtract(_clock.UtcNow));
                                                           }
                                                           catch (Exception)
                                                           {
                                                               _logger.Warn(
                                                                   "Long-running handler {0} for message {1} failed to renew its lock (had {2} seconds remaining when it attempted to).",
                                                                   longRunningHandler.GetType().FullName,
                                                                   message.MessageId,
                                                                   remainingLockTime);

                                                               throw;
                                                           }
                                                       }
                                                   },
                                             new CancellationToken(),
                                             new TaskCreationOptions(),
                                             PriorityScheduler.Highest);
            return task;
        }
    }
}