using System;
using System.Runtime.ExceptionServices;
using Nimbus.Tests.Common.Stubs;
using NUnit.Framework;
using Serilog;
using Serilog.Events;

namespace Nimbus.Tests.Common.Extensions
{
    public static class TestLoggingExtensions
    {
        static TestLoggingExtensions()
        {
            TestHarnessLoggerFactory.Create();
            AppDomain.CurrentDomain.FirstChanceException += OnFirstChanceException;
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        }

        private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = e.ExceptionObject as Exception;
            if (exception == null) return;

            if (exception.Source == "nunit.framework") return; // sigh.

            Log.Warning(exception, "An unhandled exception was thrown by {ExceptionSource}", exception.Source);
        }

        private static void OnFirstChanceException(object sender, FirstChanceExceptionEventArgs e)
        {
            var exception = e.Exception;
            if (exception == null) return;

            if (exception is OperationCanceledException) return;
            if (exception.Source == "nunit.framework") return; // sigh.
            if (exception.Source == "System.Xml") return; // sigh.

            Log.Warning(exception, "A first-chance exception was thrown by {ExceptionSource}", exception.Source);
        }

        public static void LogTestStart()
        {
            TestContext.CurrentContext.Test.Properties["TestId"] = Guid.NewGuid();
            Log.Information("Test {TestName} starting", TestContext.CurrentContext.Test.FullName);
        }

        public static void LogTestResult()
        {
            var testContext = TestContext.CurrentContext;
            var testStatus = testContext.Result.Status;
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