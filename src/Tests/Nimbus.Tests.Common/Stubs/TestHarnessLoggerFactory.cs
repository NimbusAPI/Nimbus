using System.Diagnostics;
using Nimbus.Enrichers;
using Nimbus.Extensions;
using Nimbus.Logger.Serilog;
using Serilog;
using Serilog.Exceptions;

namespace Nimbus.Tests.Common.Stubs
{
    public class TestHarnessLoggerFactory
    {
        public static ILogger Create()
        {
            var log = new LoggerConfiguration()
                .Enrich.WithProcessId()
                .Enrich.WithThreadId()
                .Enrich.With<TestNameEnricher>()
                .Enrich.With<NimbusMessageEnricher>()
                .Enrich.WithExceptionDetails()
                .WriteTo.RollingFile(@"C:\Temp\Nimbus\IntegrationTests-{Date}.txt", retainedFileCountLimit: 1)
                .WriteTo.Seq("http://localhost:5341")
                .Chain(l => { if (Debugger.IsAttached) l.WriteTo.Trace(); })
                .MinimumLevel.Verbose()
                .CreateLogger();

            Log.Logger = log;

            var logger = new SerilogLogger(log);
            return logger;
        }
    }
}