using System.Diagnostics;

namespace Nimbus.Infrastructure.Heartbeat.PerformanceCounters.System
{
    internal class PerProcessRamUsage : SystemPerformanceCounterWrapper
    {
        public PerProcessRamUsage() : base(new PerformanceCounter("Process", "Working Set", Process.GetCurrentProcess().ProcessName), v => v)
        {
        }
    }
}