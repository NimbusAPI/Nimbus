using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nimbus.Infrastructure
{
    public class NimbusMessage
    {
        public NimbusMessage(byte[] payload)
        {
            Payload = payload;
        }

        public NimbusMessage()
        {
        }

        public Guid CorrelationId { get; set; }
        public string ReplyTo { get; set; }
        public TimeSpan TimeToLive { get; set; }
        public DateTime ScheduledEnqueueTimeUtc { get; set; }
        public Dictionary<string, object> Properties { get; set; }
        public Guid MessageId { get; set; }
        public byte[] Payload { get; set; }


        public DateTimeOffset LockedUntilUtc { get; set; }
        public int Size { get; set; }

        public async Task RenewLockAsync()
        {
        }

        public async Task CompleteAsync()
        {
        }

        public async Task AbandonAsync(Dictionary<string, object> exceptionDetailsAsProperties)
        {
        }
    }
}