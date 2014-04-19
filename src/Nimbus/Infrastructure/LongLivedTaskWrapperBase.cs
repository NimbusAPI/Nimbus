using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Nimbus.Handlers;
using Nimbus.MessageContracts.Exceptions;

namespace Nimbus.Infrastructure
{
    internal class LongLivedTaskWrapper<T> : LongLivedTaskWrapperBase
    {
        public LongLivedTaskWrapper(Task<T> handlerTask, ILongRunningTask longRunningHandler, BrokeredMessage message, IClock clock)
            : base(handlerTask, longRunningHandler, message, clock)
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
        public LongLivedTaskWrapper(Task handlerTask, ILongRunningTask longRunningHandler, BrokeredMessage message, IClock clock)
            : base(handlerTask, longRunningHandler, message, clock)
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

        private bool _completed;
        private readonly object _mutex = new object();

        // BrokeredMessage is sealed and can't easily be mocked so we sub our our
        // invocation strategies for its properties/methods instead.  -andrewh 12/3/2014
        internal static Func<BrokeredMessage, DateTimeOffset> LockedUntilUtcStrategy = m => m.LockedUntilUtc;
        internal static Action<BrokeredMessage> RenewLockStrategy = m => m.RenewLock();

        protected LongLivedTaskWrapperBase(Task handlerTask, ILongRunningTask longRunningHandler, BrokeredMessage message, IClock clock)
        {
            HandlerTask = handlerTask;
            _longRunningHandler = longRunningHandler;
            _message = message;
            _clock = clock;
        }

        protected async Task<Task> AwaitCompletionInternal(Task handlerTask)
        {
            var tasks = new List<Task> {handlerTask};

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

        private async Task WaitForCompletion(Task handlerTask)
        {
            await Task.Run(async () => { await handlerTask; });
            lock (_mutex)
            {
                _completed = true;
            }
        }

        private async Task Watch(ILongRunningTask longRunningHandler, BrokeredMessage message)
        {
            while (true)
            {
                var lockedUntil = LockedUntilUtcStrategy(message);
                var remainingLockTime = lockedUntil.Subtract(_clock.UtcNow);
                var halfOfRemainingLockTime = TimeSpan.FromMilliseconds(remainingLockTime.TotalMilliseconds/2);
                var timeToDelay = halfOfRemainingLockTime > TimeSpan.Zero ? halfOfRemainingLockTime : TimeSpan.Zero;
                await Task.Delay(timeToDelay);

                if (_completed) return;
                lock (_mutex)
                {
                    if (_completed) return;
                    if (!longRunningHandler.IsAlive) throw new BusException("Long-running handler died or stopped responding.");
                    RenewLockStrategy(message);
                }
            }
        }
    }
}