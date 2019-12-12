using System;
using Nimbus.ConcurrentCollections;
using Nimbus.Enrichers;
using Nimbus.IntegrationTests.Configuration;
using Nimbus.Logger.Serilog;
using Serilog;
using Serilog.Exceptions;

namespace Nimbus.IntegrationTests.TestScenarioGeneration.ConfigurationSources
{
    public class TestHarnessLoggerFactory
    {
        private static readonly ThreadSafeLazy<Serilog.ILogger> _baseLogger;

        static TestHarnessLoggerFactory()
        {
            _baseLogger = new ThreadSafeLazy<Serilog.ILogger>(CreateLogger);
        }

        private static Serilog.ILogger CreateLogger()
        {
            var seqServerUrl = AppSettingsLoader.Settings.Logging.Seq.Url.ToString();

            var logger = new LoggerConfiguration()
                .Enrich.WithProcessId()
                .Enrich.WithThreadId()
                .Enrich.With<NimbusMessageEnricher>()
                .Enrich.WithExceptionDetails()
                .WriteTo.Seq(seqServerUrl)
                .MinimumLevel.Verbose()
                .CreateLogger();

            Log.Logger = logger;
            return logger;
        }

        public static ILogger Create(Guid testId, string testName)
        {
            var loggerForContext = _baseLogger.Value
                                              .ForContext("TestId", testId)
                                              .ForContext("TestName", testName);

            return new SerilogLogger(loggerForContext);
        }
    }
}