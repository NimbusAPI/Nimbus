using System;
using System.Linq;

namespace Nimbus.Infrastructure
{
    public static class PathFactory
    {
        // Entity segments can contain only letters, numbers, periods (.), hyphens (-), and underscores.
        const string QueueCharacterWhitelist = "abcdefghijklmnopqrstuvwxyz1234567890.-";

        public static string QueuePathFor(Type type)
        {
            return Sanitize("Q." + StripGenericQualification(type));
        }

        public static string TopicPathFor(Type type)
        {
            return Sanitize("T." + StripGenericQualification(type));
        }

        private static string StripGenericQualification(Type type)
        {
            if (! type.IsGenericType)
                return type.FullName;

            var genericArgs = type.GetGenericArguments().Select(arg => arg.Name);

            return type.Namespace + "." + type.Name + "-" + string.Join("-", genericArgs);

        }

        private static string Sanitize(string path)
        {
            
            path = string.Join("", path.ToLower().ToCharArray().Select(SanitiseCharacter));


            return path;
                
        }


        private static char SanitiseCharacter(char currentChar)
        {
            var whiteList = QueueCharacterWhitelist.ToCharArray();

            if (!whiteList.Contains(currentChar))
                return '.';

            return currentChar;
        }
    }
}