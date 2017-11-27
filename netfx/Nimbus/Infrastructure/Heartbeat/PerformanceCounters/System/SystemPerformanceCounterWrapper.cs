using System;
using System.Diagnostics;

namespace Nimbus.Infrastructure.Heartbeat.PerformanceCounters.System
{
    internal abstract class SystemPerformanceCounterWrapper : PerformanceCounterBase, IDisposable
    {
        private readonly PerformanceCounter _counter;
        private readonly Func<float, float> _transform;

        protected SystemPerformanceCounterWrapper(PerformanceCounter counter, Func<float, float> transform)
        {
            _counter = counter;
            _transform = transform;
        }

        public override string CategoryName
        {
            get { return _counter.CategoryName; }
        }

        public override string CounterName
        {
            get { return _counter.CounterName; }
        }

        public override string InstanceName
        {
            get { return _counter.InstanceName; }
        }

        public override long GetNextTransformedValue()
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