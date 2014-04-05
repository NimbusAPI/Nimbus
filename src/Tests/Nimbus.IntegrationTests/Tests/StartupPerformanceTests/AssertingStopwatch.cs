using System;
using System.Diagnostics;

namespace Nimbus.IntegrationTests.Tests.StartupPerformanceTests
{
    public class AssertingStopwatch : IDisposable
    {
        private readonly string _name;
        private readonly TimeSpan _timeout;
        private readonly Stopwatch _sw;

        public AssertingStopwatch(string name, TimeSpan timeout)
        {
            _name = name;
            _timeout = timeout;
            _sw = Stopwatch.StartNew();
            Trace.WriteLine(string.Format("Stopwatch '{0}' starting...", _name));
        }

        public void Dispose()
        {
            _sw.Stop();
            Trace.WriteLine(string.Format("Stopwatch '{0}' stopped. Total elapsed time: {1}", _name, _sw.Elapsed));
            if (_sw.Elapsed > _timeout) throw new TimeoutException();
        }
    }
}