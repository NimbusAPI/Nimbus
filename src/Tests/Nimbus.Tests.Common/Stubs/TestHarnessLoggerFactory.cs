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
                .Enrich.WithExceptionDetails()
                .WriteTo.Seq("http://localhost:5341")
                .WriteTo.Trace()
                .MinimumLevel.Verbose()
                .CreateLogger();

            Log.Logger = log;

            var logger = new SerilogLogger(log);
            return logger;
        }
    }
}