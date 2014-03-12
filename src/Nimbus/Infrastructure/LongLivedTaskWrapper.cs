using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Nimbus.Handlers;
using Nimbus.MessageContracts.Exceptions;

namespace Nimbus.Infrastructure
{
    internal class LongLivedTaskWrapper
    {
        private readonly Task _handlerTask;
        private readonly ILongRunningHandler _longRunningHandler;
        private readonly BrokeredMessage _message;
        private readonly IClock _clock;

        private bool _completed;
        private readonly object _mutex = new object();

        // BrokeredMessage is sealed and can't easily be mocked so we sub our our
        // invocation strategies for its properties/methods instead.  -andrewh 12/3/2014
        internal static Func<BrokeredMessage, DateTimeOffset> LockedUntilUtcStrategy = m => m.LockedUntilUtc;
        internal static Action<BrokeredMessage> RenewLockStrategy = m => m.RenewLock();

        public LongLivedTaskWrapper(Task handlerTask, ILongRunningHandler longRunningHandler, BrokeredMessage message, IClock clock)
        {
            _handlerTask = handlerTask;
            _longRunningHandler = longRunningHandler;
            _message = message;
            _clock = clock;
        }

        public async Task AwaitCompletion()
        {
            var tasks = new List<Task>();

            var handlerTask = WaitForCompletion(_handlerTask);
            tasks.Add(handlerTask);

            if (_longRunningHandler != null)
            {
                var watcherTask = Watch(_longRunningHandler, _message);
                tasks.Add(watcherTask);
            }

            await Task.WhenAny(tasks);
        }

        private async Task WaitForCompletion(Task handlerTask)
        {
            await Task.Run(async () => await handlerTask);
            lock (_mutex)
            {
                _completed = true;
            }
        }

        private async Task Watch(ILongRunningHandler longRunningHandler, BrokeredMessage message)
        {
            while (true)
            {
                var lockedUntil = LockedUntilUtcStrategy(message);
                var remainingLockTime = lockedUntil.Subtract(_clock.UtcNow);
                var halfOfRemainingLockTime = TimeSpan.FromMilliseconds(remainingLockTime.TotalMilliseconds/2);

                await Task.Delay(halfOfRemainingLockTime);

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