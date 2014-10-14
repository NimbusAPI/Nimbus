using System;
using System.Threading;
using System.Threading.Tasks;

namespace Nimbus.Infrastructure.TaskScheduling
{
    internal interface INimbusTaskFactory
    {
        Task StartNew(Action taskAction, ThreadPriority priority);
        Task<TResult> StartNew<TResult>(Func<TResult> taskFunc, ThreadPriority priority);
    }
}