using System;

namespace Nimbus.Infrastructure
{
    public static class PathFactory
    {
        public static string QueuePathFor(Type type)
        {
            return "Q." + type.FullName;
        }

        public static string TopicPathFor(Type type)
        {
            return "T." + type.FullName;
        }
    }
}