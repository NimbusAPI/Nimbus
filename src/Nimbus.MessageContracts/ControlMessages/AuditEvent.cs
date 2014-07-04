using System;
using System.Collections.Generic;
using System.Linq;

namespace Nimbus.MessageContracts.ControlMessages
{
    public class AuditEvent : IBusEvent
    {
        public object MessageBody { get; set; }
        public Dictionary<string, object> Properties { get; set; }
        public DateTimeOffset Timestamp { get; set; }

        public AuditEvent()
        {
        }

        public AuditEvent(object messageBody, IEnumerable<KeyValuePair<string, object>> properties, DateTimeOffset timestamp)
        {
            MessageBody = messageBody;
            Properties = properties.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            Timestamp = timestamp;
        }
    }
}