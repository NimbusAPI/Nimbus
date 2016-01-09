using Nimbus.Infrastructure.Logging;
using Serilog.Core;
using Serilog.Events;

namespace Nimbus.Enrichers
{
    public class NimbusMessageEnricher : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            var message = DispatchLoggingContext.NimbusMessage;
            if (message == null) return;

            logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty("MessageType", message.Payload?.GetType().FullName));
            foreach (var property in typeof (NimbusMessage).GetProperties())
            {
                if (property.Name == nameof(NimbusMessage.Payload)) continue;

                logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty(property.Name, property.GetValue(message)));
            }
        }
    }
}