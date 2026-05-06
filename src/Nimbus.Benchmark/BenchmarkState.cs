using System.Diagnostics;

namespace Nimbus.Benchmark;

public class BenchmarkState
{
    private readonly long[] _latencyTicks;
    private readonly CountdownEvent _countdown;
    private long _lastReceiveTick;

    public BenchmarkState(int count)
    {
        _latencyTicks = new long[count];
        _countdown = new CountdownEvent(count);
    }

    public void RecordReceived(long sentAtTicks, int sequenceNumber)
    {
        var now = Stopwatch.GetTimestamp();
        _latencyTicks[sequenceNumber] = now - sentAtTicks;
        Interlocked.Exchange(ref _lastReceiveTick, now);
        _countdown.Signal();
    }

    public bool WaitForCompletion(TimeSpan timeout) => _countdown.Wait(timeout);

    public (long lastReceiveTick, long[] latencyTicks) GetData() => (_lastReceiveTick, _latencyTicks);
}
