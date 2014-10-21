using System;

namespace Nimbus.MessageContracts.ControlMessages
{
    public class HeartbeatEvent : IBusEvent
    {
        public DateTimeOffset Timestamp { get; set; }
        public int ProcessId { get; set; }
        public string ProcessName { get; set; }
        public string MachineName { get; set; }
        public string OSVersion { get; set; }
        public int ProcessorCount { get; set; }
        public DateTimeOffset StartTime { get; set; }
        public TimeSpan UserProcessorTime { get; set; }
        public TimeSpan TotalProcessorTime { get; set; }
        public long WorkingSet64 { get; set; }
        public long PeakWorkingSet64 { get; set; }
        public long VirtualMemorySize64 { get; set; }
        public PerformanceCounterDto[] PerformanceCounters { get; set; }
    }
}