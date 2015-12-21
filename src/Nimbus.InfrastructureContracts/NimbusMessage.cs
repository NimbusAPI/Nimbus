using System;
using System.Collections.Generic;
using System.Linq;

namespace Nimbus
{
    [Serializable]
    public class NimbusMessage
    {
        protected NimbusMessage()
        {
        }

        public NimbusMessage(string to)
        {
            To = to;
            MessageId = Guid.NewGuid();
            Properties = new Dictionary<string, object>();
            DeliveryAttempts = new DateTimeOffset[0];
            ExpiresAfter = DateTimeOffset.UtcNow.AddMinutes(30); //FIXME awful hack.
        }

        public NimbusMessage(string to, object payload) : this(to)
        {
            Payload = payload;
        }

        public Guid MessageId { get; protected set; }
        public Guid CorrelationId { get; set; }
        public Guid? PrecedingMessageId { get; set; }
        public string From { get; set; }
        public string To { get; protected set; }
        public Guid? InReplyToMessageId { get; set; }
        public DateTime ScheduledEnqueueTimeUtc { get; set; }
        public DateTimeOffset ExpiresAfter { get; set; }
        public DateTimeOffset[] DeliveryAttempts { get; set; }
        public IDictionary<string, object> Properties { get; set; }
        public object Payload { get; protected set; }

        public void RecordDeliveryAttempt(DateTimeOffset deliveryAttemptTimestamp)
        {
            var updatedDeliveryAttempts = DeliveryAttempts
                .Union(new[] {deliveryAttemptTimestamp})
                .OrderBy(d => d)
                .ToArray();

            DeliveryAttempts = updatedDeliveryAttempts;
        }
    }
}