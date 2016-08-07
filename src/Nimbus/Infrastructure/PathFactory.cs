using System;
using System.Linq;
using System.Text;
using Nimbus.Routing;

namespace Nimbus.Infrastructure
{
    public class PathFactory : IPathFactory
    {
        internal const int MaxPathLength = 260;
        internal const int MaxNameLength = 50;

        private const string _queuePrefix = "q";
        private const string _topicPrefix = "t";
        private const string _instanceInputQueuePrefix = "inputqueue";

        // Entity segments can contain only letters, numbers, periods (.), hyphens (-), and underscores.
        private const string _queueCharacterWhitelist = "abcdefghijklmnopqrstuvwxyz1234567890.-";

        private readonly string _globalPrefix;

        private PathFactory(string globalPrefix)
        {
            _globalPrefix = globalPrefix;
        }

        public static PathFactory CreateWithNoPrefix()
        {
            return new PathFactory(string.Empty);
        }

        public static PathFactory CreateWithPrefix(string globalPrefix)
        {
            return new PathFactory($"{globalPrefix}.");
        }

        public string InputQueuePathFor(string applicationName, string instanceName)
        {
            var unsanitizedPath = $"{_globalPrefix}{_instanceInputQueuePrefix}.{applicationName}.{instanceName}";
            var sanitizedPath = Sanitize(unsanitizedPath);
            var path = Shorten(sanitizedPath, MaxPathLength);
            return path;
        }

        public string QueuePathFor(Type type)
        {
            var typeName = StripGenericQualification(type);
            var unsanitizedPath = $"{_globalPrefix}{_queuePrefix}.{typeName}";
            var sanitizedPath = Sanitize(unsanitizedPath);
            var path = Shorten(sanitizedPath, MaxPathLength);
            return path;
        }

        public string TopicPathFor(Type type)
        {
            var typeName = StripGenericQualification(type);
            var unsanitizedPath = $"{_globalPrefix}{_topicPrefix}.{typeName}";
            var sanitizedPath = Sanitize(unsanitizedPath);
            var path = Shorten(sanitizedPath, MaxPathLength);
            return path;
        }

        public string SubscriptionNameFor(string applicationName, Type handlerType)
        {
            var unsanitizedName = $"{applicationName}.{handlerType.Name}";
            var sanitizedName = Sanitize(unsanitizedName);
            var name = Shorten(sanitizedName, MaxNameLength);
            return name;
        }

        public string SubscriptionNameFor(string applicationName, string instanceName, Type handlerType)
        {
            var unsanitizedName = $"{applicationName}.{instanceName}.{handlerType.Name}";
            var sanitizedName = Sanitize(unsanitizedName);
            var name = Shorten(sanitizedName, MaxNameLength);
            return name;
        }

        private static string StripGenericQualification(Type type)
        {
            if (!type.IsGenericType) return type.FullName;

            var genericArgs = type.GetGenericArguments().Select(arg => arg.Name);

            return type.Namespace + "." + type.Name + "-" + string.Join("-", genericArgs);
        }

        private static string Sanitize(string path)
        {
            path = string.Join("", path.ToLower().ToCharArray().Select(SanitiseCharacter));
            return path;
        }

        internal static string Shorten(string path, int maxLength)
        {
            if (path.Length <= maxLength)
                return path;

            var hash = CalculateAdler32Hash(path);

            var shortPath = path.Substring(0, maxLength - hash.Length) + hash;
            return shortPath;
        }

        private static char SanitiseCharacter(char currentChar)
        {
            var whiteList = _queueCharacterWhitelist.ToCharArray();

            if (!whiteList.Contains(currentChar))
                return '.';

            return currentChar;
        }

        private static string CalculateAdler32Hash(string inputString)
        {
            const uint BASE = 65521;
            var buffer = Encoding.UTF8.GetBytes(inputString);
            uint checksum = 1;
            var offset = 0;
            var count = buffer.Length;

            var s1 = checksum & 0xFFFF;
            var s2 = checksum >> 16;

            while (count > 0)
            {
                var n = 3800;
                if (n > count)
                {
                    n = count;
                }
                count -= n;
                while (--n >= 0)
                {
                    s1 = s1 + (uint) (buffer[offset++] & 0xff);
                    s2 = s2 + s1;
                }
                s1 %= BASE;
                s2 %= BASE;
            }

            checksum = (s2 << 16) | s1;
            return checksum.ToString();
        }
    }
}