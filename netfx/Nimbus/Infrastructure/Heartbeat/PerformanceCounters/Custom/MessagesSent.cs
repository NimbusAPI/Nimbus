using Nimbus.Infrastructure.MessageSendersAndReceivers;

namespace Nimbus.Infrastructure.Heartbeat.PerformanceCounters.Custom
{
    internal class MessagesSent : NimbusPerformanceCounterBase
    {
        public override long GetNextTransformedValue()
        {
            return GlobalMessageCounters.GetAndClearSentMessageCount();
        }
    }
}