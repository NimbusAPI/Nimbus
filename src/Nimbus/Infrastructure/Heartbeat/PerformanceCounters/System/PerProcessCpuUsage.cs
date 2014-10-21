using System;
using System.Diagnostics;

namespace Nimbus.Infrastructure.Heartbeat.PerformanceCounters.System
{
    internal class PerProcessCpuUsage : SystemPerformanceCounterWrapper
    {
        public PerProcessCpuUsage() : base(new PerformanceCounter("Process", "% Processor Time", "_Total"), v => v/Environment.ProcessorCount)
        {
        }
    }
}