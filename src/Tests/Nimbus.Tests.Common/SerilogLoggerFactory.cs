using Nimbus.Logger.Serilog;
using Serilog;

namespace Nimbus.Tests.Common
{
    public class TestHarnessLoggerFactory
    {
        public static ILogger Create()
        {
            var log = new LoggerConfiguration()
                .Enrich.WithProcessId()
                .Enrich.WithThreadId()
                .WriteTo.Seq("http://localhost:5341")
                .MinimumLevel.Debug()
                .CreateLogger();

            var logger = new SerilogLogger(log);
            return logger;
        }
    }
}