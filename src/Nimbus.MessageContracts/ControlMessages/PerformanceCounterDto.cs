using System;
using System.Diagnostics;

namespace Nimbus.MessageContracts.ControlMessages
{
    [DebuggerDisplay("{Timestamp} {CounterName} {CategoryName} {Instance} {Value}")]
    public class PerformanceCounterDto
    {
        public PerformanceCounterDto()
        {
        }

        public PerformanceCounterDto(DateTimeOffset timestamp, string counterName, string categoryName, string instance, long value)
        {
            Timestamp = timestamp;
            CounterName = counterName;
            CategoryName = categoryName;
            Instance = instance;
            Value = value;
        }

        public DateTimeOffset Timestamp { get; set; }
        public string CounterName { get; set; }
        public string CategoryName { get; set; }
        public string Instance { get; set; }
        public long Value { get; set; }
    }
}