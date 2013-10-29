using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Nimbus.IntegrationTests
{
    public static class TaskExtensions
    {
        public static T WaitForResult<T>(this Task<T> task)
        {
            task.Wait();
            return task.Result;
        }
    }

    public static class ThreadExtensions
    {
        public static void SleepUntil(this TimeSpan timeout, Func<bool> exitCondition)
        {
            var sw = Stopwatch.StartNew();
            while (true)
            {
                if (exitCondition()) return;
                if (sw.Elapsed >= timeout) return;
                Thread.Sleep(TimeSpan.FromSeconds(1));
            }
        }
    }
}