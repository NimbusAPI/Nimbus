namespace Nimbus.Infrastructure.Heartbeat.PerformanceCounters
{
    internal abstract class PerformanceCounterBase
    {
        public abstract string CategoryName { get; }
        public abstract string CounterName { get; }
        public abstract string InstanceName { get; }
        public abstract long GetNextTransformedValue();
    }
}