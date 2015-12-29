using NUnit.Framework;
using Serilog;
using Serilog.Events;

namespace Nimbus.Tests.Common.Extensions
{
    public static class TestLoggingExtensions
    {
        public static void LogTestStart()
        {
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