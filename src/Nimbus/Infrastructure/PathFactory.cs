using System;
using System.Linq;
using System.Text;

namespace Nimbus.Infrastructure
{
    public static class PathFactory
    {
        private const string _queuePrefix = "q";
        private const string _topicPrefix = "t";
        private const string _instanceInputQueuePrefix = "inputqueue";

		private static string _masterPrefix = "";
	    private static string _prefixedQueuePrefix = _queuePrefix;
	    private static string _prefixedTopicPrefix = _topicPrefix;
	    private static string _prefixedInstanceInputQueuePrefix = _instanceInputQueuePrefix;

        // Entity segments can contain only letters, numbers, periods (.), hyphens (-), and underscores.
        private const string _queueCharacterWhitelist = "abcdefghijklmnopqrstuvwxyz1234567890.-";

	    public static void SetMasterPrefix(string masterPrefix)
	    {
		    _masterPrefix = masterPrefix;

		    if (string.IsNullOrEmpty(_masterPrefix))
		    {
			    _masterPrefix = "";
			    _prefixedQueuePrefix = _queuePrefix;
			    _prefixedTopicPrefix = _topicPrefix;
			    _prefixedInstanceInputQueuePrefix = _instanceInputQueuePrefix;
		    }
		    else
		    {
				_prefixedQueuePrefix = _masterPrefix + "." + _queuePrefix;
				_prefixedTopicPrefix = _masterPrefix + "." + _topicPrefix;
				_prefixedInstanceInputQueuePrefix = _masterPrefix + "." + _instanceInputQueuePrefix;
			}
		}

        public static string InputQueuePathFor(string applicationName, string instanceName)
        {
			return Sanitize(string.Format("{0}.{1}.{2}", _prefixedInstanceInputQueuePrefix, applicationName, instanceName));
        }

        public static string QueuePathFor(Type type)
        {
			return Sanitize(_prefixedQueuePrefix + "." + StripGenericQualification(type));
        }

        public static string TopicPathFor(Type type)
        {
			return Sanitize(_prefixedTopicPrefix + "." + StripGenericQualification(type));
        }

        public static string SubscriptionNameFor(string applicationName)
        {
			return Shorten(Sanitize(string.Join(".", new[] { _masterPrefix, applicationName })), 50);
        }

        public static string SubscriptionNameFor(string applicationName, string instanceName)
        {
			return Shorten(Sanitize(string.Join(".", new[] { _masterPrefix, applicationName, instanceName })), 50);
        }

        public static string SubscriptionNameFor(string applicationName, Type handlerType)
        {
			return Shorten(Sanitize(string.Join(".", new[] { _masterPrefix, applicationName, handlerType.Name })), 50);
        }

        public static string SubscriptionNameFor(string applicationName, string instanceName, Type handlerType)
        {
			return Shorten(Sanitize(string.Join(".", new[] { _masterPrefix, applicationName, instanceName, handlerType.Name })), 50);
        }

        private static string StripGenericQualification(Type type)
        {
            if (! type.IsGenericType) return type.FullName;

            var genericArgs = type.GetGenericArguments().Select(arg => arg.Name);

            return type.Namespace + "." + type.Name + "-" + string.Join("-", genericArgs);
        }

        private static string Sanitize(string path)
        {
	        var pathArray = path.StartsWith(".") 
				? path.Substring(1).ToLower().ToCharArray() 
				: path.ToLower().ToCharArray();
			path = string.Join("", pathArray.Select(SanitiseCharacter));
            return path;
        }

        private static string Shorten(string path, int maxlength)
        {
            if (path.Length <= maxlength)
                return path;

            var hash = CalculateAdler32Hash(path);

            var shortPath = path.Substring(0, maxlength - hash.Length) + hash;
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