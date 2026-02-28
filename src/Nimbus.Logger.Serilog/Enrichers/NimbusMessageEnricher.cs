using Nimbus.Infrastructure.Logging;
using Nimbus.InfrastructureContracts;
using Serilog.Core;
using Serilog.Events;

namespace Nimbus.Logger.Serilog.Enrichers
{
    public class NimbusMessageEnricher : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            const string prefix = "NimbusMessage.";

            var message = DispatchLoggingContext.NimbusMessage;
            if (message == null) return;

            logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty($"{prefix}MessageType", message.Payload?.GetType().FullName));
            foreach (var property in typeof (NimbusMessage).GetProperties())
            {
                if (property.Name == nameof(NimbusMessage.Payload)) continue;

                logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty($"{prefix}{property.Name}", property.GetValue(message)));
            }
        }
    }
}