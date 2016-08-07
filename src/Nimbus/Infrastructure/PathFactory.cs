using System;
using System.Linq;
using System.Text;
using Nimbus.Routing;

namespace Nimbus.Infrastructure
{
    public class PathFactory : IPathFactory
    {
        private const string _queuePrefix = "q";
        private const string _topicPrefix = "t";
        private const string _instanceInputQueuePrefix = "inputqueue";

        // Entity segments can contain only letters, numbers, periods (.), hyphens (-), and underscores.
        private const string _queueCharacterWhitelist = "abcdefghijklmnopqrstuvwxyz1234567890.-";

        public string InputQueuePathFor(string applicationName, string instanceName)
        {
            return Sanitize(string.Format("{0}.{1}.{2}", _instanceInputQueuePrefix, applicationName, instanceName));
        }

        public string QueuePathFor(Type type)
        {
            return Sanitize(_queuePrefix + "." + StripGenericQualification(type));
        }

        public string TopicPathFor(Type type)
        {
            return Sanitize(_topicPrefix + "." + StripGenericQualification(type));
        }

        public string SubscriptionNameFor(string applicationName)
        {
            return Shorten(Sanitize(applicationName), 50);
        }

        public string SubscriptionNameFor(string applicationName, string instanceName)
        {
            return Shorten(Sanitize(string.Join(".", new[] {applicationName, instanceName})), 50);
        }

        public string SubscriptionNameFor(string applicationName, Type handlerType)
        {
            return Shorten(Sanitize(string.Join(".", new[] {applicationName, handlerType.Name})), 50);
        }

        public string SubscriptionNameFor(string applicationName, string instanceName, Type handlerType)
        {
            return Shorten(Sanitize(string.Join(".", new[] {applicationName, instanceName, handlerType.Name})), 50);
        }

        private string StripGenericQualification(Type type)
        {
            if (! type.IsGenericType) return type.FullName;

            var genericArgs = type.GetGenericArguments().Select(arg => arg.Name);

            return type.Namespace + "." + type.Name + "-" + string.Join("-", genericArgs);
        }

        private string Sanitize(string path)
        {
            path = string.Join("", path.ToLower().ToCharArray().Select(SanitiseCharacter));
            return path;
        }

        private string Shorten(string path, int maxlength)
        {
            if (path.Length <= maxlength)
                return path;

            var hash = CalculateAdler32Hash(path);

            var shortPath = path.Substring(0, maxlength - hash.Length) + hash;
            return shortPath;
        }

        private char SanitiseCharacter(char currentChar)
        {
            var whiteList = _queueCharacterWhitelist.ToCharArray();

            if (!whiteList.Contains(currentChar))
                return '.';

            return currentChar;
        }

        private string CalculateAdler32Hash(string inputString)
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