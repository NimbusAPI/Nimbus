using System;
using System.Threading.Tasks;

namespace Nimbus.IntegrationTests.Extensions
{
    public static class TaskExtensions
    {
        public static T WaitForResult<T>(this Task<T> task)
        {
            return task.WaitForResult(TimeSpan.MaxValue);
        }

        public static T WaitForResult<T>(this Task<T> task, TimeSpan timeout)
        {
            task.Wait(timeout);
            return task.Result;
        }
    }
}