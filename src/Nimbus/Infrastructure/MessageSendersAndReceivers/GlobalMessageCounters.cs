using System.Threading;

namespace Nimbus.Infrastructure.MessageSendersAndReceivers
{
    public static class GlobalMessageCounters
    {
        private static long _sentMessageCount;
        private static long _receivedMessageCount;

        public static void IncrementSentMessageCount(int count)
        {
            Interlocked.Add(ref _sentMessageCount, count);
        }

        public static long GetAndClearSentMessageCount()
        {
            return Interlocked.Exchange(ref _sentMessageCount, 0);
        }

        public static void IncrementReceivedMessageCount(int count)
        {
            Interlocked.Add(ref _receivedMessageCount, count);
        }

        public static long GetAndClearReceivedMessageCount()
        {
            return Interlocked.Exchange(ref _receivedMessageCount, 0);
        }
    }
}