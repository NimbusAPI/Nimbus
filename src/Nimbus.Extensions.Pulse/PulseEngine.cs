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
        private readonly IPulseCoordinator _coordinator;
        private readonly ILogger _logger;
        private CancellationTokenSource _cts;

        internal PulseEngine(IBus bus, PulseScheduleEntry[] entries, IPulseCoordinator coordinator, ILogger logger)
        {
            _bus = bus;
            _entries = entries;
            _coordinator = coordinator;
            _logger = logger;
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
            DateTimeOffset? lastOccurrence = null;

            while (!ct.IsCancellationRequested)
            {
                // Task.Delay can return a few milliseconds early, which would make us compute the same
                // occurrence twice. Anchoring off the last occurrence we handled keeps us moving forward.
                var now = DateTimeOffset.UtcNow;
                var from = lastOccurrence.HasValue && lastOccurrence.Value > now ? lastOccurrence.Value : now;

                var next = entry.Cron.GetNextOccurrence(from, TimeZoneInfo.Utc);
                if (next == null)
                {
                    _logger.Info("Pulse schedule {PulseScheduleName} has no further occurrences and will not run again.", entry.Name);
                    return;
                }

                lastOccurrence = next.Value;

                var delay = next.Value - DateTimeOffset.UtcNow;
                if (delay > TimeSpan.Zero)
                {
                    try
                    {
                        await Task.Delay(delay, ct);
                    }
                    catch (OperationCanceledException)
                    {
                        return;
                    }
                }

                if (ct.IsCancellationRequested) return;

                try
                {
                    if (await _coordinator.TryClaim(entry.Name, next.Value, ct))
                    {
                        await Fire(entry, next.Value);
                    }
                    else
                    {
                        _logger.Debug("Pulse occurrence {PulseScheduleName} at {PulseTime} was claimed by another instance.",
                                      entry.Name,
                                      next.Value.ToString("o"));
                    }
                }
                catch (OperationCanceledException) when (ct.IsCancellationRequested)
                {
                    return;
                }
                catch (Exception exc)
                {
                    _logger.Error(exc,
                                  "Pulse failed to fire {PulseScheduleName} scheduled for {PulseTime}. This occurrence has been skipped.",
                                  entry.Name,
                                  next.Value.ToString("o"));
                }
            }
        }

        private async Task Fire(PulseScheduleEntry entry, DateTimeOffset scheduledTime)
        {
            var message = entry.CreateMessage();
            if (message is IPulseMessage pulseMessage)
                pulseMessage.PulseTime = scheduledTime;

            if (entry.IsCommand)
                await _bus.Send((dynamic) message);
            else
                await _bus.Publish((dynamic) message);
        }
    }
}
