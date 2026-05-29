using System;
using System.Threading;
using System.Threading.Tasks;
using Nimbus.InfrastructureContracts;
using Nimbus.MessageContracts;

namespace Nimbus.Extensions.Pulse
{
    internal class PulseEngine
    {
        private readonly IBus _bus;
        private readonly PulseScheduleEntry[] _entries;
        private CancellationTokenSource _cts;

        internal PulseEngine(IBus bus, PulseScheduleEntry[] entries)
        {
            _bus = bus;
            _entries = entries;
        }

        internal Task Start()
        {
            _cts = new CancellationTokenSource();
            foreach (var entry in _entries)
                _ = RunEntry(entry, _cts.Token);
            return Task.CompletedTask;
        }

        internal Task Stop()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
            return Task.CompletedTask;
        }

        private async Task RunEntry(PulseScheduleEntry entry, CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                var next = entry.Cron.GetNextOccurrence(DateTimeOffset.UtcNow, TimeZoneInfo.Utc);
                if (next == null) return;

                var delay = next.Value - DateTimeOffset.UtcNow;
                if (delay > TimeSpan.Zero)
                {
                    try { await Task.Delay(delay, ct); }
                    catch (OperationCanceledException) { return; }
                }

                if (ct.IsCancellationRequested) return;

                try { await Fire(entry, next.Value); }
                catch { }
            }
        }

        private async Task Fire(PulseScheduleEntry entry, DateTimeOffset scheduledTime)
        {
            var message = entry.Message;
            if (message is IPulseMessage pulseMessage)
                pulseMessage.PulseTime = scheduledTime;

            if (entry.IsCommand)
                await _bus.Send((dynamic)message);
            else
                await _bus.Publish((dynamic)message);
        }
    }
}
