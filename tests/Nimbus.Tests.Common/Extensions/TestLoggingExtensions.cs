using System;
using Nimbus.Tests.Common.Stubs;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using Serilog;
using Serilog.Events;

namespace Nimbus.Tests.Common.Extensions
{
    public static class TestLoggingExtensions
    {
        static TestLoggingExtensions()
        {
            TestHarnessLoggerFactory.Create(Guid.Empty, string.Empty);
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        }

        private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = e.ExceptionObject as Exception;
            if (exception == null) return;

            if (exception.Source == "nunit.framework") return; // sigh.
            if (exception.Source == typeof(TestLoggingExtensions).Assembly.GetName().Name) return;

            Log.Warning(exception, "An unhandled exception was thrown by {ExceptionSource}", exception.Source);
        }

        //TODO
        public static void LogTestStart()
        {
            //TestContext.CurrentContext.Test.Properties.Set("TestId", Guid.NewGuid());
            Log.Information("Test {TestName} starting", TestContext.CurrentContext.Test.FullName);
        }

        public static void LogTestResult()
        {
            var testContext = TestContext.CurrentContext;
            var testStatus = testContext.Result.Outcome.Status;
            LogEventLevel level;
            switch (testStatus)
            {
                case TestStatus.Failed:
                    level = LogEventLevel.Error;
                    break;
                case TestStatus.Inconclusive:
                case TestStatus.Skipped:
                    level = LogEventLevel.Warning;
                    break;
                default:
                    level = LogEventLevel.Information;
                    break;
            }

            Log.Logger.Write(level, "Test {TestName} completed with result {TestResult}", testContext.Test.FullName, testStatus);
        }
    }
}