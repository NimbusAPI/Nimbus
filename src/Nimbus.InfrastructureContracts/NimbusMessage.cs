using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Nimbus
{
    [DataContract]
    public class NimbusMessage
    {
        protected NimbusMessage()
        {
        }

        public NimbusMessage(string destinationPath)
        {
            DestinationPath = destinationPath;
            MessageId = Guid.NewGuid();
            Properties = new Dictionary<string, object>();
            DeliveryAttempts = new DateTimeOffset[0];
            ExpiresAfter = DateTimeOffset.UtcNow.AddMinutes(30); //FIXME awful hack.
        }

        public NimbusMessage(string destinationPath, object payload) : this(destinationPath)
        {
            Payload = payload;
        }

        [DataMember]
        public Guid MessageId { get; protected set; }

        [DataMember]
        public Guid CorrelationId { get; set; }

        [DataMember]
        public string DestinationPath { get; protected set; }

        [DataMember]
        public object Payload { get; protected set; }

        [DataMember]
        public string ReplyTo { get; set; }

        [DataMember]
        public DateTime ScheduledEnqueueTimeUtc { get; set; }

        [DataMember]
        public DateTimeOffset ExpiresAfter { get; set; }

        [DataMember]
        public DateTimeOffset[] DeliveryAttempts { get; set; }

        [DataMember]
        public IDictionary<string, object> Properties { get; set; }

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