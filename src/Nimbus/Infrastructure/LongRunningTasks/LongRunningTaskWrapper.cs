using System;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Nimbus.Handlers;
using Nimbus.Infrastructure.TaskScheduling;

namespace Nimbus.Infrastructure.LongRunningTasks
{
    internal class LongRunningTaskWrapper<T> : LongRunningTaskWrapperBase
    {
        public LongRunningTaskWrapper(Task<T> handlerTask,
                                      ILongRunningTask longRunningHandler,
                                      NimbusMessage message,
                                      IClock clock,
                                      ILogger logger,
                                      TimeSpan messageLockDuration,
                                      INimbusTaskFactory taskFactory)
            : base(handlerTask, longRunningHandler, message, clock, logger, taskFactory, messageLockDuration)
        {
        }

        public async Task<T> AwaitCompletion()
        {
            var firstTaskToComplete = await AwaitCompletionInternal(HandlerTask);
            return await ((Task<T>) firstTaskToComplete);
        }
    }

    internal class LongRunningTaskWrapper : LongRunningTaskWrapperBase
    {
        public LongRunningTaskWrapper(Task handlerTask,
                                      ILongRunningTask longRunningHandler,
                                      NimbusMessage message,
                                      IClock clock,
                                      ILogger logger,
                                      TimeSpan messageLockDuration,
                                      INimbusTaskFactory taskFactory)
            : base(handlerTask, longRunningHandler, message, clock, logger, taskFactory, messageLockDuration)
        {
        }

        public async Task AwaitCompletion()
        {
            var firstTaskToComplete = await AwaitCompletionInternal(HandlerTask);
            await firstTaskToComplete;
        }
    }
}