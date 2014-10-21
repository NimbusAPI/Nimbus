using System;
using System.Diagnostics;

namespace Nimbus.Infrastructure.Heartbeat.PerformanceCounters.System
{
    internal class TotalCpuUsage : SystemPerformanceCounterWrapper
    {
        public TotalCpuUsage() : base(new PerformanceCounter("Process", "% Processor Time", Process.GetCurrentProcess().ProcessName), v => v/Environment.ProcessorCount)
        {
        }
    }
}