using System.Threading;

namespace Nimbus.Infrastructure.TaskScheduling
{
    internal class TaskContext
    {
        public static ThreadPriority Send = ThreadPriority.Lowest;
        public static ThreadPriority Dispatch = ThreadPriority.BelowNormal;
        public static ThreadPriority Handle = ThreadPriority.BelowNormal;
        public static ThreadPriority CompleteOrAbandon = ThreadPriority.Normal;
        public static ThreadPriority LongRunningTaskWatcher = ThreadPriority.Highest;
    }
}