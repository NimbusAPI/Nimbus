using Nimbus.InfrastructureContracts.Handlers;

namespace Nimbus.Benchmark;

public class BenchmarkCommandHandler(BenchmarkState state) : IHandleCommand<BenchmarkCommand>
{
    public Task Handle(BenchmarkCommand busCommand)
    {
        state.RecordReceived(busCommand.SentAtTicks, busCommand.SequenceNumber);
        return Task.CompletedTask;
    }
}
