using NUnit.Framework;
using Serilog.Core;
using Serilog.Events;

namespace Nimbus.Tests.Common.Stubs
{
    public class TestNameEnricher : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            try
            {
                var testName = TestContext.CurrentContext?.Test.FullName;
                if (testName == null) return;

                logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty("TestName", new ScalarValue(testName)));
            }
            catch
            {
                // NUnit throws internal NullReferenceExceptions sometimes when we try to fetch
                // the test details :(
            }
        }
    }
}