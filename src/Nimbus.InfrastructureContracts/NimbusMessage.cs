using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nimbus.Infrastructure
{
    public class NimbusMessage
    {
        public NimbusMessage(byte[] payload) : this()
        {
            Payload = payload;
        }

        public NimbusMessage()
        {
            TimeToLive = TimeSpan.FromMinutes(30);
            Properties = new Dictionary<string, object>();
        }

        public Guid CorrelationId { get; set; }
        public string ReplyTo { get; set; }
        public TimeSpan TimeToLive { get; set; }
        public DateTime ScheduledEnqueueTimeUtc { get; set; }
        public IDictionary<string, object> Properties { get; set; }
        public Guid MessageId { get; set; }
        public byte[] Payload { get; set; }

        public DateTimeOffset LockedUntilUtc { get; set; }
        public int Size { get; set; }

        [Obsolete("We'll be deleting this shortly.")]
        public async Task RenewLockAsync()
        {
        }

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