using System.Diagnostics;
using Nimbus.ConcurrentCollections;
using Nimbus.Enrichers;
using Nimbus.Extensions;
using Nimbus.Logger.Serilog;
using Serilog;
using Serilog.Exceptions;

namespace Nimbus.Tests.Common.Stubs
{
    public class TestHarnessLoggerFactory
    {
        private static readonly ThreadSafeLazy<ILogger> _logger;

        static TestHarnessLoggerFactory()
        {
            _logger = new ThreadSafeLazy<ILogger>(CreateLogger);
        }

        private static ILogger CreateLogger()
        {
            var log = new LoggerConfiguration()
                .Enrich.WithProcessId()
                .Enrich.WithThreadId()
                .Enrich.With<TestNameEnricher>()
                .Enrich.With<NimbusMessageEnricher>()
                .Enrich.WithExceptionDetails()
                .WriteTo.Seq("http://localhost:5341")
                .Chain(l => { if (Debugger.IsAttached) l.WriteTo.Trace(); })
                .MinimumLevel.Verbose()
                .CreateLogger();

            Log.Logger = log;

            var logger = new SerilogLogger(log);
            return logger;
        }

        public static ILogger Create()
        {
            return _logger.Value;
        }
    }
}