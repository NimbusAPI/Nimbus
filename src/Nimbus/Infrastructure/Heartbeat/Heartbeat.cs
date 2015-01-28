using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Nimbus.ConcurrentCollections;
using Nimbus.Configuration.Settings;
using Nimbus.Extensions;
using Nimbus.Infrastructure.Events;
using Nimbus.Infrastructure.Heartbeat.PerformanceCounters;
using Nimbus.MessageContracts.ControlMessages;

namespace Nimbus.Infrastructure.Heartbeat
{
    internal class Heartbeat : IHeartbeat, IDisposable
    {
        private readonly HeartbeatIntervalSetting _heartbeatInterval;
        private readonly IEventSender _eventSender;
        private readonly ILogger _logger;
        private readonly ApplicationNameSetting _applicationName;
        private readonly InstanceNameSetting _instanceName;
        private readonly IClock _clock;

        private readonly List<PerformanceCounterBase> _performanceCounters = new List<PerformanceCounterBase>();
        private readonly List<PerformanceCounterDto> _performanceCounterHistory = new List<PerformanceCounterDto>();
        private readonly object _mutex = new object();

        private static readonly ThreadSafeLazy<Type[]> _performanceCounterTypes = new ThreadSafeLazy<Type[]>(() => typeof (Heartbeat).Assembly
                                                                                                                                     .DefinedTypes
                                                                                                                                     .Select(ti => ti.AsType())
                                                                                                                                     .Where(
                                                                                                                                         t =>
                                                                                                                                         typeof (PerformanceCounterBase)
                                                                                                                                             .IsAssignableFrom(t))
                                                                                                                                     .Where(t => !t.IsAbstract)
                                                                                                                                     .ToArray());

        private Timer _heartbeatTimer;
        private Timer _collectTimer;
        private bool _isRunning;

        public Heartbeat(ApplicationNameSetting applicationName, HeartbeatIntervalSetting heartbeatInterval, InstanceNameSetting instanceName, IClock clock, IEventSender eventSender, ILogger logger)
        {
            _applicationName = applicationName;
            _heartbeatInterval = heartbeatInterval;
            _instanceName = instanceName;
            _eventSender = eventSender;
            _logger = logger;
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
            if (_heartbeatInterval.Value == TimeSpan.MaxValue) return;
            if (_isRunning) return;

            foreach (var counterType in _performanceCounterTypes.Value)
            {
                try
                {
                    var counter = (PerformanceCounterBase) Activator.CreateInstance(counterType);
                    _performanceCounters.Add(counter);
                }
                catch (Exception exc)
                {
                    // We're not running with admin privileges? Oh, well. No performance counter for you.
                    _logger.Warn("Could not create performance counter {PerformanceCounter}: {Message}", counterType.FullName, exc.ToString());
                }
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

            _isRunning = true;
        }

        public async Task Stop()
        {
            if (_heartbeatInterval.Value == TimeSpan.MaxValue) return;
            if (!_isRunning) return;

            _collectTimer.Stop();
            _collectTimer.Dispose();
            _collectTimer = null;

            _heartbeatTimer.Stop();
            _heartbeatTimer.Dispose();
            _heartbeatTimer = null;

            _performanceCounters
                .AsParallel()
                .OfType<IDisposable>()
                .Do(c => c.Dispose())
                .Done();
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
                                     ApplicationName = _applicationName,
                                     InstanceName = _instanceName,
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
    }
}