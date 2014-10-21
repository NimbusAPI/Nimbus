using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Nimbus.Configuration.Settings;
using Nimbus.Infrastructure.Events;
using Nimbus.MessageContracts.ControlMessages;

namespace Nimbus.Infrastructure.Heartbeat
{
    internal class Heartbeat : IHeartbeat, IDisposable
    {
        private readonly HeartbeatIntervalSetting _heartbeatInterval;
        private readonly IEventSender _eventSender;
        private readonly IClock _clock;
        private Timer _heartbeatTimer;
        private Timer _collectTimer;
        private bool _isRunning;
        private readonly List<PerformanceCounterWrapper> _performanceCounters = new List<PerformanceCounterWrapper>();
        private readonly List<PerformanceCounterDto> _performanceCounterHistory = new List<PerformanceCounterDto>();
        private readonly object _mutex = new object();

        public Heartbeat(HeartbeatIntervalSetting heartbeatInterval, IClock clock, IEventSender eventSender)
        {
            _heartbeatInterval = heartbeatInterval;
            _eventSender = eventSender;
            _clock = clock;
        }

        private void OnCollectTimerElapsed(object sender, ElapsedEventArgs e)
        {
            Task.Run(() => Collect());
        }

        private async Task Collect()
        {
            lock (_mutex)
            {
                var dtos = _performanceCounters
                    .AsParallel()
                    .Select(c => new PerformanceCounterDto(_clock.UtcNow, c.CounterName, c.CategoryName, c.InstanceName, c.GetNextTransformedValue()))
                    .ToArray();

                _performanceCounterHistory.AddRange(dtos);
            }
        }

        private void OnHeartbeatTimerElapsed(object sender, ElapsedEventArgs e)
        {
            Task.Run(() => Beat());
        }

        private async Task Beat()
        {
            var heartbeatEvent = CreateHeartbeatEvent();
            await _eventSender.Publish(heartbeatEvent);
        }

        public async Task Start()
        {
            if (_isRunning) return;

            try
            {
                var processName = Process.GetCurrentProcess().ProcessName;

                _performanceCounters.Add(new PerformanceCounterWrapper(new PerformanceCounter("Process", "% Processor Time", processName), v => v/Environment.ProcessorCount));
                _performanceCounters.Add(new PerformanceCounterWrapper(new PerformanceCounter("Process", "% Processor Time", "_Total"), v => v/Environment.ProcessorCount));
                _performanceCounters.Add(new PerformanceCounterWrapper(new PerformanceCounter("Process", "Working Set", processName), v => v));
                _performanceCounters.Add(new PerformanceCounterWrapper(new PerformanceCounter("Process", "Working Set", "_Total"), v => v));
            }
            catch (UnauthorizedAccessException)
            {
                // We're not running with admin privileges? Oh, well. No performance counters for you.
            }

            _collectTimer = new Timer(TimeSpan.FromSeconds(5).TotalMilliseconds)
                            {
                                AutoReset = true,
                            };
            _collectTimer.Elapsed += OnCollectTimerElapsed;
            _collectTimer.Start();

            _heartbeatTimer = new Timer(_heartbeatInterval.Value.TotalMilliseconds)
                              {
                                  AutoReset = true,
                              };
            _heartbeatTimer.Elapsed += OnHeartbeatTimerElapsed;
            _heartbeatTimer.Start();

            if (_heartbeatInterval.Value < TimeSpan.MaxValue) await Beat();
            _isRunning = true;
        }

        public async Task Stop()
        {
            if (!_isRunning) return;

            _collectTimer.Stop();
            _collectTimer.Dispose();
            _collectTimer = null;

            _heartbeatTimer.Stop();
            _heartbeatTimer.Dispose();
            _heartbeatTimer = null;

            foreach (var counter in _performanceCounters) counter.Dispose();
            _performanceCounters.Clear();
        }

        private HeartbeatEvent CreateHeartbeatEvent()
        {
            var process = Process.GetCurrentProcess();

            PerformanceCounterDto[] performanceCounterHistory;
            lock (_mutex)
            {
                performanceCounterHistory = _performanceCounterHistory.ToArray();
                _performanceCounterHistory.Clear();
            }

            var heartbeatEvent = new HeartbeatEvent
                                 {
                                     Timestamp = _clock.UtcNow,
                                     ProcessId = process.Id,
                                     ProcessName = process.ProcessName,
                                     MachineName = Environment.MachineName,
                                     OSVersion = Environment.OSVersion.VersionString,
                                     ProcessorCount = Environment.ProcessorCount,
                                     StartTime = new DateTimeOffset(process.StartTime),
                                     UserProcessorTime = process.UserProcessorTime,
                                     TotalProcessorTime = process.TotalProcessorTime,
                                     WorkingSet64 = process.WorkingSet64,
                                     PeakWorkingSet64 = process.PeakWorkingSet64,
                                     VirtualMemorySize64 = process.VirtualMemorySize64,
                                     PerformanceCounters = performanceCounterHistory,
                                 };

            return heartbeatEvent;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;
            Stop().Wait();
        }

        private sealed class PerformanceCounterWrapper : IDisposable
        {
            private readonly PerformanceCounter _counter;
            private readonly Func<float, float> _transform;

            public PerformanceCounterWrapper(PerformanceCounter counter, Func<float, float> transform)
            {
                _counter = counter;
                _transform = transform;
            }

            public string CategoryName
            {
                get { return _counter.CategoryName; }
            }

            public string CounterName
            {
                get { return _counter.CounterName; }
            }

            public string InstanceName
            {
                get { return _counter.InstanceName; }
            }

            public long GetNextTransformedValue()
            {
                var value = _counter.NextValue();
                var transformedValue = _transform(value);
                return (long) Math.Floor(transformedValue);
            }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            private void Dispose(bool disposing)
            {
                if (!disposing) return;
                _counter.Dispose();
            }
        }
    }
}