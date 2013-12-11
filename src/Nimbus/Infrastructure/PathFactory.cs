using System;

namespace Nimbus.Infrastructure
{
    public static class PathFactory
    {
        public static string QueuePathFor(Type type)
        {
            return Sanitize("Q." + type.FullName);
        }

        public static string TopicPathFor(Type type)
        {
            return Sanitize("T." + type.FullName);
        }

        private static string Sanitize(string path)
        {
            // Entity segments can contain only letters, numbers, periods (.), hyphens (-), and underscores.

            //FIXME we should tidy this up a bit.  -andrewh 11/12/2013
            return path
                .Replace("+", ".")
                .Replace("`", ".")
                ;
        }
    }
}