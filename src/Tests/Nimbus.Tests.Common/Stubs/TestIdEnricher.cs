using NUnit.Framework;
using Serilog.Core;
using Serilog.Events;

namespace Nimbus.Tests.Common.Stubs
{
    public class TestIdEnricher : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            var testProperties = TestContext.CurrentContext?.Test.Properties;
            if (testProperties == null) return;
            if (!testProperties.ContainsKey("TestId")) return;

            logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty("TestId", new ScalarValue(testProperties["TestId"])));
        }
    }
}