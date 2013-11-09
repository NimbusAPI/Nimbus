using System;
using System.Diagnostics;
using System.Threading;

namespace Nimbus.IntegrationTests.Extensions
{
    public static class ThreadExtensions
    {
        public static void SleepUntil(this TimeSpan timeout, Func<bool> exitCondition)
        {
            var sw = Stopwatch.StartNew();
            while (true)
            {
                if (exitCondition()) return;
                if (sw.Elapsed >= timeout) return;
                Thread.Sleep(TimeSpan.FromMilliseconds(100));
            }
        }
    }
}