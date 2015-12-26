using System.Threading.Tasks;
using Nimbus.Configuration;
using Nimbus.Tests.Common.TestUtilities;
using NUnit.Framework;
using Serilog;
using Serilog.Events;

namespace Nimbus.IntegrationTests
{
    [TestFixture]
    [Timeout(TimeoutSeconds*1000)]
    public abstract class TestForBus
    {
        protected const int TimeoutSeconds = 30;

        protected Bus Bus { get; private set; }

        protected virtual async Task Given(BusBuilderConfiguration busBuilderConfiguration)
        {
            MethodCallCounter.Clear();

            Bus = busBuilderConfiguration.Build();
            await Bus.Start();
        }

        protected abstract Task When();

        [TearDown]
        public void TearDown()
        {
            var bus = Bus;
            Bus = null;

            bus?.Dispose();

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