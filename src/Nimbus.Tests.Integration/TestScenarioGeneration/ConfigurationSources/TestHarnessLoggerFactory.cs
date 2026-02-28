using System;
using Nimbus.ConcurrentCollections;
using Nimbus.Logger.Serilog.Enrichers;
using Nimbus.Logger.Serilog.Logger.Serilog;
using Nimbus.Tests.Integration.Configuration;
using Serilog;
using Serilog.Exceptions;
using ILogger = Nimbus.InfrastructureContracts.ILogger;

namespace Nimbus.Tests.Integration.TestScenarioGeneration.ConfigurationSources
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