using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Nimbus.Tests.Common.Extensions
{
    public static class ThreadExtensions
    {
        public static async Task WaitUntil(this TimeSpan timeout, Func<bool> exitCondition)
        {
            var sw = Stopwatch.StartNew();
            while (true)
            {
                if (exitCondition()) return;
                if (sw.Elapsed >= timeout) throw new TimeoutException();

                await Task.Delay(TimeSpan.FromMilliseconds(250));
            }
        }
    }
}