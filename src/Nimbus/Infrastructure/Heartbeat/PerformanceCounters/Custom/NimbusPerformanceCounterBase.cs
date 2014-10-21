using System.Diagnostics;

namespace Nimbus.Infrastructure.Heartbeat.PerformanceCounters.Custom
{
    internal abstract class NimbusPerformanceCounterBase : PerformanceCounterBase
    {
        public override string CategoryName
        {
            get { return "Nimbus"; }
        }

        public override string CounterName
        {
            get { return GetType().Name; }
        }

        public override string InstanceName
        {
            get { return Process.GetCurrentProcess().ProcessName; }
        }
    }
}