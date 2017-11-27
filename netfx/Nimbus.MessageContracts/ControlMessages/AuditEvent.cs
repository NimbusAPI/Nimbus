using System;
using System.Collections.Generic;
using System.Linq;

namespace Nimbus.MessageContracts.ControlMessages
{
    public class AuditEvent : IBusEvent
    {
        public string MessageType { get; set; }
        public object MessageBody { get; set; }
        public Dictionary<string, object> Properties { get; set; }
        public DateTimeOffset Timestamp { get; set; }

        public AuditEvent()
        {
        }

        public AuditEvent(string messageType, object messageBody, IEnumerable<KeyValuePair<string, object>> properties, DateTimeOffset timestamp)
        {
            MessageBody = messageBody;
            Properties = properties.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            Timestamp = timestamp;
            MessageType = messageType;
        }
    }
}