using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
            ExpiresAfter = DateTimeOffset.UtcNow.AddMinutes(30); //FIXME awful hack.
        }

        public Guid CorrelationId { get; set; }
        public string ReplyTo { get; set; }
        public DateTime ScheduledEnqueueTimeUtc { get; set; }
        public IDictionary<string, object> Properties { get; set; }
        public Guid MessageId { get; set; }
        public object Payload { get; set; }
        public DateTimeOffset ExpiresAfter { get; set; }

        [Obsolete("We'll be deleting this shortly.")]
        public async Task CompleteAsync()
        {
        }

        [Obsolete("We'll be deleting this shortly.")]
        public async Task AbandonAsync(Dictionary<string, object> exceptionDetailsAsProperties)
        {
        }
    }
}