using System;
using System.Collections.Generic;

namespace Nimbus.Infrastructure
{
    public class NimbusMessage
    {
        public NimbusMessage(object payload) : this()
        {
            Payload = payload;
        }

        public NimbusMessage()
        {
            MessageId = Guid.NewGuid();
            Properties = new Dictionary<string, object>();
            DeliveryAttempts = new List<DateTimeOffset>();
            ExpiresAfter = DateTimeOffset.UtcNow.AddMinutes(30); //FIXME awful hack.
        }

        public Guid CorrelationId { get; set; }
        public string ReplyTo { get; set; }
        public DateTime ScheduledEnqueueTimeUtc { get; set; }
        public IDictionary<string, object> Properties { get; set; }
        public Guid MessageId { get; set; }
        public object Payload { get; set; }
        public DateTimeOffset ExpiresAfter { get; set; }
        public ICollection<DateTimeOffset> DeliveryAttempts { get; set; }

        public void RecordDeliveryAttempt(DateTimeOffset deliveryAttemptTimestamp)
        {
            DeliveryAttempts.Add(deliveryAttemptTimestamp);
        }
    }
}