using Nimbus.MessageContracts;

namespace Nimbus.Benchmark;

public class BenchmarkCommand : IBusCommand
{
    public long SentAtTicks { get; set; }
    public int SequenceNumber { get; set; }
}
