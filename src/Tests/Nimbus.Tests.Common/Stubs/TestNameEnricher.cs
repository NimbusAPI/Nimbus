using NUnit.Framework;
using Serilog.Core;
using Serilog.Events;

namespace Nimbus.Tests.Common.Stubs
{
    public class TestNameEnricher : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            var testName = TestContext.CurrentContext?.Test.FullName;
            if (testName == null) return;

            logEvent.AddOrUpdateProperty(new LogEventProperty("TestName", new ScalarValue(testName)));
        }
    }
}