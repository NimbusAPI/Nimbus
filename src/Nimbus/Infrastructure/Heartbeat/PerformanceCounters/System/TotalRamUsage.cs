using System.Diagnostics;

namespace Nimbus.Infrastructure.Heartbeat.PerformanceCounters.System
{
    internal class TotalRamUsage : SystemPerformanceCounterWrapper
    {
        public TotalRamUsage() : base(new PerformanceCounter("Process", "Working Set", "_Total"), v => v)
        {
        }
    }
}