using System;
using System.Linq;

namespace Nimbus.Infrastructure
{
    public static class PathFactory
    {
        private const string _queuePrefix = "q";
        private const string _topicPrefix = "t";
        private const string _instanceInputQueuePrefix = "inputqueue";

        // Entity segments can contain only letters, numbers, periods (.), hyphens (-), and underscores.
        private const string _queueCharacterWhitelist = "abcdefghijklmnopqrstuvwxyz1234567890.-";

        public static string InputQueuePathFor(string applicationName, string instanceName)
        {
            return Sanitize(string.Format("{0}.{1}.{2}", _instanceInputQueuePrefix, applicationName, instanceName));
        }

        public static bool IsInputQueuePath(this string queuePath)
        {
            return Sanitize(queuePath).StartsWith(_instanceInputQueuePrefix + ".");
        }

        public static string QueuePathFor(Type type)
        {
            return Sanitize(_queuePrefix + "." + StripGenericQualification(type));
        }

        public static string TopicPathFor(Type type)
        {
            return Sanitize(_topicPrefix + "." + StripGenericQualification(type));
        }

        public static string SubscriptionNameFor(string applicationName, Type handlerType)
        {
            return Sanitize(string.Join(".", new[] {applicationName, handlerType.Name}));
        }

        public static string SubscriptionNameFor(string applicationName, string instanceName, Type handlerType)
        {
            return Sanitize(string.Join(".", new[] {applicationName, instanceName, handlerType.Name}));
        }

        private static string StripGenericQualification(Type type)
        {
            if (! type.IsGenericType) return type.FullName;

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
            var whiteList = _queueCharacterWhitelist.ToCharArray();

            if (!whiteList.Contains(currentChar))
                return '.';

            return currentChar;
        }
    }
}