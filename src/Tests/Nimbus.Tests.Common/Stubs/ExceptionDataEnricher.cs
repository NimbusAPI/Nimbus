using System.Collections;
using System.Linq;
using Serilog.Core;
using Serilog.Events;

namespace Nimbus.Tests.Common.Stubs
{
    public class ExceptionDataEnricher : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            if (logEvent.Exception?.Data == null) return;
            if (logEvent.Exception.Data.Count == 0) return;

            var properties = logEvent.Exception.Data
                                     .Cast<DictionaryEntry>()
                                     .Where(e => e.Key is string)
                                     .ToDictionary(e => (string)e.Key, e => e.Value);

            var property = propertyFactory.CreateProperty("ExceptionData", properties, destructureObjects: true);

            logEvent.AddPropertyIfAbsent(property);
        }
    }
}