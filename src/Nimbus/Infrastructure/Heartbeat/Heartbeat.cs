using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Timers;
using Nimbus.Configuration.Settings;
using Nimbus.Infrastructure.Events;
using Nimbus.MessageContracts.ControlMessages;

namespace Nimbus.Infrastructure.Heartbeat
{
    internal class Heartbeat : IHeartbeat, IDisposable
    {
        private readonly IEventSender _eventSender;
        private readonly IClock _clock;
        private readonly Timer _timer;

        public Heartbeat(HeartbeatIntervalSetting heartbeatInterval, IClock clock, IEventSender eventSender)
        {
            _eventSender = eventSender;
            _clock = clock;
            _timer = new Timer(heartbeatInterval.Value.TotalMilliseconds)
                     {
                         AutoReset = true,
                     };
            _timer.Elapsed += OnTimerElapsed;
        }

        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
#pragma warning disable 4014
            Beat();
#pragma warning restore 4014
        }

        private async Task Beat()
        {
            var heartbeatEvent = CreateHeartbeatEvent();
            await _eventSender.Publish(heartbeatEvent);
        }

        void IHeartbeat.Start()
        {
            _timer.Start();
#pragma warning disable 4014
            Beat();
#pragma warning restore 4014
        }

        public void Stop()
        {
            _timer.Stop();
        }

        private HeartbeatEvent CreateHeartbeatEvent()
        {
            var process = Process.GetCurrentProcess();

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

            _timer.Dispose();
        }
    }
}